internal class RelationCounts
{
    public string ClassName { get; }

    public IDictionary<string, int> RelationCountsGraph { get; set; } = new Dictionary<string, int>();

    public void AddRelation(string name)
    {
        if (!RelationCountsGraph.ContainsKey(name))
        {
            RelationCountsGraph.Add(name, 0);
        }
        RelationCountsGraph[name]++;
    }
}
