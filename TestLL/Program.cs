using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using LoadL.CalcExtendedLogics;
using LoadL.DataLayer.DbTables;
using LoadL.Infrastructure.Abstract;
using LoadL.TestLL.AccessLayer;

namespace LoadL.TestLL
{

    class Program
    {
        public static ILoadL Iloadlevelling { get; set; }

        static void Main(string[] args)
        {
            Iloadlevelling = new EFLoadL();
            
            CalcExLogicClass celc = new CalcExLogicClass();

            DataTable dt = LINQToDataTable(Iloadlevelling.LoadLevellingTable);
            dt.TableName = "LoadLevelling";

            DataSet ds = new DataSet();
            ds.Tables.Add(dt);

            celc.Execute(ds, "goz", "LoadLevelling");
        }

        /// <summary>
        /// converte una collection IQueriable gestita con Linq/Entity Framework, 
        /// in una DataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="varlist"></param>
        /// <returns></returns>
        private static DataTable LINQToDataTable<T>(IQueryable<T> varlist)
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
                    oProps = ((Type)rec.GetType()).GetProperties();
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
    }
}
