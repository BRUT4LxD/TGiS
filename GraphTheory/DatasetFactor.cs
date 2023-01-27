using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Xml.Serialization;
using GraphTheory.Models;
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.Random;

public class DatasetFactor
{
    private readonly Random _r = new();

    private const int NumClasses = 600;
    private const int SampleInputs = 5;
    private const int SampleInputSize = 10;
    private const int TrainSamplesPerPicture = 5;
    private const string BoxesPathEncoded = @"..\..\..\Datasets\train-annotations-bbox_encoded.csv";
    private const string BoxesPath = @"..\..\..\Datasets\train-annotations-bbox.csv";
    private const string NamesAnnotationsPath = @"..\..\..\Datasets\class-descriptions-boxable.csv";
    private const string NamesAnnotationsEncodedPath = @"..\..\..\Datasets\class-descriptions-boxable_encoded.csv";
    private const string TrainPath = @"..\..\..\Datasets\train.csv";
    private const string TestPath = @"..\..\..\Datasets\test.csv";
    private const string Coco2017Labels = @"..\..\..\Datasets\coco_labels.csv";

    public async Task GenerateEncodedBoxes(IReadOnlyDictionary<string, string> names)
    {
        var namesEncoded = names.Keys.Select((e, i) => (e, i)).ToDictionary(e => e.e, e => e.i);

        foreach (var item in namesEncoded)
        {
            Console.WriteLine(item.Key + " : " + item.Value);
        }

        const string boxesPath = @"..\..\..\Datasets\train-annotations-bbox.csv";
        const string boxesPathEncoded = @"..\..\..\Datasets\train-annotations-bbox_encoded.csv";
        var imageHash = new HashSet<string>();
        var imageIdsCounter = 0;
        using StreamReader fileStream = new(boxesPath);
        _ = await fileStream.ReadLineAsync();
        int c = 0;
        var sb = new StringBuilder();
        while (!fileStream.EndOfStream)
        {
            var line = (await fileStream.ReadLineAsync()).Split(",").ToList();
            if (imageHash.Add(line[0]) && imageIdsCounter++ >= 0) { }
            line[0] = imageIdsCounter.ToString();
            line[2] = namesEncoded[line[2]].ToString();

            sb.Append(string.Join(",", line.Where((e, i) => i != 1 && i != 3).Select(e => e)));
            sb.AppendLine();
            if (++c % 100000 == 0) Console.WriteLine("CALCULATING : " + c + "TH ENTRY");
        }

        await File.WriteAllTextAsync(boxesPathEncoded, sb.ToString());
    }

    public async Task GenerateEncodedNames()
    {
        var streamReader = new StreamReader(NamesAnnotationsPath);

        var sb = new StringBuilder();
        int c = 0;
        while (!streamReader.EndOfStream)
        {
            var line = (await streamReader.ReadLineAsync()).Split(",");

            sb.AppendJoin(",", c++, line[1]);
            sb.AppendLine();
        }

        await File.WriteAllTextAsync(NamesAnnotationsEncodedPath, sb.ToString());
    }

    public async Task GenerateLimitedTrainAndTestDatasets(List<int> classIds)
    {
        string trainPath = $@"..\..\..\Datasets\train_top_{classIds.Count}.csv";
        string testPath = $@"..\..\..\Datasets\test_top_{classIds.Count}.csv";
        const string sep = ",";

        using StreamReader fileStream = new(BoxesPathEncoded);
        var list = new List<string[]>();

        var enumerables = Enumerable
            .Range(0, 100)
            .Select(e => (e, Enumerable.Range(0, e + 1)))
            .ToDictionary(e => e.e, e => e.Item2);

        Dictionary<int, int> classToIndexMap = classIds.Select((e, i) => (e, i)).ToDictionary(e => e.e, e => e.i);

        int id = 1;

        var trainSb = new StringBuilder();
        var testSb = new StringBuilder();

        while (!fileStream.EndOfStream)
        {
            var arr = (await fileStream.ReadLineAsync()).Split(",");
            int nextId = int.Parse(arr[0]);
            if (id != nextId)
            {
                if (list.Count != 1)
                {
                    var numOfTrainSamples = Math.Min(list.Count - 1, TrainSamplesPerPicture);
                    var train = new HashSet<int[]>();
                    while (numOfTrainSamples-- > 0)
                    {
                        HashSet<int> localTrain = GenerateRandomTrainClasses(list, SampleInputs, train);
                        double[] trainArr = GenerateTrainSample(list, SampleInputs, SampleInputSize, localTrain);

                        trainSb.AppendJoin(sep, trainArr);
                        trainSb.AppendLine();

                        var testArr = new int[classIds.Count];
                        var iterator = enumerables.ContainsKey(list.Count - 1) ? enumerables[list.Count - 1] : Enumerable.Range(0, list.Count);
                        foreach (var t in iterator.Except(localTrain).ToList())
                        {
                            testArr[classToIndexMap[int.Parse(list[t][0])]] = 1;
                        }

                        testSb.AppendJoin(sep, testArr);
                        testSb.AppendLine();
                    }
                    await SaveTrainingAndTestSamples(trainSb, trainPath, testSb, testPath);

                    trainSb.Clear();
                    testSb.Clear();
                }
                id = nextId;
                list.Clear();
            }
            if (classIds.Any(e => int.Parse(arr[1]) == e))
            {
                list.Add(arr[1..]);
            }
            if (id % 100000 == 0)
            {
                Console.WriteLine($"Processing {id}th image");
            }
        }
    }

    public async Task GenerateLimitedTrainAndTestForSentimentAnalysis(List<int> classIds)
    {
        string trainPath = $@"..\..\..\Datasets\train_sentiment_top_{classIds.Count}.csv";
        string testPath = $@"..\..\..\Datasets\test_sentiment_top_{classIds.Count}.csv";
        const string sep = ",";

        using StreamReader fileStream = new(BoxesPathEncoded);
        var list = new List<string[]>();

        var enumerables = Enumerable
            .Range(0, 100)
            .Select(e => (e, Enumerable.Range(0, e + 1)))
            .ToDictionary(e => e.e, e => e.Item2);

        Dictionary<int, int> classToIndexMap = classIds.Select((e, i) => (e, i)).ToDictionary(e => e.e, e => e.i);

        int id = 1;

        var trainSb = new StringBuilder();
        var testSb = new StringBuilder();

        while (!fileStream.EndOfStream)
        {
            var arr = (await fileStream.ReadLineAsync()).Split(",");
            int nextId = int.Parse(arr[0]);
            if (id != nextId)
            {
                if (list.Count != 1)
                {
                    var missingIdxs = classIds
                        .Except(list.Select(e => e.Select(d => (int)d[0]).First()))
                        .ToList()
                        .Shuffle();

                    var numOfPositiveTrainSamples = Math.Min(list.Count - 1, TrainSamplesPerPicture);
                    var train = new HashSet<int[]>();
                    while (numOfPositiveTrainSamples-- > 0)
                    {
                        HashSet<int> localTrain = GenerateRandomTrainClassesForSentiment(list, SampleInputs, train);
                        double[] trainArr = GenerateTrainSampleForSentiment(list, SampleInputs, SampleInputSize, localTrain, missingIdxs);

                        trainSb.AppendJoin(sep, trainArr);
                        trainSb.AppendLine();

                        testSb.AppendJoin(sep, "1");
                        testSb.AppendLine();
                    }
                    var numOfNegativeTrainSamples = Math.Min(list.Count - 1, TrainSamplesPerPicture);
                    train = new HashSet<int[]>();
                    while (numOfNegativeTrainSamples-- > 0)
                    {
                        HashSet<int> localTrain = GenerateRandomTrainClassesForSentiment(list, SampleInputs, train);
                        double[] trainArr = GenerateTrainSampleForSentiment(list, SampleInputs, SampleInputSize, localTrain, missingIdxs, positiveSample: false);

                        trainSb.AppendJoin(sep, trainArr);
                        trainSb.AppendLine();

                        testSb.AppendJoin(sep, "0");
                        testSb.AppendLine();
                    }
                    await SaveTrainingAndTestSamples(trainSb, trainPath, testSb, testPath);

                    trainSb.Clear();
                    testSb.Clear();
                }
                id = nextId;
                list.Clear();
            }
            if (classIds.Any(e => int.Parse(arr[1]) == e))
            {
                list.Add(arr[1..]);
            }
            if (id % 100000 == 0)
            {
                Console.WriteLine($"Processing {id}th image");
            }
        }
    }

    private int GeneratePositivePredictor(List<string[]> list, HashSet<int> trainIds)
    {
        return Enumerable
            .Range(0, list.Count)
            .Except(trainIds)
            .ToList()
            .Shuffle()
            .First();
    }

    private int GenerateNegativePredictor(List<int> missingIdxs)
    {
        return missingIdxs[_r.Next(missingIdxs.Count)];
    }

    public async Task GenerateTrainAndTestDatasets()
    {
        const string sep = ",";

        using StreamReader fileStream = new(BoxesPathEncoded);
        var list = new List<string[]>();

        var enumerables = Enumerable
            .Range(0, 100)
            .Select(e => (e, Enumerable.Range(0, e + 1)))
            .ToDictionary(e => e.e, e => e.Item2);

        int id = 1;

        var trainSb = new StringBuilder();
        var testSb = new StringBuilder();

        while (!fileStream.EndOfStream)
        {
            var arr = (await fileStream.ReadLineAsync()).Split(",");
            int nextId = int.Parse(arr[0]);
            if (id != nextId)
            {
                if (list.Count != 1)
                {
                    GenerateTrainAndTestSamples(sep, list, enumerables, trainSb, testSb, NumClasses);
                    await SaveTrainingAndTestSamples(trainSb, TrainPath, testSb, TestPath);

                    trainSb.Clear();
                    testSb.Clear();
                }
                id = nextId;
                list.Clear();
            }
            list.Add(arr[1..]);
            if (id % 100000 == 0)
            {
                Console.WriteLine($"Processing {id}th image");
            }
        }
    }

    public async Task<bool> CheckSamples()
    {
        var trainTask = Task.Run(() => CountLines(TrainPath));
        var testTask = Task.Run(() => CountLines(TestPath));


        await Task.WhenAll(trainTask, testTask);


        return testTask.Result == trainTask.Result;

    }

    public async Task<List<(int, int)>> GetTopNClassesCounts(int n, bool save = false, bool fromCoco2017 = false)
    {
        var streamReader = new StreamReader(BoxesPathEncoded);

        var dict = new Dictionary<int, int>();

        while (!streamReader.EndOfStream)
        {
            var line = (await streamReader.ReadLineAsync()).Split(",");
            int id = int.Parse(line[1]);

            if (!dict.ContainsKey(id))
            {
                dict.Add(id, 0);
            }

            dict[id]++;
        }

        var cocoLabels = new List<string>();
        if (fromCoco2017)
        {
            var cocoStreamReader = new StreamReader(Coco2017Labels);

            while (!cocoStreamReader.EndOfStream)
            {
                var className = (await cocoStreamReader.ReadLineAsync()).Split(",")[1];
                cocoLabels.Add(className);
            }

            var encodedNames = await GetEncodedNames();

            dict = dict
                .Where(e => cocoLabels.Any(c => encodedNames[e.Key].Contains(c, StringComparison.InvariantCultureIgnoreCase)))
                .ToDictionary(e => e.Key, e => e.Value);
        }

        var results = dict.Select(e => (e.Key, e.Value)).OrderByDescending(e => e.Value).Take(n).ToList();
        if (save)
        {
            string topPath = $@"..\..\..\Datasets\top_{n}_classes_counts{(fromCoco2017 ? "_from_coco" : "")}.csv";
            var sb = new StringBuilder();
            results.ForEach(e => sb.AppendLine(string.Join(",", e.Key, e.Value)));
            await File.WriteAllTextAsync(topPath, sb.ToString());
        }
        return results;
    }

    public async Task<List<(int, int)>> LoadTop20ClassesCounts(bool fromCoco = false)
    {
        string topPath = $@"..\..\..\Datasets\top_20_classes_counts{(fromCoco ? "_from_coco" : "")}.csv";
        var streamReader = new StreamReader(topPath);

        var results = new List<(int, int)>();


        while (!streamReader.EndOfStream)
        {
            var line = (await streamReader.ReadLineAsync()).Split(",");
            results.Add((int.Parse(line[0]), int.Parse(line[1])));
        }

        return results;
    }

    public async Task<Dictionary<int, string>> GetEncodedNames()
    {
        var streamReader = new StreamReader(NamesAnnotationsEncodedPath);
        var dict = new Dictionary<int, string>();
        while (!streamReader.EndOfStream)
        {
            var line = (await streamReader.ReadLineAsync()).Split(",");
            dict.Add(int.Parse(line[0]), line[1]);
        }

        return dict;
    }

    public async Task<Dictionary<string, string>> GetNames()
    {
        var streamReader = new StreamReader(NamesAnnotationsPath);
        var dict = new Dictionary<string, string>();
        while (!streamReader.EndOfStream)
        {
            var line = (await streamReader.ReadLineAsync()).Split(",");
            dict.Add(line[0], line[1]);
        }

        return dict;
    }

    public async Task GetImagesClassesIds(List<int> ids, int top = 50000)
    {
        string ImagesIdsPath = $@"..\..\..\Datasets\images_ids_{top}.txt";
        var encodedNames = await GetEncodedNames();

        var chosenNames = ids.Select(e => encodedNames[e]);

        var names = await GetNames();

        var chosenNamesHash = new HashSet<string>(names.Where(e => chosenNames.Any(c => c == e.Value)).Select(e => e.Key));

        var streamReader = new StreamReader(BoxesPath);

        string id = "000002b66c9c498e";
        bool saved = false;

        int savedCount = 0;

        var sb = new StringBuilder();

        int c = 0;
        while (!streamReader.EndOfStream && savedCount < top)
        {
            var line = (await streamReader.ReadLineAsync()).Split(",");
            var nextId = line[0];
            if (id == nextId && !saved)
            {
                if (chosenNamesHash.Contains(line[2]))
                {
                    sb.AppendLine($"train/{id}");
                    savedCount++;
                    saved = true;
                }
            }
            if (id != nextId)
            {
                id = nextId;
                saved = false;
            }

            if (++c % 100000 == 0)
            {
                Console.WriteLine("Processing " + c);
            }

        }

        await File.WriteAllTextAsync(ImagesIdsPath, sb.ToString());
    }

    public async Task GetTopTrainBoxesSamples(List<int> ids, int top = 50000)
    {
        string trainingImagesBoxes = $@"..\..\..\Datasets\boxes_voc_top_{top}.txt";

        string trainingAnnotationsDirectory = @"..\..\..\Datasets\TrainYOLO\";

        Directory.CreateDirectory(trainingAnnotationsDirectory);

        var imagesIds = await GetTopTrainingImageIds(top);

        var encodedNames = await GetEncodedNames();

        var namesToEncode = encodedNames.ToDictionary(e => e.Value, e => e.Key);

        var chosenNames = ids.Select(e => encodedNames[e]);

        var names = await GetNames();

        var chosenNamesHash = new HashSet<string>(names.Where(e => chosenNames.Any(c => c == e.Value)).Select(e => e.Key));

        var streamReader = new StreamReader(BoxesPath);

        var sb = new StringBuilder();

        int c = 0;

        var annotations = new Dictionary<string, List<string>>();

        foreach (var name in imagesIds)
        {
            annotations.Add(name, new List<string>());
        }

        while (!streamReader.EndOfStream)
        {
            var line = (await streamReader.ReadLineAsync()).Split(",");
            var id = line[0];
            if (imagesIds.Contains(id) && chosenNamesHash.Contains(line[2]))
            {
                var annot = namesToEncode[names[line[2]]] + " " + line[4] + " " + line[5] + " " + line[6] + " " + line[7];
                annotations[id].Add(annot);

            }

            if (++c % 100000 == 0)
            {
                Console.WriteLine("Processing " + c);
            }

        }

        await Parallel.ForEachAsync(annotations, async (k, v) =>
        {
            await File.WriteAllLinesAsync(trainingAnnotationsDirectory + k.Key + ".txt", k.Value);
        });
    }

    public async Task ConvertYOLOToVOC(string yoloDirPath)
    {
        var dir = new DirectoryInfo(yoloDirPath);

        string sizesPath = @"C:\Users\BRUT4LxD\OneDrive - Wojskowa Akademia Techniczna\Pulpit\Moje\Uczelnia\Studia doktoranckie\SEM III\TGIS\Projekt\TGiS\GraphTheory\Datasets\images_sizes_results.csv";

        var sizesDict = (await File.ReadAllLinesAsync(sizesPath))
            .Select(e => e.Split(','))
            .ToDictionary(e => e[0], e => (Width: int.Parse(e[1]), Height: int.Parse(e[2])));

        var dict = new ConcurrentDictionary<string, VOCAnnotation>();

        var tasks = dir.GetFiles().Select(e => Task.Run(async () =>
        {
            var lines = await File.ReadAllLinesAsync(e.FullName);
            var voc = new VOCAnnotation
            {
                Filename = e.Name.Split(".")[0] + ".jpg",
                Width = sizesDict[e.Name.Split(".")[0]].Width,
                Height = sizesDict[e.Name.Split(".")[0]].Height,
                Annot = new()
            };

            foreach (var line in lines)
            {
                var obj = line.Split(" ");
                voc.Annot.Add(new()
                {
                    Name = obj[0],
                    BndBox = new()
                    {
                        XMin = double.Parse(obj[1]) * voc.Width,
                        XMax = double.Parse(obj[2]) * voc.Width,
                        YMin = double.Parse(obj[3]) * voc.Height,
                        YMax = double.Parse(obj[4]) * voc.Height
                    }
                });
            }

            dict.TryAdd(e.Name.Split(".")[0], voc);
        }));

        await Task.WhenAll(tasks);

        string XMLDirPath = @"C:\Users\BRUT4LxD\OneDrive - Wojskowa Akademia Techniczna\Pulpit\Moje\Uczelnia\Studia doktoranckie\SEM III\TGIS\Projekt\TGiS\GraphTheory\Datasets\TrainXml\";

        Directory.CreateDirectory(XMLDirPath);
        await Parallel.ForEachAsync(dict, async (k, v) =>
        {
            await File.WriteAllTextAsync(XMLDirPath + k.Key + ".xml", k.Value.SerializeXml());
        });

    }

    private async Task<HashSet<string>> GetTopTrainingImageIds(int top = 50000)
    {
        var hash = new HashSet<string>();
        string trainingImagesPath = $@"..\..\..\Datasets\images-ids_{top}.txt";

        var streamReader = new StreamReader(trainingImagesPath);

        while (!streamReader.EndOfStream)
        {
            var line = (await streamReader.ReadLineAsync()).Replace("train/", "").Replace("\\n", "");
            hash.Add(line);
        }

        return hash;
    }

    private int CountLines(string path)
    {
        int count = 0;
        var streamReader = new StreamReader(path);
        while (!streamReader.EndOfStream)
        {
            streamReader.ReadLine();
            count++;
        }

        return count;
    }

    private static async Task SaveTrainingAndTestSamples(StringBuilder trainSb, string trainPath, StringBuilder testSb, string testPath)
    {
        await File.AppendAllTextAsync(trainPath, trainSb.ToString());
        await File.AppendAllTextAsync(testPath, testSb.ToString());
    }

    private void GenerateTrainAndTestSamples(
        string sep,
        List<string[]> list,
        Dictionary<int, IEnumerable<int>> enumerables,
        StringBuilder trainSb,
        StringBuilder testSb,
        int numClasses)
    {
        var numOfTrainSamples = Math.Min(list.Count - 1, TrainSamplesPerPicture);
        var train = new HashSet<int[]>();
        while (numOfTrainSamples-- > 0)
        {
            HashSet<int> localTrain = GenerateRandomTrainClasses(list, SampleInputs, train);
            double[] trainArr = GenerateTrainSample(list, SampleInputs, SampleInputSize, localTrain);

            trainSb.AppendJoin(sep, trainArr);
            trainSb.AppendLine();

            int[] testArr = GenerateTestSample(list, enumerables, numClasses, localTrain);

            testSb.AppendJoin(sep, testArr);
            testSb.AppendLine();
        }
    }

    private static int[] GenerateTestSample(List<string[]> list, Dictionary<int, IEnumerable<int>> enumerables, int numClasses, HashSet<int> localTrain)
    {
        var testArr = new int[numClasses + 1];
        var iterator = enumerables.ContainsKey(list.Count - 1) ? enumerables[list.Count - 1] : Enumerable.Range(0, list.Count);
        foreach (var t in iterator.Except(localTrain).ToList())
        {
            testArr[int.Parse(list[t][0])] = 1;
        }

        return testArr;
    }

    private double[] GenerateTrainSample(List<string[]> list, int sampleInputs, int sampleInputSize, HashSet<int> localTrain)
    {
        var trainArr = new double[sampleInputs * sampleInputSize];
        var s = GenerateRandomStack(sampleInputs);
        foreach (int index in localTrain)
        {
            var randomIndex = s.Pop();

            for (int j = 0; j < sampleInputSize; j++)
            {
                trainArr[randomIndex * sampleInputs + j] = double.Parse(list[index][j]);
            }
        }

        return trainArr;
    }

    private double[] GenerateTrainSampleForSentiment(List<string[]> list, int sampleInputs, int sampleInputSize, HashSet<int> localTrain, List<int> missingIdxs, bool positiveSample = true)
    {
        var trainArr = new double[sampleInputs * sampleInputSize];
        if (positiveSample)
        {
            AddPositiveSample(trainArr, list, localTrain);
        }
        else
        {
            AddNegativeSample(trainArr, missingIdxs);
        }

        var s = GenerateRandomStack(sampleInputs);
        foreach (int index in localTrain)
        {
            var randomIndex = s.Pop();
            if (randomIndex == 0) continue;

            for (int j = 0; j < sampleInputSize; j++)
            {
                trainArr[randomIndex * sampleInputs + j] = double.Parse(list[index][j]);
            }
        }

        return trainArr;
    }

    private void AddNegativeSample(double[] trainArr, List<int> missingIdxs)
    {
        trainArr[0] = GenerateNegativePredictor(missingIdxs);
        trainArr[1] = _r.NextDouble();
        trainArr[2] = _r.NextDouble();
        trainArr[3] = _r.NextDouble();
        trainArr[4] = _r.NextDouble();
    }

    private void AddPositiveSample(double[] trainArr, List<string[]> list, HashSet<int> localTrain)
    {
        var predictor = GeneratePositivePredictor(list, localTrain);

        trainArr[0] = double.Parse(list[predictor][0]);
        trainArr[1] = double.Parse(list[predictor][1]);
        trainArr[2] = double.Parse(list[predictor][2]);
        trainArr[3] = double.Parse(list[predictor][3]);
        trainArr[4] = double.Parse(list[predictor][4]);
    }

    private HashSet<int> GenerateRandomTrainClasses(List<string[]> list, int sampleInputs, HashSet<int[]> train)
    {
        var localTrain = new HashSet<int>();
        do
        {
            int trainClasses = Math.Min(_r.Next(list.Count / 2, list.Count), sampleInputs);
            localTrain = new HashSet<int>(GenerateRandomList(list.Count).Take(trainClasses).ToArray());
        }
        while (!train.Add(localTrain.ToArray()));
        return localTrain;
    }

    private HashSet<int> GenerateRandomTrainClassesForSentiment(List<string[]> list, int sampleInputs, HashSet<int[]> train)
    {
        var localTrain = new HashSet<int>();
        do
        {
            int trainClasses = Math.Min(_r.Next(list.Count - 1), sampleInputs);
            localTrain = new HashSet<int>(GenerateRandomList(list.Count).Take(trainClasses).ToArray());
        }
        while (!train.Add(localTrain.ToArray()));
        return localTrain;
    }

    private Stack<int> GenerateRandomStack(int n)
    {
        return new Stack<int>(GenerateRandomList(n));
    }

    private List<int> GenerateRandomList(int n)
    {
        return Enumerable.Range(0, n).ToList().Shuffle();
    }
}
