using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

using SocialFORM.Models.Question;
using SocialFORM.Models.Project;
using SocialFORM.Models.Group;
using SocialFORM.Models;
using Newtonsoft.Json;
using SocialFORM.Models.Utilite;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SocialFORM.Controllers
{
    public class QuestionController : Controller
    {
        QuestionContext db = new QuestionContext();
        ApplicationContext db2 = new ApplicationContext();
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
        public void Answer(AnswerModel tmp)
        {
            if (ModelState.IsValid)
            {
                
                if (tmp.Id > 0)
                {
                    QuestionModel question = db.SetQuestions.Where(u => u.Id == tmp.QuestionID).FirstOrDefault();
                    AnswerModel answer = db.SetAnswers.Where(u => u.Id == tmp.Id).FirstOrDefault();
                    answer.QuestionID = question.Id;
                    answer.AnswerText = tmp.AnswerText;
                    answer.isFreeArea = tmp.isFreeArea;
                    db.SaveChanges();
                    //answerAll.AnswerKey = tmp.Id;
                    //answerAll.QuestionID = tmp.QuestionID;
                    //answerAll.AnswerType = 1;
                    //db.Entry(answerAll).State = EntityState.Modified;
                }
                else
                {
                    AnswerAll answerAll = new AnswerAll();
                    db.Entry(tmp).State = EntityState.Added;
                    db.SaveChanges();
                    answerAll.AnswerKey = tmp.Id;
                    answerAll.QuestionID = tmp.QuestionID;
                    answerAll.AnswerType = 1;
                    answerAll.BindGroup = null;
                    setAnswerAll(answerAll);
                }
            }
        }

        [HttpPost]
        public void deleteAnswer(int Id)
        {
            AnswerModel answer = db.SetAnswers.Where(u => u.Id == Id).First();
            AnswerAll tmpAnswerAll = db.SetAnswerAll.Where(u => u.AnswerKey == Id).FirstOrDefault();
            List<AnswerModel> list_tmp_answer = db.SetAnswers.Where(u => u.QuestionID == answer.QuestionID && u.Index > answer.Index).ToList();
            if (list_tmp_answer != null)
            {
                int count = answer.Index;
                foreach(var item in list_tmp_answer)
                {
                    item.Index = count;
                    count++;
                }
            }
            DeleteQuota(db.SetQuestions.First(u => u.Id == answer.QuestionID).ProjectID, answer.QuestionID);
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
                DeleteQuota((int)item_tmp.ProjectID, (int)item_tmp.QuestionID);
                if (tmp.TypeQuestion == Models.Question.Type.Table)
                {
                    List<TableRow> table_row_list = db.SetTableRows.Where(u => u.TableID == tmp.Id).ToList();
                    db.SetTableRows.RemoveRange(table_row_list);
                    db.SaveChanges();
                }
                List<AnswerAll> answerAll_tmp = db.SetAnswerAll.Where(u => u.QuestionID == tmp.Id).ToList();
                db.SetAnswerAll.RemoveRange(answerAll_tmp);
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
            System.Diagnostics.Debug.WriteLine("Length of data answer ---->" + data.Count);
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
        public async Task<JsonResult> getAllAnswerGrow(string massiv) // Асинхроный метод выгрузки всех ответов по вопросам
        {
            var myArrayInt = massiv.Split(',').Select(x => Int32.Parse(x)).ToArray();
            List<AnswerModel> list_answer = new List<AnswerModel>();
            foreach (var item in myArrayInt)
            {
                list_answer.AddRange(await db.SetAnswers.Where(u => u.QuestionID == item).ToListAsync());
            }
            return Json(list_answer, JsonRequestBehavior.AllowGet);
        }

        // Нужна ли эта функция?????
        //Функция выгрузки листа id всех ответов
        [HttpGet]
        public async Task<JsonResult> getIdAnswerAllGrow(string massiv) // Асинхронный метод выгрузки всех id ответов по вопросам
        {
            var myArrayInt = massiv.Split(',').Select(x => Int32.Parse(x)).ToArray();
            List<AnswerAll> list_answer_all = new List<AnswerAll>();
            foreach(var item in myArrayInt)
            {
                list_answer_all.AddRange(await db.SetAnswerAll.Where(u => u.QuestionID == item).ToListAsync());
            }
            return Json(list_answer_all, JsonRequestBehavior.AllowGet);
        }

        //Функция выгрузки строк табличного вопроса
        [HttpGet]
        public async Task<JsonResult> getAllTableRow(string massiv) // Асинхронный метод выгрузки всех строк для табличных вопросов
        {
            var myArrayInt = massiv.Split(',').Select(x => Int32.Parse(x)).ToArray();
            List<TableRow> list_table_row = new List<TableRow>();
            foreach (var item in myArrayInt)
            {
                list_table_row.AddRange(await db.SetTableRows.Where(u => u.TableID == item).ToListAsync());
            }
            return Json(list_table_row, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult getAnswerAllCount(int question_id)
        {
            return Json(db.SetAnswerAll.Where(u => u.QuestionID == question_id).Count(), JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public async Task<JsonResult> getListQuestion(int id_p) // Асинхронный метод вывода листа с вопросами
        {
            //List<GroupModel> tmp_list_group = new List<GroupModel>();
            //using (GroupContext group_context = new GroupContext())
            //{
            //    tmp_list_group.AddRange(await group_context.SetGroupModels.Where(u => u.ProjectID == id_p && u.GroupID != null && u.Group == null).OrderBy(u=>u.IndexQuestion).ToListAsync());
            //}
            //List<QuestionModel> tmp_list_question = new List<QuestionModel>();
            //foreach(var item in tmp_list_group)
            //{
                //tmp_list_question.Add(await db.SetQuestions.FirstOrDefaultAsync(u => u.Id == item.QuestionID));
            //}
            return Json(await db.SetQuestions.Where(u=>u.ProjectID == id_p).ToListAsync(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> getListAnswerBase() // Асинхроный метод вывода базовых ответов
        {
            return Json(await db.SetAnswerBaseModels.ToListAsync(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> getListTransition(int id_p) //Асинхроный метод вывода таблицы переходов
        {
            return Json(await db.SetTransition.Where(u => u.ProjectID == id_p).ToListAsync(), JsonRequestBehavior.AllowGet);
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
        public async Task<JsonResult> getListMassk() // Асинхронный метод выгрузки таблицы регулярных выражений
        {
            return Json(await db.GetMassk.ToListAsync(), JsonRequestBehavior.AllowGet);
        }

        /*
         *РАбота с блокировками.
         * 
         */

        [HttpGet]
        public async Task<JsonResult> getListBlocks(int id_p)
        {
            return Json(await db.SetBlock.Where(u => u.ProjectID == id_p).ToListAsync(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult getListBlocksQuestion(int id_q)
        {
            return Json(db.SetBlock.Where(u => u.fromQuestion == id_q).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void setBlock(Block block)
        {
            QuestionModel tmp = db.SetQuestions.FirstOrDefault(u => u.Id == block.fromQuestion);
            db.SetBlock.Add(block);
            tmp.Bind_Blocks = block.toQuestion;
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

        [HttpGet]
        public JsonResult getSchoolDay()
        {
            List<string> tmp = new List<string>();
            foreach(var item in db2.SetSchoolDay.ToList())
            {
                string tmp_str = "";
                tmp_str += item.UserId + "#" + item.Date.ToShortDateString();
                tmp.Add(tmp_str);
            }
            return Json(tmp, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void SetKvot(List<Kvot> _kvot)
        {
            db.SetKvots.AddRange(_kvot);
            db.SaveChanges();
        }

        [HttpGet]
        public JsonResult GetListQuestionKvot(int id_p)
        {
            List<QuestionModel> tmp_list = db.SetQuestions.Where(u => u.ProjectID == id_p && u.IsKvot == true).ToList();
            return Json(tmp_list, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetListKvot(int id_p)
        {
            List<Kvot> tmp = db.SetKvots.Where(u => u.ProjectID == id_p).ToList();
            return Json(tmp, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task MinusCountKvot(int id_kvot)
        {
            Kvot tmp_kvot = await db.SetKvots.FirstOrDefaultAsync(u => u.Id == id_kvot);
            tmp_kvot.CountKvot = tmp_kvot.CountKvot - 1;
            await db.SaveChangesAsync();
        }

        [HttpPost]
        public void DeleteKvot(int id_question)
        {
            List<Kvot> tmp_kvot = db.SetKvots.Where(u => u.QuestionID == id_question).ToList();
            db.SetKvots.RemoveRange(tmp_kvot);
            db.SaveChanges();
        }

        [HttpPost]
        public void ChangeKvot(List<string> new_changes)
        {
            foreach(var item in new_changes)
            {
                System.Diagnostics.Debug.WriteLine(item);
                string[] tmp = item.Split('#');
                int id_kvot = Int32.Parse(tmp[0]);
                Kvot tmp_kvot = db.SetKvots.FirstOrDefault(u => u.Id == id_kvot);
                tmp_kvot.CountKvot = Int32.Parse(tmp[1]);
                db.SaveChanges();
            }
        }

        [HttpPost]
        public void SaveQuota(int id_p, List<string> list_quota)
        {
            foreach(var item in list_quota)
            {
                var tmp_string = item.Split('=');
                string chain_string = tmp_string[0];
                QuotaModel tmp_quota = db.SetQuotaModels.FirstOrDefault(u => u.ChainString == chain_string);
                if (tmp_quota == null)
                {
                    tmp_quota = new QuotaModel();
                    tmp_quota.ProjectID = id_p;
                    tmp_quota.ChainString = tmp_string[0];
                    tmp_quota.QuotaCount = Int32.Parse(tmp_string[1]);
                    db.SetQuotaModels.Add(tmp_quota);
                } else
                {
                    tmp_quota.QuotaCount = Int32.Parse(tmp_string[1]);
                }
            }
            db.SaveChanges();
        }

        [HttpGet]
        public JsonResult GetQuota(int id_p)
        {
            return Json(db.SetQuotaModels.Where(u => u.ProjectID == id_p).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void SaveRange(int id_p, int bind_q, List<string> list_range)
        {
            foreach(var item in list_range)
            {
                var str_tmp = item.Split('#');
                string range_pos = str_tmp[0];
                int index_range = Int32.Parse(str_tmp[1]);
                RangeModel rangeModel = db.SetRangeModels.FirstOrDefault(u => u.ProjectID == id_p && u.BindQuestion == bind_q && u.RangeString == range_pos);
                if (rangeModel == null)
                {
                    rangeModel = new RangeModel();
                    rangeModel.ProjectID = id_p;
                    rangeModel.BindQuestion = bind_q;
                    rangeModel.RangeString = range_pos;
                    rangeModel.IndexRange = index_range;
                    db.SetRangeModels.Add(rangeModel);
                }
            }
            db.SaveChanges();
        }

        [HttpGet]
        public JsonResult GetRange(int id_p, int bind_q)
        {
            return Json(db.SetRangeModels.Where(u => u.ProjectID == id_p && u.BindQuestion == bind_q).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAllRange(int id_p)
        {
            return Json(db.SetRangeModels.Where(u => u.ProjectID == id_p).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void DeleteRangePos(int id_range, int id_q)
        {
            RangeModel tmp = db.SetRangeModels.FirstOrDefault(u => u.Id == id_range);
            if (tmp != null)
            {
                DeleteQuota(tmp.ProjectID, id_q);
                db.SetRangeModels.Remove(tmp);
                db.SaveChanges();
            }
        }

        [HttpGet]
        public JsonResult CheckQuotaCount(int id_quota)
        {
            return Json(db.SetQuotaModels.FirstOrDefault(u => u.Id == id_quota), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task SubtractionQuota(int id_quota)
        {
            QuotaModel quotaModel = await db.SetQuotaModels.FirstOrDefaultAsync(u => u.Id == id_quota);
            if (quotaModel != null)
            {
                quotaModel.QuotaCount -= 1;
                await db.SaveChangesAsync();
            }
        }

        [HttpPost]
        public void DeleteQuota(int id_p, int id_q)
        {
            List<QuotaModel> tmp_quota = db.SetQuotaModels.Where(u => u.ProjectID == id_p).ToList();
            List<QuotaModel> quota_list_to_remove = new List<QuotaModel>();
            foreach(var item in tmp_quota)
            {
                if (item.ChainString.Contains(id_q.ToString()))
                {
                    quota_list_to_remove.Add(item);
                }
            }
            db.SetQuotaModels.RemoveRange(quota_list_to_remove);
            db.SaveChanges();
        }

        public ActionResult Loop(int id_p)
        {
            ViewBag.ProjectID = id_p;
            return PartialView();
        }

        [HttpPost]
        public int SetLoopRange(LoopModel tmp)
        {
            int id_loop;
            db.SetLoopModels.Add(tmp);
            db.SaveChanges();
            id_loop = tmp.Id;
            return id_loop;
        }

        [HttpGet]
        public JsonResult GetLoopRange(int id_p)
        {
            return Json(db.SetLoopModels.Where(u => u.ProjectID == id_p).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void DeleteLoopRange(int id_loop)
        {
            LoopModel tmp = db.SetLoopModels.FirstOrDefault(u => u.Id == id_loop);
            if (tmp != null)
            {
                db.SetLoopModels.Remove(tmp);
                db.SaveChanges();
            }
        }
    }
}