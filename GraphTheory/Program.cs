using Graph;

var graphManager = new GraphManager();

await graphManager.CalculateRelationCounts();

graphManager.PrintRelationCountMatrix(5);

await graphManager.SaveResults("relationCounts.json");