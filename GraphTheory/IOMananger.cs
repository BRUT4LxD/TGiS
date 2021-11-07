﻿using System.Text.Json;

namespace Graph
{
    internal static class IOMananger
    {
        internal static async Task SaveToFile(string path, object data, FileFormat format)
        {
            string content = "";
            if (format == FileFormat.JSON)
            {
                content = JsonSerializer.Serialize(data);
            }

            await File.WriteAllTextAsync(path, content);
        }
    }
}