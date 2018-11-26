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

        //Сохранение вопроса в базу данных
        [HttpPost]
        public int Question(QuestionModel q)
        {
            QuestionModel tmp = q;
            db.Entry(tmp).State = EntityState.Modified;
            db.SaveChanges();
            return tmp.Id;
        }

        //[HttpGet]
        //public ActionResult SingleFormQuestion(int array)
        //{
        //    QuestionModel q = db.SetQuestions.FirstOrDefault(u => u.Id == array);
        //    ViewBag.Text = q.TextQuestion;
        //    return View();
        //}

        //public string SingleFormQuestion(string answer)
        //{

        //    AnswerModel a = new AnswerModel();
        //    a.AnswerText = answer;
        //    db.SetAnswers.Add(a);
        //    db.SaveChanges();
        //    return answer.ToString();
        //}

        //[HttpGet]
        //public ActionResult MultipleFormQuestion()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public string MultipleFormQuestion(string[] resp)
        //{
        //    string answer = "";
        //    for (int i = 0; i < resp.Length; i += 2)
        //    {
        //        answer += resp[i] + " ";
        //    }
        //    AnswerModel tmp = new AnswerModel();
        //    tmp.AnswerText = answer;
        //    db.SetAnswers.Add(tmp);
        //    db.SaveChanges();
        //    return answer;
        //}

        //Сохранение списка ответов (с переадресацией в другую функцию)
        [HttpPost]
        public void SaveAnswerRange(List<AnswerModel> tmp)
        {
            if (tmp != null)
            {
                List<AnswerModel> list = tmp.OrderBy(u => u.Index).ToList();
                foreach (var item in list)
                {
                    Answer(item);
                }
            }
        }

        //Сохранение одного ответа
        [HttpPost]
        public void Answer(AnswerModel tmp)
        {
            if (ModelState.IsValid)
            {
                if (tmp.Id > 0)
                {
                    QuestionModel question = db.SetQuestions.FirstOrDefault(u => u.Id == tmp.QuestionID);
                    AnswerModel answer = db.SetAnswers.Where(u => u.Id == tmp.Id).FirstOrDefault();
                    answer.QuestionID = question.Id;
                    answer.AnswerText = tmp.AnswerText;
                    answer.isFreeArea = tmp.isFreeArea;
                    answer.Index = tmp.Index;
                    db.SaveChanges();
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

        //Удаление списка ответов (с переадресацией в другую функцию)
        [HttpPost]
        public void DeleteListAnswer(List<int> list)
        {
            foreach(var item in list)
            {
                deleteAnswer(item);
            }
        }

        //Удалить одного ответа
        [HttpPost]
        public void deleteAnswer(int Id)
        {
            AnswerModel answer = db.SetAnswers.Where(u => u.Id == Id).FirstOrDefault();
            if (answer != null)
            {
                AnswerAll tmpAnswerAll = db.SetAnswerAll.Where(u => u.AnswerKey == Id).FirstOrDefault();
                List<AnswerModel> list_tmp_answer = db.SetAnswers.Where(u => u.QuestionID == answer.QuestionID && u.Index > answer.Index).ToList();
                if (list_tmp_answer != null)
                {
                    int count = answer.Index;
                    foreach (var item in list_tmp_answer)
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
        }

        //Взять вопрос по id
        [HttpGet]
        public JsonResult getQuestion(int id)
        {
            QuestionModel question = db.SetQuestions.Where(u => u.Id == id).First();
            return Json(question, JsonRequestBehavior.AllowGet);
        }

        //Удалить вопрос по id
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
                        db2.Entry(item).State = EntityState.Modified;
                        db2.SaveChanges();
                        index++;
                    }

                }
            }

            return 200;
        }

        //Взать ответ по id
        [HttpGet]
        public JsonResult getAnswer(int id_question)
        {
            List<AnswerModel> answers = db.SetAnswers.Where(u => u.QuestionID == id_question).OrderBy(u=>u.Index).ToList();
            return Json(answers, JsonRequestBehavior.AllowGet);
        }

        //Удалить ответы относящиеся к вопрос id_question
        [HttpPost]
        public int deleteAllAnswer(int id_question)
        {
            var result = db.SetAnswers.Where(u => u.QuestionID == id_question);
            List<int> tmp_id_answer = new List<int>();
            foreach (var item in result)
            {
                tmp_id_answer.Add(item.Id);
            }
            DeleteListAnswer(tmp_id_answer);
            return 200;
        }

        //public ActionResult showQuestion()
        //{
        //    return View();
        //}

        //Взятие вопроса по id
        [HttpGet]
        public JsonResult sQuestion(int id)
        {
            QuestionModel tmp = db.SetQuestions.First(u => u.Id == id);
            return Json(tmp, JsonRequestBehavior.AllowGet);
        }

        //Сохранить строки для таблицы
        [HttpPost]
        public void setTableRow(int id_q, List<TableRow> text_row, List<AnswerModel> data)
        {
            List<TableRow> tmp = new List<TableRow>();
            if (text_row != null)
            {
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
            }
            if (data != null)
            {
                foreach (var item in data)
                {
                    Answer(item);
                }
            }
        }

        //Взять строки для таблицы
        [HttpGet]
        public JsonResult getTableRow(int id_q, int id_a)
        {
            if (id_a == 0)
                return Json(db.SetTableRows.Where(u => u.TableID == id_q).ToList(), JsonRequestBehavior.AllowGet);
            else
            {
                AnswerModel tmp = db.SetAnswers.FirstOrDefault(u => u.Id == id_a);
                return Json(db.SetTableRows.Where(u => u.TableID == id_q && (u.IndexRow == tmp.Index || u.IndexRow == null)).ToList(), JsonRequestBehavior.AllowGet);
            }
        }

        //Удалить строки для таблицы
        [HttpPost]
        public void DeleteTableRow(int id_table_row)
        {
            TableRow tmpTableRow = db.SetTableRows.Where(u => u.Id == id_table_row).FirstOrDefault();
            if (tmpTableRow != null)
            {
                db.SetTableRows.Remove(tmpTableRow);
                db.SaveChanges();
            }
        }

        //Удалить список строк для таблицы
        [HttpPost]
        public void DeleteAllTableRow(int id_q)
        {
            if (db.SetTableRows.Where(u=>u.TableID == id_q).Count() != 0)
            {
                List<TableRow> list_tmp = db.SetTableRows.Where(u => u.TableID == id_q).ToList();
                db.SetTableRows.RemoveRange(list_tmp);
                db.SaveChanges();
            }
        }

        //Получение количества строк в таблице
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

        //Получение списка базовых ответов
        [HttpGet]
        public JsonResult getAnswerBase()
        {
            return Json(db.SetAnswerBaseModels.ToList(), JsonRequestBehavior.AllowGet);
        }

        //Сохранение указателя на ответ
        [HttpPost]
        public void setAnswerAll(AnswerAll answer)
        {
            db.SetAnswerAll.Add(answer);
            db.SaveChanges();
        }

        //Получение указателя на ответ
        [HttpGet]
        public JsonResult getAnswerAll(int question_id)
        {
            return Json(db.SetAnswerAll.Where(u => u.QuestionID == question_id).ToList(), JsonRequestBehavior.AllowGet);
        }

        //Удалить указателя на ответ
        [HttpPost]
        public void deleteAnswerAll(int id_answer)
        {
            AnswerAll tmpAnswerAll = db.SetAnswerAll.Where(u => u.Id == id_answer).FirstOrDefault();
            if (tmpAnswerAll != null)
            {
                db.SetAnswerAll.Remove(tmpAnswerAll);
                db.SaveChanges();
            }
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

        //Получение количество указателей на ответ по вопросу
        [HttpGet]
        public JsonResult getAnswerAllCount(int question_id)
        {
            return Json(db.SetAnswerAll.Where(u => u.QuestionID == question_id).Count(), JsonRequestBehavior.AllowGet);
        }

        //Получение списка вопросов относящихся к проекту
        [HttpGet]
        public async Task<JsonResult> getListQuestion(int id_p) // Асинхронный метод вывода листа с вопросами
        {
            return Json(await db.SetQuestions.Where(u=>u.ProjectID == id_p).ToListAsync(), JsonRequestBehavior.AllowGet);
        }

        //Получение списка базовых ответов
        [HttpGet]
        public async Task<JsonResult> getListAnswerBase() // Асинхроный метод вывода базовых ответов
        {
            return Json(await db.SetAnswerBaseModels.ToListAsync(), JsonRequestBehavior.AllowGet);
        }

        //Получение списка переходов
        [HttpGet]
        public async Task<JsonResult> getListTransition(int id_p) //Асинхроный метод вывода таблицы переходов
        {
            return Json(await db.SetTransition.Where(u => u.ProjectID == id_p).ToListAsync(), JsonRequestBehavior.AllowGet);
        }

        //Получение списка переходов относящиеся к вопросу
        [HttpGet]
        public JsonResult getListTransitionQuestion(int id_q)
        {
            return Json(db.SetTransition.Where(u => u.fromQuestion == id_q).ToList(), JsonRequestBehavior.AllowGet);
        }

        //Сохранение перехода
        [HttpPost]
        public int setTransition(Transition transition)
        {
            db.SetTransition.Add(transition);
            db.SaveChanges();
            return transition.Id;
        }

        //Удаление одного перехода
        [HttpPost]
        public void deleteTransition(int id_transition)
        {
            Transition tmp = db.SetTransition.FirstOrDefault(u => u.Id == id_transition);
            db.SetTransition.Remove(tmp);
            db.SaveChanges();
        }

        //Удаление списка переходов относящиеся к вопросу
        [HttpPost]
        public void deleteAllTransition(int id_question)
        {
            List<Transition> list_transition = db.SetTransition.Where(u => u.fromQuestion == id_question).ToList();
            db.SetTransition.RemoveRange(list_transition);
            db.SaveChanges();
        }

        //????
        //[HttpGet]
        //public JsonResult getListQuestionForTransite(int id_p)
        //{
        //    return Json(db.SetQuestions.Where(u => u.ProjectID == id_p).ToList(), JsonRequestBehavior.AllowGet);
        //}

        
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

        //РАбота с блокировками.
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
        public int setBlock(Block block)
        {
            QuestionModel tmp = db.SetQuestions.FirstOrDefault(u => u.Id == block.fromQuestion);
            db.SetBlock.Add(block);
            tmp.Bind_Blocks = block.toQuestion;
            db.SaveChanges();
            return block.Id;
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
            List<Block> list_block = db.SetBlock.Where(u => u.fromQuestion == id_question).ToList();
            db.SetBlock.RemoveRange(list_block);
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
        public void DeleteQuota(int id_p, int id_q)
        {
            bool is_find = false;
            if (db.SetQuotaModels.Where(u => u.ProjectID == id_p).Count() > 0)
            {
                List<QuotaModel> tmp_kvot = db.SetQuotaModels.Where(u => u.ProjectID == id_p).ToList();
                foreach(var item in tmp_kvot)
                {
                    foreach(var arg in item.ChainString.Split('#'))
                    {
                        if (Int32.Parse(arg.Split('/')[0]) == id_q)
                        {
                            is_find = true;
                        }
                    }
                    if (is_find)
                    {
                        db.SetQuotaModels.Remove(item);
                        is_find = false;
                    }
                }
                db.SaveChanges();
            }
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
            if (list_quota != null)
            {
                foreach (var item in list_quota)
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
                    }
                    else
                    {
                        tmp_quota.QuotaCount = Int32.Parse(tmp_string[1]);
                    }
                }
                db.SaveChanges();
            }
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

        [HttpPost]
        public void DeletaAllRangeQ(int id_question)
        {
            if (db.SetRangeModels.Where(u=>u.BindQuestion == id_question).Count() > 0)
            {
                List<RangeModel> list_tmp = db.SetRangeModels.Where(u => u.BindQuestion == id_question).ToList();
                db.SetRangeModels.RemoveRange(list_tmp);
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

        public ActionResult BaseAnswer()
        {
            return PartialView();
        }

        [HttpGet]
        public JsonResult GetListBaseAnswer()
        {
            return Json(db.SetAnswerBaseModels.ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public int AddBaseAnswer(AnswerBaseModel tmp)
        {
            db.SetAnswerBaseModels.Add(tmp);
            db.SaveChanges();
            int id_base = tmp.Id;
            return id_base;
        }

        [HttpPost]
        public void DeleteBaseAnswer(int id_base)
        {
            AnswerBaseModel tmp = db.SetAnswerBaseModels.FirstOrDefault(u => u.Id == id_base);
            if (tmp != null)
            {
                List<AnswerAll> tmp_remove_list = db.SetAnswerAll.Where(u => u.AnswerKey == id_base && u.AnswerType == 2).ToList();
                db.SetAnswerAll.RemoveRange(tmp_remove_list);
                db.SetAnswerBaseModels.Remove(tmp);
                db.SaveChanges();
            }
        }

        [HttpPost]
        public void SaveChangeBaseAnswer(AnswerBaseModel tmp)
        {
            AnswerBaseModel change_item = db.SetAnswerBaseModels.FirstOrDefault(u => u.Id == tmp.Id);
            if (change_item != null)
            {
                change_item.AnswerText = tmp.AnswerText;
                change_item.BaseIndex = tmp.BaseIndex;
                change_item.Transcription = tmp.Transcription;
                db.SaveChanges();
            }
        }
    }
}