namespace MarkupDiff
{
    /// <summary>
    /// Types of lines
    /// </summary>
    public enum LineTypes
    {
        Other = 0,
        Whitespace = 1,
        LinkingTag  = 2,
        Markup = 3,

        // padding is not whitespace ; it is inserted so matching lines can appear next to eachother
        Padding = 4 
    }
}
