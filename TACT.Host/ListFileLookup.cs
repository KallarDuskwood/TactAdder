using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TACT.Net.Common;
using TACT.Net.FileLookup;

namespace TACT.Host
{
    public class ListFileLookup : IFileLookup
    {
        public bool IsLoaded { get; private set; }

        private const string ListFilePath = "listfile.csv";

        private Queue<uint> _unusedIds;
        private readonly Dictionary<string, uint> _fileLookup;

        public ListFileLookup()
        {
            _unusedIds = new Queue<uint>();
            _fileLookup = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase);
        }

        public void Open()
        {
            uint id;
            string name;

            // load Id-Name map
            foreach (var file in File.ReadAllLines(ListFilePath))
            {
                int commaIndex = file.IndexOf(',');
                if (commaIndex == -1)
                    continue;

                id = uint.Parse(file.Substring(0, commaIndex));
                name = file.Substring(commaIndex + 1);

                // unique-ify unnamed
                if (name == "")
                    name = "UNNAMED_" + id;

                _fileLookup.Add(name.WoWNormalise(), id);
            }

            Console.WriteLine($"Loaded {_fileLookup.Count} Ids");
            LoadUnusedIDs();

            IsLoaded = true;
        }

        /// <summary>
        /// Exports an updated CSV and also a standard txt file
        /// </summary>
        public void Close()
        {
            Console.WriteLine("Exporting Listfiles");

            using (var txt = new StreamWriter("listfile.txt"))
            using (var csv = new StreamWriter(ListFilePath))
            {
                foreach (var lookup in _fileLookup.OrderBy(x => x.Value))
                {
                    csv.WriteLine(lookup.Value + "," + lookup.Key);
                    txt.WriteLine(lookup.Key);
                }
            }
        }

        public uint GetOrCreateFileId(string filename)
        {
            // check the filename exists
            if (!_fileLookup.TryGetValue(filename, out uint id))
            {
                // attempt to take an id from the pool
                if (_unusedIds.Count > 0)
                {
                    id = _fileLookup.Last().Value +1;
                    _fileLookup.Add(filename, id);
                    return id;
                }

                // TODO verify the best way of handling this
                throw new Exception("Out of unused Ids - SEND HELP!");
            }

            return id;
        }

        #region Misc

        private void LoadUnusedIDs()
        {
            var idRange = Enumerable.Range(1, (int)_fileLookup.Values.Max())
                                    .Select(x => (uint)x)
                                    .Except(_fileLookup.Values);

            _unusedIds = new Queue<uint>(idRange);

            Console.WriteLine($"Found {_unusedIds.Count} unused Ids");
        }

        public Task Sync() => throw new NotImplementedException();


        public string WoWNormalise(string str)
        {
            return str.TrimStart(new char[] { '\\', '/' })
                      .Replace('\\', '/')
                      .ToLowerInvariant();
        }

        #endregion


    }
}

