namespace MarkupDiff
{
    /// <summary>
    /// Comparison results.
    /// </summary>
    public enum LineComparisonTypes
    {
        Match = 0,
        NoMatch = 1,
        Ignore = 2,
        PartialMatch = 3,
        Whitespace = 4,
    }
}
