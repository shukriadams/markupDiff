using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace MarkupDiff
{
    public partial class MainForm : Form
    {
        #region FIELDS

        private FileLink _currentFileComparison ;
        private string _currentProjectFile;
        private FileSystemWatcher _sourceWatcher;
        private FileSystemWatcher _destinationWatcher;

        /// <summary> 
        /// Delegate used to invoke void methods with no arugments. this is mainly used for manipulating
        /// Windform Control objects that do not behave will when called from foreign threads 
        /// </summary>
        public delegate void WinFormActionDelegate();

        #endregion

        #region METHODS

        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  quick and dirty way to ensure files arent locked
        /// </summary>
        /// <param name="filename"></param>
        private void WaitForFileFree(string filename) 
        {
            FileStream stream = null;
            FileInfo file;
            int maxtries = 10;
            int wait = 500;
            int tries = 0;

            while (tries < maxtries)
            try
            {
                file = new FileInfo(filename);
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                return;
            }
            catch (IOException)
            {
                Thread.Sleep(wait);
                tries++;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadFiles()
        {
            if (_currentFileComparison == null)
                return;

            // files to watch may be locked by other process, wait for them to be free
            WaitForFileFree(_currentFileComparison.SourceFile);
            WaitForFileFree(_currentFileComparison.DestinationFile);

            // get file comparison, this is where the actual "diff" analysis is done
            FileAnaylser fileAnaylser = new FileAnaylser { 
                ShowCodeLines = cbShowCode.Checked 
            };
            FileComparison result = fileAnaylser.Process(_currentFileComparison.SourceFile, _currentFileComparison.DestinationFile, _currentFileComparison.LinkingTag);

            // write analysis to rich text boxes
            rtbSource.Text = string.Empty;
            rtbDestination.Text = string.Empty;
             
            // todo : see later for flicker reduction :http://www.c-sharpcorner.com/UploadFile/mgold/ColorSyntaxEditor12012005235814PM/ColorSyntaxEditor.aspx
            for (var i = 0; i < result.SourceFile.Count(); i++)
            {
                RenderLine(rtbSource, result.SourceFile.ElementAt(i), i, result.SourceFile.Count());
            }

            for (var i = 0; i < result.DestinationFile.Count(); i++)
            {
                RenderLine(rtbDestination, result.DestinationFile.ElementAt(i), i, result.SourceFile.Count());
            }

            lblDestinationFilePath.Text = _currentFileComparison.DestinationFile;
            lblSourceFilePath.Text = _currentFileComparison.SourceFile;

            // start watching files for changes
            if (cbLiveUpdateFileContent.Checked)
            {
                _sourceWatcher = new FileSystemWatcher();
                _sourceWatcher.Path = Path.GetDirectoryName(_currentFileComparison.SourceFile);
                // need to watch everything apparently, else changes are not detected 
                _sourceWatcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.Security;
                _sourceWatcher.Filter = Path.GetFileName(_currentFileComparison.SourceFile);
                _sourceWatcher.Changed += OnWatchedFileChanged;
                _sourceWatcher.EnableRaisingEvents = true;


                _destinationWatcher = new FileSystemWatcher();
                _destinationWatcher.Path = Path.GetDirectoryName(_currentFileComparison.DestinationFile);
                _destinationWatcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.Security;
                _destinationWatcher.Filter = Path.GetFileName(_currentFileComparison.DestinationFile);
                _destinationWatcher.Changed += OnWatchedFileChanged;
                _destinationWatcher.EnableRaisingEvents = true;
            }
        }

        private string PadUntilLength(string str, int length) 
        {
            while (str.Length < length)
                str = str + " ";
            return str;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="richTextBox"></param>
        /// <param name="line"></param>
        /// <param name="linenumber"></param>
        private void RenderLine(RichTextBox richTextBox, Line line, int rawLineNumber, int totalLineNumbers)
        {
            string originalLineNumber = line.LineType == LineComparisonTypes.Whitespace ? " " : (line.OriginalLineNumber + 1).ToString();
            originalLineNumber = PadUntilLength(originalLineNumber, totalLineNumbers.ToString().Length);

            //raw line number
            string lineNumberOut = (rawLineNumber + 1).ToString();
            lineNumberOut = PadUntilLength(lineNumberOut, totalLineNumbers.ToString().Length);

            richTextBox.AppendText(lineNumberOut);

            // file line number
            richTextBox.AppendText(originalLineNumber, LineStyle.Get(LineStyleNames.LineNumber));

            foreach (var section in line.Sections)
            {
                richTextBox.AppendText(section.Text, section.LineStyle);
            }
            richTextBox.AppendText(Environment.NewLine);
        }

        /// <summary>
        /// Loads source and desination files in a project, tries to find links between them, and 
        /// renders them in file list.
        /// </summary>
        private void LinkFilesInProject()
        {

            if (_currentProjectFile == null)
                return;

            if (!File.Exists(_currentProjectFile))
                throw new Exception(string.Format("Expected file '{0}' was not found.", _currentProjectFile));

            Project project; 
            try
            {
                project = new Project(_currentProjectFile);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }

            if (!Directory.Exists(project.SourceRootFolder))
            {
                MessageBox.Show(string.Format("The project source folder '{0}' does not exist.", project.SourceRootFolder));
                return;
            }

            if (!Directory.Exists(project.DestinationRootFolder))
            {
                MessageBox.Show(string.Format("The project destination folder '{0}' does not exist.", project.DestinationRootFolder));
                return;
            }

            projectContainer.Visible = true;
            pnlProjects.Visible = false;

            // fetch all files in source and destination folders
            IEnumerable<string> sourceFiles = FileSystemHelper.GetFilesUnder(project.SourceRootFolder, project.SourceFilesToSearch);
            IEnumerable<string> destinationFiles = FileSystemHelper.GetFilesUnder(project.DestinationRootFolder, project.TargetFilesToSearch);


            lvFiles.Items.Clear();

            // for each destination file, find any file link tags in its content, extract file name, then try to find a matching source
            // file with that name or path fragment.
            foreach (string destinationFile in destinationFiles)
            {
                string fileContent = File.ReadAllText(destinationFile);
                if (!fileContent.Contains(project.MatchTagStart))
                    continue;

                // more than one source file can be specified in source file
                IEnumerable<string> linkedSourceFiles = StringHelper.ReturnBetweenAll(fileContent, project.MatchTagStart, project.MatchTagEnd);
                if (!linkedSourceFiles.Any())
                    continue;

                foreach (string linkedSourceFile in linkedSourceFiles) {
                    string linkingTag = project.MatchTagStart + linkedSourceFile + project.MatchTagEnd;
                    string foundSourceFile = sourceFiles.FirstOrDefault(r => r.EndsWith(linkedSourceFile));

                    // todo warn about broken link
                    if (foundSourceFile == null)
                        continue; 

                    FileLinkListViewItem row = new FileLinkListViewItem
                    {
                        Text = Path.GetFileName(destinationFile) + " < " + Path.GetFileName(foundSourceFile),
                        FileLink = new FileLink { DestinationFile = destinationFile, SourceFile = foundSourceFile, LinkingTag = linkingTag }
                    };
                    lvFiles.Items.Insert(0, row);
                }

            }

            lvFiles.Sorting = SortOrder.Ascending;
            lvFiles.Sort(); 
        }

        /// <summary>
        /// 
        /// </summary>
        private void ListProjects() 
        {
            string projectsFolder = Path.Combine(Environment.CurrentDirectory, "projects");
            if (!Directory.Exists(projectsFolder))
                Directory.CreateDirectory(projectsFolder);

            IEnumerable<string> projectFiles = FileSystemHelper.GetFilesUnder(projectsFolder, new []{"xml"});
            

            if (projectFiles.Count() == 0)
            {
                pnlProjects.Visible = false;
                pnlNoProjects.Visible = true;
                return;
            }

            pnlProjects.Visible = true;
            pnlNoProjects.Visible = false;

            lvProjects.Clear();
            foreach(string projectFile in projectFiles){
                ProjectListViewItem row = new ProjectListViewItem
                {
                    Text = Path.GetFileName(projectFile),
                    ProjectFile = projectFile
                };
                lvProjects.Items.Insert(0, row);
            }

        }

        #endregion

        #region EVENTS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnWatchedFileChanged(object sender, FileSystemEventArgs args)
        {
            WinFormActionDelegate dlgyConsoleUpdate = LoadFiles;
            this.Invoke(dlgyConsoleUpdate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            projectContainer.Visible = false;
            pnlNoProjects.Visible = false;
            pnlProjects.Dock = DockStyle.Fill;
            pnlNoProjects.Dock = DockStyle.Fill;
            projectContainer.Dock = DockStyle.Fill;
            // set windows things up
            

            lvFiles.SelectedIndexChanged += OnListViewClicked;
            lvProjects.SelectedIndexChanged += OnProjectClicked;
            rtbSource.Font = new Font("COURIER New", 10);
            rtbDestination.Font = new Font("COURIER New", 10);
            rtbSource.WordWrap = false;
            rtbDestination.WordWrap = false;

            ListProjects();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReload_Click(object sender, EventArgs e)
        {
            LinkFilesInProject();
        }

        private void cbShowCode_CheckedChanged(object sender, EventArgs e)
        {
            this.LoadFiles();
        }

        void SourceCopyAction(object sender, EventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, rtbSource.SelectedText);
        }

        void DestinationCopyAction(object sender, EventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, rtbDestination.SelectedText);
        }

        private void rtbSource_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = new MenuItem("Copy");
            menuItem.Click += SourceCopyAction;
            contextMenu.MenuItems.Add(menuItem);
            rtbSource.ContextMenu = contextMenu;
        }

        private void rtbDestination_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = new MenuItem("Copy");
            menuItem.Click += DestinationCopyAction;
            contextMenu.MenuItems.Add(menuItem);
            rtbDestination.ContextMenu = contextMenu;
        }

        /// <summary>
        /// Invoked when a file is selected 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnListViewClicked(object sender, EventArgs e)
        {
            if (lvFiles.SelectedItems.Count == 0)
                return;

            FileLinkListViewItem item = lvFiles.SelectedItems[0] as FileLinkListViewItem;
            _currentFileComparison = item.FileLink;
            this.LoadFiles();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProjectClicked(object sender, EventArgs e)
        {
            if (lvProjects.SelectedItems.Count == 0)
                return;

            ProjectListViewItem item = lvProjects.SelectedItems[0] as ProjectListViewItem;
            _currentProjectFile = item.ProjectFile;
            LinkFilesInProject();
        }

        private void btnViewProjects_Click(object sender, EventArgs e)
        {
            ListProjects();
        }

        #endregion

    }
}
