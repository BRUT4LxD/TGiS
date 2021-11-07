internal class NumberComparer : IComparer<int>
{
    private readonly bool _ascending = true;


    public NumberComparer(bool ascending)
    {
        _ascending = ascending;
    }

    public int Compare(int x, int y)
    {
        return _ascending ? x - y : y - x;
    }
}
