using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MarkupDiff
{
    public class FileAnaylser
    {
        #region PROPERTIES

        public bool ShowCodeLines { get; set; }

        public string LinkingTag { get; set; }

        #endregion

        #region CTORS

        public FileAnaylser() 
        {

        }

        #endregion

        #region METHODS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="targetFile"></param>
        /// <returns></returns>
        public FileComparison Process(string sourceFile, string targetFile, string linkingTag)
        {
            this.LinkingTag = linkingTag;

            // read raw source files as lines
            string[] sourceLinesIn = File.ReadAllLines(sourceFile);
            string[] destinationLinesIn = File.ReadAllLines(targetFile);
            
            // send source files to analysis, get them abck as a FileComparison object
            FileComparison anaysis = Analyse(sourceLinesIn, destinationLinesIn);

            // file analysis returns markup only, replace missing lines, this can be optional
            if (this.ShowCodeLines)
            {
                for (int i = 0; i < sourceLinesIn.Length; i++)
                {
                    if (anaysis.SourceFile.Any(r => r.OriginalLineNumber == i))
                        continue;

                    anaysis.SourceFile.Add(new Line { OriginalText = sourceLinesIn[i], OriginalLineNumber = i, LineType = LineType.Code });
                }

                for (int i = 0; i < destinationLinesIn.Length; i++)
                {
                    if (anaysis.DestinationFile.Any(r => r.OriginalLineNumber == i))
                        continue;

                    anaysis.DestinationFile.Add(new Line { OriginalText = destinationLinesIn[i], OriginalLineNumber = i, LineType = LineType.Code });
                }
            }

            // add sections to lines code, comments, etc
            BuildDefaultLineSections(anaysis.SourceFile);
            BuildDefaultLineSections(anaysis.DestinationFile);

            // do partial line matches
            ProcessPartialMatches(anaysis.SourceFile, anaysis.DestinationFile);
            ProcessPartialMatches(anaysis.DestinationFile, anaysis.SourceFile);

            // remove padding
            anaysis.SourceFile = anaysis.SourceFile.Where(r => r.LineType != LineType.Padding).ToList();
            anaysis.DestinationFile = anaysis.DestinationFile.Where(r => r.LineType != LineType.Padding).ToList();

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
        /// <param name="sourceFile"></param>
        /// <param name="targetFile"></param>
        /// <returns></returns>
        private FileComparison Analyse(string[] sourceLinesIn, string[] destinationLinesIn)
        {

            IList<Line> sourceLinesOut = new List<Line>();
            IList<Line> destinationLinesOut = new List<Line>();

            // build raw lists first
            for (int i = 0; i < sourceLinesIn.Length; i++)
            {
                // ignores empty lines
                if (sourceLinesIn[i].Trim().Length == 0)
                    continue;

                sourceLinesOut.Add(new Line { OriginalText = sourceLinesIn[i], OriginalLineNumber = i, LineType = LineType.NoMatch });
            }

            int linkingTagOffset = 0;
            if (!string.IsNullOrEmpty(this.LinkingTag))
            {
                for (int i = 0; i < destinationLinesIn.Length; i++)
                {
                    if (destinationLinesIn[i].Contains(this.LinkingTag))
                    {
                        linkingTagOffset = i;
                        break;
                    }
                }
            }



            for (int i = 0; i < destinationLinesIn.Length; i++)
            {
                if (i < linkingTagOffset)
                    continue;

                // ignores empty lines
                if (destinationLinesIn[i].Trim().Length == 0)
                    continue;

                destinationLinesOut.Add(new Line { OriginalText = destinationLinesIn[i], OriginalLineNumber = i, LineType = LineType.NoMatch });
            }

            // remove code and comments
            string[] codeFlags = { "@", "{{", "<!--", "{{!" };
            RemoveTags(sourceLinesOut, codeFlags);
            RemoveTags(destinationLinesOut, codeFlags);

            // adds padding to each file so exact matches are aligned with eachother
            FindMatches(sourceLinesOut, destinationLinesOut);
            FindMatches(destinationLinesOut, sourceLinesOut);


            // process comments
            //string[] commentFlags = { "//", "<!--", "{{!" };
            //MarkTypes(sourceLinesOut, commentFlags, LineType.Comment);
            //MarkTypes(destinationLinesOut, commentFlags, LineType.Comment);

            // proces code
            //string[] codeFlags = { "@", "{{" };
            //MarkTypes(sourceLinesOut, codeFlags, LineType.Code);
            //MarkTypes(destinationLinesOut, codeFlags, LineType.Code);

            // creates a single section from entire original text. this is the default state.
            BuildDefaultLineSections(sourceLinesOut);
            BuildDefaultLineSections(destinationLinesOut);

            // try to do partial matches on sections - if partial match is made, sections will be split into 2-3 chunks.
            //ProcessPartialMatches(sourceLinesOut, destinationLinesOut);
            //ProcessPartialMatches(destinationLinesOut, sourceLinesOut);

            return new FileComparison(sourceLinesOut, destinationLinesOut);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        private static int Trace(string first, string second)
        {
            int position = 0;
            while (position < first.Length && position < second.Length)
            {
                if (first.Substring(position, 1) != second.Substring(position, 1))
                    return position;
                position++;
            }
            // failed to find so far
            if (first.StartsWith(second))
                return second.Length;
            if (second.StartsWith(first))
                return first.Length;
            // give up
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        private static int TraceFromEnd(string first, string second)
        {
            int countFirst = first.Length - 1;
            int countSecond = second.Length - 1;

            while (true)
            {
                if (countFirst < 0 || countSecond < 0)
                    break;

                if (first.Substring(countFirst, 1) != second.Substring(countSecond, 1))
                    return countFirst + 1; // return +1 because by the time we've discovered differne, we've gone 1 too far
                countFirst--;
                countSecond--;
            }
            return -1;
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
                LineType matchType = line.LineType;
                IList<LineSection> sections = new List<LineSection>();
                string className = string.Empty;


                if (matchType == LineType.Padding)
                    className = "padding";
                else if (matchType == LineType.Match)
                    className = "code-match";
                else if (matchType == LineType.Comment)
                    className = "comment";
                else if (matchType == LineType.Code)
                    className = "code";
                else
                    className = "code-nomatch";

                if (text == null)
                    text = string.Empty;

                if (matchType == LineType.Padding)
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
        /// 
        /// </summary>
        /// <param name="lines"></param>
        private static void MarkTypes(IEnumerable<Line> lines, string[] flags, LineType lineType)
        {
            foreach (Line line in lines)
            {
                if (string.IsNullOrEmpty(line.OriginalText))
                    continue;

                var rawText = line.OriginalText.Trim();
                foreach (string flag in flags)
                {
                    if (rawText.StartsWith(flag))
                    {
                        line.LineType = lineType;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="flags"></param>
        private static void RemoveTags(IList<Line> lines, string[] flags)
        {
            int originalCount = lines.Count();
            for (int i = 0; i < originalCount; i++)
            {
                Line line = lines.ElementAt(originalCount - i - 1);
                if (string.IsNullOrEmpty(line.OriginalText))
                    continue;

                var rawText = line.OriginalText.Trim();
                foreach (string flag in flags)
                {
                    if (rawText.StartsWith(flag))
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
                if (thisLine.LineType != LineType.NoMatch || string.IsNullOrEmpty(thisLine.OriginalText) || string.IsNullOrEmpty(thatLine.OriginalText))
                    continue;

                string thisLineText = thisLine.OriginalText.Trim();
                string thatLineText = thatLine.OriginalText.Trim();

                int startPadding = thisLine.OriginalText.Length - thisLine.OriginalText.TrimStart().Length;
                if (startPadding != 0) {
                    sections.Insert(0, new LineSection { LineStyle = LineStyle.Get("padding"), Text = string.Empty.PadLeft(startPadding) });
                }


                int startSplit = Trace(thisLineText, thatLineText);
                int endSplit = TraceFromEnd(thisLineText, thatLineText);
                bool partialMatch = false;


                LineSection differentTextSection = null;
                LineSection leadTextSection = null;
                LineSection tailTextSection = null;



                // a startsplit means there is common text at start of strings. 0 indicates that there is at least some text
                if (startSplit > 0)
                {
                    leadTextSection = new LineSection
                    {
                        LineStyle = LineStyle.Get("code-match"),
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
                        LineStyle = LineStyle.Get("code-match"),
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
                    LineStyle = LineStyle.Get("code-nomatch"),
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
                    thisLine.LineType = LineType.PartialMatch;
                    thisLine.MatchedWithLineNumber = thatLine.OriginalLineNumber;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="theseLines"></param>
        /// <param name="thoseLines"></param>
        private static void FindMatches(IList<Line> theseLines, IList<Line> thoseLines)
        {
            int thisLinecount = 0;

            while (thisLinecount < theseLines.Count)
            {
                Line thisLine = theseLines[thisLinecount];
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
                        thisLine.LineType = LineType.Match;
                        thatLine.LineType = LineType.Match;
                    }

                    if (match && thisLinecount < thatLineCount) {
                        
                        thisLine.MatchedWithLineNumber = thatLine.OriginalLineNumber;
                        
                        /* not sure padding is meaningful
                        for (int padCount = 0; padCount < thatLineCount - thisLinecount; padCount++) {

                            theseLines.Insert(thisLinecount - 1, new Line { 
                                LineType = LineType.Padding, 
                                PadsOriginalLineNumber = thisLine.OriginalLineNumber - 1, 
                                OriginalLineNumber = thisLine.OriginalLineNumber 
                            });

                            thisLinecount++;
                        }
                        */
                        break;
                    }

                }

                thisLinecount++;
            }
        }
        
        #endregion
    }
}
