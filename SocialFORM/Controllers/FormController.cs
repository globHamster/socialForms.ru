using SocialFORM.Models;
using SocialFORM.Models.Form;
using SocialFORM.Models.Group;
using SocialFORM.Models.Question;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SocialFORM.Models.Project;
using System.Threading.Tasks;
using SocialFORM.Models.Utilite;

namespace SocialFORM.Controllers
{
    public class FormController : Controller
    {

        QuestionContext db = new QuestionContext();
        ApplicationContext db2 = new ApplicationContext();


        public List<QuestionModel> listQ;
        public List<AnswerModel> listA;
        public List<Result> listResult;
        public ResultRepository repo;

        public static int _index = 0;
        public static int max_index;
        public bool isReload = false;
        public FormModel form;
        // GET: Form
        public ActionResult FormView(int id_p)
        {
            ViewBag.QuestionID = db.SetQuestions.Where(u => u.ProjectID == id_p).ToList().First().Id;
            System.Diagnostics.Debug.WriteLine("Starting view is run ...");
            _index = 0;
            listResult = new List<Result>();
            using (ProjectContext db3 = new ProjectContext())
            {
                ViewBag.Title = db3.SetProjectModels.First(u => u.Id == id_p).NameProject;
            }

            ViewBag.ProjectID = id_p;
            System.Diagnostics.Debug.WriteLine("Uploading the view on client ... ");

            return PartialView();
        }

        public JsonResult getQuestion(int id_q)
        {
            QuestionModel tmp = db.SetQuestions.First(u => u.Id == id_q);
            return Json(tmp, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getNextQuestion(int id_q)
        {
            QuestionModel tmp = db.SetQuestions.First(u => u.Id == id_q);
            return Json(tmp, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getPreviousQuestion()
        {
            QuestionModel tmp = new QuestionModel();
            if (_index > 0)
            {
                tmp = form.listQuestion[--_index];
            }
            else
            {
                tmp.TextQuestion = "-stop";
            }

            return Json(tmp, JsonRequestBehavior.AllowGet);
        }



        public int getMaxCount(int id_p)
        {
            return db.SetQuestions.Where(u => u.ProjectID == id_p).Count();
        }

        public JsonResult getIdQuestions(int id_p)
        {
            using (GroupContext context_group = new GroupContext())
            {
                System.Diagnostics.Debug.WriteLine("Get question on project -> " + id_p);
                var result = context_group.SetGroupModels.Where(u => u.ProjectID == id_p && u.GroupID != null).OrderBy(u => u.IndexQuestion).Select(u => u.QuestionID).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult getAnswer(int id_q)
        {

            var result = db.SetAnswers.Where(u => u.QuestionID == id_q);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult showResult()
        {
            return PartialView(listResult);
        }

        [HttpGet]
        public JsonResult getAnswerResult(int id_q)
        {

            List<AnswerModel> tmp = new List<AnswerModel>();

            foreach (var item in listResult)
            {

                if (item.answerResult.QuestionID == id_q)
                {

                    tmp.Add(item.answerResult);
                }
            }

            return Json(tmp, JsonRequestBehavior.AllowGet);
        }

        public async Task<int> SaveData(string name, int project_id, int operator_id, string phone_number, List<SaveDataModel> list, string time_begin)
        {

            ResultModel result = new ResultModel();
            result.ProjectID = project_id;
            result.UserID = operator_id;
            result.UserName = name;

            //!!!Переделать присвоение ииндексов!!!
            result.ResultIndex = 1;

            result.PhoneNumber = phone_number;
            result.Data = DateTime.Parse(time_begin);
            result.Time = DateTime.Now.ToString();
            result.CoordWidth = "0";
            result.CoordHeight = "0";
            db2.SetResultModels.Add(result);
            await db2.SaveChangesAsync();

            //Выгрузка данных анкетирования в базу
            int blank_id = db2.SetResultModels.Where(u => u.ProjectID == project_id).Count();
            result.BlankID = blank_id;
            await db2.SaveChangesAsync();
            int result_id = db2.SetResultModels.Where(u => u.ProjectID == result.ProjectID).ToList().Last().Id;

            List<BlankModel> tmp_list = new List<BlankModel>();
            System.Diagnostics.Debug.WriteLine("Length -> " + list.Count);
            foreach (var item in list)
            {
                BlankModel tmp = new BlankModel();
                if (item.Id > 0)
                {
                    tmp.QuestionID = item.QuestionID;
                    tmp.AnswerID = item.Id;
                    if (db.SetAnswers.FirstOrDefault(u => u.Id == item.Id) != null)
                        tmp.AnswerIndex = db.SetAnswers.FirstOrDefault(u => u.Id == item.Id).Index;
                    else
                        tmp.AnswerIndex = item.Id;
                    if (item.Text != "null" && item.Text != "undefined")
                    {
                        tmp.Text = item.Text;
                    }
                    tmp.BlankID = result.Id;
                }
                else
                {
                    if (item.Id != -404)
                    {
                        tmp.QuestionID = item.QuestionID;
                        tmp.AnswerIndex = item.Id;
                        tmp.AnswerID = db.SetAnswerBaseModels.FirstOrDefault(u => u.BaseIndex == item.Id).Id;
                        tmp.BlankID = result.Id;
                    }
                    else
                    {
                        tmp.QuestionID = item.QuestionID;
                        tmp.AnswerID = 404;
                        tmp.AnswerIndex = -404;
                        tmp.BlankID = result.Id;
                    }
                }
                tmp_list.Add(tmp);
                tmp = null;
            }

            db2.SetBlankModels.AddRange(tmp_list);
            await db2.SaveChangesAsync();

            return blank_id;
        }

        [HttpGet]
        public JsonResult getReadyAnswer(int id_q)
        {
            List<int> tmp = new List<int>();
            System.Diagnostics.Debug.WriteLine("Get from client -> " + id_q);

            foreach (var item in listResult.Where(u => u.questionResult.Id == id_q))
            {
                tmp.Add(item.answerResult.Id);
            }

            foreach (var item in listResult)
            {
                System.Diagnostics.Debug.WriteLine("Sending -> " + item.answerResult.AnswerText + " " + item.questionResult.Id);
            }
            foreach (var item in tmp)
            {
                System.Diagnostics.Debug.WriteLine("Sending all ready ansswer to client ... " + item);
                listResult.Remove(listResult.First(u => u.answerResult.Id == item));
            }

            return Json(tmp, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult getBlanks(int id_project)
        {
            return Json(db2.SetResultModels.Where(u => u.ProjectID == id_project).ToList(), JsonRequestBehavior.AllowGet);
        }
    }
}