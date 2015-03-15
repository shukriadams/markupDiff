using System.Collections.Generic;

namespace MarkupDiff
{
    public class FileComparison
    {
        public IList<Line> SourceFile { get; set; }
        public IList<Line> DestinationFile { get; set; }

        public FileComparison(IList<Line> sourceFile, IList<Line> destinationFile) 
        {
            this.SourceFile = sourceFile;
            this.DestinationFile = destinationFile;
        }
    }
}
