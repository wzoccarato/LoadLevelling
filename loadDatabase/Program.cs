using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using AutoMapper;
using LoadL.Infrastructure.Abstract;
using LoadL.Infrastructure.AccessLayer;
using LoadL.DataLayer;
using LoadL.DataLayer.DbTables;
using LoadL.DataLayer;

namespace LoadL.loadDatabase
{
    // questo programma legge gli insert statements dal file di input,
    // genera ed esegue le query di inserimento nel database.
    // tratta 1000 linee alla volta, perche' T-SQL non permette query
    // piu' corpose.
    // il file contenente gli insert statementes viene generato 
    // esternamente, utilizzando uno script di bash, che legge un file
    // csv, e genera un file .sql contenente soltanto le linee da inserire
    // A questo proposito vedere csv2insert.sh
    // il resto viene fatto qui dentro 
    class Program
    {
        public static ILoadL Iloadlevelling { get; set; }

        private static string _pre = @"USE [SDG_Consulting]
    INSERT INTO [dbo].[LoadLevelling]
        ([PRODUCTION_CATEGORY],
        [IND_SEASONAL_STATUS],
        [TCH_WEEK],
        [PLANNING_LEVEL],
        [EVENT],
        [WEEK_PLAN],
        [a],[b],[c],
        [d],
        [e],
        [f],
        [g],
        [h],
        [i],
        [j],
        [k])
        VALUES";

        /// <summary>
        /// in argomento passare il path completao del file contenente 
        /// gli statement di insert al database.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Iloadlevelling = new EFLoadL();

            if (args.Length != 1)
            {
                Console.Write($"Usage: loadDatabase(<path to Insert statements file>)\nExit");
                Environment.Exit(1);
            }
            //RunScript(args[0]);
            VerifyDataCongruence();
        }

        private static void RunScript(string filepath)
        {
            Database db = Iloadlevelling.LLDatabase;

            // tratta 1000 linee alla volta, perche' T-SQL non permette di piu'

            using (StreamReader file = new StreamReader(filepath))
            {
                int externalcount = 0;
                while (!file.EndOfStream)
                {
                    string sqlcmd = _pre;
                    string line;
                    int count = 0;
                    do
                    {
                        if ((line = file.ReadLine()) != null)
                        {
                            if (file.EndOfStream)
                            {
                                // non mette la virgola alla fine
                                sqlcmd = $"{sqlcmd}\n{line}";
                                ++count;
                                break;
                            }
                        }
                        // solo per l'ultima linea non mette la virgola
                        if (++count < 1000)
                            line += ",";
                        sqlcmd = $"{sqlcmd}\n{line}";
                    } while (count < 1000);
                    externalcount += count;
                    db.ExecuteSqlCommand(sqlcmd);
                    Console.Write($"{externalcount} - {DateTime.Now}\n");
                }
            }
        }

        private static void VerifyDataCongruence()
        {

            var pbu = Iloadlevelling.GetDistinctPlanBu();
            foreach (var p in pbu)
            {
                var fhr = Iloadlevelling.GetDistinctFlagHr(p);
                foreach (var f in fhr)
                {
                    var pcat = Iloadlevelling.GetDistinctProductionCategory(p, f);
                    foreach (var pc in pcat)
                    {
                        
                    }
                }

            }
        }
    }
}
