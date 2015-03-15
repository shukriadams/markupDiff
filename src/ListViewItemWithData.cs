using System.Windows.Forms;

namespace MarkupDiff
{
    public class ListViewItemWithData : ListViewItem
    {
        public FileLink FileLink { get; set; }

        public object Data { get; set; }

        public ListViewItemWithData() {

        }

        public ListViewItemWithData(string[] args) :base(args)
        {

        }

    }
}
