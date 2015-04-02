using System.Windows.Forms;

namespace MarkupDiff
{
    /// <summary>
    /// Extends a standard .net ListViewItem, adding extra data to it.
    /// </summary>
    public class FileLinkListViewItem : ListViewItem
    {
        /// <summary>
        /// File linke for the given item (an item in the file listview is a link between a source and destination file.
        /// </summary>
        public FileLink FileLink { get; set; }

        //public object Data { get; set; }

        public FileLinkListViewItem() {

        }

        public FileLinkListViewItem(string[] args)
            : base(args)
        {

        }

    }
}
