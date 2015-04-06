using System;
using System.Collections.Generic;
using System.Xml;

namespace MarkupDiff
{
    /// <summary>
    /// Stores information about a project. Project data is stored in an xml file which is user-editable.
    /// A project is built around source files and destination files, how they are connected, and will
    /// eventually also include historical / change data.
    /// </summary>
    public class Project
    {
        #region PROPERTIES

        /// <summary>
        /// 
        /// </summary>
        public string MatchTagStart { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string MatchTagEnd { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string LinkedTagTerminate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> TargetFilesToSearch { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> SourceFilesToSearch { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SourceRootFolder { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DestinationRootFolder { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int StringLengthForStrongMatch { get; private set; }

        public IEnumerable<string> IgnoreFlags { get; private set; }

        #endregion

        #region CTORS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public Project(string path) 
        {
            this.StringLengthForStrongMatch = 5; // todo : move this value to project file
            this.IgnoreFlags = new[] { "@", "{{", "<!--", "{{!" }; // todo : move this value to project file

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
                LinkedTagTerminate = doc.DocumentElement.Attributes["matchTagTerminate"].Value;
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

            this.MatchTagStart = StringHelper.ReturnUpto(matchTag, "{?}");
            this.MatchTagEnd = StringHelper.ReturnAfter(matchTag, "{?}");
        }

        #endregion
    }
}
