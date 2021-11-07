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
        return (boundingBox.XMax - boundingBox.XMin) * (boundingBox.YMax - boundingBox.YMin);
    }

    internal static double CalculateSizeRatio(BoundingBox boundingBox)
    {
        return (boundingBox.XMax - boundingBox.XMin) / (boundingBox.YMax - boundingBox.YMin);
    }

    internal static double AddToMean(double meanX, int noSamples, double value)
    {
        return (meanX * noSamples + value) / (noSamples + 1);
    }

    internal static double AddToMeanQuadraticSquare(double mqs, double mean, int noSamples, double value)
    {
        var newMean = AddToMean(mean, noSamples, value);

        return noSamples * (Math.Pow(newMean - mean, 2)) + (newMean - value);
    }
}