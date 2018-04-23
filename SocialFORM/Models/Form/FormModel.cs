using SocialFORM.Models.Question;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialFORM.Models.Form
{
    public sealed class FormModel
    {
        private FormModel() { }
        public static FormModel Source { get { return Nested.source; } }
        private class Nested
        {
            static Nested() { }
            internal static readonly FormModel source = new FormModel();
        }
        public List<QuestionModel> listQuestion { get; set; }
        public List<AnswerModel> listAnswer { get; set; }

    }
  
}