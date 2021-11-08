internal class AreaRatios
{
    public IDictionary<string, Area> AreaRatiosGraph { get; set; } = new Dictionary<string, Area>();

    public void AddSample(Area currentArea, string name, double value)
    {
        if (!AreaRatiosGraph.ContainsKey(name))
        {
            AreaRatiosGraph.Add(name, new Area());
        }

        AreaRatiosGraph[name].AddSample((value == 0 ? 1 : value) / currentArea.LastAreaValue);
    }
}
