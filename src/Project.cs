using System;
using System.Collections.Generic;
using System.Xml;

namespace MarkupDiff
{
    public class Project
    {
        public string MatchTagStart { get; set; }
        public string MatchTagEnd { get; set; }
        public IEnumerable<string> TargetFilesToSearch { get; set; }
        public IEnumerable<string> SourceFilesToSearch { get; set; }
        public string SourceRootFolder { get; set; }
        public string DestinationRootFolder { get; set; }

        public Project(string path) 
        {
            XmlDocument doc = new XmlDocument();
            
            try 
            {
                doc.Load(path);
            }
            catch(Exception ex)
            {
                throw new Exception("Unbale to load project file Invalid xml : ", ex);
            }

            if (doc.DocumentElement == null || doc.DocumentElement.Attributes == null)
                throw new Exception("Project file is badly formed.");

            if (doc.DocumentElement.Attributes["sourceFolder"] == null)
                throw new Exception("Project file does not have the expected attribute 'sourceFolder' in root element ");

            if (doc.DocumentElement.Attributes["destinationFolder"] == null)
                throw new Exception("Project file does not have the expected attribute 'destinationFolder' in root element ");

            string matchTag;
            try
            {
                SourceRootFolder = doc.DocumentElement.Attributes["sourceFolder"].Value;
                DestinationRootFolder = doc.DocumentElement.Attributes["destinationFolder"].Value;
                matchTag = doc.DocumentElement.Attributes["matchTag"].Value;

                TargetFilesToSearch = doc.DocumentElement.Attributes["destinationFileTypes"].Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                SourceFilesToSearch = doc.DocumentElement.Attributes["sourceFileTypes"].Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
            catch (Exception ex)
            {
                throw new Exception("Error reading project file contents, xml structure likely invalid", ex);
            }

            if (matchTag.IndexOf("{?}") == -1)
            {
                throw new Exception("match tag in app settings must contain '{?}'");
            }

            this.MatchTagStart = ParserLib.ReturnUpto(matchTag, "{?}");
            this.MatchTagEnd = ParserLib.ReturnAfter(matchTag, "{?}");
        }
    }
}
