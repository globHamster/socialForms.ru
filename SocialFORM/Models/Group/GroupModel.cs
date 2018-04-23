using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using SocialFORM.Models.Question;

namespace SocialFORM.Models.Group
{

    public class GroupModel
    {
        public int Id { get; set; }
        public int? Group { get; set; }
        public int? GroupID { get; set; }
        public int? QuestionID { get; set; }
        public int? ProjectID { get; set; }
        public string GroupName { get; set; }
        public int? IndexQuestion { get; set; }
        public QuestionModel Question { get; set; }


    }
}