internal interface ISimpleMetric : IMetric
{
    double MeanRatio { get; set; }
    int NoSamples { get; set; }
    double StandardDeviation { get; set; }
}