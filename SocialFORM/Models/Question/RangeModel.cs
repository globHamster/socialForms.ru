using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialFORM.Models.Question
{
    public class RangeModel
    {
        public int Id { get; set; }
        public int ProjectID { get; set; }
        public string RangeString { get; set; }
        public int IndexRange { get; set; }
        public int BindQuestion { get; set; }
    }
}