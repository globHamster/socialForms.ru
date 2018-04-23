using SocialFORM.Models.Question;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace SocialFORM.Models.Form
{
   

    public class Result
    {
       public QuestionModel questionResult { get; set; }
       public AnswerModel answerResult { get; set; }
    }


    public class ResultRepository : IDisposable
    {
        List<Result> result = new List<Result>();

        public void Add(Result tmp)
        {
            result.Add(tmp);
        }

        public List<Result> GetAll()
        {
            return result;
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (result != null)
                {
                    result = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

}