using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace plc3D
{
    /// <summary>
    /// Handle communication with javascript localStorage
    /// </summary>
    public class Com
    {
        private bool connected = false;
        private string database;
        private SQLiteConnection connectionSQLite;
        private SQLiteCommand commandSQLite;
        private SQLiteDataAdapter adapterSQLite;
        private DataTable dataTable;

        public Com(string database)
        {
            this.database = database;
            Connect();

            //int[] data = ReadData();
            //WriteData(data);
        }

        private int Connect()
        {
            try
            {
                string dbConnection = "Data Source=" + database;
                connectionSQLite = new SQLiteConnection(dbConnection);
                connectionSQLite.Open();
                commandSQLite = new SQLiteCommand();
                commandSQLite.Connection = connectionSQLite;
                connected = true;
                return 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + database, "Error connecting to database", MessageBoxButton.OK, MessageBoxImage.Error);
                return -1;
            }
        }

        public int[] ReadData()
        {
            if (!connected) return null;
            try
            {
                string select = "SELECT value FROM data WHERE key='pins';";

                dataTable = new DataTable();
                commandSQLite.CommandText = select;
                adapterSQLite = new SQLiteDataAdapter(commandSQLite);
                commandSQLite.Parameters.Clear();
                int count = adapterSQLite.Fill(dataTable);

                byte[] pins = (byte[])dataTable.Rows[0].ItemArray[0];
                string _pins = Encoding.UTF8.GetString(pins);
                int[] data = JsonConvert.DeserializeObject<int[]>(_pins);
                return data;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Error reading from database", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public int WriteData(int[] data)
        {
            if (!connected) return -1;
            try
            {
                data = new int[] { 0, 0, 0, 0, 501, 500, 0, 0, 0, 0 };
                string _pins = JsonConvert.SerializeObject(data);
                byte[] pins = Encoding.UTF8.GetBytes(_pins);

                string insert = "REPLACE INTO data (key, utf16_length, conversion_type, compression_type, value) VALUES('pins'," + pins.Length + ",1,1,@value);";
                commandSQLite.CommandText = insert;
                commandSQLite.Parameters.Clear();
                commandSQLite.Parameters.AddWithValue("@value", pins);
                commandSQLite.ExecuteNonQuery();
                return 0;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Error writing to database", MessageBoxButton.OK, MessageBoxImage.Error);
                return -1;
            }
        }

    }
}
