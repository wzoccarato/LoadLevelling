using System.Collections.Generic;

namespace LoadL.CalcExtendedLogics
{
    public static class Global
    {

        // posizione in tabella loadlevelling degli heading
        // che possono cambiare nome
        public enum LlAlias
        {
            F1 = 0,
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

        public static Dictionary<string, int> Index = new Dictionary<string, int>()
        {
            {"a", 7},
            {"b", 8},
            {"c", 9},
            {"d", 10},
            {"e", 11},
            {"f", 12},
            {"g", 13},
            {"h", 14},
            {"i", 15},
            {"j", 16},
            {"k", 17}
        };

        

    }
}