using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialFORM.Models.Question
{
    public class File
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public int ProjectID { get; set; }
        public int QuestionID { get; set; }
        public string PathFile { get; set; }
    }
} 