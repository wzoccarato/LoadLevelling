using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using LoadL.DataLayer.DbTables;
using LoadL.Infrastructure;
using LoadL.Infrastructure.Abstract;
using static LoadL.CalcExtendedLogics.Helper;


namespace LoadL.CalcExtendedLogics
{
    public class CalcExLogicClass
    {
        //private IList<LoadLevellingWork> _loadLevelling;
        private IList<Schema> _schema;                  // lista contenente la conversione della DataTable Schema
        private IEnumerable<string>_plan_bu;            // lista contenente tutti i PLAN_BU distinct
        private IEnumerable<string> _flag_hr;           // lista contenente tutti i FLAG_HR distinct
        private readonly ElementsList _waitList;        // contiene tutti i record relativi alle week in attesa per l'elaborazione
        private readonly ElementsList _sortedweeklist;  // contiene la lista sortata per week e per priorita' dei record filtrati per plan_bu,flag_hr e production?category

        private readonly ILoadLevelling _iloadl;

        //private int _totcount;

        #region ctor

        public CalcExLogicClass()
        {
            // qui le inizializzazioni

            //_totcount = 0;

            //_loadLevelling = new List<LoadLevellingWork>();
            _schema = new List<Schema>();
            //_plan_bu = new List<LoadLevellingWork>();

            _plan_bu = new List<string>();
            _flag_hr = new List<string>();
            _waitList = new ElementsList();
            _sortedweeklist = new ElementsList();
            

            _iloadl = new LlWrapperClass(new List<LoadLevellingWork>());

            // per il momento non installo AutoMapper in questo progetto
            //Mapper.Initialize(cfg => cfg.CreateMap<LoadLevelling, LoadLevellingWork>()
            //                            .ForMember(dest => dest.F1, opt => opt.MapFrom(src => src.a))
            //                            .ForMember(dest => dest.F2, opt => opt.MapFrom(src => src.b))
            //                            .ForMember(dest => dest.F3, opt => opt.MapFrom(src => src.c))
            //                            .ForMember(dest => dest.Ahead, opt => opt.MapFrom(src => src.d))
            //                            .ForMember(dest => dest.Late, opt => opt.MapFrom(src => src.e))
            //                            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.f))
            //                            .ForMember(dest => dest.Capacity, opt => opt.MapFrom(src => src.g))
            //                            .ForMember(dest => dest.Required, opt => opt.MapFrom(src => src.h))
            //                            .ForMember(dest => dest.PLAN_BU, opt => opt.MapFrom(src => src.i))
            //                            .ForMember(dest => dest.FLAG_HR, opt => opt.MapFrom(src => src.j))
            //                            .ForMember(dest => dest.Allocated, opt => opt.MapFrom(src => src.k)));

        }
        #endregion

        #region metodi pubblici
        public bool Execute(DataSet dataset, string requiredoperation, string targetdatatablename)
        {
            var fname = MethodBase.GetCurrentMethod().DeclaringType?.Name + "." + MethodBase.GetCurrentMethod().Name;
            try
            {
                switch (requiredoperation)
                {
                    case "LoadL":
                        var targetdt = dataset.Tables[targetdatatablename];
                        var schemadt = dataset.Tables["Schema"];    // il nome della tabella bisognera' passarlo in argomento

                        Initialize(targetdt, schemadt);

                        OptimizeWorkload();

                        break;
                    default:
                        break;
                }
                return true; 
            }
            catch (TraceException e)
            {
                Console.WriteLine(e.TraceMessage);
                // throw
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //throw new TraceException(fname, e.Message);
                return false;
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
            var fname = MethodBase.GetCurrentMethod().DeclaringType?.Name + "." + MethodBase.GetCurrentMethod().Name;

            try
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

                // questo trascodifica dagli heading della tabella passata in argomento, agli 
                // heading utilizzati internamente, per semplicita' di gestione e anche 
                // leggibilit' del codice
                Dictionary<string, string> remap = new Dictionary<string, string>()
                                                   {
                                                       {loadlevellingdt.Columns[Index["a"]].ColumnName, "F1"},
                                                       {loadlevellingdt.Columns[Index["b"]].ColumnName, "F2"},
                                                       {loadlevellingdt.Columns[Index["c"]].ColumnName, "F3"},
                                                       {loadlevellingdt.Columns[Index["d"]].ColumnName, "Ahead"},
                                                       {loadlevellingdt.Columns[Index["e"]].ColumnName, "Late"},
                                                       {loadlevellingdt.Columns[Index["f"]].ColumnName, "Priority"},
                                                       {loadlevellingdt.Columns[Index["g"]].ColumnName, "Capacity"},
                                                       {loadlevellingdt.Columns[Index["h"]].ColumnName, "Required"},
                                                       {loadlevellingdt.Columns[Index["i"]].ColumnName, "PLAN_BU"},
                                                       {loadlevellingdt.Columns[Index["j"]].ColumnName, "FLAG_HR"},
                                                       {loadlevellingdt.Columns[Index["k"]].ColumnName, "Allocated"}
                                                   };

                // assegna gli heading, come definiti sopra
                loadlevellingdt.Columns[Index["a"]].ColumnName = remap[loadlevellingdt.Columns[Index["a"]].ColumnName];
                loadlevellingdt.Columns[Index["b"]].ColumnName = remap[loadlevellingdt.Columns[Index["b"]].ColumnName];
                loadlevellingdt.Columns[Index["c"]].ColumnName = remap[loadlevellingdt.Columns[Index["c"]].ColumnName];
                loadlevellingdt.Columns[Index["d"]].ColumnName = remap[loadlevellingdt.Columns[Index["d"]].ColumnName];
                loadlevellingdt.Columns[Index["e"]].ColumnName = remap[loadlevellingdt.Columns[Index["e"]].ColumnName];
                loadlevellingdt.Columns[Index["f"]].ColumnName = remap[loadlevellingdt.Columns[Index["f"]].ColumnName];
                loadlevellingdt.Columns[Index["g"]].ColumnName = remap[loadlevellingdt.Columns[Index["g"]].ColumnName];
                loadlevellingdt.Columns[Index["h"]].ColumnName = remap[loadlevellingdt.Columns[Index["h"]].ColumnName];
                loadlevellingdt.Columns[Index["i"]].ColumnName = remap[loadlevellingdt.Columns[Index["i"]].ColumnName];
                loadlevellingdt.Columns[Index["j"]].ColumnName = remap[loadlevellingdt.Columns[Index["j"]].ColumnName];
                loadlevellingdt.Columns[Index["k"]].ColumnName = remap[loadlevellingdt.Columns[Index["k"]].ColumnName];

                // converte la tabella in List. Questa e' la copia di lavoro interna 
                // della tabella
                _iloadl.LoadLevellingWorkTable = ConvertDataTable<LoadLevellingWork>(loadlevellingdt).ToList();
                //_loadLevelling = MapDataTable(loadlevellingdt);

                // assegna alla tabella originale gli heading definitivi,
                // quelli ricavati dalla tabella Schema
                loadlevellingdt.Columns[Index["a"]].ColumnName = f1;
                loadlevellingdt.Columns[Index["b"]].ColumnName = f2;
                loadlevellingdt.Columns[Index["c"]].ColumnName = f3;
                loadlevellingdt.Columns[Index["d"]].ColumnName = ahead;
                loadlevellingdt.Columns[Index["e"]].ColumnName = late;
                loadlevellingdt.Columns[Index["f"]].ColumnName = priority;
                loadlevellingdt.Columns[Index["g"]].ColumnName = capacity;
                loadlevellingdt.Columns[Index["h"]].ColumnName = required;
                loadlevellingdt.Columns[Index["i"]].ColumnName = planBu;
                loadlevellingdt.Columns[Index["j"]].ColumnName = flagHr;
                loadlevellingdt.Columns[Index["k"]].ColumnName = allocated;
            }
            catch (TraceException e)
            {
                Console.WriteLine(e.TraceMessage);
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new TraceException(fname, e.Message);
            }

        }

        /// <summary>
        /// opera sulla lista _loadLevelling
        /// 1. Estrae una lista distinct di tutti i PLAN_BU (identificativo della produzione)
        /// 2. per il PLAN_BU estratto, estrae una lista distinct di tutti i FLAG_HR (Flag high rotation)
        /// 3. per il PLAN_BU estratto e il FLAG_HR estratto, estrae una lista distinct di tutti i PRODUCTION_CATEGORY (Categoria di produzione)
        /// 4. per il PLAN_BU estratto, il FLAG_HR estratto, per ciascuna PRODUCTION_CATEGORY estrae tutti i relativi record dalla liata LoadLevelling 
        /// 5. La lista cosi' ottenuta viene ordinata per WEEK_PLAN crescente e per Priority crescente (priorita' piu' alta a valore piu' basso)
        /// 6. di questa lista viene poi eleborata una settimana alla volta
        /// </summary>
        private void OptimizeWorkload()
        {
            var fname = MethodBase.GetCurrentMethod().DeclaringType?.Name + "." + MethodBase.GetCurrentMethod().Name;

            try
            {
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
                //_plan_bu = _loadLevelling.GroupBy(r => r.PLAN_BU).OrderByDescending(g => g.Count()).Select(l => l.Key).ToList();
                _plan_bu = _iloadl.GetDistinctPlanBu();

                foreach (var pbu in _plan_bu)
                {
                    //Console.WriteLine($"planbu = {rec.planbu}, count = {rec.count}");
                    Console.WriteLine($"planbu = {pbu}");

                    // FLAG_HR
                    //_flag_hr = _loadLevelling.Where(r => r.PLAN_BU == rec).GroupBy(g => g.FLAG_HR).OrderByDescending(c => c.Count()).Select(l => l.Key).ToList();
                    _flag_hr = _iloadl.GetDistinctFlagHr(pbu);


                    foreach (var fhr in _flag_hr)
                    {
                        //Console.WriteLine($"flaghr = {fhr.flag_hr}, count = {fhr.count}");
                        Console.WriteLine($"flaghr = {fhr}");

                        // PRODUCTION_CATEGORY
                        //var prodCategory = _loadLevelling.Where(r => r.PLAN_BU == rec && r.FLAG_HR == fhr).GroupBy(g => g.PRODUCTION_CATEGORY).Select(l => l.Key).ToList();
                        var prodCategory = _iloadl.GetDistinctProductionCategory(pbu, fhr);

                        foreach (var pc in prodCategory)
                        {
                            Console.WriteLine($"Production_category = {pc}");

                            // ordina per WEEK_PLAN e poi per Priority
                            //var sortedtable = ll.OrderBy(g => g.WEEK_PLAN).ThenBy(h => h.Priority).ToList();

                            // ATTENZIONE sortedweeklist E' DIVENTATO UN OGGETTO ELEMENTSLIST
                            _sortedweeklist.AddRange(_iloadl.ListByWeekAndPriority(pbu, fhr, pc));

                            Console.WriteLine($"Record da elaborare per {pc} = {_sortedweeklist.Count}");

                            if (_sortedweeklist.Count > 0)
                            {
                                do
                                {
                                    var wte = _sortedweeklist.GetFirst().WEEK_PLAN;

                                    // foreach(WEEK_PLAN).....
                                    // estrae dalla lista sortata soltanto i record relativi alla 
                                    // week che deve essere elaborata. Questa lista e' gia' ordinata per 
                                    // priorita' decrescente (la priorita' piu' alta ha il numero piu' basso).
                                    ElementsList weektoelab = new ElementsList();
                                    weektoelab.AddRange(_sortedweeklist.GetByWeek(wte));

                                    //var count = toelaborate.Count(s => s.Capacity > 0);
                                    //Console.WriteLine($"Count Capacity != 0: {count}");
                                    //_totcount += count;
                                    //Console.WriteLine($"TotCount = {_totcount}");
                                    //Console.WriteLine($"wte = {wte}");
                                    //Console.WriteLine("Premere un tasto...");
                                    //Console.ReadKey();

                                    //Console.WriteLine($"record della WEEK {wte} totali da elaborare: {toelaborate.Count}");
                                    //IList<LoadLevellingWork> toelaborate1 = sortedweeklist.Where(r => r.WEEK_PLAN == wte && r.Required>0).Select(r => r).ToList();
                                    //Console.WriteLine($"record della WEEK {wte} con \"Required\" diverso da 0: {toelaborate1.Count}");
                                    // elabora la week contenuta nella lista ordinata.
                                    ElaborateWeek(weektoelab,_sortedweeklist);
                                    // rimuove da sortedtable i records appena elaborati
                                    sortedweeklist.RemoveAll(r => r.WEEK_PLAN == wte);

                                    //Console.WriteLine($"prossimo = {toelaborate.ElementAt(0).WEEK_PLAN}");
                                    //Console.WriteLine("Premere un tasto...");
                                    //Console.ReadKey();
                                    //foreach (var rc in toelaborate)
                                    //{
                                    //    Console.WriteLine($"planbu = {rc.PLAN_BU}, flaghr ={rc.FLAG_HR}, prodcategory = {rc.PRODUCTION_CATEGORY}, WEEK_PLAN = {rc.WEEK_PLAN}, Priority = {rc.Priority}");
                                    //}
                                } while (sortedweeklist.Count > 0);
                            }
                        }
                    }
                }
            }
            catch (TraceException e)
            {
                Console.WriteLine(e.TraceMessage);
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new TraceException(fname, e.Message);
            }
        }

        // contiene la lista sortata per week e per priorita' dei record filtrati per plan_bu,flag_hr e production category

        /// <summary>
        /// Elabora la settimana i cui records son passati in argomento, nel parametro toelaborate
        /// </summary>
        /// <param name="toelaborate">Records relativi alla settimana da elaborare</param>
        /// <param name="sortedweeks"> contiene la lista sortata per week e per priorita' dei record filtrati per plan_bu,flag_hr e production_category</param>
        private void ElaborateWeek(IList<LoadLevellingWork> toelaborate,IList<LoadLevellingWork> sortedweeks)
        {
            var fname = MethodBase.GetCurrentMethod().DeclaringType?.Name + "." + MethodBase.GetCurrentMethod().Name;
            try
            {
                if (toelaborate.Count > 0)
                {
                    // verifica di consistenza. si puo' togliere una volta consolidata
                    var ll = toelaborate.Select(r => r.Capacity).Distinct().ToList();
                    if (ll.Count > 1)
                    {
                        string message =
                            $"Capacy non è la stessa per tutta la WEEK_PLAN. Week = {toelaborate[0].WEEK_PLAN}, Numero di elementi distict = {ll.Count}";
                        throw new TraceException(fname, message);
                    }
                    if (toelaborate[0].Capacity > 0)
                    {
                        // prima elabora le richieste in attesa, poi elabora
                        // la settimana che e' stata passata in argomento
                        
                        // a questa funzione passo soltanto il parametro Late, che fissa
                        // quante settimane indietro sia possibile elaborare
                        // tronca Late a intero
                        IList<LoadLevellingWork> waiting = GetWaitingRequests(toelaborate[0].WEEK_PLAN,(int)toelaborate[0].Late);
                        if (waiting.Count > 0)
                        {
                            foreach (var rec in waiting)
                            {
                                
                            }
                        }
                        // elabora le richieste relative ai record della settimana corrente
                        // Le richieste sono gia' state inderite con priorita' decrescente
                        ElabCurrentWeekRequests(toelaborate);
                        // se rimane capacità residua,
                        // anticipa la lavorazione delle settimane successive
                        if(toelaborate.Count>0)
                        {
                            if(toelaborate[0].Capacity > 0)     // la capacity è uguale per tutti i record
                            {
                                IList<LoadLevellingWork> noveup = GetAheadRequests(toelaborate[0].WEEK_PLAN, (int)toelaborate[0].Ahead,sortedweeks);
                                // TODO continua da qui
                            }
                        }
                    }
                    else
                    {
                        // solo per le richieste relative alla settimana in elaborazione
                        // prenota i records per l'elaborazione "Late"
                        // la richiesta viene accodata per l'elaborazione successiva
                        EnqueueRequests(toelaborate);
                    }
                }
            }
            catch (TraceException e)
            {
                Console.WriteLine(e.TraceMessage);
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new TraceException(fname, e.Message);
            }
        }

        /// <summary>
        /// Riceve in argomento il parametro WEEK_PLAN e il parametro Late
        /// relativo alla settimana in elaborazione.
        /// consulta la coda delle settimane in attesa di elaborazione,
        /// e ritorna una lista contenente soltanto quelle che 
        /// soddisfano al parametro late (numero di settimane elaborabili
        /// in ritardo) ricevuto in argomento. 
        /// </summary>
        /// <param name="late">numero di settimane delle quali e' possibile andare a ritroso</param>
        /// <param name="weekplan">identificativo della settimana correntemente in elaborazione</param>
        /// <returns></returns>
        private IList<LoadLevellingWork> GetWaitingRequests(string weekplan, int late)
        {
            List<string> lateweeks = GetLateWeeks(weekplan,late);
            // ricerca nella lista _waitlist 
            return (from el in lateweeks select _waitList.GetByWeek(el) into wklist where wklist.Count > 0 from r in wklist select r).ToList();
        }

        private IList<LoadLevellingWork> GetAheadRequests(string weekplan, int ahead,IList<LoadLevellingWork> sortedweeks)
        {
            List<string> aheadweeks = GetAheadWeeks(weekplan, ahead);
            return (from el in aheadweeks select sortedweeks.GetB  )

        }

        /// <summary>
        /// Accoda i record per l'elaborazione nelle settimane successive
        /// </summary>
        /// <param name="elements"></param>
        private void EnqueueRequests(IList<LoadLevellingWork> elements)
        {
            if (elements?.Count > 0)
            {
                _waitList.AddRange(elements);
            }
        }

        /// <summary>
        /// Sulla base del parametro late, e della settiman corrente,
        /// prepara una lista delle settimane "precedenti"
        /// che possone essere elaborate.
        /// Calcola anche il salto di anno, se necessario
        /// </summary>
        /// <param name="week"></param>
        /// <param name="late"></param>
        /// <returns></returns>
        List<string> GetLateWeeks(string week,int late)
        {
            var fname = MethodBase.GetCurrentMethod().DeclaringType?.Name + "." + MethodBase.GetCurrentMethod().Name;
            try
            {
                List<string> retval = new List<string>();
                if (week.Length != Global.WEEKPLAN_LENGTH)
                    throw new TraceException(fname, $"Errore nel parametro week: {week} non corrisponde alla lunghezza attesa {Global.WEEKPLAN_LENGTH}");
                if(late>0)
                {
                    var year = Convert.ToInt32(week.Substring(0, Global.YEAR_LENGTH));
                    var wk = Convert.ToInt32(week.Substring(Global.YEAR_LENGTH, Global.WEEK_LENGTH));
                    do
                    {
                        var newwk = wk - late;
                        if (newwk == 0)
                        {
                            --year;
                            newwk = GetWeeksInYear(year);
                        }
                        retval.Add(year.ToString() + newwk);
                        --late;
                    } while (late > 0);
                }
                return retval;
            }
            catch (TraceException e)
            {
                Console.WriteLine(e.TraceMessage);
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new TraceException(fname, e.Message);
            }
        }

        /// <summary>
        /// Sulla base del parametro ahead, e della settiman corrente,
        /// prepara una lista delle settimane "seguenti"
        /// che possone essere elaborate.
        /// Calcola anche il salto di anno, se necessario
        /// </summary>
        /// <param name="week"></param>
        /// <param name="ahead"></param>
        /// <returns></returns>
        private List<string> GetAheadWeeks(string week, int ahead)
        {
            var fname = MethodBase.GetCurrentMethod().DeclaringType?.Name + "." + MethodBase.GetCurrentMethod().Name;
            try
            {
                List<string> retval = new List<string>();
                if (week.Length != Global.WEEKPLAN_LENGTH)
                    throw new TraceException(fname, $"Errore nel parametro week: {week} non corrisponde alla lunghezza attesa {Global.WEEKPLAN_LENGTH}");
                if (ahead > 0)
                {
                    var year = Convert.ToInt32(week.Substring(0, Global.YEAR_LENGTH));
                    var wk = Convert.ToInt32(week.Substring(Global.YEAR_LENGTH, Global.WEEK_LENGTH));
                    int val = 1;
                    do
                    {
                        var current = GetWeeksInYear(year);
                        var newwk = wk + val;
                        if (newwk == current)
                        {
                            ++year;
                            newwk = 1;
                        }
                        retval.Add(year.ToString() + newwk);
                        ++val;
                    } while ( val <= ahead);
                }
                return retval;
            }
            catch (TraceException e)
            {
                Console.WriteLine(e.TraceMessage);
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new TraceException(fname, e.Message);
            }
        }

        private void ElabCurrentWeekRequests(IList<LoadLevellingWork>weekrecords)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Questa utilizza AutoMapper per rimappare la DataTable
        /// su una lista di oggetti LoadLevellingWork
        /// e' leggermente meno performante della funazione
        /// ConvertDataTable. Per cui nel programma utilizzo la seconda
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        //private IList<LoadLevellingWork> MapDataTable(DataTable dt)
        //{
        //    IList<LoadLevellingWork> data = new Collection<LoadLevellingWork>();

        //    foreach (DataRow row in dt.Rows)
        //    {
        //        LoadLevelling item = GetItem<LoadLevelling>(row);
        //        data.Add(Mapper.Map<LoadLevelling, LoadLevellingWork>(item));
        //    }
        //    return data;
        //}


        /// <summary>
        /// Converte l'oggetto DataTable passato in argomento 
        /// in una collection di cui ritorna l'interfaccia IList.
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
