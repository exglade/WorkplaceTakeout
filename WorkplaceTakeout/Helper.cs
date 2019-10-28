using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WorkplaceTakeout
{
    public static class Helper
    {
        private static JsonSerializerSettings _graphApiJsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };

        public static T DeserializeAnonymousType<T>(object obj, T anonymousTypeObject)
        {
            return JsonConvert.DeserializeAnonymousType(JsonConvert.SerializeObject(obj), anonymousTypeObject, _graphApiJsonSettings);
        }

        public static void WriteObjectAsJsonToFile(string filepath, object result)
        {
            WriteTextToFile(filepath, JsonConvert.SerializeObject(result));
        }

        public static void WriteTextToFile(string filepath, string output)
        {
            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(output.ToString())))
            {
                WriteToFile(filepath, memoryStream);
            }
        }

        public static void WriteToFile(string filepath, Stream stream)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filepath));
            using (var fileStream = File.Create(filepath))
            {
                stream.CopyTo(fileStream);
            }
        }

        public static string GetFileNameFromUrl(string url)
        {
            return string.IsNullOrEmpty(url.Trim()) || !url.Contains(".") ? string.Empty : Path.GetFileName(new Uri(url).AbsolutePath);
        }
    }
}