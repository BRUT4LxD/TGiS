internal interface IMetric<T>
{
    void AddSample(T value);
}
