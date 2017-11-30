using System.ComponentModel.DataAnnotations;

namespace CalcExtendedLogics.DataLayer.DbTables
{
    public class LoadLevelling
    {
        [Key]
        public int ID { get; set; }
        [StringLength(2,ErrorMessage = "errore lunghezza stringa",MinimumLength = 2)]
        public string PRODUCTION_CATEGORY { get; set; }
        public string IND_SEASONAL_STATUS { get; set; }
        [StringLength(6, ErrorMessage = "errore lunghezza stringa", MinimumLength = 6)]
        public string TCH_WEEK { get; set; }
        [StringLength(10, ErrorMessage = "errore lunghezza stringa", MinimumLength = 2)]
        public string PLANNING_LEVEL { get; set; }
        [StringLength(10, ErrorMessage = "errore lunghezza stringa", MinimumLength = 2)]
        public string EVENT { get; set; }
        [StringLength(6, ErrorMessage = "errore lunghezza stringa", MinimumLength = 6)]
        public string Week { get; set; }
        public double? a { get; set; }           // F1 spare. heading riconfigurabile
        public double? b { get; set; }           // F2 spare
        public double? c { get; set; }           // F3 spare
        public double d { get; set; }           // Ahead
        public double e { get; set; }           // Late
        public double f { get; set; }           // Priority
        public double g { get; set; }           // Capacity
        public double h { get; set; }           // Required
        [StringLength(10, ErrorMessage = "errore lunghezza stringa", MinimumLength = 2)]
        public string i { get; set; }           // PLAN_BU
        [StringLength(1, ErrorMessage = "errore lunghezza stringa", MinimumLength = 1)]
        public string j { get; set; }           // FLAG_HR
        public double? k { get; set; }           // Allocated
    }
}
