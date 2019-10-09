using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using TACT.Net;
using TACT.Net.BlockTable;
using TACT.Net.Configs;
using TACT.Net.Install;
using TACT.Net.Root;

namespace TACT.Host
{
    class Program
    {
        public static string CDN_DATA_DIR; // = CdnRootDir + @"\tpr\wow";
        public static string MANIFEST_PATH; // = CdnRootDir + @"\wow";
        


        static void Main(string[] args)
        {

            string seetingsDir = Directory.GetCurrentDirectory() + "/AppSettings.json";
            AppSettings settings = null;

            try
            {

               
                FileStream f = new FileStream(seetingsDir, FileMode.Open, FileAccess.Read, FileShare.Read);
                var streamReader = new StreamReader(f, Encoding.UTF8);
                settings = JsonConvert.DeserializeObject<AppSettings>(streamReader.ReadToEnd());

            }
            catch (Exception e)
            {

                settings = new AppSettings
                {
                    cdnRootDir = "",

                    mysqlUser = "root",
                    mysqlPassword = "root",
                    dataFolderPath = Directory.GetCurrentDirectory() + "\\Data\\",
                    exportFolderPath = Directory.GetCurrentDirectory() + "\\DataExport\\",
                    databaseName = "tacthost",
                    databaseAdress = "localhost"
                };

                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(seetingsDir, json);
            }

            CDN_DATA_DIR = settings.cdnRootDir + @"tpr\wow";
            MANIFEST_PATH = settings.cdnRootDir + @"wow";

            var tactRepo = new TACTRepo(CDN_DATA_DIR)
            {

                ManifestContainer = new ManifestContainer("wow", Locale.EU),
                ConfigContainer = new ConfigContainer()
            };

            tactRepo.ManifestContainer.OpenLocal(MANIFEST_PATH);

            tactRepo.ConfigContainer.OpenLocal(tactRepo.BaseDirectory, tactRepo.ManifestContainer);
            
            tactRepo.IndexContainer = new Net.Indices.IndexContainer();
            tactRepo.IndexContainer.Open(tactRepo.BaseDirectory);
            
            
            tactRepo.EncodingFile = new Net.Encoding.EncodingFile(tactRepo.BaseDirectory, tactRepo.ConfigContainer.EncodingEKey);
            tactRepo.EncodingFile.TryGetCKeyEntry(tactRepo.ConfigContainer.RootCKey, out var rootCEntry);
            tactRepo.RootFile = new Net.Root.RootFile(tactRepo.BaseDirectory, rootCEntry.EKey);
            tactRepo.InstallFile = new InstallFile(tactRepo.BaseDirectory, tactRepo.ConfigContainer.InstallEKey);
            tactRepo.DownloadFile = new Net.Download.DownloadFile(tactRepo.BaseDirectory, tactRepo.ConfigContainer.DownloadEKey);
            tactRepo.PatchFile = new Net.Patch.PatchFile(tactRepo.BaseDirectory, tactRepo.ConfigContainer.PatchEKey);

            tactRepo.RootFile.FileLookup = new ListFileLookup();

            string[] files = Directory.GetFiles(settings.dataFolderPath, "*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = files[i].Replace(settings.dataFolderPath, "");
            }

            Console.WriteLine("Found " + files.Length + " Files to proccess in " + settings.dataFolderPath);

            

            foreach (string s in files)
            {

                string filename = s.Substring(s.LastIndexOf("\\") + 1);
                string path = s.Substring(0, s.LastIndexOf("\\") + 1);


                LocaleFlags locale = LocaleFlags.All_WoW;

                if (path.ToLower().Contains("dbfilesclient")){

                    var match = Regex.Match(path, @"(frFR)|(enUS)|(ptBR)|(ruRU)|(esES)|(esMX)|(itIT)|(deDE)|(enGB)|(ptPT)");
                    var pathSplit = path.Split("\\");
                    var localeString = "";
                    foreach (var split in pathSplit)
                    {
                        if (split.Contains(match.Value))
                        {
                            locale = (LocaleFlags)Enum.Parse(typeof(LocaleFlags), split);
                            localeString = split;
                            path = path.Replace("\\" + localeString, "");
                            break;
                        }

                    }
                }


            

                CASRecord record = BlockTableEncoder.EncodeAndExport(settings.dataFolderPath + s, settings.exportFolderPath, (path + filename).Replace("\\", "/"));



                uint fId = tactRepo.RootFile.FileLookup.GetOrCreateFileId(record.FileName);

                Console.WriteLine("Added " + filename + " with ID: " + fId);

                tactRepo.RootFile.AddOrUpdate(record, tactRepo, locale);
                tactRepo.InstallFile.AddOrUpdate(record, tactRepo);

            }

            

            tactRepo.Save(tactRepo.BaseDirectory, settings.cdnRootDir);



        }



    }
}
