using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SocialFORM.Models.Number
{
    [Table("AOs")]
    public class AO
    {
        [Key]
        public int Id { get; set; }
        public string KodFO { get; set; }
        public string KodOB { get; set; }
        public string KodAO { get; set; }
        public string NameAO { get; set; }
    }
}