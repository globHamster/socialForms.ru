using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SocialFORM.Models.Number
{
    [Table("FormNumbers")]
    public class FormNumber
    {
        public string FO { get; set; }
        public string OB { get; set; }
        public string GOR { get; set; }
        public string NP { get; set; }
        public string VGOR { get; set; }
        [Key]
        public string Phone { get; set; }
        public string Sex { get; set; }
        public int? Age { get; set; }
        public string Address { get; set; }
        public string Education { get; set; }
        public int Type { get; set; }
    }
}