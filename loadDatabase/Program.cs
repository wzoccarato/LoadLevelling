using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using AutoMapper;
using CalcExtendedLogics.Infrastructure.Abstract;
using CalcExtendedLogics.Infrastructure.AccessLayer;


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
        public static IDbQuery Dbq { get; set; }

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
            Dbq = new EfLoadL();

            if (args.Length != 1)
            {
                Console.Write($"Usage: loadDatabase(<path to Insert statements file>)\nExit");
                Environment.Exit(1);
            }
            Console.WriteLine($"Inizio insert in database {DateTime.Now:dd.MM.yyyy-HH:mm:ss.fff}");
            RunScript(args[0]);
            Console.WriteLine($"Fine insert in database {DateTime.Now:dd.MM.yyyy-HH:mm:ss.fff}");
            VerifyDataCongruence();
        }

        private static void RunScript(string filepath)
        {
            Database db = Dbq.LlDatabase;

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
            Console.WriteLine($"VeridyDataCongruence INPUT: {DateTime.Now:dd.MM.yyyy-HH:mm:ss.fff}");
            var pbu = Dbq.GetDistinctPlanBu();
            foreach (var p in pbu)
            {
                var fhr = Dbq.GetDistinctFlagHr(p);
                foreach (var f in fhr)
                {
                    var pcat = Dbq.GetDistinctProductionCategory(p, f);
                    foreach (var pc in pcat)
                    {
                        var lbwp = Dbq.ListByWeekAndPriority(p, f, pc);
                        if (lbwp.Count > 0)
                        {
                            var rnd = new Random();
                            do
                            {
                                var wte = lbwp.ElementAt(0).WEEK_PLAN;

                                // estrae dalla lista sortata soltanto i record relativi alla 
                                // week che deve essere elaborata. Questa lista e' gia' ordinata per 
                                // priorita' decrescente (crescente in senso numerico).
                                var toelaborate = lbwp.Where(r => r.WEEK_PLAN == wte).Select(r => r).ToList();
                           
                                var ll = toelaborate.Select(r => r.Capacity).Distinct().OrderByDescending(s => s).ToList();
                                if (ll.Count > 1)
                                {
                                    // se ce n'e' piu' di uno, allora sicuramente devo correggere
                                    // e il primo e' sicuramente diverso da 0
                                    // i = plan_bu, j = flag_hr, g = Capacity
                                    //Console.WriteLine($"Capacity_0 = {ll[0]}");
                                    (from rec in Dbq.LoadLevellingTable
                                     where rec.i == p && rec.j == f && rec.PRODUCTION_CATEGORY == pc
                                     select rec).ToList().ForEach(r => r.g = ll[0]);

                                    //(from rec in Dbq.LoadLevellingTable
                                    //    where rec.i == p && rec.j == f && rec.PRODUCTION_CATEGORY == pc
                                    //    select rec).UpdateFromQuery(rec => new LoadLevelling {g = ll[0]});
                                }
                                else
                                {
                                    // condronto cosi' perche' e' un double
                                    if (ll[0] < 0.1)
                                    {
                                        // i = plan_bu, j = flag_hr, g = Capacity
                                        var newcapacity = rnd.Next(50, 600);
                                        //Console.WriteLine($"newcapacity = {newcapacity}");
                                        (from rec in Dbq.LoadLevellingTable
                                         where rec.i == p && rec.j == f && rec.PRODUCTION_CATEGORY == pc
                                         select rec).ToList().ForEach(r => r.g = newcapacity);

                                        //(from rec in Dbq.LoadLevellingTable
                                        //    where rec.i == p && rec.j == f && rec.PRODUCTION_CATEGORY == pc
                                        //    select rec).UpdateFromQuery(rec => new LoadLevelling{g = newcapacity});

                                    }
                                }
                                // rimuove da sortedtable i records appena elaborati
                                lbwp.RemoveAll(r => r.WEEK_PLAN == wte);

                            } while (lbwp.Count > 0);
                        }
                        //foreach (var rc in lbpc)
                        //{
                        //    Console.WriteLine($"planbu = {rc.PLAN_BU}, flaghr ={rc.FLAG_HR}, prodcatecory = {rc.PRODUCTION_CATEGORY}, week = {rc.WEEK_PLAN}, priority = {rc.Priority}");
                        //}
                        //Console.ReadKey();
                    }
                }
            }
            Console.WriteLine($"VeridyDataCongruence OUTPUT: {DateTime.Now:dd.MM.yyyy-HH:mm:ss.fff}");

            // corregge tutti i record che hanno Required = 0
            var newreq = new Random();
            Console.WriteLine($"START correzione valori di required == 0 : {DateTime.Now:dd.MM.yyyy-HH:mm:ss.fff}");

            (from rec in Dbq.LoadLevellingTable where rec.h < 0.1 select rec).ToList().ForEach(r => r.h = newreq.Next(24,300));
            //(from rec in Dbq.LoadLevellingTable where rec.h < 0.1 select rec).UpdateFromQuery(rec => new LoadLevelling {h = newreq.Next(24, 300)});

            Console.WriteLine($"END correzione valori di required == 0 : {DateTime.Now:dd.MM.yyyy-HH:mm:ss.fff}");

            Console.WriteLine($"START massive saving on database by Entity Framework: {DateTime.Now:dd.MM.yyyy-HH:mm:ss.fff}");

            Dbq.MassiveSaveData();

            Console.WriteLine($"END massive saving on database by Entity Framework: {DateTime.Now:dd.MM.yyyy-HH:mm:ss.fff}");
            Console.WriteLine($"VeridyDataCongruence OUTPUT: {DateTime.Now:dd.MM.yyyy-HH:mm:ss.fff}");
        }
    }
}
