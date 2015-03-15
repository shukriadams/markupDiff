using System.Windows.Forms;

namespace MarkupDiff
{
    public class ListViewFast : ListView
    {
        public ListViewFast()
        {
            this.DoubleBuffered = true;
        }

        protected override sealed bool DoubleBuffered
        {
            get { return base.DoubleBuffered; }
            set { base.DoubleBuffered = value; }
        }
    }
}
