using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadL.CalcExtendedLogics
{
    public static class Helper
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

        // esegue la mappatura da heading della colonne interni, a Heading delle
        // colonne relative alla dataTable LoadLevelling
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
    }
}
