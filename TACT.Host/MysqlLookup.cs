using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using TACT.Net.FileLookup;

namespace TACT.Host
{
    class MysqlLookup : IFileLookup
    {
        public bool IsLoaded { get; private set; }

        AppSettings settings;
        MySqlConnection conn = null;

        public MysqlLookup(AppSettings settings)
        {

            this.settings = settings;
        }

        public void Close()
        {
            conn.Close();
        }

        public uint GetOrCreateFileId(string filename)
        {
            string stm = "Select * from file_data_id where path like '%" + filename + "%'";
            MySqlCommand cmd = new MySqlCommand(stm, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();

            uint fileDataId = 0;
            int count = 0;
            while (rdr.Read())
            {

                fileDataId = (UInt32) rdr.GetInt32(1);
                count++;
            }

            if (count > 1)
            {

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Found more than one Entry for " + filename + ". Using last");
                Console.ForegroundColor = ConsoleColor.White;

            }

            if (fileDataId == 0)
            {
                fileDataId = getLastFileID();
                fileDataId++;

                stm = "Insert into file_data_id (id, path) Values (" + fileDataId + "," + filename + ")";
                cmd = new MySqlCommand(stm, conn);
                cmd.ExecuteNonQuery();
            }

            return fileDataId;
        }

        public void Open()
        {

            try
            {
                conn = new MySqlConnection("server=" + settings.databaseAdress + ";userid=" + settings.mysqlUser + ";password=" + settings.mysqlPassword + ";database=" + settings.databaseName);
                IsLoaded = true;
            }catch(Exception e)
            {

                Console.WriteLine(e.StackTrace);
            }
            

        }



        #region

        private uint getLastFileID()
        {

            string stm = "SELECT MAX(id) FROM mytable";
            MySqlCommand cmd = new MySqlCommand(stm, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();


            uint fileDataId = 0;
            while (rdr.Read())
            {

                fileDataId = (UInt32)rdr.GetInt32(2);

            }

            return fileDataId;

        }

        #endregion
    }
}
