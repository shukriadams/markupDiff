using System.Collections.Generic;

namespace MarkupDiff
{
    /// <summary>
    /// Stores results of a file comparison
    /// </summary>
    public class FileComparison
    {
        #region PROPERTIES

        /// <summary>
        /// Source file content, split into lines.
        /// </summary>
        public IList<Line> SourceFile { get; set; }

        /// <summary>
        /// Destination file content, split into lines
        /// </summary>
        public IList<Line> DestinationFile { get; set; }

        #endregion

        #region CTORS

        public FileComparison(IList<Line> sourceFile, IList<Line> destinationFile) 
        {
            this.SourceFile = sourceFile;
            this.DestinationFile = destinationFile;
        }

        #endregion
    }
}
