using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialFORM.Models
{
    public class SchoolDay
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
    }
}