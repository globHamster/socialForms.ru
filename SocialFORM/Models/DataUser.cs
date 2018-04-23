using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialFORM.Models
{
    public class DataUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Family { get; set; }
        public int Age { get; set; }
        public string Fool { get; set; }
        public string Email { get; set; }

        public int? UserId { get; set; }
        public User User { get; set; }
    }
}