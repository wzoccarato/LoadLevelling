using System.ComponentModel.DataAnnotations;

namespace CalcExtendedLogics.DataLayer.DbTables
{
    public class Schema
    {
        [Key]
        public int Id { get; set; }
        public string BlockId { get; set; }
        public string CubeName { get; set; }
        public string Heading { get; set; }
        public string WriteBack { get; set; }
    }
}