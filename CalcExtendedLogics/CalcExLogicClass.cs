﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CalcExtendedLogics.CalcExtendedLogics;
using CalcExtendedLogics.DataLayer.DbTables;
using CalcExtendedLogics.Infrastructure;
using CalcExtendedLogics.Infrastructure.Abstract;

namespace CalcExtendedLogics
{
    public class CalcEXlogicClass
    {
        //private IList<LoadLevellingWork> _loadLevelling;
        private IList<Schema> _schema;                  // lista contenente la conversione della DataTable Schema
        private IEnumerable<string>_planBu;            // lista contenente tutti i PLAN_BU distinct
        private IEnumerable<string> _flagHr;           // lista contenente tutti i FLAG_HR distinct
        
        private readonly ILoadLevelling _iloadl;

        //private int _totcount;

        #region ctor

        public CalcEXlogicClass()
        {
            // qui le inizializzazioni

            //_totcount = 0;

            //_loadLevelling = new List<LoadLevellingWork>();
            _schema = new List<Schema>();
            //_plan_bu = new List<LoadLevellingWork>();

            _planBu = new List<string>();
            _flagHr = new List<string>();


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
        /// <summary>
        /// Entry point
        /// </summary>
        /// <param name="dataset"></param>
        /// <param name="requiredoperation"></param>
        /// <param name="targetdatatablename"></param>
        /// <returns></returns>
        public static bool Execute(DataSet dataset, string requiredoperation, string targetdatatablename)
        {
            var fname = MethodBase.GetCurrentMethod().DeclaringType?.Name + "." + MethodBase.GetCurrentMethod().Name;
            try
            {

                // questo mi serve per poter chiamare medodi non statici dall;interno del
                // metodo statico necessario per la compatibilita' con Board
                CalcEXlogicClass cexlc = new CalcEXlogicClass();
                switch (requiredoperation)
                {
                    case "loadl":
                        var targetdt = dataset.Tables[targetdatatablename];
                        var schemadt = dataset.Tables["Schema"];    // il nome della tabella bisognera' passarlo in argomento

#if LOCALTEST
#else
                        // crea insert statements per T-SQL dal dataset ricevuto in input
                        using (FileStream fs = new FileStream("c:\\temp\\SDGConsulting\\LlInsertStatements.txt",
                            FileMode.Create,
                            FileAccess.ReadWrite,
                            FileShare.ReadWrite))
                        {
                            Helper.ConvertTableToInsertSatements(targetdt, fs);
                        }

                        // crea insert statements per T-SQL dal dataset ricevuto in input
                        using (FileStream fs = new FileStream("c:\\temp\\SDGConsulting\\SchemaInsertStatements.txt",
                            FileMode.Create,
                            FileAccess.ReadWrite,
                            FileShare.ReadWrite))
                        {
                            Helper.ConvertTableToInsertSatements(schemadt, fs);
                        }
#endif


                        cexlc.Initialize(targetdt, schemadt);

                        var allocatedElements = cexlc.OptimizeWorkload();

                        //var allocated = allocatedElements.GetList().Where(r => r.ID == null).Select(r => r).TableToList();

                        targetdt.Rows.Clear();

                        foreach (var el in allocatedElements.GetList())
                        {

                            // aggiorna il dataset con i nuovi elementi
                            targetdt.Rows.Add(null,
                                el.PRODUCTION_CATEGORY,
                                el.IND_SEASONAL_STATUS,
                                el.TCH_WEEK,
                                el.PLANNING_LEVEL,
                                el.EVENT,
                                el.Week,
                                el.F1,el.F2,el.F3,
                                el.Ahead,
                                el.Late,
                                el.Priority,
                                el.Capacity,
                                el.Required,
                                el.PLAN_BU,
                                el.FLAG_HR,
                                el.Allocated);
                        }
                        break;
                    default:
                        break;
                }
                return true; 
            }
            catch (TraceException e)
            {
                Helper.WriteMessage(e.TraceMessage);
                // throw
                return false;
            }
            catch (Exception e)
            {
                Helper.WriteMessage(e.Message);
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
                // TODO: da togliere non appena sistemata la questione dei nomi dei campi della tabella Schema
                // TODO: la funzione corretta è DataTableHelper.TableToList
                _schema = ConvertSchemaDataTable<Schema>(schemadt);

                // acquisisce i valori degli heading dalla tabella schema
#if LOCALTEST
                var f1 = _schema.Where(x => x.BlockId == Helper.Alias[LlAlias.F1]).Select(r => r.Heading).First();
                var f2 = _schema.Where(x => x.BlockId == Helper.Alias[LlAlias.F2]).Select(r => r.Heading).First();
                var f3 = _schema.Where(x => x.BlockId == Helper.Alias[LlAlias.F3]).Select(r => r.Heading).First();
#else
                var f1 = "F1";
                var f2 = "F2";
                var f3 = "F3";
#endif
                var ahead = _schema.Where(x => x.BlockId == Helper.Alias[LlAlias.Ahead]).Select(r => r.Heading).First();
                var late = _schema.Where(x => x.BlockId == Helper.Alias[LlAlias.Late]).Select(r => r.Heading).First();
                var priority = _schema.Where(x => x.BlockId == Helper.Alias[LlAlias.Priority]).Select(r => r.Heading).First();
                var capacity = _schema.Where(x => x.BlockId == Helper.Alias[LlAlias.Capacity]).Select(r => r.Heading).First();
                var required = _schema.Where(x => x.BlockId == Helper.Alias[LlAlias.Required]).Select(r => r.Heading).First();
                var planBu = _schema.Where(x => x.BlockId == Helper.Alias[LlAlias.PlanBu]).Select(r => r.Heading).First();
                var flagHr = _schema.Where(x => x.BlockId == Helper.Alias[LlAlias.FlagHr]).Select(r => r.Heading).First();
                var allocated = _schema.Where(x => x.BlockId == Helper.Alias[LlAlias.Allocated]).Select(r => r.Heading).First();

                // questo trascodifica dagli heading della tabella passata in argomento, agli 
                // heading utilizzati internamente, per semplicita' di gestione e anche 
                // leggibilita' del codice
#if LOCALTEST
                Dictionary<string, string> remap = new Dictionary<string, string>()
                                                   {
                                                       {loadlevellingdt.Columns[Helper.Index["a"]].ColumnName, "F1"},
                                                       {loadlevellingdt.Columns[Helper.Index["b"]].ColumnName, "F2"},
                                                       {loadlevellingdt.Columns[Helper.Index["c"]].ColumnName, "F3"},
                                                       {loadlevellingdt.Columns[Helper.Index["d"]].ColumnName, "Ahead"},
                                                       {loadlevellingdt.Columns[Helper.Index["e"]].ColumnName, "Late"},
                                                       {loadlevellingdt.Columns[Helper.Index["f"]].ColumnName, "Priority"},
                                                       {loadlevellingdt.Columns[Helper.Index["g"]].ColumnName, "Capacity"},
                                                       {loadlevellingdt.Columns[Helper.Index["h"]].ColumnName, "Required"},
                                                       {loadlevellingdt.Columns[Helper.Index["i"]].ColumnName, "PLAN_BU"},
                                                       {loadlevellingdt.Columns[Helper.Index["j"]].ColumnName, "FLAG_HR"},
                                                       {loadlevellingdt.Columns[Helper.Index["k"]].ColumnName, "Allocated"}
                                                   };
#else
                loadlevellingdt.Columns["h"].ColumnName = "k";
                loadlevellingdt.Columns["g"].ColumnName = "j";
                loadlevellingdt.Columns["f"].ColumnName = "i";
                loadlevellingdt.Columns["e"].ColumnName = "h";
                loadlevellingdt.Columns["d"].ColumnName = "g";
                loadlevellingdt.Columns["c"].ColumnName = "f";
                loadlevellingdt.Columns["b"].ColumnName = "e";
                loadlevellingdt.Columns["a"].ColumnName = "d";


                DataColumn col = loadlevellingdt.Columns .Add("a");
                col.SetOrdinal(Helper.Index["a"]);
                col = loadlevellingdt.Columns.Add("b");
                col.SetOrdinal(Helper.Index["b"]);
                col = loadlevellingdt.Columns.Add("c");
                col.SetOrdinal(Helper.Index["c"]);
                Dictionary<string, string> remap = new Dictionary<string, string>()
                {
                    {loadlevellingdt.Columns[Helper.Index["a"]].ColumnName, "F1"},
                    {loadlevellingdt.Columns[Helper.Index["b"]].ColumnName, "F2"},
                    {loadlevellingdt.Columns[Helper.Index["c"]].ColumnName, "F3"},
                    {loadlevellingdt.Columns[Helper.Index["d"]].ColumnName, "Ahead"},
                    {loadlevellingdt.Columns[Helper.Index["e"]].ColumnName, "Late"},
                    {loadlevellingdt.Columns[Helper.Index["f"]].ColumnName, "Priority"},
                    {loadlevellingdt.Columns[Helper.Index["g"]].ColumnName, "Capacity"},
                    {loadlevellingdt.Columns[Helper.Index["h"]].ColumnName, "Required"},
                    {loadlevellingdt.Columns[Helper.Index["i"]].ColumnName, "PLAN_BU"},
                    {loadlevellingdt.Columns[Helper.Index["j"]].ColumnName, "FLAG_HR"},
                    {loadlevellingdt.Columns[Helper.Index["k"]].ColumnName, "Allocated"}
                };

#endif

                // assegna gli heading, come definiti sopra
                loadlevellingdt.Columns[Helper.Index["a"]].ColumnName = remap[loadlevellingdt.Columns[Helper.Index["a"]].ColumnName];
                loadlevellingdt.Columns[Helper.Index["b"]].ColumnName = remap[loadlevellingdt.Columns[Helper.Index["b"]].ColumnName];
                loadlevellingdt.Columns[Helper.Index["c"]].ColumnName = remap[loadlevellingdt.Columns[Helper.Index["c"]].ColumnName];
                loadlevellingdt.Columns[Helper.Index["d"]].ColumnName = remap[loadlevellingdt.Columns[Helper.Index["d"]].ColumnName];
                loadlevellingdt.Columns[Helper.Index["e"]].ColumnName = remap[loadlevellingdt.Columns[Helper.Index["e"]].ColumnName];
                loadlevellingdt.Columns[Helper.Index["f"]].ColumnName = remap[loadlevellingdt.Columns[Helper.Index["f"]].ColumnName];
                loadlevellingdt.Columns[Helper.Index["g"]].ColumnName = remap[loadlevellingdt.Columns[Helper.Index["g"]].ColumnName];
                loadlevellingdt.Columns[Helper.Index["h"]].ColumnName = remap[loadlevellingdt.Columns[Helper.Index["h"]].ColumnName];
                loadlevellingdt.Columns[Helper.Index["i"]].ColumnName = remap[loadlevellingdt.Columns[Helper.Index["i"]].ColumnName];
                loadlevellingdt.Columns[Helper.Index["j"]].ColumnName = remap[loadlevellingdt.Columns[Helper.Index["j"]].ColumnName];
                loadlevellingdt.Columns[Helper.Index["k"]].ColumnName = remap[loadlevellingdt.Columns[Helper.Index["k"]].ColumnName];

                // converte la tabella in List. Questa e' la copia di lavoro interna 
                // della tabella
                //_iloadl.LoadLevellingWorkTable = ConvertDataTable<LoadLevellingWork>(loadlevellingdt).TableToList();
                _iloadl.LoadLevellingWorkTable = DataTableHelper.TableToList<LoadLevellingWork>(loadlevellingdt).ToList();
                //_loadLevelling = MapDataTable(loadlevellingdt);

                // assegna alla tabella originale gli heading definitivi,
                // quelli ricavati dalla tabella Schema 
                //(serve veramente? per il momento ripristino i valori originali)
                //loadlevellingdt.Columns[Helper.Index["a"]].ColumnName = f1;
                //loadlevellingdt.Columns[Helper.Index["b"]].ColumnName = f2;
                //loadlevellingdt.Columns[Helper.Index["c"]].ColumnName = f3;
                //loadlevellingdt.Columns[Helper.Index["d"]].ColumnName = ahead;
                //loadlevellingdt.Columns[Helper.Index["e"]].ColumnName = late;
                //loadlevellingdt.Columns[Helper.Index["f"]].ColumnName = priority;
                //loadlevellingdt.Columns[Helper.Index["g"]].ColumnName = capacity;
                //loadlevellingdt.Columns[Helper.Index["h"]].ColumnName = required;
                //loadlevellingdt.Columns[Helper.Index["i"]].ColumnName = planBu;
                //loadlevellingdt.Columns[Helper.Index["j"]].ColumnName = flagHr;
                //loadlevellingdt.Columns[Helper.Index["k"]].ColumnName = allocated;

                // ripristino gli header originali. vedere se invece utilizzare gli header
                // ricavati dalla tabella Schema, come nelle riche di codice che precedono.
                loadlevellingdt.Columns[Helper.Index["a"]].ColumnName = "a";
                loadlevellingdt.Columns[Helper.Index["b"]].ColumnName = "b";
                loadlevellingdt.Columns[Helper.Index["c"]].ColumnName = "c";
                loadlevellingdt.Columns[Helper.Index["d"]].ColumnName = "d";
                loadlevellingdt.Columns[Helper.Index["e"]].ColumnName = "e";
                loadlevellingdt.Columns[Helper.Index["f"]].ColumnName = "f";
                loadlevellingdt.Columns[Helper.Index["g"]].ColumnName = "g";
                loadlevellingdt.Columns[Helper.Index["h"]].ColumnName = "h";
                loadlevellingdt.Columns[Helper.Index["i"]].ColumnName = "i";
                loadlevellingdt.Columns[Helper.Index["j"]].ColumnName = "j";
                loadlevellingdt.Columns[Helper.Index["k"]].ColumnName = "k";
            }
            catch (TraceException e)
            {
                Helper.WriteMessage(e.TraceMessage);
                throw;
            }
            catch (Exception e)
            {
                Helper.WriteMessage(e.Message);
                throw new TraceException(fname, e.Message);
            }

        }

        /// <summary>
        /// opera sulla lista _loadLevelling
        /// 1. Estrae una lista distinct di tutti i PLAN_BU (identificativo della produzione)
        /// 2. per il PLAN_BU estratto, estrae una lista distinct di tutti i FLAG_HR (Flag high rotation)
        /// 3. per il PLAN_BU estratto e il FLAG_HR estratto, estrae una lista distinct di tutti i PRODUCTION_CATEGORY (Categoria di produzione)
        /// 4. per il PLAN_BU estratto, il FLAG_HR estratto, per ciascuna PRODUCTION_CATEGORY estrae tutti i relativi record dalla liata LoadLevelling 
        /// 5. La lista cosi' ottenuta viene ordinata per Week crescente e per Priority crescente (priorita' piu' alta a valore piu' basso)
        /// 6. di questa lista viene poi eleborata una settimana alla volta
        /// </summary>
        private ElementsList OptimizeWorkload()
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
                //_plan_bu = _loadLevelling.GroupBy(r => r.PLAN_BU).OrderByDescending(g => g.Count()).Select(l => l.Key).TableToList();
                _planBu = _iloadl.GetDistinctPlanBu();

                // sortedweek list contiene la lista sortata per week e per priorita'
                // dei record filtrati per plan_bu,flag_hr e production_category
                ElementsList sortedweeklist = new ElementsList();
                // contiene tutti i record relativi alle week in attesa per l'elaborazione
                // sempre tutto filtrato come sopra
                ElementsList waitList = new ElementsList();
                // contiene i record di richieste soddisfatte, cioe' 
                // quelli il cui parametro Required e' arrivato a 0
                // per ciacuna iterazione su planbu,
                ElementsList fulfilledList = new ElementsList();
                // questa invece contiene tutte le richieste soddisfatte, su piano globale
                ElementsList totalFulfilled = new ElementsList();


                foreach (var pbu in _planBu)
                {
                    //Helper.WriteMessage($"planbu = {rec.planbu}, count = {rec.count}");
                    Helper.WriteMessage($"planbu = {pbu}");

                    // FLAG_HR
                    //_flag_hr = _loadLevelling.Where(r => r.PLAN_BU == rec).GroupBy(g => g.FLAG_HR).OrderByDescending(c => c.Count()).Select(l => l.Key).TableToList();
                    _flagHr = _iloadl.GetDistinctFlagHr(pbu);

                    // azzera le liste di lavoro
                    sortedweeklist.Purge();
                    waitList.Purge();
                    fulfilledList.Purge();

                    foreach (var fhr in _flagHr)
                    {
                        //Helper.WriteMessage($"flaghr = {fhr.flag_hr}, count = {fhr.count}");
                        Helper.WriteMessage($"flaghr = {fhr}");

                        // PRODUCTION_CATEGORY
                        //var prodCategory = _loadLevelling.Where(r => r.PLAN_BU == rec && r.FLAG_HR == fhr).GroupBy(g => g.PRODUCTION_CATEGORY).Select(l => l.Key).TableToList();
                        var prodCategory = _iloadl.GetDistinctProductionCategory(pbu, fhr);

                        foreach (var pc in prodCategory)
                        {
                            Helper.WriteMessage($"Production_category = {pc}");

                            // ordina per Week e poi per Priority
                            //var sortedtable = ll.OrderBy(g => g.Week).ThenBy(h => h.Priority).TableToList();

                            // tutti gli elementi con plan_bu, flag_hr e production_category,
                            // sortati per week_plan e poi per priority, vengono aggiunti alla sortedweeklist
                            sortedweeklist.AddRange(_iloadl.ListByWeekAndPriority(pbu, fhr, pc));

                            waitList.Purge();   // ad ogni iterazione deve resettare la lista delle week in attesa di completamento

                            Helper.WriteMessage($"Record da elaborare per {pc} = {sortedweeklist.Count}");

                            if (sortedweeklist.Count > 0)
                            {
                                do
                                {
                                    var wte = sortedweeklist.GetFirst().Week;

                                    // foreach(Week).....
                                    // estrae dalla lista sortata soltanto i record relativi alla 
                                    // week che deve essere elaborata. Questa lista e' gia' ordinata per 
                                    // priorita' decrescente (la priorita' piu' alta ha il numero piu' basso).
                                    ElementsList weektoelab = new ElementsList();
                                    weektoelab.AddRange(sortedweeklist.GetByWeek(wte));

                                    // elabora la week contenuta nella lista ordinata weektoelab
                                    ElaborateWeek(weektoelab,sortedweeklist,waitList,fulfilledList);
                                    // rimuove da sortedlist i records appena elaborati
                                    sortedweeklist.RemoveByWeekPlan(wte);    
                                } while (sortedweeklist.Count > 0);
                            }
                        }
                    }
                    // qui la lista delle allocazioni e' completa
                    totalFulfilled.AddRange(fulfilledList.GetList());

                }
                return totalFulfilled;
            }
            catch (TraceException e)
            {
                Helper.WriteMessage(e.TraceMessage);
                throw;
            }
            catch (Exception e)
            {
                Helper.WriteMessage(e.Message);
                throw new TraceException(fname, e.Message);
            }
        }

        /// <summary>
        /// Elabora la settimana i cui records son passati in argomento, nel parametro toelaborate
        /// </summary>
        /// <param name="toelaborate">Records relativi alla settimana da elaborare</param>
        /// <param name="sortedweeks"> contiene la lista sortata per week e per priorita' dei record filtrati per plan_bu,flag_hr e production_category</param>
        /// <param name="waitlist"> contiene la lista sortata dei record delle settimane la cui elaborazione non e' ancora completata</param>
        /// <param name="appendedelements">contiene la lista dei record di cui è stata completata l'allocazione</param>
        private void ElaborateWeek(ElementsList toelaborate,ElementsList sortedweeks,ElementsList waitlist,ElementsList appendedelements)
        {
            var fname = MethodBase.GetCurrentMethod().DeclaringType?.Name + "." + MethodBase.GetCurrentMethod().Name;
            try
            {
                if (toelaborate.Count > 0)
                {
                    // verifica di consistenza. si puo' togliere una volta consolidata
                    if(!toelaborate.ValidateList())
                    {
                        string message =
                            $"Capacy non è la stessa per tutta la Week. Week = {toelaborate.GetFirst().Week}";
                        throw new TraceException(fname, message);
                    }
                    if (toelaborate.GetFirst().Capacity > 0) // la capacity deve essere uguale per tutti i record, per requisito
                    {
                        // prima elabora le richieste in attesa, poi elabora
                        // la settimana che e' stata passata in argomento
                        
                        // a questa funzione passo soltanto il parametro Late, che fissa
                        // quante settimane indietro sia possibile elaborare
                        // tronca Late a intero
                        IList<LoadLevellingWork> waiting = GetWaitingRequests(toelaborate.GetFirst().Week,(int)toelaborate.GetFirst().Late,waitlist);
                        if (waiting.Count > 0)
                        {
                            ElabWaitingRequests(waiting, toelaborate, appendedelements);
                        }
                        // elabora le richieste relative ai record della settimana corrente
                        // Le richieste sono gia' state inserite con priorita' decrescente
                        ElabCurrentWeekRequests(toelaborate,appendedelements);
                        // se rimane capacità residua,
                        // anticipa la lavorazione delle settimane successive
                        if(toelaborate.Count>0)
                        {
                            if(toelaborate.GetFirst().Capacity > 0)     // la capacity è uguale per tutti i record
                            {
                                IList<LoadLevellingWork> moveup = GetAheadRequests(toelaborate.GetFirst().Week, (int)toelaborate.GetFirst().Ahead,sortedweeks);
                                ElabAheadRequests(moveup, toelaborate, appendedelements);
                            }
                            else
                            {
                                EnqueueRequests(waitlist, toelaborate);
                            }
                        }
                    }
                    else
                    {
                        // solo per le richieste relative alla settimana in elaborazione
                        // prenota i records per l'elaborazione "Late"
                        // la richiesta viene accodata per l'elaborazione successiva
                        EnqueueRequests(waitlist, toelaborate);
                    }
                }
            }
            catch (TraceException e)
            {
                Helper.WriteMessage(e.TraceMessage);
                throw;
            }
            catch (Exception e)
            {
                Helper.WriteMessage(e.Message);
                throw new TraceException(fname, e.Message);
            }
        }


        private void EnqueueRequests(ElementsList waitList, ElementsList toelaborate)
        {
            if (waitList != null)
            {
                if (toelaborate != null)
                {
                    // TODO Verificare quella che segue
                    waitList.MergeElements(toelaborate);
                }
            }
        }

        /// <summary>
        /// Riceve in argomento il parametro Week e il parametro Late
        /// relativo alla settimana in elaborazione.
        /// consulta la coda delle settimane in attesa di elaborazione,
        /// e ritorna una lista contenente soltanto quelle che 
        /// soddisfano al parametro late (numero di settimane elaborabili
        /// in ritardo) ricevuto in argomento. 
        /// </summary>
        /// <param name="late">numero di settimane delle quali e' possibile andare a ritroso</param>
        /// <param name="weekplan">identificativo della settimana correntemente in elaborazione</param>
        /// <param name="waitlist">La lista completa delle settimane in attesa di elaborazione</param>
        /// <returns>Ritorna la lista delle richieste in attesa estratte</returns>
        private IList<LoadLevellingWork> GetWaitingRequests(string weekplan, int late, ElementsList waitlist)
        {
            List<string> lateweeks = GetLateWeeks(weekplan, late);
            return (from el in lateweeks select waitlist.GetByWeek(el) into wklist where wklist.Count > 0 from r in wklist select r).ToList();
        }

        /// <summary>
        /// Riceve in argomento il parametro Week e il parametro ahead
        /// relativo alla settimana in elaborazione.
        /// consulta la coda delle settimane da elaborare,
        /// e ritorna una lista contenente soltanto quelle che 
        /// soddisfano al parametro ahead (numero di settimane elaborabili
        /// in anticipoo) ricevuto in argomento
        /// </summary>
        /// <param name="ahead">numero di settimane elaborabili in anticipo, a partire da weekplan</param>
        /// <param name="weekplan">identificativo della settimana correntemente in elaborazione</param>
        /// <param name="sortedweeks">La lista completa delle settimane in attesa di elaborazione</param>
        /// <returns>Ritorna la lista delle richieste che possono essere elaborate in anticipo</returns>
        private IList<LoadLevellingWork> GetAheadRequests(string weekplan, int ahead,ElementsList sortedweeks)
        {
            List<string> aheadweeks = GetAheadWeeks(weekplan, ahead);
            return (from el in aheadweeks select sortedweeks.GetByWeek(el) into wklist where wklist.Count > 0 from r in wklist select r).ToList();
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
                        int newwk;
                        if (wk - late <= 0)
                        {
                            --year;
                            newwk = Helper.GetWeeksInYear(year) + wk - late;
                        }
                        else
                        {
                            newwk = wk - late;
                        }
                        retval.Add(year.ToString() + $"{newwk:D2}");
                        --late;
                    } while (late > 0);
                }
                return retval;
            }
            catch (TraceException e)
            {
                Helper.WriteMessage(e.TraceMessage);
                throw;
            }
            catch (Exception e)
            {
                Helper.WriteMessage(e.Message);
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
                // Test
                if (week.Length != Global.WEEKPLAN_LENGTH)
                    throw new TraceException(fname, $"Errore nel parametro week: {week} non corrisponde alla lunghezza attesa {Global.WEEKPLAN_LENGTH}");
                if (ahead > 0)
                {
                    var year = Convert.ToInt32(week.Substring(0, Global.YEAR_LENGTH));
                    var wk = Convert.ToInt32(week.Substring(Global.YEAR_LENGTH, Global.WEEK_LENGTH));
                    int val = 1;
                    do
                    {
                        int newwk;
                        var current = Helper.GetWeeksInYear(year);
                        if (wk + val >= current)
                        {
                            ++year;
                            newwk = wk + val - current + 1;
                        }
                        else
                        {
                            newwk = wk + val;
                        }
                        retval.Add(year.ToString() + $"{newwk:D2}");
                        ++val;
                    } while ( val <= ahead);
                }
                return retval;
            }
            catch (TraceException e)
            {
                Helper.WriteMessage(e.TraceMessage);
                throw;
            }
            catch (Exception e)
            {
                Helper.WriteMessage(e.Message);
                throw new TraceException(fname, e.Message);
            }
        }

        private void ElabCurrentWeekRequests(ElementsList weekrecords,ElementsList appendedrequests)
        {
            var fname = MethodBase.GetCurrentMethod().DeclaringType?.Name + "." + MethodBase.GetCurrentMethod().Name;
            try
            {
                using (ElementsList toappend = new ElementsList())
                {

                    List<LoadLevellingWork> llw = weekrecords.GetList();
                    //ElementsList tocomplete = new ElementsList();           // queesta lista e' temporanea
                    // la ricerca viene eseguita in ordine di priorità dei record che richiedono
                    // di assegnare un carico di lavoro alla settimana corrente, a partire da quello
                    // a priorità più elevata.
                    var todo = (from rec in llw
                                      group rec by rec.Priority
                                      into g
                                      orderby g.Key, g.Count()
                                      select new {priority = (int) g.Key, count = g.Count()}).ToList();
                    // Le richieste pendenti devono essere soddisfatte raggruppandole per priorità.
                    // A parità di priorità, le lavorazioni devono essere assegnare mantenendo
                    // la percentuale reciproca delle richieste
                    var initialcap = llw.First().Capacity; // e' requisito che tutte la Capacity sia uniforma per tutta la week
                    var cap = initialcap;
                    var allocated = 0.0;
                    foreach (var rec in todo)
                    {
                        initialcap -= allocated;
                        // disponibilita' totale richiesta
                        double totreq = llw.Where(r => (int) r.Priority == rec.priority).Sum(t => t.Required);

                        foreach (var el in llw.Where(r => (int) r.Priority == rec.priority))
                        {
                            if (cap > 0)
                            {
                                if (totreq < initialcap)
                                {
                                    // in questo caso tutto il richiesto viene allocato
                                    cap -= el.Required;
                                    allocated += el.Required;

                                    LoadLevellingWork newel = el.Clone();
                                    newel.TCH_WEEK = el.Week;
                                    newel.Allocated = Math.Round(el.Required, Global.ROUNDDIGITS);
                                    newel.Required = newel.Allocated; // se required == allocated allora la richiesta risulta soddisfatta 

                                    el.Required = 0; // la richiesta e' stata soddisfatta
                                    toappend.AddElement(newel);
                                    break; // passa all'elemento successivo nella lista
                                }
                                // in questo caso il richiesto deve essere allocato in 
                                // quantita' proporzionale a ciascuna richiesta
                                var toallocate = initialcap * el.Required / totreq;
                                if (toallocate > Global.EPSILON)
                                {
                                    //throw new TraceException(fname, $"Quantità da allocare inconsistente: {toallocate}");
                                    if (toallocate <= cap)
                                    {
                                        cap -= toallocate;
                                        el.Required -= toallocate;
                                        // Test
                                        if (el.Required < 0)
                                            throw new TraceException(fname, $"Required è inconsistente: {el.Required}");
                                        allocated += toallocate;

                                        LoadLevellingWork newel = el.Clone();
                                        newel.TCH_WEEK = el.Week;
                                        newel.Allocated = Math.Round(toallocate, Global.ROUNDDIGITS);
                                        newel.Required = Math.Round(el.Required,Global.ROUNDDIGITS);
                                        newel.Capacity = 0; // la capacity del nuovo elemento la impongo a 0 
                                        toappend.AddElement(newel);

                                        if (Math.Abs(el.Required) < Global.EPSILON)
                                        {
                                            el.Required = 0; // la richiesta e' stata soddisfatta
                                            break; // passa all'elemento successivo nella lista
                                        }
                                    }
                                    else
                                    {
                                        el.Required -= cap;
                                        // Test
                                        if (el.Required < 0)
                                            throw new TraceException(fname, $"Required è inconsistente: {el.Required}");
                                        allocated += cap;
                                        cap = 0;

                                        LoadLevellingWork newel = el.Clone();
                                        newel.TCH_WEEK = el.Week;
                                        newel.Allocated = Math.Round(el.Allocated,Global.ROUNDDIGITS);
                                        newel.Required = Math.Round(el.Required,Global.ROUNDDIGITS);
                                        newel.Capacity = 0; // la capacity del nuovo elemento la impongo a 0 
                                        toappend.AddElement(newel);

                                        if (Math.Abs(el.Required) < Global.EPSILON)
                                        {
                                            el.Required = 0; // la richiesta e' stata soddisfatta
                                            break; // passa all'elemento successivo nella lista
                                        }
                                    }
                                }
                            }
                        }
                        if (toappend.Count > 0)
                        {
                            appendedrequests.AddRange(toappend.GetList());
                            Helper.WriteMessage($"{fname} - Week: {toappend.GetFirst().Week} Allocati: {appendedrequests.Count}");
                            toappend.Purge();
                        }
                    }
                    // alla fine uniforma tutti i record della week alla medesima Capacity
                    // e alla medesima allocazione
                    llw.ForEach(l =>
                    {
                        l.Capacity = Math.Round(cap, Global.ROUNDDIGITS);
                        l.Allocated = Math.Round(allocated, Global.ROUNDDIGITS);
                    });
                }
            }
            catch (TraceException e)
            {
                Helper.WriteMessage(e.TraceMessage);
                throw;
            }
            catch (Exception e)
            {
                Helper.WriteMessage(e.Message);
                throw new TraceException(fname, e.Message);
            }
        }


        /// <summary>
        /// Elabora le richieste pendenti, relative alle settimane antecedenti alla settimana attuale -> parametro Late
        /// </summary>
        /// <param name="waiting"></param>
        /// <param name="weekrecords"></param>
        /// <param name="appendedrequests"></param>
        private void ElabWaitingRequests(IList<LoadLevellingWork> waiting, ElementsList weekrecords, ElementsList appendedrequests)
        {
            var fname = MethodBase.GetCurrentMethod().DeclaringType?.Name + "." + MethodBase.GetCurrentMethod().Name;
            try
            {
                using (ElementsList toappend = new ElementsList())
                {

                    List<LoadLevellingWork> llw = weekrecords.GetList();
                    // la ricerca viene eseguita in ordine di priorità dei record che richiedono
                    // di assegnare un carico di lavoro alla settimana corrente, a partire da quello
                    // a priorità più elevata.
                    var priorities = (from rec in waiting
                                      group rec by new { Week = rec.Week, rec.Priority }
                                      into g
                                      orderby g.Key.Week, g.Key.Priority, g.Count()
                                      select new { week = g.Key.Week, priority = (int)g.Key.Priority, count = g.Count() }).ToList();
                    // Le richieste pendenti devono essere soddisfatte raggruppandole per priorità.
                    // A parità di priorità, le lavorazioni devono essere assegnare mantenendo
                    // la percentuale reciproca delle richieste
                    var initialcap = llw.First().Capacity; // e' requisito che la Capacity sia uniforme per tutta la week
                    var cap = initialcap;
                    var allocated = 0.0;
                    // verificare che i record in attesa siano gia' ordinati per anzianita' e per Priority
                    foreach (var rec in priorities)
                    {
                        initialcap -= allocated;

                        var todo = (from r in waiting
                                    where r.Week == rec.week && 
                                    (int)r.Priority == rec.priority
                                    select r).ToList();
                        double totreq = waiting.Where(r => (int)r.Priority == rec.priority && r.Week == rec.week).Sum(t => t.Required);
                        foreach (var w in todo)
                        {
                            foreach (var el in llw)
                            {
                                if (cap > 0)
                                {
                                    if (totreq < initialcap)
                                    {
                                        // in questo caso tutto il richiesto viene allocato
                                        cap -= w.Required;
                                        allocated += w.Required;

                                        LoadLevellingWork newel = el.Clone();
                                        newel.TCH_WEEK = w.Week;
                                        newel.Allocated = Math.Round(w.Required, Global.ROUNDDIGITS);
                                        newel.Required = newel.Allocated; // se required == allocated allora la richiesta risulta soddisfatta 

                                        w.Required = 0; // la richiesta e' stata soddisfatta. prenoto l'elemento per la rimozione dalla lista
                                        toappend.AddElement(newel);
                                        break; // passa all'elemento successivo nella lista
                                    }
                                    // in questo caso il richiesto deve essere allocato in 
                                    // quantita' proporzionale a ciascuna richiesta
                                    var toallocate = initialcap * w.Required / totreq;
                                    if (toallocate > Global.EPSILON)
                                    {
                                        //throw new TraceException(fname, $"Quantità da allocare inconsistente: {toallocate}");
                                        if (toallocate <= cap)
                                        {
                                            cap -= toallocate;
                                            w.Required -= toallocate;
                                            // Test
                                            if (w.Required < 0)
                                                throw new TraceException(fname, $"Required è inconsistente: {el.Required}");
                                            allocated += toallocate;

                                            LoadLevellingWork newel = el.Clone();
                                            newel.TCH_WEEK = w.Week;
                                            newel.Allocated = Math.Round(toallocate, Global.ROUNDDIGITS);
                                            newel.Required = Math.Round(w.Required,Global.ROUNDDIGITS);
                                            newel.Capacity = 0; // la capacity del nuovo elemento la impongo a 0 
                                            toappend.AddElement(newel);

                                            if (Math.Abs(w.Required) < Global.EPSILON)
                                            {
                                                w.Required = 0; // la richiesta e' stata soddisfatta. prenoto l'elemento per la rimozione dalla lista
                                                break; // passa all'elemento successivo nella lista
                                            }
                                        }
                                        else
                                        {
                                            w.Required -= cap;
                                            // Test
                                            if (w.Required < 0)
                                                throw new TraceException(fname, $"Required è inconsistente: {el.Required}");
                                            allocated += cap;
                                            cap = 0;
                                                
                                            LoadLevellingWork newel = el.Clone();
                                            newel.TCH_WEEK = w.Week;

                                            newel.Allocated = Math.Round(el.Allocated, Global.ROUNDDIGITS);
                                            newel.Required = Math.Round(w.Required, Global.ROUNDDIGITS);

                                            newel.Capacity = 0; // la capacity del nuovo elemento la impongo a 0 
                                            toappend.AddElement(newel);

                                            if (Math.Abs(el.Required) < Global.EPSILON)
                                            {
                                                w.Required = 0; // la richiesta e' stata soddisfatta. prenoto l'elemento per la rimozione dalla lista
                                                break; // passa all'elemento successivo nella lista
                                            }
                                        }
                                    }
                                }
                            }
                            if (toappend.Count > 0)
                            {
                                appendedrequests.AddRange(toappend.GetList());
                                Helper.WriteMessage($"{fname} - Week: {toappend.GetFirst().Week} Allocati: {appendedrequests.Count}");
                                toappend.Purge();
                            }
                        }
                        // alla fine uniforma tutti i record della week alla medesima Capacity
                        // e alla medesima allocazione
                        llw.ForEach(l =>
                        {
                            l.Capacity = Math.Round(cap, Global.ROUNDDIGITS);
                            l.Allocated = Math.Round(allocated, Global.ROUNDDIGITS);
                        });
                        var todelete = todo.Where(w => Math.Abs(w.Required) < Global.EPSILON).Select(r => r).ToList();
                        foreach (var w in todelete)
                        {
                            waiting.Remove(w);
                        }
                    }
                }
            }
            catch (TraceException e)
            {
                Helper.WriteMessage(e.TraceMessage);
                throw;
            }
            catch (Exception e)
            {
                Helper.WriteMessage(e.Message);
                throw new TraceException(fname, e.Message);
            }
        }

        /// <summary>
        /// Esegue la elaborazione anticipata delle richieste relative alle settimane successive rispetto a quella attuale -> parametro Ahead
        /// </summary>
        /// <param name="moveuprequests">Lista contenente tutti i record che virtualmente possono essere elaborati, 
        /// relativi alle settimane successive rispetto a quella rappresentata da weekrecords</param>
        /// <param name="weekrecords">Contiene i record relativi alla settimana corrente</param>
        /// <param name="appendedrequests">Contiene i record relativi alle rischieste che sono state completate</param>
        private void ElabAheadRequests(IList<LoadLevellingWork> moveuprequests, ElementsList weekrecords, ElementsList appendedrequests)
        {
            var fname = MethodBase.GetCurrentMethod().DeclaringType?.Name + "." + MethodBase.GetCurrentMethod().Name;
            try
            {
                using (ElementsList toappend = new ElementsList())
                {
                    List<LoadLevellingWork> llw = weekrecords.GetList();
                    // la ricerca viene eseguita in ordine di priorità dei record che richiedono
                    // di assegnare un carico di lavoro alla settimana corrente, a partire da quello
                    // a priorità più elevata.
                    var priorities = (from rec in moveuprequests
                                      group rec by new { Week = rec.Week, rec.Priority }
                                      into g
                                      orderby g.Key.Week, g.Key.Priority, g.Count()
                                      select new { week = g.Key.Week, priority = (int)g.Key.Priority, count = g.Count() }).ToList();
                    // Le richieste devono essere soddisfatte raggruppandole per priorità.
                    // A parità di priorità, le lavorazioni devono essere assegnare mantenendo
                    // la percentuale reciproca delle richieste
                    var initialcap = llw.First().Capacity; // e' requisito che la Capacity sia uniforme per tutta la week
                    var cap = initialcap;
                    var allocated = 0.0;
                    foreach (var rec in priorities)
                    {
                        initialcap -= allocated;

                        var todo = (from r in moveuprequests
                                    where r.Week == rec.week &&
                                    (int)r.Priority == rec.priority
                                    select r).ToList();
                        double totreq = moveuprequests.Where(r => (int)r.Priority == rec.priority && r.Week == rec.week).Sum(t => t.Required);
                        foreach (var h in todo)
                        {
                            foreach (var el in llw)
                            {
                                if (cap > 0)
                                {
                                    if (totreq < initialcap)
                                    {
                                        // in questo caso tutto il richiesto viene allocato
                                        cap -= h.Required;
                                        allocated += h.Required;

                                        LoadLevellingWork newel = el.Clone();
                                        newel.TCH_WEEK = h.Week;
                                        newel.Allocated = Math.Round(h.Required, Global.ROUNDDIGITS);
                                        newel.Required = newel.Allocated; // se required == allocated allora la richiesta risulta soddisfatta 

                                        h.Required = 0; // la richiesta e' stata soddisfatta. prenoto l'elemento per la rimozione dalla lista
                                        toappend.AddElement(newel);
                                        break; // passa all'elemento successivo nella lista
                                    }
                                    // in questo caso il richiesto deve essere allocato in 
                                    // quantita' proporzionale a ciascuna richiesta
                                    var toallocate = initialcap * h.Required / totreq;
                                    if (toallocate > Global.EPSILON)
                                    {
                                        //throw new TraceException(fname, $"Quantità da allocare inconsistente: {toallocate}");
                                        if (toallocate <= cap)
                                        {
                                            cap -= toallocate;
                                            h.Required -= toallocate;
                                            // Test
                                            if (h.Required < 0)
                                                throw new TraceException(fname, $"Required è inconsistente: {el.Required}");

                                            LoadLevellingWork newel = el.Clone();
                                            newel.TCH_WEEK = h.Week;
                                            newel.Allocated = Math.Round(toallocate, Global.ROUNDDIGITS);
                                            newel.Required = Math.Round(h.Required,Global.ROUNDDIGITS);
                                            newel.Capacity = 0; // la capacity del nuovo elemento la impongo a 0 
                                            toappend.AddElement(newel);

                                            if (Math.Abs(h.Required) < Global.EPSILON)
                                            {
                                                h.Required = 0; // la richiesta e' stata soddisfatta. prenoto l'elemento per la rimozione dalla lista
                                                break; // passa all'elemento successivo nella lista
                                            }
                                        }
                                        else
                                        {
                                            h.Required -= cap;
                                            // Test
                                            if (h.Required < 0)
                                                throw new TraceException(fname, $"Required è inconsistente: {el.Required}");
                                            allocated += cap;
                                            cap = 0;

                                            LoadLevellingWork newel = el.Clone();
                                            newel.TCH_WEEK = h.Week;
                                            newel.Allocated = Math.Round(el.Allocated,Global.ROUNDDIGITS);
                                            newel.Required = Math.Round(h.Required,Global.ROUNDDIGITS);
                                            newel.Capacity = 0; // la capacity del nuovo elemento la impongo a 0 
                                            toappend.AddElement(newel);

                                            if (Math.Abs(el.Required) < Global.EPSILON)
                                            {
                                                h.Required = 0; // la richiesta e' stata soddisfatta. prenoto l'elemento per la rimozione dalla lista
                                                break; // passa all'elemento successivo nella lista
                                            }
                                        }
                                    }
                                }
                            }
                            if (toappend.Count > 0)
                            {
                                appendedrequests.AddRange(toappend.GetList());
                                Helper.WriteMessage($"{fname} - Week: {toappend.GetFirst().Week} Allocati: {appendedrequests.Count}");
                                toappend.Purge();
                            }
                            
                        }
                        // alla fine uniforma tutti i record della week alla medesima Capacity
                        // e alla medesima allocazione
                        llw.ForEach(l =>
                        {
                            l.Capacity = Math.Round(cap, Global.ROUNDDIGITS);
                            l.Allocated = Math.Round(allocated, Global.ROUNDDIGITS);
                        });
                        var todelete = todo.Where(w => Math.Abs(w.Required) < Global.EPSILON).Select(r => r).ToList();
                        foreach (var w in todelete)
                        {
                            moveuprequests.Remove(w);
                        }
                    }
                }
            }
            catch (TraceException e)
            {
                Helper.WriteMessage(e.TraceMessage);
                throw;
            }
            catch (Exception e)
            {
                Helper.WriteMessage(e.Message);
                throw new TraceException(fname, e.Message);
            }

        }

        /// <summary>
        /// Questa utilizza AutoMapper per rimappare la DataTable
        /// su una lista di oggetti LoadLevellingWork
        /// e' leggermente meno performante della funazione
        /// ConvertDataTable. Per cui nel programma utilizzo la seconda
        /// </summary>
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
        //private IList<T> ConvertDataTable<T>(DataTable dt) where T:new()
        //{
        //    IList<T> data = new Collection<T>();
        //    foreach (DataRow row in dt.Rows)
        //    {
        //        var item = GetItem<T>(row);
        //        data.Add(item);
        //    }
        //    return data;
        //}

        /// <summary>
        /// utilizza Reflection per popolare le proprieta'
        /// dell'oggetto T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <returns></returns>
        //private T GetItem<T>(DataRow dr) where T: new()
        //{
        //    Type temp = typeof(T);
        //    T item = new T();

        //    foreach (DataColumn column in dr.Table.Columns)
        //    {
        //        foreach (PropertyInfo pro in temp.GetProperties())
        //        {
        //            if (pro.Name != column.ColumnName) continue;
        //            //var value = dr[column.ColumnName] is DBNull ? string.Empty : dr[column.ColumnName].ToString();
        //            var value = dr[column.ColumnName];
        //            if (value is DBNull)
        //            {
        //                if (pro.PropertyType == typeof(double))
        //                {
        //                    pro.SetValue(item, 0,null);
        //                }
        //                else if (pro.PropertyType == typeof(int))
        //                {
        //                    pro.SetValue(item, 0, null);
        //                }
        //                else if (pro.PropertyType == typeof(string))
        //                {
        //                    pro.SetValue(item, string.Empty, null);
        //                }
        //                else
        //                {
        //                    pro.SetValue(item, null, null);
        //                }
        //            }
        //            else
        //            {
        //                pro.SetValue(item, value, null);
        //            }
        //            break;
        //        }
        //    }
        //    return item;
        //}

        /// <summary>
        /// Converte l'oggetto DataTable passato in argomento 
        /// in una collection di cui ritorna l'interfaccia IList.
        /// TODO: togliere questa non appena sistemata la questione relativa al nome dei campi nella tabella Schema
        /// TODO: la funzione corretta è DataTableHelper.TableToList
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        /// <returns></returns>
        private IList<T> ConvertSchemaDataTable<T>(DataTable dt)
        {
            IList<T> data = new Collection<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetSchemaItem<T>(row);
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
        private T GetSchemaItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
#if LOCALTEST
                    if (pro.Name == column.ColumnName)
                    {
                        pro.SetValue(obj, dr[column.ColumnName], null);
                        break;
                    }
#else
                    // in questo caso disaccoppia i nomi dei campi della
                    // tabella ricevuta da Board,dai nomi dei campi dell'oggetto
                    // definito internamente
                    if (Helper.SchemaMap[pro.Name] != column.ColumnName) continue;
                    var value = dr[column.ColumnName] is DBNull ? string.Empty : dr[column.ColumnName].ToString();
                    pro.SetValue(obj, value, null);
                    break;
#endif
                }
            }
            return obj;
        }

        #endregion
    }
}
