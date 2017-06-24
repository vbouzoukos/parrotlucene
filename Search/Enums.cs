public enum SearchFieldOption
{
    TERM,
    LIKE,
    INTRANGE,
    DOUBLERANGE,
    FUZZY
}

public enum Occurance
{
    MUST = 0,
    SHOULD = 1,
    NOT = 2
}
