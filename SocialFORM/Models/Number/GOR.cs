using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SocialFORM.Models.Number
{
    public class GOR
    {
        public int Id { get; set; }
        public string KodGOR { get; set; }
        public string NameGOR { get; set; }
        public string KodOB { get; set; }
        public string KodFO { get; set; }
    }
}