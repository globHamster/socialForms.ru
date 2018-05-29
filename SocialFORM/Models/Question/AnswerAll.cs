using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialFORM.Models.Question
{
    public class AnswerAll
    {
        public int Id { get; set; }
        public int AnswerKey { get; set; }
        public int AnswerType { get; set; }
        public int QuestionID { get; set; }
        public int? BindGroup { get; set; }
    }
}