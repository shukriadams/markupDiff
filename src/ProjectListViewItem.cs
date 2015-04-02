using System.Windows.Forms;

namespace MarkupDiff
{
    /// <summary>
    /// Extends a standard .net ListViewItem, adding extra data to it.
    /// </summary>
    public class ProjectListViewItem : ListViewItem
    {
        public string ProjectFile { get; set; }

        public ProjectListViewItem() {

        }

        public ProjectListViewItem(string[] args)
            : base(args)
        {

        }
    }
}
