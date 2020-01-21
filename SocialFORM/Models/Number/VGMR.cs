using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SocialFORM.Models.Number
{
    [Table("VGMRs")]
    public class VGMR
    {
        [Key]
        public int Id { get; set; }
        public string KodFO { get; set; }
        public string KodOB { get; set; }
        public string KodAO { get; set; }
        public string KodVGMR { get; set; }
        public string NameVGMR { get; set; }
    }
}