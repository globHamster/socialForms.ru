using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialFORM.Models.Form
{
    public class BlankModel
    {
        public int Id { get; set; }
        public int QuestionID { get; set; }
        public int AnswerID { get; set; }
        public int BlankID { get; set; }
        public int AnswerIndex { get; set; }
        public string Text { get; set; }
    }
}