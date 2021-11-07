internal class ClassDescription
{
    public string Name { get; set; }

    public SizeRatio SizeRatio { get; set; }

    public AreaRatio AreaRatio { get; set; }

    public IReadOnlyDictionary<string, int> RelationCounts { get; set; }

    public IReadOnlyDictionary<string, AreaRatio> AreaRatios { get; set; }

}
