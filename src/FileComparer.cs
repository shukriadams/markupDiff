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
        #region FIELDs

        /// <summary>
        /// 
        /// </summary>
        private string _linkingTag;

        /// <summary>
        /// 
        /// </summary>
        private Project _project;

        #endregion

        #region METHODS

        /// <summary>
        /// Compares two files at the given paths, returns file contents as lists of Line objects. 
        /// </summary>
        /// <param name="sourceFilePath">Full path to source file.</param>
        /// <param name="destinationFilePath">Full path to target file.</param>
        /// <param name="linkingTag">The exact value of linking tag in destination file used to make connection to source file.</param>
        /// <param name="project"></param>
        /// <returns>FileComparison object ready for rendering.</returns>
        public FileComparison Compare(string sourceFilePath, string destinationFilePath, string linkingTag, Project project)
        {
            _linkingTag = linkingTag;
            _project = project;

            // emsure files exist
            if (!File.Exists(sourceFilePath))
                throw new Exception(string.Format("Source file {0} not found.", sourceFilePath));

            if (!File.Exists(destinationFilePath))
                throw new Exception(string.Format("Destination file {0} not found.", destinationFilePath));

            // read raw source files as lines
            string[] sourceLinesIn = File.ReadAllLines(sourceFilePath);
            string[] destinationLinesIn = File.ReadAllLines(destinationFilePath);

            IList<Line> sourceLinesOut = new List<Line>();
            IList<Line> destinationLinesOut = new List<Line>();

            // build raw lists first, flag empty or whitespace lines as ignore
            for (int i = 0; i < sourceLinesIn.Length; i++)
            {
                if (sourceLinesIn[i].Trim().Length == 0)
                    sourceLinesOut.Add(new Line { RawText = sourceLinesIn[i], OriginalLineNumber = i, LineType = LineTypes.Whitespace, IgnoreFromProcess = true });
                else
                    sourceLinesOut.Add(new Line { RawText = sourceLinesIn[i], OriginalLineNumber = i, LineType = LineTypes.Markup });
            }


            // find position of linking tag in destination file - it is assumed that if the user has put the tag somewhere other than start of file,
            // that position indicates the start of linked content, and everything before the tag position can be be ignored
            int linkingTagOffset = 0;
            if (!string.IsNullOrEmpty(_linkingTag))
            {
                for (int i = 0; i < destinationLinesIn.Length; i++)
                {
                    if (destinationLinesIn[i].Contains(_linkingTag))
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

                if (destinationLinesIn[i].Trim().Length == 0)
                    destinationLinesOut.Add(new Line { RawText = destinationLinesIn[i], OriginalLineNumber = i, LineType = LineTypes.Whitespace, IgnoreFromProcess = true });
                else
                    destinationLinesOut.Add(new Line { RawText = destinationLinesIn[i], OriginalLineNumber = i, LineType = LineTypes.Markup });
            }

            // ignore inline code, comments etc : todo move this to user-editable settings
            this.FlagIgnored(sourceLinesOut);
            this.FlagIgnored(destinationLinesOut);

            // find terminators and ignore everything after them
            this.IgnoreAfterTerminate(sourceLinesOut);
            this.IgnoreAfterTerminate(destinationLinesOut);

            // find other linked files, ignore them
            IgnoreOtherLinkedFiles(sourceLinesOut, 0, false, _linkingTag);
            IgnoreOtherLinkedFiles(destinationLinesOut, 0, false, _linkingTag);

            // find exact matches
            FindFullMatches(sourceLinesOut, destinationLinesOut);
            FindFullMatches(destinationLinesOut, sourceLinesOut);
            
            // break raw text into a single line section
            BuildDefaultLineSections(sourceLinesOut);
            BuildDefaultLineSections(destinationLinesOut);

            // do partial line matches
            FindPartialMatches(sourceLinesOut, destinationLinesOut);
            FindPartialMatches(destinationLinesOut, sourceLinesOut);

            // insert padding lines to align strong matches
            EqualizeLinePositions(sourceLinesOut, destinationLinesOut);
            EqualizeLinePositions(destinationLinesOut, sourceLinesOut);

            return new FileComparison(sourceLinesOut, destinationLinesOut);
        }



        #endregion

        #region METHODS PRIVATE

        /// <summary>
        /// Adds padding to theseLines list so corresponding mathing lines in thoseLones
        /// have the same indexes in collections.
        /// </summary>
        /// <param name="theseLines"></param>
        /// <param name="thoseLines"></param>
        private void EqualizeLinePositions(IList<Line> theseLines, IList<Line> thoseLines)
        {
            bool changed = true;
            while (changed)
            {
                changed = false;
                for (int i = 0; i < theseLines.Count; i++)
                {
                    Line thisLine = theseLines[i];
                    if (!thisLine.IsMatchQualityStrong)
                        continue;

                    Line thatLine = thoseLines.First(r => r.OriginalLineNumber == thisLine.MatchedWithLineNumber);
                    int thatLineIndex = thoseLines.IndexOf(thatLine);
                    if (thatLineIndex > i)
                    {
                        for (int j = 0; j < thatLineIndex - i; j++)
                        {
                            theseLines.Insert(i, new Line
                            {
                                IgnoreFromProcess = true,
                                LineType = LineTypes.Padding,
                                Sections = new[]{
                                    new LineSection{
                                        Style = LineStyle.Get(LineStyleNames.Padding),
                                        Text = "................................................"
                                    }
                                }
                            });
                        }

                        changed = true;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Finds terminator tag for current link ; flags for ignore everything after that
        /// </summary>
        /// <param name="lines"></param>
        private void IgnoreAfterTerminate(IEnumerable<Line> lines) 
        {
            int otherLinkTagsFound = 0;
            bool terminateFound = false;
            foreach (Line line in lines) 
            {
                string rawText = line.RawText.Trim();
                if (rawText.StartsWith(_project.MatchTagStart) && rawText != _linkingTag)
                    otherLinkTagsFound++;

                if (rawText == _project.LinkedTagTerminate)
                {
                    line.LineType = LineTypes.LinkingTag;
                    otherLinkTagsFound--;
                    if (otherLinkTagsFound < 0)
                        terminateFound = true;
                }

                if (terminateFound)
                    line.IgnoreFromProcess = true;
            }
        }

        /// <summary>
        /// Finds content for other linked files and marks them as ignored
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="startIndex"></param>
        /// <param name="markAsIgnore"></param>
        /// <param name="parentLevelLinkingTag"></param>
        private void IgnoreOtherLinkedFiles(IList<Line> lines, int startIndex, bool markAsIgnore, string parentLevelLinkingTag) 
        {
            for (int i = startIndex; i < lines.Count(); i++) 
            {
                Line line = lines[i];
                if (line.RawText.Trim() == _project.LinkedTagTerminate) {
                    line.LineType = LineTypes.LinkingTag;
                    line.IgnoreFromProcess = true;
                    return;
                }
                    

                string lineRawText = line.RawText.Trim();
                if (lineRawText.StartsWith(_project.MatchTagStart) && lineRawText != parentLevelLinkingTag)
                {
                    line.LineType = LineTypes.LinkingTag;
                    line.IgnoreFromProcess = true;
                    IgnoreOtherLinkedFiles(lines, i, true, lineRawText);
                }

                if (markAsIgnore)
                    line.IgnoreFromProcess = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lines"></param>
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
        private void FlagIgnored(IEnumerable<Line> lines)
        {
            foreach(Line line in lines)
            {
                if (string.IsNullOrEmpty(line.RawText))
                    continue;

                var rawText = line.RawText.Trim();

                // assign linkingtag on destination content
                if (rawText == _linkingTag)
                {
                    line.LineType = LineTypes.LinkingTag;
                    line.IgnoreFromProcess = true;
                }
                else
                {
                    foreach (string checkFor in _project.IgnoreFlags)
                    {
                        if (rawText.StartsWith(checkFor))
                        {
                            line.IgnoreFromProcess = true;
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
        private void FindPartialMatches(IList<Line> theseLines, IList<Line> thoseLines)
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

                if (thisLine.IgnoreFromProcess)
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


                LineSection leadTextSection = null;
                LineSection tailTextSection = null;


                // a startsplit means there is common text at start of strings. 0 indicates that there is at least some text
                bool matchQualityStrong = false;
                if (startSplit > 0)
                {
                    leadTextSection = new LineSection
                    {
                        Style = LineStyle.Get(LineStyleNames.Match),
                        Text = thisLineText.Substring(0, startSplit)
                    };
                    partialMatch = true;

                    if (startSplit > _project.StringLengthForStrongMatch ) // todo : make user settable
                        matchQualityStrong = true;
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

                    if (endSplit > _project.StringLengthForStrongMatch) // todo : make user settable
                        matchQualityStrong = true;

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
                
                LineSection differentTextSection = new LineSection
                {
                    Style = LineStyle.Get(LineStyleNames.NoMatch),
                    Text = differentText
                };

                // tail == there is matching text at end of string, put before the matching string
                if (leadTextSection != null)
                    sections.Add(leadTextSection);
                
                sections.Add(differentTextSection);

                if (tailTextSection != null)
                    sections.Add(tailTextSection);

                thisLine.Sections = sections;

                if (partialMatch)
                {
                    thisLine.MatchType = MatchTypes.PartialMatch;
                    thisLine.MatchedWithLineNumber = thatLine.OriginalLineNumber;
                    thisLine.IsMatchQualityStrong = matchQualityStrong;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="thisLines"></param>
        /// <param name="thoseLines"></param>
        private void FindFullMatches(IList<Line> thisLines, IList<Line> thoseLines)
        {
            int thisLineIterate = 0;
            int thatLineLastFoundIndex = 0;
            while (thisLineIterate < thisLines.Count)
            {
                Line thisLine = thisLines[thisLineIterate];
                if (thisLine.IgnoreFromProcess)
                {
                    thisLineIterate++;
                    continue;
                }

                // do not try to match if this line is already linked to an opposite line. cross-linking
                // causes errors
                if (thoseLines.Any(r => r.MatchedWithLineNumber == thisLine.OriginalLineNumber))
                {
                    thisLineIterate++;
                    continue;
                }

                for (int thatLineIterate = thatLineLastFoundIndex; thatLineIterate < thoseLines.Count; thatLineIterate++)
                {
                    Line thatLine = thoseLines[thatLineIterate];
                    if (thatLine.IgnoreFromProcess)
                        continue;

                    // --------------------------
                    // this is where full line matching logic is 
                    // do trim to ignore white space, this should toggleable as an app setting
                    string rawSource = thisLine.RawText;
                    string rawDestination = thatLine.RawText;
                    
                    rawSource = rawSource.Trim();
                    rawDestination = rawDestination.Trim();

                    bool match = rawSource == rawDestination;

                    // --------------------------
                    if (match) {
                        thisLine.MatchType = MatchTypes.Match;
                        thatLine.MatchType = MatchTypes.Match;
                    }

                    if (match) {
                        thatLineLastFoundIndex = thatLineIterate + 1; // + 1 so next match scan starts after current match, else we can match the same line repeatedly
                        thisLine.MatchedWithLineNumber = thatLine.OriginalLineNumber;
                        thisLine.IsMatchQualityStrong = rawSource.Length > _project.StringLengthForStrongMatch;
                        break;
                    }

                } // for

                thisLineIterate++;

            } // while
        }
        
        #endregion
    }
}
