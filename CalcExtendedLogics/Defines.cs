using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace LoadL.CalcExtendedLogics
{
    // posizione in tabella loadlevelling degli heading
    // che possono cambiare nome
    public enum LlAlias
    {
        F1 = 7,
        F2,
        F3,
        Ahead,
        Late,
        Priority,
        Capacity,
        Required,
        PlanBu,
        FlagHr,
        Allocated
    };

    public static class Global
    {
        public const int WEEKPLAN_LENGTH = 6;   // lunghezza in caratteri di WEEK_PLAN
        public const int YEAR_LENGTH = 4;       // lunghezza in caratteri della componente YEAR
        public const int WEEK_LENGTH = 2;       // lunghezza in caratteri della componente WEEK
        public const int MINYEAR = 2010;        // anno minimo accettato dal programma
        public const int MAXYEAR = 2100;        // anno massimo accettato dal programma
    }
}