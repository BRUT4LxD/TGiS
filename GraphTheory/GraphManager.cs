namespace Graph
{
    internal class GraphManager
    {
        private const string boxesPath = @"..\..\..\Datasets\train-annotations-bbox.csv";
        private const string namesPath = @"..\..\..\Datasets\class-descriptions-boxable.csv";
        private readonly IReadOnlyDictionary<string, string> _names;

        public IDictionary<string, ClassDescription> ClassDescriptions { get; } = new Dictionary<string, ClassDescription>();

        public GraphManager()
        {
            _names = LoadCategories();
        }

        private static IReadOnlyDictionary<string, string> LoadCategories()
        {
            Console.WriteLine("LOADING CATEGORIES...");
            var dict = new Dictionary<string, string>();
            using StreamReader sr = new(namesPath);
            while (!sr.EndOfStream)
            {
                var x = sr.ReadLine().Split(",");
                dict.Add(x[0], x[1]);
            }

            Console.WriteLine("LOADED");
            return dict;
        }

        internal async Task CalculateClassDescriptions()
        {
            Console.WriteLine("PROCESSING IMAGES...");
            using StreamReader fileStream = new(boxesPath);
            _ = await fileStream.ReadLineAsync();
            var line = (await fileStream.ReadLineAsync()).Split(",");
            var id = line[0];
            List<BoundingBox> boxes = new();
            boxes.Add(BoundingBox.Create(line));
            int c = 0;
            while (!fileStream.EndOfStream)
            {
                var nextLine = (await fileStream.ReadLineAsync()).Split(",");
                var nextId = nextLine[0];

                if (nextId == id)
                {
                    boxes.Add(BoundingBox.Create(nextLine));
                    continue;
                }

                if (++c % 100000 == 0) Console.WriteLine("CALCULATING : " + c + "TH IMAGE");
                AddToMatrix(boxes);
                boxes.Clear();
                boxes.Add(BoundingBox.Create(nextLine));
                id = nextId;
            }

            Console.WriteLine("DONE");
        }

        private void AddToMatrix(List<BoundingBox> boxes)
        {
            var simpleClassDescriptions = boxes.Select(e => SimpleClassDescription.Create(e, _names[e.LabelName])).ToList();

            for (int i = 0; i < simpleClassDescriptions.Count; i++)
            {
                var c1 = simpleClassDescriptions[i];

                if (!ClassDescriptions.ContainsKey(c1.Name))
                {
                    ClassDescriptions.Add(c1.Name, new ClassDescription(c1.Name));
                }

                var c1Description = ClassDescriptions[c1.Name];

                for (int j = i + 1; j < simpleClassDescriptions.Count; j++)
                {
                    var c2 = simpleClassDescriptions[j];

                    if (!ClassDescriptions.ContainsKey(c2.Name))
                    {
                        ClassDescriptions.Add(c2.Name, new ClassDescription(c2.Name));
                    }

                    c1Description.AddSample(c2);

                    if (c1.Name != c2.Name)
                    {
                        ClassDescriptions[c2.Name].AddSample(c1);
                    }
                }
            }
        }

        internal void PrintRelationCountMatrix(int top = -1)
        {
            top = top == -1 ? ClassDescriptions.Count : Math.Min(top, ClassDescriptions.Count);
            foreach (var item in ClassDescriptions)
            {
                Console.Write(item.Key + ": ");
                PriorityQueue<KeyValuePair<string, int>, int> queue = new(top, new NumberComparer(false));

                foreach (var j in item.Value.RelationCounts.RelationCountsGraph)
                {
                    queue.Enqueue(new KeyValuePair<string, int>(j.Key, j.Value), j.Value);
                }

                while (queue.Count != 0)
                {
                    var el = queue.Dequeue();

                    Console.Write($"{el.Key}:{el.Value}, ");
                }

                Console.WriteLine();
                Console.WriteLine();
            }
        }

        internal async Task SaveResults(string path)
        {
            await IOMananger.SaveToFile(path, ClassDescriptions, FileFormat.JSON);
        }
    }
}