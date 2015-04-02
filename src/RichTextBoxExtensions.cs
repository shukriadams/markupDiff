using System.Drawing;
using System.Windows.Forms;

namespace MarkupDiff
{
    /// <summary>
    /// Adds conveniant methods to standard RTB, for rendering styles.
    /// </summary>
    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, LineStyle lineStyle)
        {
            AppendText(box, text, lineStyle.ForeColor, lineStyle.BackColor, lineStyle.IsBold);

        }

        public static void AppendText(this RichTextBox box, string text, Color textColor, Color backgroundColor, bool bold)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            Color originalBackcolor = box.SelectionBackColor;

            Font origin = box.Font;
            if (bold)
            {
               
                Font f = new Font(box.Font.Name, box.Font.Size, FontStyle.Bold);
                box.SelectionFont = f;
            }

            box.SelectionColor = textColor;
            box.SelectionBackColor = backgroundColor;
            
            
            box.AppendText(text);

            // reset
            box.SelectionColor = box.ForeColor;
            box.SelectionBackColor = originalBackcolor;
            if (bold)
            {
                box.SelectionFont = origin;
            }
            
        }
    }
}
