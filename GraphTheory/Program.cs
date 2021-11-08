using Graph;

var graphManager = new GraphManager();

await graphManager.CalculateClassDescriptions();

//graphManager.PrintRelationCountMatrix(5);

await graphManager.SaveResults("classDescriptions.json");

