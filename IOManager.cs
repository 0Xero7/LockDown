using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace lockdown
{
    public static class IOManager
    {
        public static List<EntityDetails> GetEntities(string path, string extention = null)
        {
            var entities = new List<EntityDetails>();
            var dirs = Directory.GetDirectories(path);
            foreach (var i in dirs)
                entities.Add(new EntityDetails(i, 0, true));

            var files = Directory.GetFiles(path);
            foreach (var i in files)
            {
                if (extention == null || i.EndsWith($".{extention}"))
                    entities.Add(new EntityDetails(i, (new FileInfo(i)).Length, false));
            }
            return entities;
        }

        public static List<EntityDetails> GetDirectories(string path)
        {
            var entities = new List<EntityDetails>();
            var dirs = Directory.GetDirectories(path);
            foreach (var i in dirs)
                entities.Add(new EntityDetails(i, 0, true));
            return entities;
        }
    }
}
