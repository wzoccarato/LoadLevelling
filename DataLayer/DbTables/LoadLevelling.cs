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
        public string ProductionCategory { get; set; }
        public string IndSeasonalStatus { get; set; }
        [StringLength(6, ErrorMessage = "errore lunghezza stringa", MinimumLength = 6)]
        public string TckWeek { get; set; }
        [StringLength(10, ErrorMessage = "errore lunghezza stringa", MinimumLength = 2)]
        public string PlanningLevel { get; set; }
        [StringLength(10, ErrorMessage = "errore lunghezza stringa", MinimumLength = 2)]
        public string Event { get; set; }
        [StringLength(6, ErrorMessage = "errore lunghezza stringa", MinimumLength = 6)]
        public string WeekPlan { get; set; }
        public float Ahead { get; set; }
        public float Late { get; set; }
        public float Priority { get; set; }
        public float Capacity { get; set; }
        public float Required { get; set; }
        public short Uno { get; set; }
        public short Due { get; set; }
        public short Tre { get; set; }
        public short Quattro { get; set; }
        [StringLength(10, ErrorMessage = "errore lunghezza stringa", MinimumLength = 2)]
        public string PlanBu { get; set; }
        [StringLength(1, ErrorMessage = "errore lunghezza stringa", MinimumLength = 1)]
        public string FlagHr { get; set; }
        public float Allocated { get; set; }
        public float NotAllocated { get; set; }
    }
}
