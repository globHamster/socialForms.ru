using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SocialFORM.Models.Group;
using SocialFORM.Models.Project;

namespace SocialFORM.Models.Question
{
    public class QuestionModel
    {
        public int Id { get; set; }
        public Type TypeQuestion { get; set; }
        public TypeMassk TypeMassk { get; set; }
        [AllowHtml]
        public string TextQuestion { get; set; }
        public int ProjectID { get; set; }
        public int? Bind { get; set; }
        public ProjectModel Project { get; set; }

        public ICollection<GroupModel> Groups { get; set; }
        public List<AnswerModel> Answers { get; set; }

        public QuestionModel()
        {
            Groups = new List<GroupModel>();
        }

    }

    public enum Type : int
    {
        Single = 1,
        Multiple = 2,
        Free = 3,
        Table = 4,
        Text = 5,
        Filter = 6
    }
    public enum TypeMassk : int
    {
        Age = 1,
        Nambers = 2,
        Text = 3,
    }
}