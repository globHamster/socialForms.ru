using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SocialFORM.Models.Question
{
    public class AnswerModel
    {
        public int Id { get; set; }
        public string AnswerText { get; set; }
        [ForeignKey("QuestionModel")]
        public int QuestionID { get; set; }
        public int Index { get; set; }
        public bool isFreeArea { get; set; }

        public QuestionModel QuestionModel { get; set; }
    }
}