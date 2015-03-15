namespace MarkupDiff
{
    public class FileLink
    {
        public string SourceFile { get; set; }

        // exact link in destination file which was used to establish the link
        public string LinkingTag { get; set; }

        public string DestinationFile { get; set; }
    }
}
