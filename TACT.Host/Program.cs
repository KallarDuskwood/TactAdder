using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using TACT.Net;
using TACT.Net.BlockTable;
using TACT.Net.Configs;
using TACT.Net.Install;
using TACT.Net.Root;

namespace TACT.Host
{
    class Program
    {
        public const string ROOT_DIR = @"G:\WoW-Modding\Apache24\htdocs";
        public const string OrigRepo = ROOT_DIR + @"\tpr\wow";
        public const string MANIFEST_PATH = ROOT_DIR + @"\wow";
        


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

            var tactRepo = new TACTRepo(OrigRepo)
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

                CASRecord record = BlockTableEncoder.EncodeAndExport(settings.dataFolderPath + s, settings.exportFolderPath, (path + filename).Replace("\\", "/"));

                LocaleFlags Locale = LocaleFlags.All_WoW;

                if (filename.Contains("db2"))
                {
                    Locale = LocaleFlags.deDE;
                    Console.WriteLine("Added " + filename);
                }

                uint fId = tactRepo.RootFile.FileLookup.GetOrCreateFileId(record.FileName);
                tactRepo.RootFile.AddOrUpdate(record, tactRepo, Locale);
                tactRepo.InstallFile.AddOrUpdate(record, tactRepo);

            }

            

            tactRepo.Save(tactRepo.BaseDirectory, ROOT_DIR);



        }



    }
}
