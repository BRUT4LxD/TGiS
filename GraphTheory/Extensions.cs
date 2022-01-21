using System.IO;
using System.Text.Unicode;
using System.Xml;
using System.Xml.Serialization;

public static class Extensions
{
    public static List<T> Shuffle<T>(this List<T> list)
    {
        Random random = new Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
        return list;
    }

    public static HashSet<T> AddRange<T>(this HashSet<T> hash, IEnumerable<T> list)
    {
        foreach (var item in list)
        {
            hash.Add(item);
        }

        return hash;
    }

    public static T DeserializeXml<T>(this string toDeserialize)
    {
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
        using (StringReader textReader = new StringReader(toDeserialize))
        {
            return (T)xmlSerializer.Deserialize(textReader);
        }
    }

    public static string SerializeXml<T>(this T toSerialize)
    {
        var emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
        var settings = new XmlWriterSettings();
        settings.Indent = true;
        settings.OmitXmlDeclaration = true;

        XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
        using var textWriter = new StringWriter();
        using var writer = XmlWriter.Create(textWriter, settings);
        xmlSerializer.Serialize(textWriter, toSerialize, emptyNamespaces);
        return textWriter.ToString().Substring(39);
    }
}
