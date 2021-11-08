internal class Area : ISimpleMetric
{
    public double MeanRatio { get; set; }

    public double StandardDeviation { get; set; }

    public int NoSamples { get; set; }

    public double LastAreaValue { get; set; }

    public void AddSample(double value)
    {
        StandardDeviation = Calculator.AddToStandardDeviation(StandardDeviation, MeanRatio, NoSamples, value);
        MeanRatio = Calculator.AddToMean(MeanRatio, NoSamples, value);
        NoSamples++;
        LastAreaValue = value;
    }

}