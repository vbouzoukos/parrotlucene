namespace ParrotLucene.Search
{
    public enum SearchFieldOption
    {
        TERM,
        LIKE,
        INTRANGE,
        DOUBLERANGE,
        FUZZY,
        EXACT,
        DECIMALRANGE
    }

    public enum Occurance
    {
        MUST = 0,
        SHOULD = 1,
        NOT = 2
    }
}