using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CalcExtendedLogics
{
    public static class Helper
    {

        // esegue la mappatura da heading della colonne interni, a Heading delle
        // colonne relative alla dataTable LoadLevelling
#if LOCALTEST
        public static Dictionary<LlAlias, string> Alias = new Dictionary<LlAlias, string>()
                                                          {
                                                              {LlAlias.F1, "a"},
                                                              {LlAlias.F2, "b"},
                                                              {LlAlias.F3, "c"},
                                                              {LlAlias.Ahead, "d"},
                                                              {LlAlias.Late, "e"},
                                                              {LlAlias.Priority, "f"},
                                                              {LlAlias.Capacity, "g"},
                                                              {LlAlias.Required, "h"},
                                                              {LlAlias.PlanBu,"i"},
                                                              {LlAlias.FlagHr,"j"},
                                                              {LlAlias.Allocated,"k"}
                                                          };
#else
        public static Dictionary<LlAlias, string> Alias = new Dictionary<LlAlias, string>()
        {
            {LlAlias.Ahead, "a"},
            {LlAlias.Late, "b"},
            {LlAlias.Priority, "c"},
            {LlAlias.Capacity, "d"},
            {LlAlias.Required, "e"},
            {LlAlias.PlanBu,"f"},
            {LlAlias.FlagHr,"g"},
            {LlAlias.Allocated,"h"}
        };
#endif



        // index dei campi variabili relativi ai record della tabella LoadLevelling
        // ricevuta in argomentoi dal metodo "Execute" dell'oggetto CalcEx
        public static Dictionary<string, int> Index = new Dictionary<string, int>()
                                                      {
                                                          {"a", (int)LlAlias.F1},
                                                          {"b", (int)LlAlias.F2},
                                                          {"c", (int)LlAlias.F3},
                                                          {"d", (int)LlAlias.Ahead},
                                                          {"e", (int)LlAlias.Late},
                                                          {"f", (int)LlAlias.Priority},
                                                          {"g", (int)LlAlias.Capacity},
                                                          {"h", (int)LlAlias.Required},
                                                          {"i", (int)LlAlias.PlanBu},
                                                          {"j", (int)LlAlias.FlagHr},
                                                          {"k", (int)LlAlias.Allocated}
                                                      };


        // serve per rimappare i nomi dei campi nella tabella schema contenuta sul db passato da Board,
        // sui nomi dei campi del relativo oggetto mantenuto internamente
        // la chiave sono i nomi dei campi dell'oggetto mantneuto internamente
        public static Dictionary<string,string> SchemaMap = new Dictionary<string, string>()
        {
            {"Id",""},
            {"BlockId","Block ID"},
            {"CubeName","CubeName"},
            {"Heading","Heading"},
            {"WriteBack","WriteBack"}
        };

        public static int GetWeeksInYear(int year)
        {
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            DateTime date1 = new DateTime(year, 12, 31);
            Calendar cal = dfi.Calendar;
            return cal.GetWeekOfYear(date1, dfi.CalendarWeekRule,
                                     dfi.FirstDayOfWeek);
        }

        public static bool ValidateWeekFormat(string week)
        {
            var isvalid = false;

            if (week?.Length == Global.WEEKPLAN_LENGTH)
            {
                var year = Convert.ToInt32(week.Substring(0, Global.YEAR_LENGTH));
                var wk = Convert.ToInt32(week.Substring(Global.YEAR_LENGTH, Global.WEEK_LENGTH));
                if (year > Global.MINYEAR && year < Global.MAXYEAR)
                {
                    var wkinyear = GetWeeksInYear(year);
                    if (wk > 0 && wk <= wkinyear)
                    {
                        isvalid = true;
                    }
                }
            }
            return isvalid;
        }

        public static void ConvertTableToInsertSatements(DataTable dt, FileStream fs)
        {
            List<string> insert = new List<string>();
            UTF8Encoding utf8Encoding = new UTF8Encoding();
            UnicodeEncoding uniEncoding = new  UnicodeEncoding();
            fs.Seek(0, SeekOrigin.End);
            foreach (DataRow r in dt.Rows)
            {
                string newinsert = "(";
                var length = r.ItemArray.Length;
                for (int i = 0; i < length; i++)
                {
                    newinsert += $"'{r.ItemArray[i].ToString()}'";
                    if (i < length - 1)
                    {
                        newinsert += ",";
                    }
                }
                newinsert += ")\r\n";
                fs.Write(utf8Encoding.GetBytes(newinsert),0,utf8Encoding.GetByteCount(newinsert));
            }
            fs.Flush();
        }

        public static void WriteMessage(string message)
        {
#if LOCALTEST
            Console.WriteLine(message);
#else
#endif
        }

    }
}
