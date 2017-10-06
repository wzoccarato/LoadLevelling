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
        public double F1 { get; set; }
        public double F2 { get; set; }
        public double F3 { get; set; }
        public double Ahead { get; set; }
        public double Late { get; set; }
        public double Priority { get; set; }
        public double Capacity { get; set; }
        public double Required { get; set; }
        [StringLength(10, ErrorMessage = "errore lunghezza stringa", MinimumLength = 2)]
        public string PLAN_BU { get; set; }
        [StringLength(1, ErrorMessage = "errore lunghezza stringa", MinimumLength = 1)]
        public string FLAG_HR { get; set; }
        public double Allocated { get; set; }
    }
}
