using System.ComponentModel.DataAnnotations;

namespace CalcExtendedLogics.DataLayer.DbTables
{
    // identica alla classe LoadLevelling, ma utilizzata per l'elaborazione interna
    // i nomi dei campi variabili sono parlanti, in modo da semplificare la
    // comprensione del codice
    public class LoadLevellingWork
    {
        [Key]
        public int? Id { get; set; }
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

        // esegue una copia clone dell'oggetto
        public LoadLevellingWork Clone()
        {
            LoadLevellingWork retval = (LoadLevellingWork)this.MemberwiseClone();
            retval.Id = null;
            return retval;
        }
    }
}
