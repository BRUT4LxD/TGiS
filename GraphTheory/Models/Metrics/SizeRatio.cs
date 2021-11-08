internal class SizeRatio : ISimpleMetric<double>
{
    public SizeRatio(double value)
    {
        Mean = value;
        StandardDeviation = 0;
        NoSamples = 1;
    }

    public double Mean { get; set; }

    public double StandardDeviation { get; set; }

    public int NoSamples { get; set; }

    public void AddSample(double value)
    {
        StandardDeviation = Calculator.AddToStandardDeviation(StandardDeviation, Mean, NoSamples, value);
        Mean = Calculator.AddToMean(Mean, NoSamples, value);
        NoSamples++;
    }
}
