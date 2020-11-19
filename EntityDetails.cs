using System;
using System.Collections.Generic;
using System.Text;

namespace lockdown
{
    public class EntityDetails
    {
        private static string[] sizeDivisions = { "B ", "KB", "MB", "GB", "TB" };

        public string entityName { get; private set; }
        public long size { get; private set; }
        public bool isDirectory { get; private set; }

        string _shortName { get; set; }
        public string shortName
        {
            get => _shortName;
        }

        public string formattedSize
        {
            get
            {
                int divisions = 0;
                long temp = size;
                while (temp >= 1000)
                {
                    ++divisions;
                    temp /= 1024;
                }

                return $"{temp} {sizeDivisions[divisions]}";
            }
        }

        public EntityDetails(string entityName, long size, bool isDirectory)
        {
            this.entityName = entityName;
            this.size = size;
            this.isDirectory = isDirectory;

            _shortName = entityName.Split('\\')[^1];
        }
    }
}
