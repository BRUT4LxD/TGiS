using Graph;

var graphManager = new GraphManager();

await graphManager.ProcessImages();

graphManager.PrintRelationCountMatrix(5);
