using System.Drawing;
using System.Windows.Forms;

namespace MarkupDiff
{
    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, LineStyle lineStyle)
        {
            AppendText(box, text, lineStyle.ForeColor, lineStyle.BackColor, lineStyle.IsBold);

        }

        public static void AppendText(this RichTextBox box, string text, Color? textColor, Color? backgroundColor, bool bold)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            Color originalBackcolor = box.SelectionBackColor;

            if (backgroundColor == null)
                backgroundColor = box.ForeColor;

            if (textColor == null)
                textColor = box.SelectionBackColor;

            Font origin = box.Font;
            if (bold)
            {
               
                Font f = new Font(box.Font.Name, box.Font.Size, FontStyle.Bold);
                box.SelectionFont = f;
            }

            box.SelectionColor = textColor.Value;
            box.SelectionBackColor = backgroundColor.Value;
            
            
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
