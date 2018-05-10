using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SocialFORM.Models.Group;
using SocialFORM.Models.Question;
using System.Data.Entity;
using SocialFORM.Models.Project;
using System.Threading.Tasks;
using System.IO;
using System.Text;

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
            var result = db.SetGroupModels.Include(u => u.Question).Where(c => c.ProjectID == id_project);
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

        public ActionResult FilterForm()
        {
            return PartialView();
        }

        [HttpPost]
        public void AddGroup(GroupModel tmp)
        {
            System.Diagnostics.Debug.WriteLine("GroupID : " + tmp.GroupID);
            QuestionModel tmp_q = new QuestionModel();
            tmp_q.TextQuestion = "New question";
            tmp_q.TypeQuestion = (SocialFORM.Models.Question.Type)1;
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
        [HttpPost]
        public JsonResult Upload()
        {
            string path_file = "";
            foreach (string file in Request.Files)
            {
                var upload = Request.Files[file];
                if (upload != null)
                {
                    string fileName = System.IO.Path.GetFileName(upload.FileName);
                    System.Diagnostics.Debug.WriteLine("File name : " + fileName);
                    upload.SaveAs(Server.MapPath("~/Content/" + fileName));
                    path_file = Server.MapPath("~/Content/" + fileName);
                    using (QuestionContext q_context = new QuestionContext())
                    {
                        Models.Question.File file_db = new Models.Question.File();
                        file_db.FileName = fileName;
                        file_db.PathFile = Server.MapPath("~/Content/" + fileName);
                        q_context.GetFiles.Add(file_db);
                        q_context.SaveChanges();
                    }
                }
            }
            return Json(path_file);
        }

        [HttpPost]
        public async Task BindIDFilter(int id_p, int id_q, string path)
        {
            using (QuestionContext q_context = new QuestionContext())
            {
                Models.Question.File file = await q_context.GetFiles.FirstOrDefaultAsync(u => u.PathFile == path);
                file.ProjectID = id_p;
                file.QuestionID = id_q;
                await q_context.SaveChangesAsync();
            }
        }

        [HttpGet]
        public JsonResult CheckFile(int id_q)
        {
            using (QuestionContext q_context = new QuestionContext())
            {
                Models.Question.File file_db = q_context.GetFiles.FirstOrDefault(u => u.QuestionID == id_q);
                if (file_db != null)
                    return Json(file_db.PathFile, JsonRequestBehavior.AllowGet);
                else
                    return null;
            }
        }

        [HttpGet]
        public JsonResult Load(string path)
        {
            FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(file, Encoding.GetEncoding("windows-1251"));
            string str = reader.ReadToEnd();
            reader.Close();
            string[] mas_str = str.Split('\n');
            return Json(mas_str, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void DeleteFile(string path)
        {
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
            using (QuestionContext q_context = new QuestionContext())
            {
                var tmp = q_context.GetFiles.FirstOrDefault(u => u.PathFile == path);
                q_context.GetFiles.Remove(tmp);
                q_context.SaveChanges();
            }
        }

        [HttpGet]
        public JsonResult GetTimeBegin()
        {
            string tmp = DateTime.Now.ToString();
            return Json(tmp, JsonRequestBehavior.AllowGet);
        }

    }
}