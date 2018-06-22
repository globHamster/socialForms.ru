using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialFORM.Models.Question
{
    public class Kvot
    {
        public int Id { get; set; }
        public int ProjectID { get; set; }
        public int QuestionID { get; set; }
        public string Target { get; set; }
        public int CountKvot { get; set; }
        public int TypeKvot { get; set; }
    }
}