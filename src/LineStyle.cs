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

        public Color ForeColor { get; private set; }

        public Color BackColor { get; private set; }

        public bool IsBold { get; private set; }

        public LineStyleNames Name { get; private set; }

        #endregion

        #region CTORS

        public LineStyle(LineStyleNames name, Color backColor, Color foreColor, bool isBold)
        {
            this.Name = name;
            this.ForeColor = foreColor;
            this.BackColor = backColor;
            this.IsBold = isBold;
        }

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

            styles.Add(new LineStyle ( LineStyleNames.LineNumber, Color.Black, Color.White, true ));
            styles.Add(new LineStyle ( LineStyleNames.Match, Color.LightGreen, Color.Green, true ));
            styles.Add(new LineStyle ( LineStyleNames.NoMatch, Color.Red, Color.Black, true ));
            styles.Add(new LineStyle ( LineStyleNames.Whitespace, Color.Gray, Color.DarkGray, false ));
            styles.Add(new LineStyle ( LineStyleNames.Ignore, Color.DarkGreen, Color.LightGreen, false ));
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

            LineStyle item = _styles.FirstOrDefault(r => r.Name == style);
            return item;
        }

        #endregion

    }
}
