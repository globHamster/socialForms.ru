using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using SocialFORM.Models.Question;

namespace SocialFORM.Models.Question
{
    public class QuestionContext : DbContext
    {
        public QuestionContext() : base("DefaultConnection") { }

        public DbSet<QuestionModel> SetQuestions { get; set; }
        public DbSet<AnswerModel> SetAnswers { get; set; }
        public DbSet<TableRow> SetTableRows { get; set; }
        public DbSet<AnswerBaseModel> SetAnswerBaseModels { get; set; }
        public DbSet<AnswerAll> SetAnswerAll { get; set; }
        public DbSet<Transition> SetTransition { get; set; }
        public DbSet<Block> SetBlock { get; set; }
        public DbSet<Massk> GetMassk { get; set; }
        public DbSet<File> GetFiles { get; set; }
    }
}