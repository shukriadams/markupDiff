using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MarkupDiff
{
    /// <summary>
    /// Contains all logic for comparing files.
    /// </summary>
    public class FileAnaylser
    {
        #region PROPERTIES

        public bool ShowCodeLines { get; set; }

        public string LinkingTag { get; set; }

        #endregion

        #region METHODS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceFilePath">Full path to source file.</param>
        /// <param name="destinationFilePath">Full path to target file.</param>
        /// <param name="linkingTag"></param>
        /// <returns></returns>
        public FileComparison Process(string sourceFilePath, string destinationFilePath, string linkingTag)
        {
            this.LinkingTag = linkingTag;

            // check files first
            if (!File.Exists(sourceFilePath))
                throw new Exception(string.Format("Source file {0} not found.", sourceFilePath));

            if (!File.Exists(destinationFilePath))
                throw new Exception(string.Format("Destination file {0} not found.", destinationFilePath));

            // read raw source files as lines
            string[] sourceLinesIn = File.ReadAllLines(sourceFilePath);
            string[] destinationLinesIn = File.ReadAllLines(destinationFilePath);
            
            // send source files to analysis, get them back as a FileComparison object
            FileComparison anaysis = this.Analyse(sourceLinesIn, destinationLinesIn);

            // file analysis returns markup only, replace missing lines, this can be optional
            if (this.ShowCodeLines)
            {
                for (int i = 0; i < sourceLinesIn.Length; i++)
                {
                    if (anaysis.SourceFile.Any(r => r.OriginalLineNumber == i))
                        continue;

                    anaysis.SourceFile.Add(new Line { OriginalText = sourceLinesIn[i], OriginalLineNumber = i, LineType = LineComparisonTypes.Ignore });
                }

                for (int i = 0; i < destinationLinesIn.Length; i++)
                {
                    if (anaysis.DestinationFile.Any(r => r.OriginalLineNumber == i))
                        continue;

                    anaysis.DestinationFile.Add(new Line { OriginalText = destinationLinesIn[i], OriginalLineNumber = i, LineType = LineComparisonTypes.Ignore });
                }
            }

            // add sections to lines code, comments, etc
            BuildDefaultLineSections(anaysis.SourceFile);
            BuildDefaultLineSections(anaysis.DestinationFile);

            // do partial line matches
            ProcessPartialMatches(anaysis.SourceFile, anaysis.DestinationFile);
            ProcessPartialMatches(anaysis.DestinationFile, anaysis.SourceFile);

            // remove padding
            anaysis.SourceFile = anaysis.SourceFile.Where(r => r.LineType != LineComparisonTypes.Whitespace).ToList();
            anaysis.DestinationFile = anaysis.DestinationFile.Where(r => r.LineType != LineComparisonTypes.Whitespace).ToList();

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
        private FileComparison Analyse(IList<string> sourceLinesIn, IList<string> destinationLinesIn)
        {
            IList<Line> sourceLinesOut = new List<Line>();
            IList<Line> destinationLinesOut = new List<Line>();

            // build raw lists first
            for (int i = 0; i < sourceLinesIn.Count; i++)
            {
                // ignores empty lines
                if (sourceLinesIn[i].Trim().Length == 0)
                    continue;

                sourceLinesOut.Add(new Line { OriginalText = sourceLinesIn[i], OriginalLineNumber = i, LineType = LineComparisonTypes.NoMatch });
            }

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

                destinationLinesOut.Add(new Line { OriginalText = destinationLinesIn[i], OriginalLineNumber = i, LineType = LineComparisonTypes.NoMatch });
            }

            // remove code and comments : todo move this to user-editable settings
            string[] codeFlags = { "@", "{{", "<!--", "{{!" };
            RemoveTags(sourceLinesOut, codeFlags);
            RemoveTags(destinationLinesOut, codeFlags);

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
        /// <param name="text"></param>
        /// <param name="matchType"></param>
        /// <returns></returns>
        private static void BuildDefaultLineSections(IEnumerable<Line> lines)
        {
            foreach (var line in lines)
            {
                if (line.Sections.Any())
                    continue;

                string text = line.OriginalText;
                LineComparisonTypes matchType = line.LineType;
                IList<LineSection> sections = new List<LineSection>();
                LineStyleNames className;


                if (matchType == LineComparisonTypes.Whitespace)
                    className = LineStyleNames.Whitespace;
                else if (matchType == LineComparisonTypes.Match)
                    className = LineStyleNames.Match;
                else if (matchType == LineComparisonTypes.Ignore)
                    className = LineStyleNames.Ignore;
                else
                    className = LineStyleNames.NoMatch;

                if (text == null)
                    text = string.Empty;

                if (matchType == LineComparisonTypes.Whitespace)
                    text = "........"; // todo replace this with showing full line in bg color

                sections.Add(new LineSection
                {
                    LineStyle = LineStyle.Get(className),
                    Text = text
                });

                line.Sections = sections;
            }

            
        }

        /// <summary>
        /// Removes lines from collection if they start with any string in remove list.
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="remove"></param>
        private static void RemoveTags(IList<Line> lines, string[] remove)
        {
            int originalCount = lines.Count();
            for (int i = 0; i < originalCount; i++)
            {
                Line line = lines.ElementAt(originalCount - i - 1);
                if (string.IsNullOrEmpty(line.OriginalText))
                    continue;

                var rawText = line.OriginalText.Trim();
                foreach (string checkFor in remove)
                {
                    if (rawText.StartsWith(checkFor))
                    {
                        lines.RemoveAt(originalCount - i - 1);
                        break;
                    }
                }
            }
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
            var partialLinesCheckCount = theseLines.Count > thoseLines.Count ? thoseLines.Count : theseLines.Count;
            for (int i = 0; i < partialLinesCheckCount; i++)
            {
                Line thisLine = theseLines[i];
                Line thatLine = thoseLines[i];
                IList<LineSection> sections = new List<LineSection>();

                // we want nomatch only, and only if there is text on both sides to compare
                if (thisLine.LineType != LineComparisonTypes.NoMatch || string.IsNullOrEmpty(thisLine.OriginalText) || string.IsNullOrEmpty(thatLine.OriginalText))
                    continue;

                string thisLineText = thisLine.OriginalText.Trim();
                string thatLineText = thatLine.OriginalText.Trim();

                int startPadding = thisLine.OriginalText.Length - thisLine.OriginalText.TrimStart().Length;
                if (startPadding != 0) {
                    sections.Insert(0, new LineSection { LineStyle = LineStyle.Get(LineStyleNames.Whitespace), Text = string.Empty.PadLeft(startPadding) });
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
                        LineStyle = LineStyle.Get(LineStyleNames.Match),
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
                        LineStyle = LineStyle.Get(LineStyleNames.Match),
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
                    LineStyle = LineStyle.Get(LineStyleNames.NoMatch),
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
                    thisLine.LineType = LineComparisonTypes.PartialMatch;
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
                    string rawSource = thisLine.OriginalText;
                    string rawDestination = thatLine.OriginalText;
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
                        thisLine.LineType = LineComparisonTypes.Match;
                        thatLine.LineType = LineComparisonTypes.Match;
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
