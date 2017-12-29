using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CalcExtendedLogics.Infrastructure
{
    public static class DataTableHelper
    {
        // mantiene le associazioni tipo / lista di proprieta'
        private static readonly Dictionary<Type, IList<PropertyInfo>> TypeDictionary = new Dictionary<Type, IList<PropertyInfo>>();

        /// <summary>
        /// dato il tipo, ritorna la lista contenente tutte le relative proprieta'
        /// </summary>
        /// <typeparam name="T">l-oggetto da cui si desidera la lista delle proprieta'</typeparam>
        /// <returns>Una IList contenente tutte le proprieta' dell'oggetto</returns>
        public static IList<PropertyInfo> GetPropertiesForType<T>()
        {
            var type = typeof(T);
            if (!TypeDictionary.ContainsKey(typeof(T)))
            {
                TypeDictionary.Add(type, type.GetProperties().ToList());
            }
            return TypeDictionary[type];
        }

        /// <summary>
        /// passata in argomento una datatable, la converte in un oggetto List,
        /// e ne torna la relativa interfaccia IList
        /// </summary>
        /// <typeparam name="T">l'oggetto che costituisce gli elementi della lista</typeparam>
        /// <param name="table"></param>
        /// <returns>Interfaccia IList alla lista frutto della conversione</returns>
        public static IList<T> TableToList<T>(DataTable table) where T : new()
        {
            IList<PropertyInfo> properties = GetPropertiesForType<T>();
            IList<T> result = new List<T>();

            foreach (var row in table.Rows)
            {
                var item = CreateItemFromRow<T>((DataRow)row, properties);
                result.Add(item);
            }

            return result;
        }

        /// <summary>
        /// converte una lista di cui viene passata l'interfaccia IQueryable,
        /// in un DataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="varlist"></param>
        /// <returns></returns>
        public static DataTable QueryableToTable<T>(IQueryable<T> varlist)
        {
            DataTable dtReturn = new DataTable();

            // nomi delle colonne
            PropertyInfo[] oProps = null;

            if (varlist == null) return dtReturn;

            foreach (T rec in varlist)
            {
                // utilizza reflection per ottenere i nomi delle proprieta', e per creare la tabella (solo al primo giro)
                if (oProps == null)
                {
                    oProps = rec.GetType().GetProperties();
                    foreach (PropertyInfo pi in oProps)
                    {
                        Type colType = pi.PropertyType;

                        if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                        {
                            colType = colType.GetGenericArguments()[0];
                        }

                        dtReturn.Columns.Add(new DataColumn(pi.Name, colType));
                    }
                }

                DataRow dr = dtReturn.NewRow();

                foreach (PropertyInfo pi in oProps)
                {
                    dr[pi.Name] = pi.GetValue(rec, null) == null ? DBNull.Value : pi.GetValue(rec, null);
                }
                dtReturn.Rows.Add(dr);
            }
            return dtReturn;
        }

        /// <summary>
        /// popola l'oggetto T a partire dal nome delle prorieta'
        /// naturalmente i campi della tabella, e i nomi delle proprieta' passati
        /// in argomento devono corrispondere
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr">il DataRow da convertire</param>
        /// <param name="properties">La lista dei campi che devono essere presenti
        /// in tabella</param>
        /// <returns>l'oggetto T frutto della conversione della riga di tabella</returns>
        private static T CreateItemFromRow<T>(DataRow dr, IEnumerable<PropertyInfo> properties) where T : new()
        {
            T item = new T();
            foreach (var pro in properties)
            {
                //pro.SetValue(item, dr[pro.Name], null);
         
                var value = dr[pro.Name];
                if (value is DBNull)
                {
                    if (pro.PropertyType == typeof(double))
                    {
                        pro.SetValue(item, 0, null);
                    }
                    else if (pro.PropertyType == typeof(int))
                    {
                        pro.SetValue(item, 0, null);
                    }
                    else if (pro.PropertyType == typeof(string))
                    {
                        pro.SetValue(item, string.Empty, null);
                    }
                    else
                    {
                        pro.SetValue(item, null, null);
                    }
                }
                else
                {
                    pro.SetValue(item, value, null);
                }

            }
            return item;
        }
    }
}
