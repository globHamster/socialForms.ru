using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SocialFORM.Models.Number
{
    [Table("PTs")]
    public class PT
    {
        public string FO { get; set; }
        public string OB { get; set; }
        public string GOR { get; set; }
        [Key]
        public string Phone { get; set; }
        public string Status { get; set; }
        public int Type { get; set; }
        public bool isActual { get; set; }
        public DateTime TimeCall { get; set; }
    }
}