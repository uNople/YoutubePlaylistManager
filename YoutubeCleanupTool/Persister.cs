using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YoutubeCleanupTool.Interfaces;

namespace YoutubeCleanupTool
{
    public class Persister : IPersister
    {
        public Persister()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
            };
        }

        public bool DataExists(string playlistFile)
        {
            return File.Exists(playlistFile);
        }

        public T GetData<T>(string playlistFile)
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(playlistFile));
        }

        public void SaveData<T>(string playlistFile, T saveThis)
        {
            File.WriteAllText(playlistFile, JsonConvert.SerializeObject(saveThis));
        }
    }
}
