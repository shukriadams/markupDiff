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
        private Project _currentProject;
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

        private void SetStyle(IEnumerable<Line> lines)
        {
            foreach (Line line in lines) 
            {
                foreach (LineSection section in line.Sections) 
                {
                    // if section has no style, get style from line
                    if (section.Style != null) 
                        continue;

                    // ignore overrides everything
                    if (line.IgnoreFromProcess) {
                        section.Style = LineStyle.Get(LineStyleNames.Ignore);
                    }
                    else if (line.MatchType.HasValue)
                    {
                        if (line.MatchType == MatchTypes.Match)
                            section.Style = LineStyle.Get(LineStyleNames.Match);
                        else
                            section.Style = LineStyle.Get(LineStyleNames.NoMatch);
                    }
                    else
                    {
                        if (line.LineType == LineTypes.Whitespace)
                            section.Style = LineStyle.Get(LineStyleNames.Whitespace);
                        else
                            section.Style = LineStyle.Get(LineStyleNames.Ignore);
                    }

                }
 
            }
        }

        /// <summary>
        /// Show differences between a source and target file.
        /// </summary>
        private void LoadDifferences()
        {
            if (_currentFileComparison == null)
                return;

            // files to watch may be locked by other process, wait for them to be free
            WaitForFileFree(_currentFileComparison.SourceFile);
            WaitForFileFree(_currentFileComparison.DestinationFile);

            // get file comparison, this is where the actual "diff" analysis is done
            FileComparer fileComparer = new FileComparer(_currentFileComparison.LinkingTag, _currentProject.MatchTagStart, _currentProject.LinkedTagTerminate);
            FileComparison result = fileComparer.Process(_currentFileComparison.SourceFile, _currentFileComparison.DestinationFile);

            // set style classes for lines and line sections
            SetStyle(result.DestinationFile);
            SetStyle(result.SourceFile);

            // write analysis to rich text boxes
            rtbSource.Text = string.Empty;
            rtbDestination.Text = string.Empty;
             
            // todo : see later for flicker reduction :http://www.c-sharpcorner.com/UploadFile/mgold/ColorSyntaxEditor12012005235814PM/ColorSyntaxEditor.aspx
            for (var i = 0; i < result.SourceFile.Count(); i++)
            {
                Line line = result.SourceFile.ElementAt(i);
                if (!cbShowCode.Checked && line.IgnoreFromProcess)
                    continue;

                RenderLine(rtbSource, line, i, result.SourceFile.Count());
            }

            for (var i = 0; i < result.DestinationFile.Count(); i++)
            {
                Line line = result.DestinationFile.ElementAt(i);
                if (!cbShowCode.Checked && line.IgnoreFromProcess)
                    continue;

                RenderLine(rtbDestination, line, i, result.DestinationFile.Count());
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
        /// <param name="rawLineNumber"></param>
        /// <param name="totalLineNumbers"></param>
        private void RenderLine(RichTextBox richTextBox, Line line, int rawLineNumber, int totalLineNumbers)
        {
            //raw line number
            string lineNumberOut = (rawLineNumber + 1).ToString();
            lineNumberOut = PadUntilLength(lineNumberOut, totalLineNumbers.ToString().Length);
            richTextBox.AppendText(lineNumberOut);

            // file line number
            string originalLineNumber = line.LineType == LineTypes.Padding ? " " : (line.OriginalLineNumber + 1).ToString();
            originalLineNumber = PadUntilLength(originalLineNumber, totalLineNumbers.ToString().Length);
            richTextBox.AppendText(originalLineNumber, LineStyle.Get(LineStyleNames.LineNumber));

            // linked line number
            string linkedLineNumber = line.MatchType.HasValue ? (line.MatchedWithLineNumber + 1).ToString() : " ";
            linkedLineNumber = PadUntilLength(linkedLineNumber, totalLineNumbers.ToString().Length);
            richTextBox.AppendText(linkedLineNumber, LineStyle.Get(LineStyleNames.NoMatch));

            foreach (var section in line.Sections)
            {
                LineStyle sectionStyle = section.Style;
                // todo : reorganize style fetching, it's spread everywhere
                if (sectionStyle == null)
                    sectionStyle = LineStyle.Get(LineStyleNames.Ignore);

                richTextBox.AppendText(section.Text, sectionStyle);
            }
            richTextBox.AppendText(Environment.NewLine);
        }

        /// <summary>
        /// Loads source and desination files in a project, tries to find links between them, and 
        /// renders them in file list.
        /// </summary>
        private void LoadProject()
        {

            if (_currentProjectFile == null)
                return;

            if (!File.Exists(_currentProjectFile))
                throw new Exception(string.Format("Expected file '{0}' was not found.", _currentProjectFile));

            
            try
            {
                _currentProject = new Project(_currentProjectFile);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }

            if (!Directory.Exists(_currentProject.SourceRootFolder))
            {
                MessageBox.Show(string.Format("The project source folder '{0}' does not exist.", _currentProject.SourceRootFolder));
                return;
            }

            if (!Directory.Exists(_currentProject.DestinationRootFolder))
            {
                MessageBox.Show(string.Format("The project destination folder '{0}' does not exist.", _currentProject.DestinationRootFolder));
                return;
            }

            projectContainer.Visible = true;
            pnlProjects.Visible = false;

            // fetch all files in source and destination folders
            IEnumerable<string> sourceFiles = FileSystemHelper.GetFilesUnder(_currentProject.SourceRootFolder, _currentProject.SourceFilesToSearch);
            IEnumerable<string> destinationFiles = FileSystemHelper.GetFilesUnder(_currentProject.DestinationRootFolder, _currentProject.TargetFilesToSearch);


            lvFiles.Items.Clear();

            // for each destination file, find any file link tags in its content, extract file name, then try to find a matching source
            // file with that name or path fragment.
            foreach (string destinationFile in destinationFiles)
            {
                string fileContent = File.ReadAllText(destinationFile);
                if (!fileContent.Contains(_currentProject.MatchTagStart))
                    continue;

                // more than one source file can be specified in source file
                IEnumerable<string> linkedSourceFiles = StringHelper.ReturnBetweenAll(fileContent, _currentProject.MatchTagStart, _currentProject.MatchTagEnd);
                if (!linkedSourceFiles.Any())
                    continue;

                foreach (string linkedSourceFile in linkedSourceFiles) {
                    string linkingTag = _currentProject.MatchTagStart + linkedSourceFile + _currentProject.MatchTagEnd;
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

            // autofocus first item in list
            if (lvFiles.Items.Count > 0) {
                lvFiles.Items[0].Selected = true;
                lvFiles.Select();
            }
        }

        /// <summary>
        /// Loads all project files into project list view.
        /// </summary>
        private void LoadProjects() 
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
            WinFormActionDelegate dlgyConsoleUpdate = LoadDifferences;
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

            LoadProjects();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReload_Click(object sender, EventArgs e)
        {
            LoadProject();
        }

        private void cbShowCode_CheckedChanged(object sender, EventArgs e)
        {
            this.LoadDifferences();
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
            this.LoadDifferences();
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
            LoadProject();
        }

        private void btnViewProjects_Click(object sender, EventArgs e)
        {
            LoadProjects();
        }

        #endregion

    }
}
