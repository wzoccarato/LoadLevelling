using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using LoadL.DataLayer.DbTables;
using static LoadL.CalcExtendedLogics.Global;

namespace LoadL.CalcExtendedLogics
{
    public class CalcExLogicClass
    {
        #region ctor


        public CalcExLogicClass()
        {
            // TODO inizializzazioni qui
        }
        #endregion
        public bool Execute(DataSet dataset, string requiredoperation, string targetdatatablename)
        {
            try
            {
                switch (requiredoperation)
                {
                    case "LoadL":
                        var targetdt = dataset.Tables[targetdatatablename];
                        var schemadt = dataset.Tables["Schema"];    // il nome della tabella bisognera' passarlo in argomento


                        var schema = ConvertDataTable<Schema>(schemadt);

                        var f1 = schema.Where(x => x.BlockId == Alias[LlAlias.F1]).Select(r => r.Heading).First();
                        var f2 = schema.Where(x => x.BlockId == Alias[LlAlias.F2]).Select(r => r.Heading).First();
                        var f3 = schema.Where(x => x.BlockId == Alias[LlAlias.F3]).Select(r => r.Heading).First();
                        var ahead = schema.Where(x => x.BlockId == Alias[LlAlias.Ahead]).Select(r => r.Heading).First();
                        var late = schema.Where(x => x.BlockId == Alias[LlAlias.Late]).Select(r => r.Heading).First();
                        var priority = schema.Where(x => x.BlockId == Alias[LlAlias.Priority]).Select(r => r.Heading).First();
                        var capacity = schema.Where(x => x.BlockId == Alias[LlAlias.Capacity]).Select(r => r.Heading).First();
                        var required = schema.Where(x => x.BlockId == Alias[LlAlias.Required]).Select(r => r.Heading).First();
                        var plan_bu = schema.Where(x => x.BlockId == Alias[LlAlias.PlanBu]).Select(r => r.Heading).First();
                        var flag_hr = schema.Where(x => x.BlockId == Alias[LlAlias.FlagHr]).Select(r => r.Heading).First();
                        var allocated = schema.Where(x => x.BlockId == Alias[LlAlias.Allocated]).Select(r => r.Heading).First();

                        Dictionary<string, string> remap = new Dictionary<string, string>()
                        {
                            {targetdt.Columns[index["a"]].ColumnName,"F1"},
                            {targetdt.Columns[index["b"]].ColumnName,"F2"},
                            {targetdt.Columns[index["c"]].ColumnName,"F3"},
                            {targetdt.Columns[index["d"]].ColumnName,"Ahead"},
                            {targetdt.Columns[index["e"]].ColumnName,"Late"},
                            {targetdt.Columns[index["f"]].ColumnName,"Priority"},
                            {targetdt.Columns[index["g"]].ColumnName,"Capacity"},
                            {targetdt.Columns[index["h"]].ColumnName,"Required"},
                            {targetdt.Columns[index["i"]].ColumnName,"PLAN_BU"},
                            {targetdt.Columns[index["j"]].ColumnName,"FLAG_HR"},
                            {targetdt.Columns[index["k"]].ColumnName,"Allocated"}
                        };
                        
                        targetdt.Columns[index["a"]].ColumnName = remap[targetdt.Columns[index["a"]].ColumnName];
                        targetdt.Columns[index["b"]].ColumnName = remap[targetdt.Columns[index["b"]].ColumnName];
                        targetdt.Columns[index["c"]].ColumnName = remap[targetdt.Columns[index["c"]].ColumnName];
                        targetdt.Columns[index["d"]].ColumnName = remap[targetdt.Columns[index["d"]].ColumnName];
                        targetdt.Columns[index["e"]].ColumnName = remap[targetdt.Columns[index["e"]].ColumnName];
                        targetdt.Columns[index["f"]].ColumnName = remap[targetdt.Columns[index["f"]].ColumnName];
                        targetdt.Columns[index["g"]].ColumnName = remap[targetdt.Columns[index["g"]].ColumnName];
                        targetdt.Columns[index["h"]].ColumnName = remap[targetdt.Columns[index["h"]].ColumnName];
                        targetdt.Columns[index["i"]].ColumnName = remap[targetdt.Columns[index["i"]].ColumnName];
                        targetdt.Columns[index["j"]].ColumnName = remap[targetdt.Columns[index["j"]].ColumnName];
                        targetdt.Columns[index["k"]].ColumnName = remap[targetdt.Columns[index["k"]].ColumnName];


                        var loadLevelling = ConvertDataTable<LlWorkTable>(targetdt);


                        targetdt.Columns[index["a"]].ColumnName = f1;
                        targetdt.Columns[index["b"]].ColumnName = f2;
                        targetdt.Columns[index["c"]].ColumnName = f3;
                        targetdt.Columns[index["d"]].ColumnName = ahead;
                        targetdt.Columns[index["e"]].ColumnName = late;
                        targetdt.Columns[index["f"]].ColumnName = priority;
                        targetdt.Columns[index["g"]].ColumnName = capacity;
                        targetdt.Columns[index["h"]].ColumnName = required;
                        targetdt.Columns[index["i"]].ColumnName = plan_bu;
                        targetdt.Columns[index["j"]].ColumnName = flag_hr;
                        targetdt.Columns[index["k"]].ColumnName = allocated;

                        var planbuq = from rec in loadLevelling group rec by rec.PLAN_BU into g orderby g.Count() descending select g.Distinct() ;
                        
                        // inizialmente raggruppa per PLAN_BU
                        //foreach (var record in planbuq)
                        //{
                        //    var p = record.Where(r => r.FLAG_HR != null).Select(r => r).ToList();
                        //    foreach (var q in p)
                        //    {
                        //        var ccc = q.FLAG_HR;
                        //    }
                        //}

                        var produzioni = loadLevelling.GroupBy(r => r.PLAN_BU).OrderByDescending(g => g.Count()).Select(f => f.Distinct()).ToList();
                        foreach (var rec in produzioni)
                        {
                            var p = rec.Where(r => r.FLAG_HR != null).Select(r => r).ToList();
                            foreach (var q in p)
                            {
                                var ccc = q.FLAG_HR;
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
        //private GroupAndSort()


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
    }
}
