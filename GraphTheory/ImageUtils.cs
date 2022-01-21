using System.Collections.Concurrent;
using System.Text;
using System.Xml.Linq;
using ImageMagick;
using static System.Net.Mime.MediaTypeNames;

namespace GraphTheory
{
    internal static class ImageUtils
    {
        public static (int, int) GetImageSize(string path)
        {
            using var image = new MagickImage(path);
            return (image.Width, image.Height);
        }

        public static async Task<ConcurrentDictionary<string, (int Width, int Height)>> GetImagesSizes(string folderDict, bool saveToFile = false)
        {
            var dir = new DirectoryInfo(folderDict);

            var names = dir.GetFiles().Select(e => e.Name);

            var dict = new ConcurrentDictionary<string, (int Width, int Height)>();

            var tasks = names.Select(name =>
                Task.Run(() =>
                {
                    var size = GetImageSize(Path.Combine(folderDict, name));
                    dict.TryAdd(name.Split(".")[0], size);
                })
            );

            await Task.WhenAll(tasks);

            if (saveToFile)
            {
                var sb = new StringBuilder();

                foreach (var item in dict)
                {
                    sb.AppendJoin(",", item.Key, item.Value.Width, item.Value.Height);
                    sb.AppendLine();
                }


                var savePath = "images_sizes_results.csv";
                await File.WriteAllTextAsync(savePath, sb.ToString());
            }

            return dict;
        }
    }
}
