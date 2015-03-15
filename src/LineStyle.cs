using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MarkupDiff
{
    public class LineStyle
    {
        public Color? ForeColor { get; set; }

        public Color? BackColor { get; set; }
        
        public bool IsBold { get; set; }

        public string Name { get; set; } 

        private static  IEnumerable<LineStyle> LoadStyles() 
        {
            List<LineStyle> styles = new List<LineStyle>();
            // todo : load from style sheet
            styles.Add(new LineStyle { Name = "line-number", BackColor = Color.Black, ForeColor = Color.White, IsBold = true });
            styles.Add(new LineStyle { Name = "code-match", BackColor = Color.LightGreen, ForeColor = Color.Green, IsBold = true });
            styles.Add(new LineStyle { Name = "code-nomatch", BackColor = Color.Red, ForeColor = Color.Black, IsBold = true });
            styles.Add(new LineStyle { Name = "padding", BackColor = Color.Gray, ForeColor = Color.DarkGray, IsBold = false });
            styles.Add(new LineStyle { Name = "comment", BackColor = Color.DarkGreen, ForeColor = Color.LightGreen, IsBold = false });
            styles.Add(new LineStyle { Name = "code", BackColor = Color.DarkGreen, ForeColor = Color.LightGreen, IsBold = false });
            return styles;
        }

        private static IEnumerable<LineStyle> _styles;

        public static LineStyle Get(string name)
        {
            return Styles.FirstOrDefault(r => r.Name == name);
        }

        public static IEnumerable<LineStyle> Styles 
        {
            get 
            {
                if (_styles == null) {
                    _styles = LoadStyles();
                }
                return _styles;
            }
        }
    }
}
