using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Globalization;
using System.Data.Common;
using System.Data.SqlClient;
//using System.Data.OleDb;

namespace Lab5_psp_server
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Data Source=FINDER-ПК;Initial Catalog=Lab6;Integrated Security=True";
            //string connectionString = "Driver={Microsoft Access Driver (*.mdb)}; " + "DBQ=C:\\Users\\Finder\\Documents\\Cpprdinates.mdb";
            //const string databaseName = @"C:\Users\Finder\Documents\Coordinates.db";
            SqlConnection conn = new SqlConnection(connectionString);
            //OleDbConnection conn =new OleDbConnection(connectionString);
            ThreadedServer ts = new ThreadedServer(80,conn);
            ts.Start();
            Console.ReadLine();
            
        }
    }
}
