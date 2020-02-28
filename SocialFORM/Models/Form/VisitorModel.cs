using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialFORM.Models.Form
{
    public class VisitorModel
    {
        public int Id { get; set; }
        public string AddressIP { get; set; }
        public DateTime EnterTime { get; set; }
    }
}