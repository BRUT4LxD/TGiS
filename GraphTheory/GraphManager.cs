namespace Graph
{
    internal class GraphManager
    {
        private const string boxesPath = @"..\..\..\Datasets\train-annotations-bbox.csv";
        private const string namesPath = @"..\..\..\Datasets\class-descriptions-boxable.csv";
        private readonly IReadOnlyDictionary<string, string> _names;

        public IDictionary<string, IDictionary<string, int>> RelationCountMatrix { get; } = new Dictionary<string, IDictionary<string, int>>();

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

        internal async Task CalculateRelationCounts()
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
            for (int i = 0; i < boxes.Count; i++)
            {
                for (int j = i + 1; j < boxes.Count; j++)
                {
                    var x = _names[boxes[i].LabelName];
                    var y = _names[boxes[j].LabelName];

                    if (!RelationCountMatrix.ContainsKey(x))
                    {
                        RelationCountMatrix.Add(x, new Dictionary<string, int>());
                    }

                    if (!RelationCountMatrix.ContainsKey(y))
                    {
                        RelationCountMatrix.Add(y, new Dictionary<string, int>());
                    }

                    if (!RelationCountMatrix[x].ContainsKey(y))
                    {
                        RelationCountMatrix[x].Add(y, 0);
                    }

                    if (!RelationCountMatrix[y].ContainsKey(x))
                    {
                        RelationCountMatrix[y].Add(x, 0);
                    }

                    if (x != y)
                    {
                        RelationCountMatrix[x][y]++;
                        RelationCountMatrix[y][x]++;
                        continue;
                    }

                    RelationCountMatrix[x][x]++;
                }
            }
        }

        internal void PrintRelationCountMatrix(int top = -1)
        {
            top = top == -1 ? RelationCountMatrix.Count : Math.Min(top, RelationCountMatrix.Count);
            foreach (var item in RelationCountMatrix)
            {
                Console.Write(item.Key + ": ");
                PriorityQueue<KeyValuePair<string, int>, int> queue = new(top, new NumberComparer(false));

                foreach (var j in item.Value)
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
            await IOMananger.SaveToFile(path, RelationCountMatrix, FileFormat.JSON);
        }
    }
}