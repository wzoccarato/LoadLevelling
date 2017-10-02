using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace LoadL.loadDatabase
{
    class Program
    {
        static void Main(string[] args)
        {
            RunScript();
        }

        private static void RunScript()
        {
            My_DataEntities db = new System.Data.Entity   My_DataEntities();

            string line;

            System.IO.StreamReader file =
               new System.IO.StreamReader("c:\\ukpostcodesmssql.sql");
            while ((line = file.ReadLine()) != null)
            {
                db.Database.ExecuteSqlCommand(line);
            }

            file.Close();
        }
    }
}
