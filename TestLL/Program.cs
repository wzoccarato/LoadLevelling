using System;
using System.Data;
using System.Linq;
using System.Reflection;
using LoadL.CalcExtendedLogics;
using LoadL.Infrastructure.Abstract;
using LoadL.Infrastructure.AccessLayer;

namespace LoadL.TestLL
{

    class Program
    {
        public static IDbQuery ILl { get; set; }

        static void Main()
        {
            ILl = new EfLoadL();
            
            CalcExLogicClass celc = new CalcExLogicClass();

            DataTable dt = LinqToDataTable(ILl.LoadLevellingTable);
            dt.TableName = "LoadLevelling";     // il nome della tabella nel db
            DataTable schema = LinqToDataTable(ILl.SchemaTable);
            schema.TableName = "Schema";

            DataSet ds = new DataSet();
            ds.Tables.Add(dt);
            ds.Tables.Add(schema);

            celc.Execute(ds, "LoadL", "LoadLevelling");
        }

        /// <summary>
        /// converte una collection IQueriable gestita con Linq/Entity Framework, 
        /// in una DataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="varlist"></param>
        /// <returns></returns>
        private static DataTable LinqToDataTable<T>(IQueryable<T> varlist)
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
    }
}
