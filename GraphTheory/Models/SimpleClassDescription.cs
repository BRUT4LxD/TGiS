namespace Graph
{
    internal class SimpleClassDescription
    {
        public string Name { get; set; }

        public double SizeRatio { get; set; }

        public double Area { get; set; }

        public static SimpleClassDescription Create(BoundingBox boundingBox, string name)
        {
            return new SimpleClassDescription
            {
                Name = name,
                SizeRatio = Calculator.CalculateSizeRatio(boundingBox),
                Area = Calculator.CalculateArea(boundingBox)
            };
        }
    }
}