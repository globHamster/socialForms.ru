using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

using SocialFORM.Models.Question;
using SocialFORM.Models.Project;
using SocialFORM.Models.Group;
using Newtonsoft.Json;
using SocialFORM.Models.Utilite;

namespace SocialFORM.Controllers
{
    public class QuestionController : Controller
    {
        QuestionContext db = new QuestionContext();
        // GET: Question


        public ActionResult ListQuestions(int project_id)
        {


            IEnumerable<QuestionModel> result = db.SetQuestions
                .Where(u => u.ProjectID == project_id)
                .ToList();

            int[] array = new int[result.Count()];

            int count = 0;
            foreach (var i in result)
            {
                array[count] = i.Id;
                count++;
            }

            ViewBag.Array = array;
            ViewBag.Count = 0;
            return PartialView(result);
        }

        public ActionResult Question(int array)
        {

            QuestionModel q = db.SetQuestions.FirstOrDefault(u => u.Id == 1);
            IEnumerable<ProjectModel> projects;
            using (ProjectContext project_db = new ProjectContext())
            {
                projects = project_db.SetProjectModels;
            }
            ViewBag.Text = q.TextQuestion;
            ViewBag.ProList = projects;
            return View();
        }



        [HttpPost]
        public int Question(QuestionModel q)
        {
            System.Diagnostics.Debug.WriteLine("Text : " + q.TextQuestion);
            QuestionModel tmp = q;
            if (ModelState.IsValid)
            {
                db.Entry(tmp).State = EntityState.Modified;
                db.SaveChanges();
                return tmp.Id;

            }

            return 0;

        }

        [HttpPost]
        public void Info(string str)
        {
            System.Diagnostics.Debug.WriteLine("Post test : " + str);
        }

        [HttpGet]
        public ActionResult SingleFormQuestion(int array)
        {
            QuestionModel q = db.SetQuestions.FirstOrDefault(u => u.Id == array);
            ViewBag.Text = q.TextQuestion;
            return View();
        }

        public string SingleFormQuestion(string answer)
        {

            AnswerModel a = new AnswerModel();
            a.AnswerText = answer;
            db.SetAnswers.Add(a);
            db.SaveChanges();


            return answer.ToString();
        }

        [HttpGet]
        public ActionResult MultipleFormQuestion()
        {
            return View();
        }

        [HttpPost]
        public string MultipleFormQuestion(string[] resp)
        {

            string answer = "";
            for (int i = 0; i < resp.Length; i += 2)
            {
                answer += resp[i] + " ";
            }
            AnswerModel tmp = new AnswerModel();
            tmp.AnswerText = answer;
            db.SetAnswers.Add(tmp);
            db.SaveChanges();
            return answer;
        }

        [HttpPost]
        public JsonResult Answer(AnswerModel tmp)
        {
            if (ModelState.IsValid)
            {
                AnswerAll answerAll = new AnswerAll();
                if (tmp.Id > 0)
                {
                    QuestionModel question = db.SetQuestions.Where(u => u.Id == tmp.QuestionID).FirstOrDefault();
                    AnswerModel answer = db.SetAnswers.Where(u => u.Id == tmp.Id).FirstOrDefault();
                    answer.QuestionID = question.Id;
                    answer.AnswerText = tmp.AnswerText;
                    db.SaveChanges();
                    //answerAll.AnswerKey = tmp.Id;
                    //answerAll.QuestionID = tmp.QuestionID;
                    //answerAll.AnswerType = 1;
                    //db.Entry(answerAll).State = EntityState.Modified;
                }
                else
                {
                    db.Entry(tmp).State = EntityState.Added;
                    db.SaveChanges();
                    answerAll.AnswerKey = db.SetAnswers.ToList().Last().Id;
                    answerAll.QuestionID = db.SetAnswers.ToList().Last().QuestionID;
                    answerAll.AnswerType = 1;
                    db.SetAnswerAll.Add(answerAll);
                    db.SaveChanges();
                }
            }

            return Json(db.SetAnswers.ToList().Last());
        }

        [HttpPost]
        public void deleteAnswer(int Id)
        {
            AnswerModel answer = db.SetAnswers.Where(u => u.Id == Id).First();
            AnswerAll tmpAnswerAll = db.SetAnswerAll.Where(u => u.AnswerKey == Id).FirstOrDefault();
            db.SetAnswerAll.Remove(tmpAnswerAll);
            db.Entry(answer).State = EntityState.Deleted;
            db.SaveChanges();

        }

        [HttpGet]
        public JsonResult getQuestion(int id)
        {
            System.Diagnostics.Debug.WriteLine("Get id -> " + id);
            QuestionModel question = db.SetQuestions.Where(u => u.Id == id).First();
            return Json(question, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public int deleteQuestion(int id)
        {
            QuestionModel tmp = db.SetQuestions.FirstOrDefault(u => u.Id == id);

            using (GroupContext db2 = new GroupContext())
            {
                GroupModel item_tmp = db2.SetGroupModels.FirstOrDefault(u => u.QuestionID == tmp.Id);
                if (tmp.TypeQuestion == Models.Question.Type.Table)
                {
                    List<TableRow> table_row_list = db.SetTableRows.Where(u => u.TableID == tmp.Id).ToList();
                    db.SetTableRows.RemoveRange(table_row_list);
                    db.SaveChanges();
                }
                db.SetQuestions.Remove(tmp);
                db.SaveChanges();
                List<GroupModel> list_tmp = db2.SetGroupModels.Where(u => u.ProjectID == tmp.ProjectID && u.GroupID == item_tmp.GroupID && u.IndexQuestion > item_tmp.IndexQuestion).ToList();
                if (list_tmp != null)
                {
                    int index = 0;
                    foreach (var item in list_tmp)
                    {
                        item.IndexQuestion = item_tmp.IndexQuestion + index;
                        item.GroupName = "Вопрос " + (item_tmp.IndexQuestion + index);
                        db2.Entry(item).State = EntityState.Modified;
                        db2.SaveChanges();
                        index++;
                    }

                }
            }

            return 200;
        }

        [HttpGet]
        public JsonResult getAnswer(int id_question)
        {
            System.Diagnostics.Debug.WriteLine("Зашло " + id_question);
            List<AnswerModel> answers = db.SetAnswers.Where(u => u.QuestionID == id_question).ToList();
            return Json(answers.ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public int deleteAllAnswer(int id_question)
        {
            var result = db.SetAnswers.Where(u => u.QuestionID == id_question);
            foreach (var item in result)
            {
                db.SetAnswers.Remove(item);
            }
            db.SaveChanges();
            return 200;
        }

        public ActionResult showQuestion()
        {
            return View();
        }

        [HttpGet]
        public JsonResult sQuestion(int id)
        {
            QuestionModel tmp = db.SetQuestions.First(u => u.Id == id);
            return Json(tmp, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void setTableRow(int id_q, List<TableRow> text_row, List<AnswerModel> data)
        {
            List<TableRow> tmp = new List<TableRow>();
            foreach (var item in text_row)
            {
                if (item.Id > 0)
                {
                    db.Entry(item).State = EntityState.Modified;
                }
                else
                {
                    db.Entry(item).State = EntityState.Added;
                }
            }

            db.SetTableRows.AddRange(tmp);
            db.SaveChanges();
            foreach (var item in data)
            {
                Answer(item);
            }

        }

        [HttpGet]
        public JsonResult getTableRow(int id_q, int id_a)
        {
            System.Diagnostics.Debug.WriteLine("Get element TableRow - >" + id_q + " " + id_a);
            if (id_a == 0)
                return Json(db.SetTableRows.Where(u => u.TableID == id_q).ToList(), JsonRequestBehavior.AllowGet);
            else
            {
                AnswerModel tmp = db.SetAnswers.FirstOrDefault(u => u.Id == id_a);
                return Json(db.SetTableRows.Where(u => u.TableID == id_q && (u.IndexRow == tmp.Index || u.IndexRow == null)).ToList(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public void DeleteTableRow(int id_table_row)
        {
            TableRow tmpTableRow = db.SetTableRows.Where(u => u.Id == id_table_row).FirstOrDefault();
            db.SetTableRows.Remove(tmpTableRow);
            db.SaveChanges();
        }

        [HttpGet]
        public JsonResult getTableRowCount(int id_q)
        {
            var is_bind = db.SetQuestions.FirstOrDefault(u => u.Id == id_q).Bind;
            if (is_bind != null)
            {
                List<int?> list_row = db.SetTableRows.Where(u => u.TableID == id_q).OrderBy(u => u.IndexRow).Select(u => u.IndexRow).ToList();
                int max = 0;
                int count = 0;
                int count_null = 0;
                int? prev = list_row[0];
                foreach (var item in list_row)
                {
                    if (item != prev)
                    {
                        prev = item;
                        count = 1;
                        if (count > max) max = count;
                    }
                    else
                    {
                        if (prev != null)
                        {
                            count++;
                        }
                        else
                        {
                            count_null++;
                        }
                    }
                }
                return Json(max + count_null, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(db.SetTableRows.Where(u => u.TableID == id_q).Count(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult getAnswerBase()
        {
            return Json(db.SetAnswerBaseModels.ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void setAnswerAll(AnswerAll answer)
        {
            db.SetAnswerAll.Add(answer);
            db.SaveChanges();
        }

        [HttpGet]
        public JsonResult getAnswerAll(int question_id)
        {
            return Json(db.SetAnswerAll.Where(u => u.QuestionID == question_id).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void deleteAnswerAll(int id_answer)
        {
            AnswerAll tmpAnswerAll = db.SetAnswerAll.Where(u => u.Id == id_answer).FirstOrDefault();
            db.SetAnswerAll.Remove(tmpAnswerAll);
            db.SaveChanges();
        }

        //Функция выгузки листа вопросов
        [HttpGet]
        public JsonResult getAllAnswerGrow(string massiv)
        {
            var myArrayInt = massiv.Split(',').Select(x => Int32.Parse(x)).ToArray();
            List<AnswerModel> list_answer = new List<AnswerModel>();
            foreach (var item in myArrayInt)
            {
                list_answer.AddRange(db.SetAnswers.Where(u => u.QuestionID == item).ToList());
            }
            return Json(list_answer, JsonRequestBehavior.AllowGet);
        }

        //Функция выгрузки листа id всех ответов
        [HttpGet]
        public JsonResult getIdAnswerAllGrow(string massiv)
        {
            var myArrayInt = massiv.Split(',').Select(x => Int32.Parse(x)).ToArray();
            List<AnswerAll> list_answer_all = new List<AnswerAll>();
            foreach(var item in myArrayInt)
            {
                list_answer_all.AddRange(db.SetAnswerAll.Where(u => u.QuestionID == item).ToList());
            }
            return Json(list_answer_all, JsonRequestBehavior.AllowGet);
        }

        //Функция выгрузки строк табличного вопроса
        [HttpGet]
        public JsonResult getAllTableRow(string massiv)
        {
            var myArrayInt = massiv.Split(',').Select(x => Int32.Parse(x)).ToArray();
            List<TableRow> list_table_row = new List<TableRow>();
            foreach (var item in myArrayInt)
            {
                list_table_row.AddRange(db.SetTableRows.Where(u => u.TableID == item).ToList());
            }
            return Json(list_table_row, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult getAnswerAllCount(int question_id)
        {
            return Json(db.SetAnswerAll.Where(u => u.QuestionID == question_id).Count(), JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult getListQuestion(int id_p)
        {
            return Json(db.SetQuestions.Where(u => u.ProjectID == id_p).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult getListAnswerBase()
        {
            return Json(db.SetAnswerBaseModels.ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult getListTransition(int id_p)
        {
            return Json(db.SetTransition.Where(u => u.ProjectID == id_p).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult getListTransitionQuestion(int id_q)
        {
            return Json(db.SetTransition.Where(u => u.fromQuestion == id_q).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void setTransition(Transition transition)
        {
            db.SetTransition.Add(transition);
            db.SaveChanges();
        }

        [HttpPost]
        public void deleteTransition(int id_transition)
        {
            Transition tmp = db.SetTransition.FirstOrDefault(u => u.Id == id_transition);
            db.SetTransition.Remove(tmp);
            db.SaveChanges();
        }

        [HttpPost]
        public void deleteAllTransition(int id_question)
        {
            List<Transition> list_transition = db.SetTransition.Where(u => u.fromQuestion == id_question).ToList();
            db.SetTransition.RemoveRange(list_transition);
            db.SaveChanges();
        }

        [HttpGet]
        public JsonResult getListQuestionForTransite(int id_p)
        {
            return Json(db.SetQuestions.Where(u => u.ProjectID == id_p).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult getMassk(int Id)
        {
            return Json(db.GetMassk.FirstOrDefault(m => m.Id == Id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult getListMassk()
        {
            return Json(db.GetMassk.ToList(), JsonRequestBehavior.AllowGet);
        }

        /*
         *РАбота с блокировками.
         * 
         */

        [HttpGet]
        public JsonResult getListBlocks(int id_p)
        {
            return Json(db.SetBlock.Where(u => u.ProjectID == id_p).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult getListBlocksQuestion(int id_q)
        {
            return Json(db.SetBlock.Where(u => u.fromQuestion == id_q).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void setBlock(Block block)
        {
            db.SetBlock.Add(block);
            db.SaveChanges();
        }

        [HttpPost]
        public void deleteBlocks(int id_block)
        {
            Block tmp = db.SetBlock.FirstOrDefault(u => u.Id == id_block);
            db.SetBlock.Remove(tmp);
            db.SaveChanges();
        }

        [HttpPost]
        public void deleteAllBlocks(int id_question)
        {
            List<Transition> list_transition = db.SetTransition.Where(u => u.fromQuestion == id_question).ToList();
            db.SetTransition.RemoveRange(list_transition);
            db.SaveChanges();
        }

        [HttpGet]
        public JsonResult getListQuestionForBlocks(int id_p)
        {
            return Json(db.SetQuestions.Where(u => u.ProjectID == id_p).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetListFile(int id_p)
        {
            return Json(db.GetFiles.Where(u => u.ProjectID == id_p).ToList(), JsonRequestBehavior.AllowGet);
        }
    }
}