internal class Area : ISimpleMetric<double>
{
    public Area(double value)
    {
        Mean = value;
        StandardDeviation = 0;
        NoSamples = 1;
        LastAreaValue = value;
    }

    public double Mean { get; set; }

    public double StandardDeviation { get; set; }

    public int NoSamples { get; set; }

    public double LastAreaValue { get; set; }

    public void AddSample(double value)
    {
        StandardDeviation = Calculator.AddToStandardDeviation(StandardDeviation, Mean, NoSamples, value);
        Mean = Calculator.AddToMean(Mean, NoSamples, value);
        NoSamples++;
        LastAreaValue = value;
    }

}