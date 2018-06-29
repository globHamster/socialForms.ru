using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialFORM.Models.Question
{
    public class QuotaModel
    {
        public int Id { get; set; }
        public int ProjectID {get; set;}
        public string ChainString { get; set; }
        public int QuotaCount { get; set; }
    }
}