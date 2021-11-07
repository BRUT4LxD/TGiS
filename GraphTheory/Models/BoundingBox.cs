internal class BoundingBox
{
    public string ImageId { get; set; }

    public string Source { get; set; }

    public string LabelName { get; set; }

    public bool Confidence { get; set; }

    public double XMin { get; set; }

    public double XMax { get; set; }

    public double YMin { get; set; }

    public double YMax { get; set; }

    public bool IsOccluded { get; set; }

    public bool IsTruncated { get; set; }

    public bool IsGroupOf { get; set; }

    public bool IsDepiction { get; set; }

    public bool IsInside { get; set; }

    public static BoundingBox Create(string[] line)
    {
        return new BoundingBox
        {
            ImageId = line[0],
            Source = line[1],
            LabelName = line[2],
            Confidence = line[3] == "1",
            XMax = double.Parse(line[4]),
            XMin = double.Parse(line[5]),
            YMin = double.Parse(line[6]),
            YMax = double.Parse(line[7]),
            IsOccluded = line[8] == "1",
            IsTruncated = line[9] == "1",
            IsGroupOf = line[10] == "1",
            IsDepiction = line[11] == "1",
            IsInside = line[12] == "1"
        };
    }

    public double CalculateArea => (XMax - XMin) * (YMax - YMin);
}