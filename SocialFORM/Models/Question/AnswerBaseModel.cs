using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialFORM.Models.Question
{
    public class AnswerBaseModel
    {
        public int Id { get; set; }
        public string AnswerText { get; set; }
        public bool withFreeArea { get; set; }
        public string Transcription { get; set; }
        public int BaseIndex { get; set; }
    }
}