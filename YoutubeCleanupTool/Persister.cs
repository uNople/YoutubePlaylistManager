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

        public bool DataExists(string name)
        {
            return File.Exists(name);
        }

        public T GetData<T>(string name) where T : new()
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(File.ReadAllText(name));
            }
            catch
            {
                return new T();
            }
        }

        public void SaveData<T>(string name, T saveThis)
        {
            File.WriteAllText(name, JsonConvert.SerializeObject(saveThis));
        }
    }
}
