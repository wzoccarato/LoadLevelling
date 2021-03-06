﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CalcExtendedLogics.CalcExtendedLogics;
using CalcExtendedLogics.DataLayer.DbTables;
using CalcExtendedLogics.Infrastructure;
using CalcExtendedLogics.Infrastructure.Abstract;
using CalcExtendedLogics.Infrastructure.AccessLayer;

namespace CalcExtendedLogics.TestLL
{

    class Program
    {
        public static IDbQuery ILl { get; set; }

        static void Main()
        {
            ILl = new EfLoadL();
            
            DataTable dt = DataTableHelper.QueryableToTable(ILl.LoadLevellingTable);
            dt.TableName = "LoadLevelling";     // il nome della tabella nel db
            DataTable schema = DataTableHelper.QueryableToTable(ILl.SchemaTable);
            schema.TableName = "Schema";

            DataSet ds = new DataSet();
            ds.Tables.Add(dt);
            ds.Tables.Add(schema);

            CalcEXlogicClass.Execute(ds, "loadl", "LoadLevelling");

            DataTable returndt = ds.Tables[0];

            // converte il datatable 
            var retlist = DataTableHelper.TableToList<LoadLevelling>(returndt).ToList();

            ILl.MassiveAddData(retlist);
            ILl.MassiveSaveData();
        }

        /// <summary>
        /// converte una collection IQueriable gestita con Linq/Entity Framework, 
        /// in una DataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="varlist"></param>
        /// <returns></returns>
        //private static DataTable IQueryableToDataTable<T>(IQueryable<T> varlist)
        //{
        //    DataTable dtReturn = new DataTable();

        //    // nomi delle colonne
        //    PropertyInfo[] oProps = null;

        //    if (varlist == null) return dtReturn;

        //    foreach (T rec in varlist)
        //    {
        //        // utilizza reflection per ottenere i nomi delle proprieta', e per creare la tabella (solo al primo giro)
        //        if (oProps == null)
        //        {
        //            oProps = rec.GetType().GetProperties();
        //            foreach (PropertyInfo pi in oProps)
        //            {
        //                Type colType = pi.PropertyType;

        //                if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition() == typeof(Nullable<>)))
        //                {
        //                    colType = colType.GetGenericArguments()[0];
        //                }

        //                dtReturn.Columns.Add(new DataColumn(pi.Name, colType));
        //            }
        //        }

        //        DataRow dr = dtReturn.NewRow();

        //        foreach (PropertyInfo pi in oProps)
        //        {
        //            dr[pi.Name] = pi.GetValue(rec, null) == null ? DBNull.Value : pi.GetValue(rec, null);
        //        }
        //        dtReturn.Rows.Add(dr);
        //    }
        //    return dtReturn;
        //}
    }
}
