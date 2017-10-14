using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;

namespace LoadL.DataLayer.DbTables
{
    public class LoadLevelling
    {
        [Key]
        public int Id { get; set; }
        [StringLength(2,ErrorMessage = "errore lunghezza stringa",MinimumLength = 2)]
        public string PRODUCTION_CATEGORY { get; set; }
        public string IND_SEASONAL_STATUS { get; set; }
        [StringLength(6, ErrorMessage = "errore lunghezza stringa", MinimumLength = 6)]
        public string TCH_WEEK { get; set; }
        [StringLength(10, ErrorMessage = "errore lunghezza stringa", MinimumLength = 2)]
        public string PLANNING_LEVEL { get; set; }
        [StringLength(10, ErrorMessage = "errore lunghezza stringa", MinimumLength = 2)]
        public string Event { get; set; }
        [StringLength(6, ErrorMessage = "errore lunghezza stringa", MinimumLength = 6)]
        public string WEEK_PLAN { get; set; }
        public double a { get; set; }           // F1 spare. heading riconfigurabile
        public double b { get; set; }           // F2 spare
        public double c { get; set; }           // F3 spare
        public double d { get; set; }           // Ahead
        public double e { get; set; }           // Late
        public double f { get; set; }           // Priority
        public double g { get; set; }           // Capacity
        public double h { get; set; }           // Required
        [StringLength(10, ErrorMessage = "errore lunghezza stringa", MinimumLength = 2)]
        public string i { get; set; }           // PLAN_BU
        [StringLength(1, ErrorMessage = "errore lunghezza stringa", MinimumLength = 1)]
        public string j { get; set; }           // FLAG_HR
        public double k { get; set; }           // Allocated
    }

    // identica alla precedente, ma utilizzata per l'elaborazione interna
    // i nomi dei campi variabili sono parlanti, in modo da semplificare la
    // comprensione del codice
    public class LlWorkTable
    {
        [Key]
        public int Id { get; set; }
        [StringLength(2, ErrorMessage = "errore lunghezza stringa", MinimumLength = 2)]
        public string PRODUCTION_CATEGORY { get; set; }
        public string IND_SEASONAL_STATUS { get; set; }
        [StringLength(6, ErrorMessage = "errore lunghezza stringa", MinimumLength = 6)]
        public string TCH_WEEK { get; set; }
        [StringLength(10, ErrorMessage = "errore lunghezza stringa", MinimumLength = 2)]
        public string PLANNING_LEVEL { get; set; }
        [StringLength(10, ErrorMessage = "errore lunghezza stringa", MinimumLength = 2)]
        public string Event { get; set; }
        [StringLength(6, ErrorMessage = "errore lunghezza stringa", MinimumLength = 6)]
        public string WEEK_PLAN { get; set; }
        public double F1 { get; set; }                  // F1 spare. heading riconfigurabile
        public double F2 { get; set; }                  // F2 spare
        public double F3 { get; set; }                  // F3 spare
        public double Ahead { get; set; }               // Ahead
        public double Late { get; set; }                // Late
        public double Priority { get; set; }            // Priority
        public double Capacity { get; set; }            // Capacity
        public double Required { get; set; }            // Required
        [StringLength(10, ErrorMessage = "errore lunghezza stringa", MinimumLength = 2)]
        public string PLAN_BU { get; set; }             // PLAN_BU
        [StringLength(1, ErrorMessage = "errore lunghezza stringa", MinimumLength = 1)]
        public string FLAG_HR { get; set; }             // FLAG_HR
        public double Allocated { get; set; }           // Allocated
    }
}
