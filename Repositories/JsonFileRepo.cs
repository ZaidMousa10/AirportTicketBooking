using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AirportTicketBooking.Repositories
{
    public class JsonFileRepo<T> where T : class, new()
    {
        private readonly string _path;
        private readonly JsonSerializerOptions _opts = new JsonSerializerOptions { WriteIndented = true };


        //Ensures the folder exists; if not, it creates it.
        //Ensures the JSON file exists; if not, it creates an empty list and saves it.
        public JsonFileRepo(string path)
        {
            _path = path;
            var dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            if (!File.Exists(_path)) SaveAll(new List<T>());
        }


        //If the file is empty or invalid, it returns an empty list.
        //Converts the JSON string into a List<T> using JsonSerializer.
        public List<T> LoadAll()
        {
            try
            {
                var txt = File.ReadAllText(_path);
                if (string.IsNullOrWhiteSpace(txt)) return new List<T>();
                return JsonSerializer.Deserialize<List<T>>(txt, _opts) ?? new List<T>();
            }
            catch { return new List<T>(); }
        }


        //Converts the List<T> into a JSON string using JsonSerializer.

        public void SaveAll(List<T> items)
        {
            var txt = JsonSerializer.Serialize(items, _opts);
            File.WriteAllText(_path, txt);
        }

        //Loads the current list, adds the new item, and saves the updated list.

        public void Add(T item)
        {
            var list = LoadAll();
            list.Add(item);
            SaveAll(list);
        }
    }
}
