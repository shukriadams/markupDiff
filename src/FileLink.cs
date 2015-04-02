namespace MarkupDiff
{
    /// <summary>
    /// Used to display linked files in project file viewer. Linking is done in real-time each time a project is loaded.
    /// </summary>
    public class FileLink
    {
        /// <summary>
        /// Source file (contains a tag specifying the destination file
        /// </summary>
        public string SourceFile { get; set; }

        /// <summary>
        /// Desination file specified in source file.
        /// </summary>
        public string DestinationFile { get; set; }

        // exact link in source file which was used to establish the link
        public string LinkingTag { get; set; }
    }
}
