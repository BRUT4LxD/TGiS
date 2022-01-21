using System.Xml.Serialization;

namespace GraphTheory.Models
{
    public class Annotation
    {
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "bndbox")]
        public Box BndBox { get; set; }

    }

    public class Box
    {

        [XmlElement(ElementName = "xmin")]
        public double XMin { get; set; }

        [XmlElement(ElementName = "xmax")]
        public double XMax { get; set; }

        [XmlElement(ElementName = "ymin")]
        public double YMin { get; set; }

        [XmlElement(ElementName = "ymax")]
        public double YMax { get; set; }
    }
}
