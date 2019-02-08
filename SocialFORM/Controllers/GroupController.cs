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
            return PartialView(result.OrderBy(u => u.IndexQuestion).ToList());
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

        [HttpGet]
        public JsonResult GetListGroup(int id_p)
        {
            return Json(db.SetGroupModels.Where(u => u.ProjectID == id_p).OrderBy(u => u.IndexQuestion).ToList(), JsonRequestBehavior.AllowGet);
        }

        //Добавление новой группы вопросов
        [HttpPost]
        public int AddNewGroup(int id_p)
        {
            GroupModel tmp = new GroupModel();
            if (db.SetGroupModels.Where(u => u.ProjectID == id_p && u.Group != null).Max(u => u.Group) == 0)
            {
                tmp.Group = 1;
                tmp.GroupID = 0;
                tmp.GroupName = "Группа " + tmp.Group;
                tmp.ProjectID = id_p;
                tmp.IndexQuestion = db.SetGroupModels.Where(u => u.ProjectID == id_p).Max(u => u.IndexQuestion) + 1;
                db.SetGroupModels.Add(tmp);
                db.SaveChanges();
            }
            else
            {
                int max_index_group = (int)db.SetGroupModels.Where(u => u.ProjectID == id_p && u.Group != null).Max(u => u.Group);
                tmp.Group = max_index_group + 1;
                tmp.GroupID = 0;
                tmp.GroupName = "Группа " + tmp.Group;
                tmp.IndexQuestion = db.SetGroupModels.Where(u => u.ProjectID == id_p).Max(u => u.IndexQuestion) + 1;
                tmp.ProjectID = id_p;
                db.SetGroupModels.Add(tmp);
                db.SaveChanges();
            }
            return (int)tmp.Group;
        }

        [HttpPost]
        public JsonResult AddGroup(GroupModel tmp)
        {
            System.Diagnostics.Debug.WriteLine("GroupID : " + tmp.GroupID);
            QuestionModel tmp_q = new QuestionModel();
            tmp_q.TextQuestion = "New question";
            tmp_q.IsKvot = false;
            tmp_q.IsRotate = false;
            tmp_q.TypeQuestion = (SocialFORM.Models.Question.Type)1;
            tmp_q = db2.SetQuestions.Add(tmp_q);
            db2.SaveChanges();
            int index;
            if (db.SetGroupModels.Where(u => u.ProjectID == tmp.ProjectID && u.GroupID == tmp.GroupID).Count() != 0)
            {
                if (db.SetGroupModels.Where(u => u.ProjectID == tmp.ProjectID && u.GroupID == tmp.GroupID).Count() != 0)
                    index = (int)db.SetGroupModels.Where(u => u.ProjectID == tmp.ProjectID && u.GroupID == tmp.GroupID).Max(s => s.IndexQuestion) + 1;
                else
                    index = (int)db.SetGroupModels.First(u => u.ProjectID == tmp.ProjectID && u.Group == tmp.GroupID).IndexQuestion + 1;
            }
            else
            {
                if (tmp.GroupID > 0)
                {
                    index = (int)db.SetGroupModels.First(u => u.ProjectID == tmp.ProjectID && u.Group == tmp.GroupID).IndexQuestion + 1;
                }
                else
                {
                    index = 1;
                }
            }
            tmp.QuestionID = tmp_q.Id;
            tmp.GroupName = "Вопрос" + index;
            tmp.IndexQuestion = index;
            db.SetGroupModels.Add(tmp);
            db.SaveChanges();
            return Json(tmp);
        }

        [HttpGet]
        public JsonResult GetGroupItem(int id_group)
        {
            return Json(db.SetGroupModels.FirstOrDefault(u => u.Id == id_group), JsonRequestBehavior.AllowGet);
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
            return Json(db.SetGroupModels.Where(u => u.ProjectID == id_p && u.GroupID != null).OrderBy(u => u.IndexQuestion).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult getIdQuestionGroup(int id_p)
        {
            return Json(db.SetGroupModels.Where(u => u.ProjectID == id_p && u.GroupID != null && u.Group == null).OrderBy(u => u.IndexQuestion).Select(u => u.QuestionID).ToList(), JsonRequestBehavior.AllowGet);
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
                    upload.SaveAs(Server.MapPath("../Content/" + fileName));
                    path_file = Server.MapPath("../Content/" + fileName);
                    using (QuestionContext q_context = new QuestionContext())
                    {
                        Models.Question.File file_db = new Models.Question.File();
                        file_db.FileName = fileName;
                        file_db.PathFile = Server.MapPath("../Content/" + fileName);
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
                if (tmp != null)
                {
                    q_context.GetFiles.Remove(tmp);
                    q_context.SaveChanges();
                }
            }
        }

        [HttpPost]
        public void DeleteFileQuestion(int id_question)
        {
            using (QuestionContext q_context = new QuestionContext())
            {
                var tmp = q_context.GetFiles.FirstOrDefault(u => u.QuestionID == id_question);
                if (tmp != null)
                {
                    if (System.IO.File.Exists(tmp.PathFile))
                    {
                        System.IO.File.Delete(tmp.PathFile);
                    }
                    q_context.GetFiles.Remove(tmp);
                    q_context.SaveChanges();
                }
            }
        }

        [HttpGet]
        public JsonResult GetTimeBegin()
        {
            string tmp = DateTime.Now.ToString();
            return Json(tmp, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void ChangeIndexQuestion(List<int> new_set, int id_p)
        {
            int count = 1;
            int count_group = 1;
            int count_answer = 1;
            foreach (var item in new_set)
            {

                if (item < 0)
                {
                    GroupModel group_item = db.SetGroupModels.FirstOrDefault(u => u.Group == (-1) * item && u.ProjectID == id_p);
                    group_item.IndexQuestion = count;
                  //  group_item.GroupName = "Группа " + count_group;
                    count_group++;
                }
                else
                {
                    GroupModel group_item = db.SetGroupModels.FirstOrDefault(u => u.QuestionID == item);
                    group_item.IndexQuestion = count;
                  //  group_item.GroupName = "Вопрос " + count_answer;
                    count_answer++;
                }
                db.SaveChanges();
                count++;
            }
        }

        [HttpPost]
        public void ChangeGroup(int id_q, int id_g)
        {
            GroupModel tmp = db.SetGroupModels.FirstOrDefault(u => u.QuestionID == id_q);
            tmp.GroupID = id_g;
            db.SaveChanges();
        }

        [HttpPost]
        public void DeleteGroup(int id_group, int id_project)
        {
            List<GroupModel> tmp = new List<GroupModel>();
            tmp.AddRange(db.SetGroupModels.Where(u => u.GroupID == id_group && u.ProjectID == id_project).ToList());
            List<QuestionModel> question_tmp = new List<QuestionModel>();
            foreach (var item in tmp)
            {
                question_tmp.Add(db2.SetQuestions.FirstOrDefault(u => u.Id == item.QuestionID));
            }
            List<AnswerAll> answerAll_tmp = new List<AnswerAll>();
            foreach (var item in question_tmp)
            {
                answerAll_tmp.AddRange(db2.SetAnswerAll.Where(u => u.QuestionID == item.Id).ToList());
            }
            List<AnswerAll> bind_list_answer = db2.SetAnswerAll.Where(u => u.BindGroup == id_group).ToList();
            foreach(var item in bind_list_answer)
            {
                QuestionModel t_question = db2.SetQuestions.FirstOrDefault(u => u.Id == item.QuestionID);
                if (t_question != null)
                {
                    if (t_question.ProjectID == id_project)
                    {
                        item.BindGroup = null;
                    }
                }
            }
            db2.SetAnswerAll.RemoveRange(answerAll_tmp);
            db2.SetQuestions.RemoveRange(question_tmp);
            db2.SaveChanges();
            tmp.Clear();
            tmp.Add(db.SetGroupModels.FirstOrDefault(u => u.Group == id_group && u.ProjectID == id_project));
            db.SetGroupModels.Remove(tmp.First());
            db.SaveChanges();
        }

        [HttpGet]
        public JsonResult GetListALLGoup(int id_p)
        {
            return Json(db.SetGroupModels.Where(u => u.ProjectID == id_p && (u.GroupID != null && u.Group == null)).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void SetBindGroup(int id_q, int id_a, int id_g)
        {
            AnswerAll tmp = db2.SetAnswerAll.FirstOrDefault(u => u.QuestionID == id_q && u.AnswerKey == id_a);
            tmp.BindGroup = id_g;
            db2.SaveChanges();
        }

        [HttpPost]
        public void DeleteBindGroup(int id_q, int id_a)
        {
            AnswerAll tmp = db2.SetAnswerAll.FirstOrDefault(u => u.QuestionID == id_q && u.AnswerKey == id_a);
            tmp.BindGroup = null;
            db2.SaveChanges();
        }

        [HttpPost]
        public void RemoveAllBindGroup(int id_q)
        {
            List<AnswerAll> tmp_list = db2.SetAnswerAll.Where(u => u.QuestionID == id_q).ToList();
            foreach (var item in tmp_list)
            {
                item.BindGroup = null;
            }
            db2.SaveChanges();
        }

        [HttpPost]
        public void RenameGroup(int id, string name)
        {
            System.Diagnostics.Debug.WriteLine("ID group --- " + id);
            System.Diagnostics.Debug.WriteLine("Name group --- " + name);
            GroupModel tmp = db.SetGroupModels.FirstOrDefault(u => u.Id == id);
            if (tmp != null)
            {
                tmp.GroupName = name;
                db.Entry(tmp).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

    }
}