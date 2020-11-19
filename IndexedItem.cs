using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;

namespace LockDown
{
    // <INDEX SIZE(4)> <DIRECTORY? (1)> <ID(10)> <PARENT ID(10)> <FILE SIZE(16)> <REL.PATH>
    public class IndexedItem
    {
        public long id { get; private set; }
        public long parentID { get; private set; }

        public bool isDirectory { get; private set; }
        public string path { get; set; }
        public string name { get; private set; }

        public long size { get; private set; }
        public long sectorStart { get; set; }

        public IndexedItem(long id, long parentID, bool isDirectory, string path, long size)
        {
            this.id = id;
            this.parentID = parentID;
            this.isDirectory = isDirectory;
            this.path = path;
            this.name = path.Split(@"\")[^1];
            this.size = size;
            this.sectorStart = 0;
        }

        public IndexedItem(string data)
        {
            isDirectory = (data[0] == '1');
            id = long.Parse(data.Substring(1, 10));
            parentID = -1;
            
            long temp;
            if (!long.TryParse(data.Substring(11, 10), out temp)) parentID = -1;
            else parentID = temp;

            size = long.Parse(data.Substring(21, 16));
            sectorStart = long.Parse(data.Substring(37, 10));
            name = data.Substring(47);
        }

        public string Serialize()
        {
            string ret = $"{(isDirectory ? 1 : 0)}" +
                $"{id.ToString().PadLeft(10, '0')}" +
                $"{parentID.ToString().PadLeft(10, '0')}" +
                $"{size.ToString().PadLeft(16, '0')}" +
                $"{sectorStart.ToString().PadLeft(10, '0')}" +
                $"{name}";
            return $"{ret.Length.ToString().PadLeft(4, '0')}{ret}";
        }
    }
}
