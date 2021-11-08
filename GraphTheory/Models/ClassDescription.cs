using Graph;

internal class ClassDescription
{
    public string Name { get; }

    public ClassDescription(string name)
    {
        Name = name;
        SizeRatio = new SizeRatio();
        Area = new Area();
        RelationCounts = new RelationCounts();
        AreaRatios = new AreaRatios();
    }

    public SizeRatio SizeRatio { get; set; }

    public Area Area { get; set; }

    public RelationCounts RelationCounts { get; set; }

    public AreaRatios AreaRatios { get; set; }

    public void AddSample(SimpleClassDescription simpleClassDescription)
    {
        Area.AddSample(simpleClassDescription.Area);
        SizeRatio.AddSample(simpleClassDescription.SizeRatio);
        RelationCounts.AddRelation(simpleClassDescription.Name);
        AreaRatios.AddSample(Area, simpleClassDescription.Name, simpleClassDescription.Area);
    }
}
