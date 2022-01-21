using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GraphTheory.Models
{
    public class VOCAnnotation
    {
        [XmlElement(ElementName = "filename")]
        public string Filename { get; set; }

        [XmlElement(ElementName = "width")]
        public int Width { get; set; }

        [XmlElement(ElementName = "height")]
        public int Height { get; set; }

        [XmlElement(ElementName = "object")]
        public List<Annotation> Annot { get; set; }

    }
}
