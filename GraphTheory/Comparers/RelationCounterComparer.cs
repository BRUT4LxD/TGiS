internal class RelationCounterComparer<T> : IComparer<KeyValuePair<T, int>>
{
    private readonly bool _ascending;

    public RelationCounterComparer(bool ascending)
    {
        _ascending = ascending;
    }

    public int Compare(KeyValuePair<T, int> x, KeyValuePair<T, int> y)
    {
        return _ascending ? x.Value - y.Value : y.Value - x.Value;
    }
}
