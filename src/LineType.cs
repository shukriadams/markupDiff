namespace MarkupDiff
{
    public enum LineType
    {
        Match = 0,
        Code = 1,
        DestinationCode = 2,
        Comment = 3,
        PartialMatch = 4,
        NoMatch = 5,
        Padding = 6,
    }
}
