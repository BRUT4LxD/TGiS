using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using Graph;
using GraphTheory;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Microsoft.VisualBasic;

// var graphManager = new GraphManager();

//await graphManager.CalculateClassDescriptions();

////graphManager.PrintRelationCountMatrix(5);

//await graphManager.SaveResults("classDescriptions.json");

//var df = new DatasetFactor();

//Console.WriteLine(await df.CheckSamples());

var df = new DatasetFactor();

var top = await df.LoadTop20ClassesCounts(fromCoco: true);



//foreach (var item in top)
//{
//    Console.WriteLine(names[item.Item1] + " : " + item.Item2);
//}

await df.GetImagesClassesIds(top.Select(e => e.Item1).ToList());

// string yoloDir = @"C:\Users\BRUT4LxD\OneDrive - Wojskowa Akademia Techniczna\Pulpit\Moje\Uczelnia\Studia doktoranckie\SEM III\TGIS\Projekt\TGiS\GraphTheory\Datasets\TrainYOLO";

// await df.ConvertYOLOToVOC(yoloDir);

//const string path = @"C:\Users\BRUT4LxD\OneDrive - Wojskowa Akademia Techniczna\Pulpit\Moje\Projekty\keras-yolo3\data\train_image_folder\0000a1b2fba255e9.jpg";
//const string folderPath = @"C:\Users\BRUT4LxD\OneDrive - Wojskowa Akademia Techniczna\Pulpit\Moje\Projekty\keras-yolo3\data\train_image_folder";

//var d = await ImageUtils.GetImagesSizes(folderPath, true);




