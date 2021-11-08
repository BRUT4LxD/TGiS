internal static class Calculator
{
    internal static double CalculateDistance(BoundingBox boundingBox)
    {
        var x2 = Math.Pow(boundingBox.XMax - boundingBox.XMin, 2);
        var y2 = Math.Pow(boundingBox.YMax - boundingBox.YMin, 2);
        return Math.Sqrt(x2 + y2);
    }

    internal static double CalculateArea(BoundingBox boundingBox)
    {
        return Math.Abs(boundingBox.XMax - boundingBox.XMin) * Math.Abs(boundingBox.YMax - boundingBox.YMin);
    }

    internal static double CalculateSizeRatio(BoundingBox boundingBox)
    {
        var y = Math.Abs(boundingBox.YMax - boundingBox.YMin);
        return Math.Abs(boundingBox.XMax - boundingBox.XMin) / (y == 0 ? 1 : y);
    }

    internal static double AddToMean(double meanX, int noSamples, double value)
    {
        return (meanX * noSamples + value) / (noSamples + 1);
    }

    internal static double AddToMeanQuadraticSquare(double mqs, double mean, int noSamples, double value)
    {
        var newMean = AddToMean(mean, noSamples, value);

        return noSamples * Math.Pow(newMean - mean, 2) + Math.Pow(newMean - value, 2);
    }

    internal static double AddToStandardDeviation(double std, double mean, int noSamples, double value)
    {
        double mqs = Math.Pow(std, 2) * ((noSamples - 1) == 0 ? 1 : (noSamples - 1));
        mqs = AddToMeanQuadraticSquare(mqs, mean, noSamples, value);
        return Math.Sqrt(mqs / noSamples == 0 ? 1 : noSamples);
    }
}