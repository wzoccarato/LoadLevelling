using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using AutoMapper;
using AutoMapper.Configuration.Conventions;
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
        [Week],
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
                Console.Write($"Usage: loadDatabase(<path to Insert_statements_file>)\nExit");
                Environment.Exit(1);
            }
            Console.WriteLine($"Inizio insert in database {DateTime.Now:dd.MM.yyyy-HH:mm:ss.fff}");
            RunScript(args[0],Dbq.LlDatabase);
            Console.WriteLine($"Fine insert in database {DateTime.Now:dd.MM.yyyy-HH:mm:ss.fff}");

            // la funzione che segue corregge i dati in tabella, per  renderli coerenti
            // se non serve, commentare quella che segue e' necessario commentarla
            VerifyDataCongruence();

            Console.WriteLine($"START massive saving on database by Entity Framework: {DateTime.Now:dd.MM.yyyy-HH:mm:ss.fff}");
            Dbq.MassiveSaveData();
            Console.WriteLine($"END massive saving on database by Entity Framework: {DateTime.Now:dd.MM.yyyy-HH:mm:ss.fff}");
        }

        private static void RunScript(string filepath,Database db)
        {
            string newline = string.Empty;
            string[] ch = new string[1024];
            double val1, val2;

            try
            {
                var simula = new Random();
                string[] planbu = {"AA", "BB", "CC"};
                string[] flaghr = {"N", "H"};

                // tratta 1000 linee alla volta, perche' T-SQL non permette di piu'
                using (StreamReader file = new StreamReader(filepath))
                {
                    int externalcount = 0;
                    while (!file.EndOfStream)
                    {
                        string sqlcmd = _pre;
                        int count = 0;
                        do
                        {
                            string line;
                            if ((line = file.ReadLine()) != null)
                            {
                                if (file.EndOfStream)
                                {
                                    //++ questa vale solo per il nuovo database acquisito direttamente da board 13.12.2017
                                    ch = line.Split(new[] { "\'," }, StringSplitOptions.None);

                                    // toglie l'apice iniziale, devo confrontare i valori
                                    // lo faccio prima di convertire la virgola decimale. 
                                    // in caso contrario non funziona, per problemi relativi alla cultura.
                                    val1 = Convert.ToDouble(ch[10].Substring(1));
                                    val2 = Convert.ToDouble(ch[11].Substring(1));

                                    // arrotonda capacity e required
                                    val1 = Math.Round(val1, Global.ROUNDDIGITS);
                                    val2 = Math.Round(val2, Global.ROUNDDIGITS);

                                    ch[10] = ch[10].Replace(',', '.');      // capacity converte virgola decimale in punto
                                    ch[11] = ch[11].Replace(',', '.');      // required converte virgola decimale in punto
                                    if (ch[12] == "'")
                                        ch[12] += $"{planbu[simula.Next(3)]}";   // plan_bu
                                    if (ch[13] == "'")
                                        ch[13] += $"{flaghr[simula.Next(2)]}";   // flag_hr


                                    // vengono tenuti in considerazione soltanto i record per i quali capacity && required non sono entrambi a zero
                                    //if ((Math.Abs(val1) >= Global.EPSILON) || (Math.Abs(val2) >= Global.EPSILON))
                                    if ((Math.Abs(val2) >= Global.EPSILON))
                                    {
                                        //newline = $"({ch[1]}',{ch[2]}',{ch[3]}',{ch[4]}',{ch[5]}',{ch[6]}','0','0','0',{ch[7]}',{ch[8]}',{ch[9]}',{ch[10]}',{ch[11]}',{ch[12]}',{ch[13]}',{ch[14]}";
                                        newline = $"({ch[1]}',{ch[2]}',{ch[3]}',{ch[4]}',{ch[5]}',{ch[6]}','0','0','0',{ch[7]}',{ch[8]}',{ch[9]}','{val1}','{val2}',{ch[12]}',{ch[13]}',{ch[14]}";
                                        sqlcmd = $"{sqlcmd}\n{newline}";
                                        //-- questa vale solo per il nuovo database acquisito direttamente da board 13.12.2017

                                        // non mette la virgola alla fine
                                        //sqlcmd = $"{sqlcmd}\n{line}";
                                        ++count;
                                    }
                                    else
                                    {
                                        // in ogni caso bisogna correggere il comando sql, perche' essendo l'ultima linea, bisogna togliere
                                        // la virgola finale
                                        var index = sqlcmd.LastIndexOf(",");
                                        if (index != -1)
                                        {
                                            sqlcmd=sqlcmd.Remove(index, 1);
                                        }
                                    }
                                    break;
                                }
                            }
                            //++ questa vale solo per il nuovo database acquisito direttamente da board 13.12.2017
                            ch = line.Split(new[] { "\'," }, StringSplitOptions.None);

                            // toglie l'apice iniziale, devo confrontare i valori
                            // lo faccio prima di convertire la virgola decimale. 
                            // in caso contrario non funziona, per problemi relativi alla cultura.
                            val1 = Convert.ToDouble(ch[10].Substring(1));      // toglie l'apice iniziale, devo confrontare i valori
                            val2 = Convert.ToDouble(ch[11].Substring(1));

                            // arrotonda capacity e required
                            val1 = Math.Round(val1, Global.ROUNDDIGITS);
                            val2 = Math.Round(val2, Global.ROUNDDIGITS);

                            //ch[10] = ch[10].Replace(',', '.');  // capacity converte virgola decimale in punto
                            //ch[11] = ch[11].Replace(',', '.');  // required converte virgola decimale in punto
                            if (ch[12] == "'")
                                ch[12] += $"{planbu[simula.Next(3)]}";   // plan_bu
                            if (ch[13] == "'")
                                ch[13] += $"{flaghr[simula.Next(2)]}";   // flag_hr

                            // vengono tenuti in considerazione soltanto i record per i quali capacity && required non sono entrambi a zero
                            //if ((Math.Abs(val1) < Global.EPSILON) && (Math.Abs(val2) < Global.EPSILON)) continue;
                            // in questa invece scarto tutti i record che hanno required == 0
                            if (Math.Abs(val2) < Global.EPSILON) continue;

                            var str1 = $"{val1}";
                            var str2 = $"{val2}";
                            str1 = str1.Replace(',', '.');  // capacity converte virgola decimale in punto
                            str2 = str2.Replace(',', '.');  // required converte virgola decimale in punto

                            //newline = $"({ch[1]}',{ch[2]}',{ch[3]}',{ch[4]}',{ch[5]}',{ch[6]}','0','0','0',{ch[7]}',{ch[8]}',{ch[9]}',{ch[10]}',{ch[11]}',{ch[12]}',{ch[13]}',{ch[14]}";
                            newline = $"({ch[1]}',{ch[2]}',{ch[3]}',{ch[4]}',{ch[5]}',{ch[6]}','0','0','0',{ch[7]}',{ch[8]}',{ch[9]}','{str1}','{str2}',{ch[12]}',{ch[13]}',{ch[14]}";

                            //++count;
                            if (++count < 1000)
                                newline += ",";
                            sqlcmd = $"{sqlcmd}\n{newline}";
                            //-- questa vale solo per il nuovo database acquisito direttamente da board 13.12.2017

                            // solo per l'ultima linea non mette la virgola
                            //if (++count < 1000)
                            //    line += ",";
                            //sqlcmd = $"{sqlcmd}\n{line}";
                        } while (count < 1000);
                        //} while (count < 1);
                        externalcount += count;
                        db.ExecuteSqlCommand(sqlcmd);
                        Console.Write($"{externalcount} - {DateTime.Now}\n");

                        //var idx = sqlcmd.LastIndexOf(",");
                        //if (idx != -1)
                        //{
                        //    sqlcmd = sqlcmd.Remove(idx, 1);
                        //}

                        //if (externalcount>=26000)
                        //    break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        private static void VerifyDataCongruence()
        {
            try
            {
                Console.WriteLine($"VerifyDataCongruence INPUT: {DateTime.Now:dd.MM.yyyy-HH:mm:ss.fff}");
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
                                    var wte = lbwp.ElementAt(0).Week;

                                    // estrae dalla lista sortata soltanto i record relativi alla 
                                    // week che deve essere elaborata. Questa lista e' gia' ordinata per 
                                    // priorita' decrescente (crescente in senso numerico).
                                    var toelaborate = lbwp.Where(r => r.Week == wte).Select(r => r).ToList();

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
                                        // se la capacity e' a 0, ne imposta una ricavata random fra 50 e 600
                                        // confronto cosi' perche' e' un double
                                        //if (Math.Abs(ll[0]) < Global.EPSILON)
                                        //{
                                        //    // i = plan_bu, j = flag_hr, g = Capacity
                                        //    var newcapacity = rnd.Next(50, 600);
                                        //    //Console.WriteLine($"newcapacity = {newcapacity}");
                                        //    (from rec in Dbq.LoadLevellingTable
                                        //     where rec.i == p && rec.j == f && rec.PRODUCTION_CATEGORY == pc
                                        //     select rec).TableToList().ForEach(r => r.g = newcapacity);

                                        //    //(from rec in Dbq.LoadLevellingTable
                                        //    //    where rec.i == p && rec.j == f && rec.PRODUCTION_CATEGORY == pc
                                        //    //    select rec).UpdateFromQuery(rec => new LoadLevelling{g = newcapacity});

                                        //}
                                    }
                                    // rimuove da sortedtable i records appena elaborati
                                    lbwp.RemoveAll(r => r.Week == wte);

                                } while (lbwp.Count > 0);
                            }
                            //foreach (var rc in lbpc)
                            //{
                            //    Console.WriteLine($"planbu = {rc.PLAN_BU}, flaghr ={rc.FLAG_HR}, prodcatecory = {rc.PRODUCTION_CATEGORY}, week = {rc.Week}, priority = {rc.Priority}");
                            //}
                            //Console.ReadKey();
                        }
                    }
                }
                // corregge tutti i record che hanno Required = 0
                //var newreq = new Random();
                //Console.WriteLine($"START correzione valori di required == 0 : {DateTime.Now:dd.MM.yyyy-HH:mm:ss.fff}");

                //(from rec in Dbq.LoadLevellingTable where Math.Abs(rec.h) < Global.EPSILON select rec).TableToList().ForEach(r => r.h = newreq.Next(24,300));
                //(from rec in Dbq.LoadLevellingTable where rec.h < 0.1 select rec).UpdateFromQuery(rec => new LoadLevelling {h = newreq.Next(24, 300)});
                //Dbq.Save();

                //Console.WriteLine($"END correzione valori di required == 0 : {DateTime.Now:dd.MM.yyyy-HH:mm:ss.fff}");
                Console.WriteLine($"VerifyDataCongruence OUTPUT: {DateTime.Now:dd.MM.yyyy-HH:mm:ss.fff}");

            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                Exception raise = dbEx;
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}",
                                                       validationErrors.Entry.Entity.ToString(),
                                                       validationError.ErrorMessage);
                        Console.WriteLine(message);
                        // raise a new exception nesting
                        // the current instance as InnerException
                        raise = new InvalidOperationException(message, raise);
                    }
                }
                throw raise;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
