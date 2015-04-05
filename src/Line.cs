using System.Collections.Generic;

namespace MarkupDiff
{
    /// <summary>
    /// Line in a file being compared. Files are broken up into lines. A line can be broken up into LineSections - there's always at least one, 
    /// but a line can be broken up into several line sections if we try to do a partial match on it.
    /// </summary>
    public class Line
    {
        #region PROPERTIES

        /// <summary>
        /// Text to be rendered. One chunk of text, unless partial matching within line was done, in which case 2-3 chunks
        /// </summary>
        public IEnumerable<LineSection> Sections { get; set; }

        /// <summary>
        /// Match analysis of this line, against opposite file.
        /// </summary>
        public LineTypes LineType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public MatchTypes? MatchType { get; set; }

        /// <summary>
        /// If lines matched are complex enough, this can be set to true. A strong link is cosmetic only, used in line alignment
        /// when rendering results.
        /// </summary>
        public bool IsMatchQualityStrong { get; set; }

        /// <summary>
        /// Original text line number from file.
        /// </summary>
        public int OriginalLineNumber { get; set; }

        /// <summary>
        /// If line is used to push a text line down (pad), this is the line number of that text line -1. Used for a quick
        /// and dirty way sort lines so padding always come before the lines they pad.
        /// </summary>
        public int PadsOriginalLineNumber { get; set; }

        /// <summary>
        /// Line number in opposite file thisline has a full or partial match with. Only relevent
        /// if linetype is match or partial match.
        /// </summary>
        public int? MatchedWithLineNumber { get; set; }

        /// <summary>
        /// Original, unprocessed text line, from text file.
        /// </summary>
        public string RawText { get; set; }

        /// <summary>
        /// If true, line will not be compared. This holds for lines that are code, comments, or which have been demarkated as 
        /// coming from a source other than the file being compared.
        /// </summary>
        public bool IgnoreFromProcess { get; set; }

        #endregion

        #region CTORS

        public Line() 
        {
            this.Sections = new List<LineSection>();
        }

        #endregion
    }
}
