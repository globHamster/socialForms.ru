using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SocialFORM.Models.Group;
using SocialFORM.Models.Question;
using System.Data.Entity;
using SocialFORM.Models.Project;

namespace SocialFORM.Controllers
{
    public class GroupController : Controller
    {
        // GET: Group

        GroupContext db = new GroupContext();
        QuestionContext db2 = new QuestionContext();
        static int get_id_project;

        public ActionResult Manager(int id_project)
        {
            System.Diagnostics.Debug.WriteLine("input : " + id_project);
            var result = db.SetGroupModels.Include(u=>u.Question).Where(c=>c.ProjectID == id_project);
            using (ProjectContext db3 = new ProjectContext())
            {
                ProjectModel tmp = db3.SetProjectModels.First(u => u.Id == id_project);
                ViewBag.ProjectName = tmp.NameProject;
                get_id_project = id_project;
                ViewBag.ProjectID = id_project;
            }
            return PartialView(result);
        }

        public ActionResult SingleForm()
        {
            return PartialView();
        }

        public ActionResult MultipleForm()
        {
            return PartialView();
        }

        public ActionResult FreeForm()
        {
            return PartialView();
        }

        public ActionResult TableForm()
        {
            return PartialView();
        }

        [HttpPost]
        public void AddGroup(GroupModel tmp)
        {
            System.Diagnostics.Debug.WriteLine("GroupID : " + tmp.GroupID);
            QuestionModel tmp_q = new QuestionModel();
            tmp_q.TextQuestion = "New question";
            tmp_q.TypeQuestion = (SocialFORM.Models.Question.Type) 1;
            tmp_q = db2.SetQuestions.Add(tmp_q);
            db2.SaveChanges();
            int index;
            if (db.SetGroupModels.Where(u => u.ProjectID == tmp.ProjectID && u.GroupID == tmp.GroupID).Count() != 0)
                index = (int)db.SetGroupModels.Where(u => u.ProjectID == tmp.ProjectID && u.GroupID == tmp.GroupID).Max(s => s.IndexQuestion) + 1;
            else
                index = 1;
            tmp.QuestionID = tmp_q.Id;
            tmp.GroupName = "Вопрос" + index;
            tmp.IndexQuestion = index;
            db.SetGroupModels.Add(tmp);
            db.SaveChanges();
        }

        [HttpGet]
        public ActionResult Editor(int Id)
        {
            System.Diagnostics.Debug.WriteLine("In editor : " + get_id_project + DateTime.Now);
            ViewBag.ProjectID = get_id_project;
            var result = db2.SetQuestions.Where(u => u.Id == Id).First();
            return PartialView(result);
        }

        [HttpGet]
        public JsonResult getGroup(int id_p)
        {
            return Json(db.SetGroupModels.Where(u => u.ProjectID == id_p && u.GroupID != null).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult getIdQuestionGroup(int id_p)
        {
            return Json(db.SetGroupModels.Where(u => u.ProjectID == id_p && u.GroupID != null).OrderBy(u => u.IndexQuestion).Select(u => u.QuestionID).ToList(), JsonRequestBehavior.AllowGet);
        }
    }
}