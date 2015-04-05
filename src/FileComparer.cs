using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MarkupDiff
{
    /// <summary>
    /// Contains all logic for comparing files.
    /// </summary>
    public class FileComparer
    {
        #region PROPERTIES

        /// <summary>
        /// 
        /// </summary>
        public string LinkingTag { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string LinkingTagStart { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string LinkingTagTerminate { get; set; }

        #endregion

        #region CTORS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="linkingTag"></param>
        /// <param name="linkingTagTerminate"></param>
        public FileComparer(string linkingTag, string linkingTagStart, string linkingTagTerminate) 
        {
            this.LinkingTag = linkingTag;
            this.LinkingTagStart = linkingTagStart;
            this.LinkingTagTerminate = linkingTagTerminate;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceFilePath">Full path to source file.</param>
        /// <param name="destinationFilePath">Full path to target file.</param>
        /// <returns></returns>
        public FileComparison Process(string sourceFilePath, string destinationFilePath)
        {
            // check files first
            if (!File.Exists(sourceFilePath))
                throw new Exception(string.Format("Source file {0} not found.", sourceFilePath));

            if (!File.Exists(destinationFilePath))
                throw new Exception(string.Format("Destination file {0} not found.", destinationFilePath));

            // read raw source files as lines
            string[] sourceLinesIn = File.ReadAllLines(sourceFilePath);
            string[] destinationLinesIn = File.ReadAllLines(destinationFilePath);
            
            // send source files to analysis, get them back as a FileComparison object
            FileComparison anaysis = this.FirstStageMatch(sourceLinesIn, destinationLinesIn);



            // file analysis returns markup only, replace missing lines
            // todo : this should be replaced with ignore flag useage
            for (int i = 0; i < sourceLinesIn.Length; i++)
            {
                if (anaysis.SourceFile.Any(r => r.OriginalLineNumber == i))
                    continue;

                anaysis.SourceFile.Add(new Line { RawText = sourceLinesIn[i], OriginalLineNumber = i, LineType = LineTypes.Ignore });
            }

            for (int i = 0; i < destinationLinesIn.Length; i++)
            {
                if (anaysis.DestinationFile.Any(r => r.OriginalLineNumber == i))
                    continue;

                anaysis.DestinationFile.Add(new Line { RawText = destinationLinesIn[i], OriginalLineNumber = i, LineType = LineTypes.Ignore });
            }



            // add sections to lines code, comments, etc
            BuildDefaultLineSections(anaysis.SourceFile);
            BuildDefaultLineSections(anaysis.DestinationFile);

            // do partial line matches
            ProcessPartialMatches(anaysis.SourceFile, anaysis.DestinationFile);
            ProcessPartialMatches(anaysis.DestinationFile, anaysis.SourceFile);

            // first stage matching does exact matches only, partial line matching finds additional matches, but as they work independently,
            // we need to merge them and remove excessive matches
            CleanMatches(anaysis.SourceFile, anaysis.DestinationFile);
            CleanMatches(anaysis.DestinationFile, anaysis.SourceFile);

            // remove padding
            anaysis.SourceFile = anaysis.SourceFile.Where(r => r.LineType != LineTypes.Whitespace).ToList();
            anaysis.DestinationFile = anaysis.DestinationFile.Where(r => r.LineType != LineTypes.Whitespace).ToList();

            // order padding - this isnt necessary if padding is removed, but both should be toggled by user
            anaysis.SourceFile = anaysis.SourceFile.OrderBy(r => r.OriginalLineNumber).ThenBy(r => r.PadsOriginalLineNumber).ToList();
            anaysis.DestinationFile = anaysis.DestinationFile.OrderBy(r => r.OriginalLineNumber).ThenBy(r => r.PadsOriginalLineNumber).ToList();

            return anaysis;
        }

        #endregion

        #region METHODS PRIVATE


        /// <summary>
        /// Does pre-analysis of files, returns a list of match and partial matches lines, with all code and comments removed.
        /// </summary>
        /// <param name="sourceLinesIn"></param>
        /// <param name="destinationLinesIn"></param>
        /// <returns></returns>
        private FileComparison FirstStageMatch(IList<string> sourceLinesIn, IList<string> destinationLinesIn)
        {
            IList<Line> sourceLinesOut = new List<Line>();
            IList<Line> destinationLinesOut = new List<Line>();

            // build raw lists first
            for (int i = 0; i < sourceLinesIn.Count; i++)
            {
                // ignores empty lines
                if (sourceLinesIn[i].Trim().Length == 0)
                    continue;

                sourceLinesOut.Add(new Line { RawText = sourceLinesIn[i], OriginalLineNumber = i, LineType= LineTypes.Markup });
            }


            // find position of linking tag in destination file - it is assumed the user has put the tag where expected content begins,
            // and everything before the tag can be be ignored
            int linkingTagOffset = 0;
            if (!string.IsNullOrEmpty(this.LinkingTag))
            {
                for (int i = 0; i < destinationLinesIn.Count; i++)
                {
                    if (destinationLinesIn[i].Contains(this.LinkingTag))
                    {
                        linkingTagOffset = i;
                        break;
                    }
                }
            }

            for (int i = 0; i < destinationLinesIn.Count; i++)
            {
                if (i < linkingTagOffset)
                    continue;

                // ignores empty lines
                if (destinationLinesIn[i].Trim().Length == 0)
                    continue;

                destinationLinesOut.Add(new Line { RawText = destinationLinesIn[i], OriginalLineNumber = i, LineType = LineTypes.Markup });
            }

            // remove code and comments : todo move this to user-editable settings
            // todo : instead of removing, set line type to ignore and ignore.
            string[] codeFlags = { "@", "{{", "<!--", "{{!" };
            this.FlagIgnored(sourceLinesOut, codeFlags);
            this.FlagIgnored(destinationLinesOut, codeFlags);

            // 
            IgnoreOtherLinkedFiles(sourceLinesOut, 0, false, this.LinkingTag);
            IgnoreOtherLinkedFiles(destinationLinesOut, 0, false, this.LinkingTag);

            // adds padding to each file so exact matches are aligned with eachother
            FindMatches(sourceLinesOut, destinationLinesOut);
            FindMatches(destinationLinesOut, sourceLinesOut);

            // creates a single section from entire original text. this is the default state.
            BuildDefaultLineSections(sourceLinesOut);
            BuildDefaultLineSections(destinationLinesOut);
            
            return new FileComparison(sourceLinesOut, destinationLinesOut);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="startIndex"></param>
        private void IgnoreOtherLinkedFiles(IList<Line> lines, int startIndex, bool markAsIgnore, string parentLevelLinkingTag) 
        {
            for (int i = startIndex; i < lines.Count(); i++) 
            {
                Line line = lines[i];
                if (line.RawText.Trim() == this.LinkingTagTerminate)
                    return;

                string lineRawText = line.RawText.Trim();
                if (lineRawText.StartsWith(this.LinkingTagStart) && lineRawText != parentLevelLinkingTag)
                {
                    IgnoreOtherLinkedFiles(lines, i, true, lineRawText);
                }

                if (markAsIgnore)
                    line.LineType = LineTypes.Ignore;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="matchType"></param>
        /// <returns></returns>
        private static void BuildDefaultLineSections(IEnumerable<Line> lines)
        {
            foreach (var line in lines)
            {
                if (line.Sections.Any())
                    continue;

                IList<LineSection> sections = new List<LineSection>();
                //LineStyleNames className;


                sections.Add(new LineSection
                {
                    //LineStyle = LineStyle.Get(className),
                    Text = line.RawText ?? string.Empty // todo : ensure that this is never null
                });

                line.Sections = sections;
            }

            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="remove"></param>
        private void FlagIgnored(IList<Line> lines, string[] remove)
        {
            foreach(Line line in lines)
            {
                if (string.IsNullOrEmpty(line.RawText))
                    continue;

                var rawText = line.RawText.Trim();

                // assign linkingtag on destination content
                if (rawText == this.LinkingTag)
                {
                    line.LineType = LineTypes.LinkingTag;
                }
                else
                {
                    foreach (string checkFor in remove)
                    {
                        if (rawText.StartsWith(checkFor))
                        {
                            line.LineType = LineTypes.Ignore;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="theseLines"></param>
        /// <param name="thoseLines"></param>
        private static void CleanMatches(IList<Line> theseLines, IList<Line> thoseLines) 
        {
            // todo : not sure this sort is needed, ensure that list already sorted
            theseLines = theseLines.OrderBy(r => r.OriginalLineNumber).ToList();

            int? linkingTagIndex = null;
            Line linkingTagLine = theseLines.FirstOrDefault(r => r.LineType == LineTypes.LinkingTag);
            if (linkingTagLine != null)
                linkingTagIndex = theseLines.IndexOf(linkingTagLine);
            return;

            if (linkingTagIndex.HasValue)
            {
                foreach (Line line in thoseLines.Take(linkingTagIndex.Value))
                    line.LineType = LineTypes.Ignore;
            }

            
            int markupLines = theseLines.Count(r => r.LineType == LineTypes.Markup);
            if (linkingTagIndex.HasValue)
                markupLines += linkingTagIndex.Value;
            
            foreach (Line line in theseLines.Take(theseLines.Count - markupLines))
                line.LineType = LineTypes.Ignore;

                
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="theseLines"></param>
        /// <param name="thoseLines"></param>
        private static void ProcessPartialMatches(IList<Line> theseLines, IList<Line> thoseLines)
        {
            // do partial line matches
            // count the smaller list
            int partialLinesCheckCount = theseLines.Count > thoseLines.Count ? thoseLines.Count : theseLines.Count;
            int thoseLinesStartOffset = 0;
            Line linkingTag = thoseLines.FirstOrDefault(r => r.LineType == LineTypes.LinkingTag);
            if (linkingTag != null)
                thoseLinesStartOffset = thoseLines.IndexOf(linkingTag) + 1;

            int theseLinesStartOffset = 0;
            linkingTag = theseLines.FirstOrDefault(r => r.LineType == LineTypes.LinkingTag);
            if (linkingTag != null)
                theseLinesStartOffset = theseLines.IndexOf(linkingTag) + 1;


            for (int i = 0; i < partialLinesCheckCount; i++)
            {
                if (thoseLines.Count <= i + thoseLinesStartOffset)
                    continue;
                if (theseLines.Count <= i + theseLinesStartOffset)
                    continue;

                Line thisLine = theseLines[i + theseLinesStartOffset];
                Line thatLine = thoseLines[i + thoseLinesStartOffset];
                IList<LineSection> sections = new List<LineSection>();

                if (thisLine.LineType == LineTypes.Ignore)
                    continue;

                // we want nomatch only, and only if there is text on both sides to compare
                if (thisLine.MatchType == MatchTypes.Match || string.IsNullOrEmpty(thisLine.RawText) || string.IsNullOrEmpty(thatLine.RawText))
                    continue;

                string thisLineText = thisLine.RawText.Trim();
                string thatLineText = thatLine.RawText.Trim();

                int startPadding = thisLine.RawText.Length - thisLine.RawText.TrimStart().Length;
                if (startPadding != 0) {
                    sections.Insert(0, new LineSection { Style = LineStyle.Get(LineStyleNames.Whitespace), Text = string.Empty.PadLeft(startPadding) });
                }


                int startSplit = StringHelper.Trace(thisLineText, thatLineText);
                int endSplit = StringHelper.TraceFromEnd(thisLineText, thatLineText);
                bool partialMatch = false;


                LineSection differentTextSection = null;
                LineSection leadTextSection = null;
                LineSection tailTextSection = null;



                // a startsplit means there is common text at start of strings. 0 indicates that there is at least some text
                if (startSplit > 0)
                {
                    leadTextSection = new LineSection
                    {
                        Style = LineStyle.Get(LineStyleNames.Match),
                        Text = thisLineText.Substring(0, startSplit)
                    };
                    partialMatch = true;
                }


                // an end split means common text at end
                if (endSplit != -1 &&
                    endSplit < thisLineText.Length)
                {
                    tailTextSection = new LineSection
                    {
                        Style = LineStyle.Get(LineStyleNames.Match),
                        Text = thisLineText.Substring(endSplit, thisLineText.Length - endSplit)
                    }; 

                    partialMatch = true;
                }


                // different text
                int differenceStart = 0;
                int differenceLength = thisLineText.Length;
                if (startSplit > 0)
                {
                    differenceStart = startSplit;
                    differenceLength = differenceLength - startSplit;
                }
                if (endSplit != 0 && tailTextSection != null)
                {
                    differenceLength = differenceLength - tailTextSection.Text.Length;
                }

                string differentText = thisLineText.Substring(differenceStart, differenceLength);
                differentTextSection = new LineSection
                {
                    Style = LineStyle.Get(LineStyleNames.NoMatch),
                    Text = differentText
                };

                // tail == there is matching text at end of string, put before the matching string
                if (leadTextSection != null)
                    sections.Add(leadTextSection);
                if (differentTextSection != null)
                    sections.Add(differentTextSection);
                if (tailTextSection != null)
                    sections.Add(tailTextSection);

                thisLine.Sections = sections;

                if (partialMatch)
                {
                    thisLine.MatchType = MatchTypes.PartialMatch;
                    thisLine.MatchedWithLineNumber = thatLine.OriginalLineNumber;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="thisLines"></param>
        /// <param name="thoseLines"></param>
        private static void FindMatches(IList<Line> thisLines, IList<Line> thoseLines)
        {
            int thisLinecount = 0;

            while (thisLinecount < thisLines.Count)
            {
                Line thisLine = thisLines[thisLinecount];
                for (int thatLineCount = thisLinecount; thatLineCount < thoseLines.Count - thisLinecount; thatLineCount++)
                {
                    if (thatLineCount >= thoseLines.Count)
                        return; // exit no more lines to compare with

                    Line thatLine = thoseLines[thatLineCount];
                    // --------------------------
                    // this is where full line matching logic is 
                    // do trim to ignore white space, this should toggleable as an app setting
                    string rawSource = thisLine.RawText;
                    string rawDestination = thatLine.RawText;
                    if (rawSource == null)
                        rawSource = string.Empty;

                    if (rawDestination == null)
                        rawDestination = string.Empty;
                    
                    rawSource = rawSource.Trim();
                    rawDestination = rawDestination.Trim();

                    bool match = rawSource == rawDestination;

                    // ignore empty lines
                    if (rawSource.Length == 0 || rawDestination.Length == 0)
                        match = false;


                    // --------------------------
                    if (match) {
                        thisLine.MatchType = MatchTypes.Match;
                        thatLine.MatchType = MatchTypes.Match;
                    }

                    if (match && thisLinecount < thatLineCount) {
                        thisLine.MatchedWithLineNumber = thatLine.OriginalLineNumber;
                        break;
                    }

                }

                thisLinecount++;
            }
        }
        
        #endregion
    }
}
