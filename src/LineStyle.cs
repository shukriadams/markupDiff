using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MarkupDiff
{
    /// <summary>
    /// Stores rendering information for a given line style in file renderer. Line style is used by Rich Text Box.
    /// </summary>
    public class LineStyle
    {
        #region PROPERTIES

        public Color ForeColor { get; set; }

        public Color BackColor { get; set; }
        
        public bool IsBold { get; set; }

        public LineStyleNames Name { get; set; }

        #endregion

        #region STATIC

        private static IEnumerable<LineStyle> _styles;

        /// <summary>
        /// Defines all styles for file viewer. 
        /// todo : move these to user-editable style sheet
        /// </summary>
        /// <returns></returns>
        private static  IEnumerable<LineStyle> LoadStyles() 
        {
            List<LineStyle> styles = new List<LineStyle>();

            styles.Add(new LineStyle { Name = LineStyleNames.LineNumber, BackColor = Color.Black, ForeColor = Color.White, IsBold = true });
            styles.Add(new LineStyle { Name = LineStyleNames.Match, BackColor = Color.LightGreen, ForeColor = Color.Green, IsBold = true });
            styles.Add(new LineStyle { Name = LineStyleNames.Mismatch, BackColor = Color.Red, ForeColor = Color.Black, IsBold = true });
            styles.Add(new LineStyle { Name = LineStyleNames.Whitespace, BackColor = Color.Gray, ForeColor = Color.DarkGray, IsBold = false });
            styles.Add(new LineStyle { Name = LineStyleNames.Ignore, BackColor = Color.DarkGreen, ForeColor = Color.LightGreen, IsBold = false });
            styles.Add(new LineStyle { Name = LineStyleNames.Ignore, BackColor = Color.DarkGreen, ForeColor = Color.LightGreen, IsBold = false });
            return styles;
        }

        /// <summary>
        /// Gets a style item 
        /// </summary>
        /// <param name="style"></param>
        /// <returns></returns>
        public static LineStyle Get(LineStyleNames style)
        {
            if (_styles == null)
                _styles = LoadStyles();

            return _styles.FirstOrDefault(r => r.Name == style);
        }

        #endregion

    }
}
