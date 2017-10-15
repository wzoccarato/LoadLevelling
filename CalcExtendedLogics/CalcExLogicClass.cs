using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using LoadL.DataLayer.DbTables;
using static LoadL.CalcExtendedLogics.Global;

namespace LoadL.CalcExtendedLogics
{
    public class CalcExLogicClass
    {
        private IList<LlWorkRecord> _loadLevelling;
        private IList<Schema> _schema;
        private IList<string>_plan_bu;
        private IList<string> _flag_hr;
        private IList<LlWorkRecord> _singleRun;

        #region ctor

        public CalcExLogicClass()
        {
            _loadLevelling = new List<LlWorkRecord>();
            _schema = new List<Schema>();
            //_plan_bu = new List<LlWorkRecord>();

            _plan_bu = new List<string>();
            _flag_hr = new List<string>();
            _singleRun = new List<LlWorkRecord>();
            // TODO inizializzazioni qui
        }
        #endregion

        #region metodi pubblici
        public bool Execute(DataSet dataset, string requiredoperation, string targetdatatablename)
        {
            try
            {
                switch (requiredoperation)
                {
                    case "LoadL":
                        var targetdt = dataset.Tables[targetdatatablename];
                        var schemadt = dataset.Tables["Schema"];    // il nome della tabella bisognera' passarlo in argomento

                        Initialize(targetdt, schemadt);

                        //var produzioni = from rec in loadLevelling group rec by rec.PLAN_BU into g orderby g.Count() descending select g.Distinct();
                        //_plan_bu = (IList<LlWorkRecord>)_loadLevelling.GroupBy(r => r.PLAN_BU).OrderByDescending(g => g.Count()).Select(f => f.Distinct()).ToList();

                        //var pbu = from r in _loadLevelling
                        //          group r by r.PLAN_BU
                        //          into g
                        //          orderby g.Count() descending, g.Key
                        //          select new
                        //                 {
                        //                     planbu = g.Key,
                        //                     count = g.Count()
                        //                 };

                        // PLAN_BU
                        //var pbu =_loadLevelling.GroupBy(r => r.PLAN_BU).OrderByDescending(g => g.Count()).Select(l => new {planbu = l.Key, count = l.Count()});
                        _plan_bu = _loadLevelling.GroupBy(r => r.PLAN_BU).OrderByDescending(g => g.Count()).Select(l => l.Key).ToList();
                        
                        foreach (var rec in _plan_bu)
                        {
                            //Console.WriteLine($"planbu = {rec.planbu}, count = {rec.count}");
                            Console.WriteLine($"planbu = {rec}");

                            // FLAG_HR
                            //var flag_hr = _loadLevelling.Where(r => r.PLAN_BU == rec).GroupBy(g => g.FLAG_HR).OrderByDescending(c => c.Count()).Select(l => new {flag_hr = l.Key, count = l.Count()});
                            _flag_hr = _loadLevelling.Where(r => r.PLAN_BU == rec).GroupBy(g => g.FLAG_HR).OrderByDescending(c => c.Count()).Select(l => l.Key).ToList();

                            foreach (var fhr in _flag_hr)
                            {
                                //Console.WriteLine($"flaghr = {fhr.flag_hr}, count = {fhr.count}");
                                Console.WriteLine($"flaghr = {fhr}");

                                // PRODUCTION_CATEGORY
                                var prod_category = _loadLevelling.Where(r => r.PLAN_BU == rec && r.FLAG_HR == fhr).GroupBy(g => g.PRODUCTION_CATEGORY).Select(l => l.Key).ToList();

                                Console .WriteLine("Premere un tasto...");
                                Console.ReadKey();

                                foreach (var pc in prod_category)
                                {
                                    var ll = _loadLevelling.Where(r => r.PLAN_BU == rec && r.FLAG_HR == fhr && r.PRODUCTION_CATEGORY == pc).Select(rr => rr).ToList();

                                    foreach (var rc in ll)
                                    {
                                        Console.WriteLine($"planbu = {rc.PLAN_BU}, flaghr ={rc.FLAG_HR}, prodcatecory = {rc.PRODUCTION_CATEGORY}");
                                    }

                                    // ordina per WEEK_PLAN e poi per Piority
                                    //var priority = ll.OrderBy(g => g.).OrderBy(h => h.WEEK_PLAN).ToList();

                                    //foreach (var p in priority)
                                    //{
                                    //    Console.WriteLine($"prodcat = {p.PRODUCTION_CATEGORY}, week_plan = {p.WEEK_PLAN}, priority = {p.Priority}");
                                    //}

                                }

                            }
                        }


                        break;
                    default:
                        break;
                }
                return true; 
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        #endregion

        #region metodi privati

        /// <summary>
        /// inizializza gli oggetti interni, acquisisce gli heading
        /// assegnati dall'utente, e li assegna alla tabella ch deve essere ritornata
        /// alla fine della procedura
        /// </summary>
        /// <param name="loadlevellingdt">
        /// datatable contenente la tabella LoadLevelling acquisita dal database
        /// </param>
        /// <param name="schemadt">
        /// datatable contenente la tabella Schema acquisita dal database
        /// </param>
        private void Initialize(DataTable loadlevellingdt, DataTable schemadt)
        {
            _schema = ConvertDataTable<Schema>(schemadt);

            // acquisisce i valori degli heading dalla tabella schema
            var f1 = _schema.Where(x => x.BlockId == Alias[LlAlias.F1]).Select(r => r.Heading).First();
            var f2 = _schema.Where(x => x.BlockId == Alias[LlAlias.F2]).Select(r => r.Heading).First();
            var f3 = _schema.Where(x => x.BlockId == Alias[LlAlias.F3]).Select(r => r.Heading).First();
            var ahead = _schema.Where(x => x.BlockId == Alias[LlAlias.Ahead]).Select(r => r.Heading).First();
            var late = _schema.Where(x => x.BlockId == Alias[LlAlias.Late]).Select(r => r.Heading).First();
            var priority = _schema.Where(x => x.BlockId == Alias[LlAlias.Priority]).Select(r => r.Heading).First();
            var capacity = _schema.Where(x => x.BlockId == Alias[LlAlias.Capacity]).Select(r => r.Heading).First();
            var required = _schema.Where(x => x.BlockId == Alias[LlAlias.Required]).Select(r => r.Heading).First();
            var planBu = _schema.Where(x => x.BlockId == Alias[LlAlias.PlanBu]).Select(r => r.Heading).First();
            var flagHr = _schema.Where(x => x.BlockId == Alias[LlAlias.FlagHr]).Select(r => r.Heading).First();
            var allocated = _schema.Where(x => x.BlockId == Alias[LlAlias.Allocated]).Select(r => r.Heading).First();

            // questo trascodifica dagli heding della tabella passata in argomento, agli 
            // heading utilizzati internamente, per semplicita' di gestione
            Dictionary<string, string> remap = new Dictionary<string, string>()
                                               {
                                                   {loadlevellingdt.Columns[index["a"]].ColumnName, "F1"},
                                                   {loadlevellingdt.Columns[index["b"]].ColumnName, "F2"},
                                                   {loadlevellingdt.Columns[index["c"]].ColumnName, "F3"},
                                                   {loadlevellingdt.Columns[index["d"]].ColumnName, "Ahead"},
                                                   {loadlevellingdt.Columns[index["e"]].ColumnName, "Late"},
                                                   {loadlevellingdt.Columns[index["f"]].ColumnName, "Priority"},
                                                   {loadlevellingdt.Columns[index["g"]].ColumnName, "Capacity"},
                                                   {loadlevellingdt.Columns[index["h"]].ColumnName, "Required"},
                                                   {loadlevellingdt.Columns[index["i"]].ColumnName, "PLAN_BU"},
                                                   {loadlevellingdt.Columns[index["j"]].ColumnName, "FLAG_HR"},
                                                   {loadlevellingdt.Columns[index["k"]].ColumnName, "Allocated"}
                                               };

            // assegna gli heading, come definiti sopra
            loadlevellingdt.Columns[index["a"]].ColumnName = remap[loadlevellingdt.Columns[index["a"]].ColumnName];
            loadlevellingdt.Columns[index["b"]].ColumnName = remap[loadlevellingdt.Columns[index["b"]].ColumnName];
            loadlevellingdt.Columns[index["c"]].ColumnName = remap[loadlevellingdt.Columns[index["c"]].ColumnName];
            loadlevellingdt.Columns[index["d"]].ColumnName = remap[loadlevellingdt.Columns[index["d"]].ColumnName];
            loadlevellingdt.Columns[index["e"]].ColumnName = remap[loadlevellingdt.Columns[index["e"]].ColumnName];
            loadlevellingdt.Columns[index["f"]].ColumnName = remap[loadlevellingdt.Columns[index["f"]].ColumnName];
            loadlevellingdt.Columns[index["g"]].ColumnName = remap[loadlevellingdt.Columns[index["g"]].ColumnName];
            loadlevellingdt.Columns[index["h"]].ColumnName = remap[loadlevellingdt.Columns[index["h"]].ColumnName];
            loadlevellingdt.Columns[index["i"]].ColumnName = remap[loadlevellingdt.Columns[index["i"]].ColumnName];
            loadlevellingdt.Columns[index["j"]].ColumnName = remap[loadlevellingdt.Columns[index["j"]].ColumnName];
            loadlevellingdt.Columns[index["k"]].ColumnName = remap[loadlevellingdt.Columns[index["k"]].ColumnName];

            // converte la tabella in IList. Questa e' la copia di lavoro interna 
            // della tabella
            _loadLevelling = ConvertDataTable<LlWorkRecord>(loadlevellingdt);

            // assegna alla tabella originale gli heading definitivi,
            // quelli ricavati dalla tabella Schema
            loadlevellingdt.Columns[index["a"]].ColumnName = f1;
            loadlevellingdt.Columns[index["b"]].ColumnName = f2;
            loadlevellingdt.Columns[index["c"]].ColumnName = f3;
            loadlevellingdt.Columns[index["d"]].ColumnName = ahead;
            loadlevellingdt.Columns[index["e"]].ColumnName = late;
            loadlevellingdt.Columns[index["f"]].ColumnName = priority;
            loadlevellingdt.Columns[index["g"]].ColumnName = capacity;
            loadlevellingdt.Columns[index["h"]].ColumnName = required;
            loadlevellingdt.Columns[index["i"]].ColumnName = planBu;
            loadlevellingdt.Columns[index["j"]].ColumnName = flagHr;
            loadlevellingdt.Columns[index["k"]].ColumnName = allocated;
        }

        /// <summary>
        /// Converte l'oggetto DataTable passato in argomento 
        /// in una collection di cui ritorna l'interfacia IList.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        /// <returns></returns>
        private IList<T> ConvertDataTable<T>(DataTable dt)
        {
            IList<T> data = new Collection<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }

        /// <summary>
        /// utilizza Reflection per popolare le proprieta'
        /// dell'oggetto T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <returns></returns>
        private T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                }
            }
            return obj;
        }
        #endregion
    }
}
