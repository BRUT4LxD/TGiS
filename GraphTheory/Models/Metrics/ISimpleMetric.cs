internal interface ISimpleMetric<T> : IMetric<T>
{
    double Mean { get; set; }
    int NoSamples { get; set; }
    double StandardDeviation { get; set; }
}