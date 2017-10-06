using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LoadL.DataLayer.DbTables;

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
                    case "goz":
                        var targetdt = dataset.Tables[targetdatatablename];

                        var loadLevelling = ConvertDataTable<LoadLevelling>(targetdt);

                        var query = from rec in loadLevelling group rec by rec.PLAN_BU into g orderby g.Count() descending select g;

                        foreach (var record in query)
                        {
                            var p = record.Where(r => r.FLAG_HR != null).Select(r => r).ToList();
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
