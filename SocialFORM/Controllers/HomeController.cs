using SocialFORM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using SocialFORM.Models.Project;
using SocialFORM.Models.Menu;
using SocialFORM.Models.Authentication;
using SocialFORM.Models.Question;
using SocialFORM.Models.Group;
using SocialFORM.Models.Form;
using SocialFORM.Models.Session;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using PagedList.Mvc;
using PagedList;

namespace SocialFORM.Controllers
{
    public class HomeController : Controller
    {
        ApplicationContext db = new ApplicationContext();
        ProjectContext db2 = new ProjectContext();

        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin , Operator")]
        public ActionResult _Index()
        {
            string result2 = "Вы не авторизованы";
            if (User.Identity.IsAuthenticated)
            {
                result2 = User.Identity.Name;
            }
            ViewBag.res = result2;

            HttpCookie cookieReq = Request.Cookies["cookieAuth"];
            string cookieString = null;
            if (cookieReq != null)
            {
                cookieString = CryporEngine.Decrypt(cookieReq["Login"], true);
            }
            IQueryable<UserViewModel> result = (
            from User in db.SetUser
            join DataUsers in db.SetDataUsers on User.Id equals DataUsers.UserId
            join Role in db.SetRoles on User.RoleId equals Role.Id
            where User.Login == cookieString
            select new UserViewModel
            {
                IdView = User.Id,
                LoginView = User.Login,
                PasswordView = User.Password,
                NameView = DataUsers.Name,
                FamilyView = DataUsers.Family,
                AgeView = DataUsers.Age,
                FoolView = DataUsers.Fool,
                EmailView = DataUsers.Email,
                SchoolDayView = User.SchoolDay,
                RoleView = Role.Name
            }
            );
            ViewBag.operator_id = result.First().IdView;
            ViewData["Name"] = result.First().NameView;
            ViewData["Family"] = result.First().FamilyView;
            ViewData["Role"] = result.First().RoleView;
            //
            // Проверка учебного дня
            //
            int UID = result.First().IdView;
            if (result.First().SchoolDayView == true)
            {
                if (db.SetSchoolDay.FirstOrDefault(u => u.UserId == UID && DbFunctions.TruncateTime(u.Date) == DbFunctions.TruncateTime(DateTime.Now)) == null)
                {
                    db.SetSchoolDay.Add(new Models.SchoolDay { UserId = UID, Date = DateTime.Now });
                    db.SaveChanges();
                }
            }


            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult Menu()
        {
            List<Models.Menu.MenuItem> menuItems = db.SetMenuItems.ToList();
            return PartialView(menuItems);
        }

        public ActionResult MenuMin()
        {
            List<Models.Menu.MenuItem> menuItems = db.SetMenuItems.ToList();
            return PartialView(menuItems);
        }
        //
        //ОТРИСОВКА СПИСКА ПРОЕКТОВ
        ProjectContext db4 = new ProjectContext();
        public static List<ProjectModel> listProject = new List<ProjectModel>();
        public static List<ProjectModel> tmp_listProject = new List<ProjectModel>();
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Project(int? page)
        {
            tmp_listProject = db4.SetProjectModels.ToList();
            listProject = tmp_listProject.Reverse<ProjectModel>().ToList();
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return PartialView("_Project", listProject.ToPagedList(pageNumber, pageSize));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult _Project(int? page)
        {
            tmp_listProject = db4.SetProjectModels.ToList();
            listProject = tmp_listProject.Reverse<ProjectModel>().ToList();
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return PartialView("_Project", listProject.ToPagedList(pageNumber, pageSize));
        }
        //

        [Authorize(Roles = "Operator")]
        public ActionResult ProjectOperator()
        {
            return PartialView(db4.SetProjectModels.Where(u => u.ActionProject == true));
        }

        [Authorize(Roles = "Operator")]
        public ActionResult BeginForm(int id_p)
        {
            ViewBag.ProjectID = id_p;
            return PartialView();
        }

        List<UserViewModel> listUsers = null;
        [HttpGet]
        public ActionResult Users(int? page)
        {
            IQueryable<UserViewModel> result = (
            from User in db.SetUser
            join DataUsers in db.SetDataUsers on User.Id equals DataUsers.UserId
            select new UserViewModel
            {
                IdView = User.Id,
                LoginView = User.Login,
                PasswordView = User.Password,
                NameView = DataUsers.Name,
                FamilyView = DataUsers.Family,
                AgeView = DataUsers.Age,
                FoolView = DataUsers.Fool,
                EmailView = DataUsers.Email,
            }
            );
            listUsers = result.ToList().OrderBy(u => u.FamilyView).ToList();
            int pageSize = 14;
            int pageNumber = (page ?? 1);
            return PartialView("_Users", listUsers.ToPagedList(pageNumber, pageSize));
            //return PartialView(result.ToList());
        }

        [HttpGet]
        public ActionResult _Users(int? page)
        {
            IQueryable<UserViewModel> result = (
            from User in db.SetUser
            join DataUsers in db.SetDataUsers on User.Id equals DataUsers.UserId
            select new UserViewModel
            {
                IdView = User.Id,
                LoginView = User.Login,
                PasswordView = User.Password,
                NameView = DataUsers.Name,
                FamilyView = DataUsers.Family,
                AgeView = DataUsers.Age,
                FoolView = DataUsers.Fool,
                EmailView = DataUsers.Email,
            }
            );
            listUsers = result.ToList();
            int pageSize = 14;
            int pageNumber = (page ?? 1);
            return PartialView("_Users", listUsers.ToPagedList(pageNumber, pageSize));
        }

        [HttpGet]
        public JsonResult AddProject(string name_project)
        {
            System.Diagnostics.Debug.WriteLine("Adding new project in table ..." + name_project);
            ProjectModel tmp = new ProjectModel();
            tmp.NameProject = name_project;
            db4.SetProjectModels.Add(tmp);
            db4.SaveChanges();
            tmp = db4.SetProjectModels.ToList().Last();
            GroupModel tmp2 = new GroupModel
            {
                Group = 0,
                ProjectID = tmp.Id
            };
            using (GroupContext db_group = new GroupContext())
            {
                db_group.SetGroupModels.Add(tmp2);
                db_group.SaveChanges();
            }
            return Json(tmp, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult Test(int Id)
        //{ 
        //    string[] I = { Id.ToString() };
        //    ActionResult viewuser = ViewUser(Id);
        //    return viewuser;
        //}

        //
        //Отрисовка блока с результатами (НАЧАЛО)
        //
        List<ResultModel> tmp_tableBlanks = null;
        [HttpGet]
        public ActionResult TableBlanks(int id_project, int? page)
        {
            ViewBag.Id_Project_Next = id_project;
            using (ProjectContext project_db = new ProjectContext())
            {
                ViewBag.ProjectName = project_db.SetProjectModels.First(u => u.Id == id_project).NameProject;
            }
            tmp_tableBlanks = db.SetResultModels.Where(u => u.ProjectID == id_project).ToList();
            int pageSize = 15;
            int pageNumber = (page ?? 1);
            return PartialView(tmp_tableBlanks.ToPagedList(pageNumber, pageSize));
            //return PartialView(tmp);
        }

        //
        //Отрисовка блока с результатами (ПО ПАНЕЛИ)
        //
        [HttpGet]
        public ActionResult _TableBlanks(string id_project, int? page)
        {
            ViewBag.Id_Project_Next = id_project;
            int id_pr = Convert.ToInt32(id_project);
            using (ProjectContext project_db = new ProjectContext())
            {
                ViewBag.ProjectName = project_db.SetProjectModels.First(u => u.Id == id_pr).NameProject;
            }
            tmp_tableBlanks = db.SetResultModels.Where(u => u.ProjectID == id_pr).ToList();
            int pageSize = 15;
            int pageNumber = (page ?? 1);
            return PartialView("_TableBlanks", tmp_tableBlanks.ToPagedList(pageNumber, pageSize));
        }

        [HttpGet]
        public JsonResult TableResultBlank(int id_blank)
        {
            List<BlankModel> tmp = db.SetBlankModels.Where(u => u.BlankID == id_blank).ToList();
            return Json(tmp, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult getResultList(int id_project)
        {
            return Json(db.SetResultModels.Where(u => u.ProjectID == id_project).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void deleteResults(int id)
        {
            List<ResultModel> tmp = db.SetResultModels.Where(u => u.ProjectID == id).ToList();
            List<BlankModel> tmp2 = new List<BlankModel>();
            foreach (var item in tmp)
            {
                tmp2.AddRange(db.SetBlankModels.Where(u => u.BlankID == item.Id).ToList());
            }

            db.SetBlankModels.RemoveRange(tmp2);
            db.SaveChanges();
            db.SetResultModels.RemoveRange(tmp);
            db.SaveChanges();
        }


        public void ExportToEXCEL(int id_p, string name_file)
        {
            List<ResultModel> listResultExport = db.SetResultModels.Where(u => u.ProjectID == id_p).ToList();
            Dictionary<int, List<BlankModel>> listBlankExport = new Dictionary<int, List<BlankModel>>();
            foreach (var item in listResultExport)
            {
                listBlankExport.Add(item.Id, db.SetBlankModels.Where(u => u.BlankID == item.Id).ToList());
            }

            List<GroupModel> listGroupExport = new List<GroupModel>();
            using (GroupContext group_context = new GroupContext())
            {
                listGroupExport.AddRange(group_context.SetGroupModels.Where(u => u.ProjectID == id_p && u.GroupID != null && u.Group == null).OrderBy(u => u.IndexQuestion).ToList());
            }

            Dictionary<int, QuestionModel> listQuestionExport = new Dictionary<int, QuestionModel>();
            using (QuestionContext q_context = new QuestionContext())
            {
                foreach (var item in listGroupExport)
                {
                    listQuestionExport.Add((int)item.QuestionID, q_context.SetQuestions.FirstOrDefault(u => u.Id == item.QuestionID));
                }
            }

            Dictionary<int, List<AnswerModel>> listAnswerAllExport = new Dictionary<int, List<AnswerModel>>();
            Dictionary<int, List<Models.Question.TableRow>> listTableRow = new Dictionary<int, List<Models.Question.TableRow>>();
            foreach (var item in listQuestionExport)
            {
                using (QuestionContext q_context = new QuestionContext())
                {
                    listAnswerAllExport.Add(item.Key, q_context.SetAnswers.Where(u => u.QuestionID == item.Key).ToList());
                    listTableRow.Add(item.Key, q_context.SetTableRows.Where(u => u.TableID == item.Key).ToList());
                }
            }


            var products = new System.Data.DataTable();

            products.Columns.Add("Номер");
            products.Columns.Add("ФИО");
            products.Columns.Add("Номер телефона");
            products.Columns.Add("Начало анкеты");
            products.Columns.Add("Конец анкеты");
            products.Columns.Add("Учебный день");
            foreach (var item in listGroupExport)
            {
                QuestionModel tmp = listQuestionExport[(int)item.QuestionID];
                switch (tmp.TypeQuestion)
                {
                    case Models.Question.Type.Single:
                        if (item.GroupID > 0)
                            products.Columns.Add(("Г" + item.GroupID + " " + item.GroupName.ToString()).ToString());
                        else
                            products.Columns.Add(item.GroupName.ToString());
                        if (listAnswerAllExport[(int)item.QuestionID].Where(u => u.isFreeArea == true).Count() > 0)
                        {
                            if (item.GroupID > 0)
                                products.Columns.Add("Г" + item.GroupID + "\n\a" + item.GroupName + "_др");
                            else
                                products.Columns.Add(item.GroupName + "_др");
                        }
                        break;
                    case Models.Question.Type.Multiple:
                        {
                            int tmp_count = listAnswerAllExport[(int)item.QuestionID].Count();
                            for (int i = 1; i <= tmp_count; i++)
                            {
                                if (item.GroupID > 0)
                                    products.Columns.Add("Г" + item.GroupID + " " + item.GroupName + "_" + i);
                                else
                                    products.Columns.Add(item.GroupName + "_" + i);
                            }
                            if (listAnswerAllExport[(int)item.QuestionID].Where(u => u.isFreeArea == true).Count() == 1)
                            {
                                if (item.GroupID > 0)
                                    products.Columns.Add("Г" + item.GroupID + " " + item.GroupName + "_др");
                                else
                                    products.Columns.Add(item.GroupName + "_др");
                            }
                            else if (listAnswerAllExport[(int)item.QuestionID].Where(u => u.isFreeArea == true).Count() > 1)
                            {
                                int count = 1;
                                foreach (var item_answer in listAnswerAllExport[(int)item.QuestionID].Where(u => u.isFreeArea == true))
                                {
                                    if (item.GroupID > 0)
                                        products.Columns.Add("Г" + item.GroupID + " " + item.GroupName + "_др_" + count);
                                    else
                                        products.Columns.Add(item.GroupName + "_др_" + count);
                                    count++;
                                }
                            }
                        }
                        break;
                    case Models.Question.Type.Free:
                        {
                            int tmp_count = listAnswerAllExport[(int)item.QuestionID].Count();
                            if (tmp_count == 1)
                            {
                                if (item.GroupID > 0)
                                    products.Columns.Add("Г" + item.GroupID + " " + item.GroupName);
                                else
                                    products.Columns.Add(item.GroupName.ToString());
                            }
                            else
                            {
                                for (int i = 1; i <= tmp_count; i++)
                                {
                                    if (item.GroupID > 0)
                                        products.Columns.Add("Г" + item.GroupID + " " + item.GroupName + "_" + i);
                                    else
                                        products.Columns.Add(item.GroupName + "_" + i);
                                }
                            }
                        }
                        break;
                    case Models.Question.Type.Table:
                        {
                            int count_row = listTableRow[(int)item.QuestionID].Count();
                            for (int i = 1; i <= count_row; i++)
                            {
                                if (item.GroupID > 0)
                                    products.Columns.Add("Г" + item.GroupID + " " + item.GroupName + "_" + i);
                                else
                                    products.Columns.Add(item.GroupName + "_" + i);
                            }
                        }
                        break;
                    case Models.Question.Type.Filter:
                        if (item.GroupID > 0)
                        {
                            products.Columns.Add("Г" + item.GroupID + " " + item.GroupName + "_инд");
                            products.Columns.Add("Г" + item.GroupID + " " + item.GroupName + "_текст");
                        }
                        else
                        {
                            products.Columns.Add(item.GroupName + "_инд");
                            products.Columns.Add(item.GroupName + "_текст");
                        }
                        break;
                    default:
                        break;
                }
            }

            List<string> tmp_str = new List<string>();

            foreach (var item in listResultExport)
            {
                tmp_str.Add(item.BlankID.ToString());
                tmp_str.Add(item.UserName);
                tmp_str.Add(item.PhoneNumber);
                tmp_str.Add(item.Data.ToString());
                tmp_str.Add(item.Time);
                List<SchoolDay> list_schoolday = db.SetSchoolDay.ToList();
                string is_introduce_day = "0";
                foreach (var item_schoolday in list_schoolday)
                {
                    if (item.UserID == item_schoolday.UserId && item.Data.Date == item_schoolday.Date.Date)
                    {
                        is_introduce_day = "1";
                        break;
                    }
                }
                tmp_str.Add(is_introduce_day);
                List<BlankModel> tmp_blank = listBlankExport[item.Id];

                foreach (var group_item in listGroupExport)
                {
                    QuestionModel question_item = listQuestionExport[(int)group_item.QuestionID];
                    switch (question_item.TypeQuestion)
                    {
                        case Models.Question.Type.Single:
                            {
                                if (tmp_blank.FirstOrDefault(u => u.QuestionID == group_item.QuestionID) != null)
                                {
                                    if (tmp_blank.FirstOrDefault(u => u.QuestionID == group_item.QuestionID).AnswerIndex != -404)
                                        tmp_str.Add(tmp_blank.FirstOrDefault(u => u.QuestionID == group_item.QuestionID).AnswerIndex.ToString());
                                    else
                                        tmp_str.Add(" ");
                                    if (listAnswerAllExport[(int)group_item.QuestionID].Where(u => u.isFreeArea == true).Count() > 0)
                                    {
                                        if (tmp_blank.FirstOrDefault(u => u.QuestionID == group_item.QuestionID).Text != null)
                                        {
                                            tmp_str.Add(tmp_blank.FirstOrDefault(u => u.QuestionID == group_item.QuestionID).Text);
                                        }
                                        else
                                        {
                                            tmp_str.Add(" ");
                                        }
                                    }
                                }
                                else
                                {
                                    tmp_str.Add(" ");
                                    if (listAnswerAllExport[(int)group_item.QuestionID].Where(u => u.isFreeArea == true).Count() > 0)
                                        tmp_str.Add(" ");
                                }
                            }
                            break;
                        case Models.Question.Type.Multiple:
                            {
                                if (tmp_blank.Where(u => u.QuestionID == group_item.QuestionID).Count() > 0)
                                {
                                    List<BlankModel> tmp_list_blank = tmp_blank.Where(u => u.QuestionID == group_item.QuestionID).ToList();
                                    int count_all_answer = listAnswerAllExport[(int)group_item.QuestionID].Count();
                                    int count_other_column = listAnswerAllExport[(int)group_item.QuestionID].Where(u => u.isFreeArea == true).Count();
                                    List<string> other_column = new List<string>();
                                    foreach (var blank_item in tmp_list_blank)
                                    {
                                        if (blank_item.AnswerIndex != -404)
                                            tmp_str.Add(blank_item.AnswerIndex.ToString());
                                        else
                                            tmp_str.Add(" ");
                                        if (blank_item.Text != null)
                                            other_column.Add(blank_item.Text);
                                        count_all_answer--;
                                    }
                                    for (int i = 0; i < count_all_answer; i++)
                                    {
                                        tmp_str.Add(" ");
                                    }
                                    for (int i = 0; i < count_other_column; i++)
                                    {
                                        if (i <= other_column.Count() - 1)
                                        {
                                            tmp_str.Add(other_column[i]);
                                        }
                                        else
                                        {
                                            tmp_str.Add(" ");
                                        }
                                    }
                                }
                            }
                            break;
                        case Models.Question.Type.Free:
                            {
                                int count_all_answer = listAnswerAllExport[(int)group_item.QuestionID].Count();
                                int count_all_result = tmp_blank.Where(u => u.QuestionID == group_item.QuestionID).Count();
                                List<BlankModel> _blank_list = tmp_blank.Where(u => u.QuestionID == group_item.QuestionID).ToList();
                                for (int i = 0; i < count_all_answer; i++)
                                {
                                    if (i <= count_all_result - 1)
                                    {
                                        tmp_str.Add(_blank_list[i].Text);
                                    }
                                    else
                                        tmp_str.Add(" ");

                                }
                            }
                            break;
                        case Models.Question.Type.Table:
                            {
                                int count_row = listTableRow[(int)group_item.QuestionID].Count();
                                int count_row_result = tmp_blank.Where(u => u.QuestionID == group_item.QuestionID).Count();
                                List<BlankModel> _blank_list = tmp_blank.Where(u => u.QuestionID == group_item.QuestionID).ToList();
                                for (int i = 0; i < count_row; i++)
                                {

                                    if (i <= count_row_result - 1)
                                    {
                                        if (_blank_list[i].AnswerIndex != -404)
                                            tmp_str.Add(_blank_list[i].AnswerIndex.ToString());
                                        else
                                            tmp_str.Add(" ");
                                    }
                                    else
                                    {
                                        tmp_str.Add(" ");
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }

                }
                products.Rows.Add(tmp_str.ToArray());
                tmp_str.Clear();
            }
            var grid = new GridView();

            grid.DataSource = products;
            grid.DataBind();
            Response.ClearContent();
            Response.AddHeader("content-disposition", "attachment; filename=" + name_file + ".xls");
            Response.ContentType = "application/excel";
            Response.ContentEncoding = System.Text.Encoding.GetEncoding("utf-8");
            StringWriter sw = new StringWriter();
            HtmlTextWriter htmlTextWriter = new HtmlTextWriter(sw);

            grid.RenderControl(htmlTextWriter);

            Response.Write(sw.ToString());

            Response.End();
        }


        [HttpPost]
        public void actionProject(int id)
        {
            ProjectModel projectModel = db2.SetProjectModels.Where(u => u.Id == id).FirstOrDefault();
            if (projectModel.ActionProject == true) { projectModel.ActionProject = false; }
            else { projectModel.ActionProject = true; }
            db2.SaveChanges();
        }

        //
        //Статистика
        //
        public ActionResult Statistics()
        {
            List<ResultModel> tmp_statFilter = db.SetResultModels.ToList();
            return PartialView(tmp_statFilter);
        }

        public JsonResult Statistics_project()
        {
            return Json(db2.SetProjectModels.ToList(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Statistics_operator()
        {
            IQueryable<UserViewModel> result = (
            from User in db.SetUser
            join DataUsers in db.SetDataUsers on User.Id equals DataUsers.UserId
            join Role in db.SetRoles on User.RoleId equals Role.Id
            select new UserViewModel
            {
                IdView = User.Id,
                LoginView = User.Login,
                PasswordView = User.Password,
                NameView = DataUsers.Name,
                FamilyView = DataUsers.Family,
                AgeView = DataUsers.Age,
                FoolView = DataUsers.Fool,
                EmailView = DataUsers.Email,
                RoleIdView = Role.Id,
                RoleView = Role.Name
            }
            );
            return Json(result.Where(u => u.RoleIdView == 2).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult getResultListFilter(int id_project, int id_operator, string startTime, string endTime)
        {
            DateTime startTimeDT = Convert.ToDateTime(startTime.Replace("_", " "));
            DateTime endTimeDT = Convert.ToDateTime(endTime.Replace("_", " "));
            System.Diagnostics.Debug.WriteLine("-------------(--->" + id_project + "   " + id_operator);
            if (id_operator > 0)
            {
                return Json(db.SetResultModels.Where(u => u.ProjectID == id_project && u.UserID == id_operator).ToList(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(db.SetResultModels.Where(u => u.ProjectID == id_project && u.Data.CompareTo(startTimeDT) == 1 && u.Data.CompareTo(endTimeDT) == -1).ToList(), JsonRequestBehavior.AllowGet);
            }
        }
        //
        //Отрисовка блока с результатами (НАЧАЛО)
        //
        List<ResultModel> tmp_tableBlanksFilter = null;
        [HttpGet]
        public ActionResult TableBlanksFilter(int id_project, int id_operator, string startTime, string endTime, int? page)
        {
            List<string> list = new List<string>();
            List<string> list_all = new List<string>();
            ViewBag.Id_Project_Next = id_project;
            ViewBag.Id_Operator_Next = id_operator;
            ViewBag.startTimeNext = startTime;
            ViewBag.endTimeNext = endTime;
            DateTime startTimeDT = Convert.ToDateTime(startTime.Replace("_", " "));
            DateTime endTimeDT = Convert.ToDateTime(endTime.Replace("_", " "));
            System.Diagnostics.Debug.WriteLine(startTimeDT);
            System.Diagnostics.Debug.WriteLine(endTimeDT);
            DateTime tmp = new DateTime();
            DateTime tmp2 = new DateTime();
            TimeSpan min = new TimeSpan();
            TimeSpan max = new TimeSpan();

            List<SessionHubModel> session = new List<SessionHubModel>();
            Dictionary<DateTime, List<ResultModel>> keys = new Dictionary<DateTime, List<ResultModel>>();

            using (ProjectContext project_db = new ProjectContext())
            {
                ViewBag.ProjectName = project_db.SetProjectModels.First(u => u.Id == id_project).NameProject;
            }
            //
            //Общие параметры
            int count_all = 0;
            double count_ch_all = 0;
            TimeSpan min_all = new TimeSpan();
            TimeSpan max_all = new TimeSpan();
            DateTime tmp_all = new DateTime();
            DateTime tmp2_all = new DateTime();


            if (id_operator > 0)
            {
                tmp_tableBlanksFilter = db.SetResultModels.Where(u => u.ProjectID == id_project && u.UserID == id_operator && u.Data.CompareTo(startTimeDT) == 1 && u.Data.CompareTo(endTimeDT) == -1).ToList();
                DateTime BeginData = startTimeDT.Date;
                DateTime EndData = endTimeDT.Date;
                int count = 0;
                double count_ch = 0;
                count_ch_all = 0;

                //
                //Вычисление общих значений
                count_all = tmp_tableBlanksFilter.Count;
                min_all = new TimeSpan(23, 59, 59); max_all = new TimeSpan(0, 0, 0);
                foreach (ResultModel item in tmp_tableBlanksFilter)
                {
                    if (min_all >= DateTime.Parse(item.Time).Subtract(item.Data)) { min_all = DateTime.Parse(item.Time).Subtract(item.Data); }
                    if (max_all <= DateTime.Parse(item.Time).Subtract(item.Data)) { max_all = DateTime.Parse(item.Time).Subtract(item.Data); }
                }
                if (count_all > 1)
                {
                    double seconds_all = 0; double secondsOT_all = 0;
                    foreach (ResultModel item in tmp_tableBlanksFilter)
                    {
                        if (item.Time != null)
                        {
                            seconds_all += DateTime.Parse(item.Time).Subtract(item.Data).TotalSeconds;
                        }
                    }
                    seconds_all = seconds_all / count_all;
                    foreach (ResultModel item in tmp_tableBlanksFilter)
                    {
                        if (item.Time != null)
                        {
                            secondsOT_all += Math.Pow(DateTime.Parse(item.Time).Subtract(item.Data).TotalSeconds - seconds_all, 2);
                        }
                    }
                    secondsOT_all /= (tmp_tableBlanksFilter.Count - 1); secondsOT_all = Math.Sqrt(secondsOT_all);
                    int minutesOt = (int)Math.Floor(secondsOT_all / 60);
                    int minutes = (int)Math.Floor(seconds_all / 60);
                    seconds_all -= minutes * 60;
                    secondsOT_all -= minutesOt * 60;
                    tmp_all = new DateTime(1, 1, 1, 0, minutes, (int)seconds_all);
                    tmp2_all = new DateTime(1, 1, 1, 0, minutesOt, (int)secondsOT_all);
                }
                //
                //Количество анкет в час
                List<ResultModel> t_tmp_all = tmp_tableBlanksFilter.Reverse<ResultModel>().ToList();
                if (count_all != 0)
                {
                    TimeSpan t1 = TimeSpan.Parse(t_tmp_all.First().Data.ToLongTimeString()).Subtract(TimeSpan.Parse(tmp_tableBlanksFilter.First().Data.ToLongTimeString()));
                    Double t2 = t1.TotalHours;
                    count_ch_all = count_all / t2;
                }
                //
                //
                while (BeginData <= EndData)
                {
                    keys.Add(BeginData, tmp_tableBlanksFilter.Where(u => u.Data.Date == BeginData.Date).ToList());
                    BeginData = BeginData.AddDays(1);
                }

                foreach (var i in keys)
                {
                    List<ResultModel> tmp_list = i.Value;
                    //Количество анкет
                    count = tmp_list.Count();
                    //Вычисляем Срею время, Мин. - Макс. время, Сре. отклонение
                    min = new TimeSpan(23, 59, 59); max = new TimeSpan(0, 0, 0);
                    foreach (ResultModel item in tmp_list)
                    {
                        if (min >= DateTime.Parse(item.Time).Subtract(item.Data)) { min = DateTime.Parse(item.Time).Subtract(item.Data); }
                        if (max <= DateTime.Parse(item.Time).Subtract(item.Data)) { max = DateTime.Parse(item.Time).Subtract(item.Data); }
                    }
                    if (count > 1)
                    {
                        double seconds = 0; double secondsOT = 0;
                        foreach (ResultModel item in tmp_list)
                        {
                            seconds += DateTime.Parse(item.Time).Subtract(item.Data).TotalSeconds;
                        }
                        seconds = seconds / tmp_list.Count;
                        foreach (ResultModel item in tmp_list)
                        {
                            secondsOT += Math.Pow(DateTime.Parse(item.Time).Subtract(item.Data).TotalSeconds - seconds, 2);
                        }
                        secondsOT /= (tmp_list.Count - 1); secondsOT = Math.Sqrt(secondsOT);
                        int minutesOt = (int)Math.Floor(secondsOT / 60);
                        int minutes = (int)Math.Floor(seconds / 60);
                        seconds -= minutes * 60;
                        secondsOT -= minutesOt * 60;
                        tmp = new DateTime(1, 1, 1, 0, minutes, (int)seconds);
                        tmp2 = new DateTime(1, 1, 1, 0, minutesOt, (int)secondsOT);
                    }
                    //
                    //Количество анкет в час
                    List<ResultModel> t_tmp = tmp_list.Reverse<ResultModel>().ToList();
                    if (count != 0)
                    {
                        TimeSpan t1 = TimeSpan.Parse(t_tmp.First().Data.ToLongTimeString()).Subtract(TimeSpan.Parse(tmp_list.First().Data.ToLongTimeString()));
                        Double t2 = t1.TotalHours;
                        count_ch = count / t2;
                    }
                    //Время работы
                    string tmp_new = i.Key.Date.ToShortDateString();
                    session = db.SetSessionHubModel.Where(u => u.UserId == id_operator && u.Date == tmp_new).ToList();
                    string sessionStartTime = "";
                    string sessionEndTime = "";
                    string sessionTimeInSystem = "00:00:00";
                    string sessionAfkTime = "00:00:00";
                    if (session.Count != 0)
                    {
                        sessionStartTime = session.First().StartTime;
                        sessionEndTime = session.Reverse<SessionHubModel>().First().EndTime;
                        foreach (var q in session)
                        {
                            sessionTimeInSystem = (TimeSpan.Parse(sessionTimeInSystem) + TimeSpan.Parse(q.TimeInSystem)).ToString();
                            if (q.AfkTime != null)
                            {
                                sessionAfkTime = (TimeSpan.Parse(sessionAfkTime) + TimeSpan.Parse(q.AfkTime)).ToString();
                            }
                        }
                    }
                    if (count != 0)
                    {
                        list.Add(i.Key.Date + "/"
                            + count + "/"
                            + string.Format("{0:0.##}", count_ch) + "/"
                            + tmp.ToLongTimeString() + "/"
                            + min + "/"
                            + max + "/"
                            + tmp2.ToLongTimeString() + "/"
                            + sessionStartTime + "/"
                            + sessionEndTime + "/"
                            + sessionTimeInSystem + "/"
                            + sessionAfkTime
                            );
                    }
                }
            }
            else
            {
                //Создание списка 
                tmp_tableBlanksFilter = db.SetResultModels.Where(u => u.ProjectID == id_project && u.Data.CompareTo(startTimeDT) == 1 && u.Data.CompareTo(endTimeDT) == -1).ToList();
                DateTime BeginData = startTimeDT.Date;
                DateTime EndData = endTimeDT.Date;
                int count = 0;
                double count_ch = 0;
                while (BeginData <= EndData)
                {
                    keys.Add(BeginData, tmp_tableBlanksFilter.Where(u => u.Data.Date == BeginData.Date).ToList());
                    BeginData = BeginData.AddDays(1);
                }

                //
                //Вычисление общих значений
                count_all = tmp_tableBlanksFilter.Count;
                min_all = new TimeSpan(23, 59, 59); max_all = new TimeSpan(0, 0, 0);
                foreach (ResultModel item in tmp_tableBlanksFilter)
                {
                    if (min_all >= DateTime.Parse(item.Time).Subtract(item.Data)) { min_all = DateTime.Parse(item.Time).Subtract(item.Data); }
                    if (max_all <= DateTime.Parse(item.Time).Subtract(item.Data)) { max_all = DateTime.Parse(item.Time).Subtract(item.Data); }
                }
                if (count_all > 1)
                {
                    double seconds_all = 0; double secondsOT_all = 0;
                    foreach (ResultModel item in tmp_tableBlanksFilter)
                    {
                        if (item.Time != null)
                        {
                            seconds_all += DateTime.Parse(item.Time).Subtract(item.Data).TotalSeconds;
                        }
                    }
                    seconds_all = seconds_all / count_all;
                    foreach (ResultModel item in tmp_tableBlanksFilter)
                    {
                        if (item.Time != null)
                        {
                            secondsOT_all += Math.Pow(DateTime.Parse(item.Time).Subtract(item.Data).TotalSeconds - seconds_all, 2);
                        }
                    }
                    secondsOT_all /= (tmp_tableBlanksFilter.Count - 1); secondsOT_all = Math.Sqrt(secondsOT_all);
                    int minutesOt = (int)Math.Floor(secondsOT_all / 60);
                    int minutes = (int)Math.Floor(seconds_all / 60);
                    seconds_all -= minutes * 60;
                    secondsOT_all -= minutesOt * 60;
                    tmp_all = new DateTime(1, 1, 1, 0, minutes, (int)seconds_all);
                    tmp2_all = new DateTime(1, 1, 1, 0, minutesOt, (int)secondsOT_all);
                }
                //
                //
                foreach (var i in keys)
                {
                    List<ResultModel> tmp_list = i.Value;
                    //Количество анкет
                    count = tmp_list.Count();
                    //Вычисляем Срею время, Мин. - Макс. время, Сре. отклонение
                    min = new TimeSpan(23, 59, 59); max = new TimeSpan(0, 0, 0);
                    foreach (ResultModel item in tmp_list)
                    {
                        if (min >= DateTime.Parse(item.Time).Subtract(item.Data)) { min = DateTime.Parse(item.Time).Subtract(item.Data); }
                        if (max <= DateTime.Parse(item.Time).Subtract(item.Data)) { max = DateTime.Parse(item.Time).Subtract(item.Data); }
                    }
                    if (count > 1)
                    {
                        double seconds = 0; double secondsOT = 0;
                        foreach (ResultModel item in tmp_list)
                        {
                            seconds += DateTime.Parse(item.Time).Subtract(item.Data).TotalSeconds;
                        }
                        seconds = seconds / tmp_list.Count;
                        foreach (ResultModel item in tmp_list)
                        {
                            secondsOT += Math.Pow(DateTime.Parse(item.Time).Subtract(item.Data).TotalSeconds - seconds, 2);
                        }
                        secondsOT /= (tmp_list.Count - 1); secondsOT = Math.Sqrt(secondsOT);
                        int minutesOt = (int)Math.Floor(secondsOT / 60);
                        int minutes = (int)Math.Floor(seconds / 60);
                        seconds -= minutes * 60;
                        secondsOT -= minutesOt * 60;
                        tmp = new DateTime(1, 1, 1, 0, minutes, (int)seconds);
                        tmp2 = new DateTime(1, 1, 1, 0, minutesOt, (int)secondsOT);
                    }
                    //
                    //Количество анкет в час
                    List<ResultModel> t_tmp = tmp_list.Reverse<ResultModel>().ToList();
                    if (count != 0)
                    {
                        TimeSpan t1 = TimeSpan.Parse(t_tmp.First().Data.ToLongTimeString()).Subtract(TimeSpan.Parse(tmp_list.First().Data.ToLongTimeString()));
                        Double t2 = t1.TotalHours;
                        count_ch = count / t2;
                    }
                    if (count != 0)
                    {
                        list.Add(i.Key.Date + "/"
                            + count + "/"
                            + string.Format("{0:0.##}", count_ch) + "/"
                            + tmp.ToLongTimeString() + "/"
                            + min + "/"
                            + max + "/"
                            + tmp2.ToLongTimeString() + "/"
                            + " /"
                            + " /"
                            + " /"
                            + " "
                            );
                    }
                }
            }

            if (count_all != 0)
            {
                list_all.Add("ИТОГО : /"
                    + count_all + "/"
                    + string.Format("{0:0.##}", count_ch_all) + "/"
                    + tmp_all.ToLongTimeString() + "/"
                    + min_all + "/"
                    + max_all + "/"
                    + tmp2_all.ToLongTimeString()
                    );
            }
            ViewBag.list_all = list_all;
            ViewBag.list = list;
            int pageSize = 15;
            int pageNumber = (page ?? 1);
            return PartialView(tmp_tableBlanksFilter.ToPagedList(pageNumber, pageSize));
        }

        //
        //Отрисовка блока с результатами (ПО ПАНЕЛИ)
        //
        [HttpGet]
        public ActionResult _TableBlanksFilter(int id_project, int id_operator, string startTime, string endTime, int? page)
        {
            List<string> list = new List<string>();
            List<string> list_all = new List<string>();
            ViewBag.Id_Project_Next = id_project;
            ViewBag.Id_Operator_Next = id_operator;
            ViewBag.startTimeNext = startTime;
            ViewBag.endTimeNext = endTime;
            DateTime startTimeDT = Convert.ToDateTime(startTime.Replace("_", " "));
            DateTime endTimeDT = Convert.ToDateTime(endTime.Replace("_", " "));
            System.Diagnostics.Debug.WriteLine(startTimeDT);
            System.Diagnostics.Debug.WriteLine(endTimeDT);
            DateTime tmp = new DateTime();
            DateTime tmp2 = new DateTime();
            TimeSpan min = new TimeSpan();
            TimeSpan max = new TimeSpan();

            List<SessionHubModel> session = new List<SessionHubModel>();
            Dictionary<DateTime, List<ResultModel>> keys = new Dictionary<DateTime, List<ResultModel>>();

            using (ProjectContext project_db = new ProjectContext())
            {
                ViewBag.ProjectName = project_db.SetProjectModels.First(u => u.Id == id_project).NameProject;
            }
            //
            //Общие
            int count_all = 0;
            double count_ch_all = 0;
            TimeSpan min_all = new TimeSpan();
            TimeSpan max_all = new TimeSpan();
            DateTime tmp_all = new DateTime();
            DateTime tmp2_all = new DateTime();


            if (id_operator > 0)
            {
                tmp_tableBlanksFilter = db.SetResultModels.Where(u => u.ProjectID == id_project && u.UserID == id_operator && u.Data.CompareTo(startTimeDT) == 1 && u.Data.CompareTo(endTimeDT) == -1).ToList();
                DateTime BeginData = startTimeDT.Date;
                DateTime EndData = endTimeDT.Date;
                int count = 0;
                double count_ch = 0;
                count_ch_all = 0;

                //
                //Вычисление общих значений
                count_all = tmp_tableBlanksFilter.Count;
                min_all = new TimeSpan(23, 59, 59); max_all = new TimeSpan(0, 0, 0);
                foreach (ResultModel item in tmp_tableBlanksFilter)
                {
                    if (min_all >= DateTime.Parse(item.Time).Subtract(item.Data)) { min_all = DateTime.Parse(item.Time).Subtract(item.Data); }
                    if (max_all <= DateTime.Parse(item.Time).Subtract(item.Data)) { max_all = DateTime.Parse(item.Time).Subtract(item.Data); }
                }
                if (count_all > 1)
                {
                    double seconds_all = 0; double secondsOT_all = 0;
                    foreach (ResultModel item in tmp_tableBlanksFilter)
                    {
                        seconds_all += DateTime.Parse(item.Time).Subtract(item.Data).TotalSeconds;
                    }
                    seconds_all = seconds_all / count_all;
                    foreach (ResultModel item in tmp_tableBlanksFilter)
                    {
                        secondsOT_all += Math.Pow(DateTime.Parse(item.Time).Subtract(item.Data).TotalSeconds - seconds_all, 2);
                    }
                    secondsOT_all /= (tmp_tableBlanksFilter.Count - 1); secondsOT_all = Math.Sqrt(secondsOT_all);
                    int minutesOt = (int)Math.Floor(secondsOT_all / 60);
                    int minutes = (int)Math.Floor(seconds_all / 60);
                    seconds_all -= minutes * 60;
                    secondsOT_all -= minutesOt * 60;
                    tmp_all = new DateTime(1, 1, 1, 0, minutes, (int)seconds_all);
                    tmp2_all = new DateTime(1, 1, 1, 0, minutesOt, (int)secondsOT_all);
                }
                //
                //Количество анкет в час
                List<ResultModel> t_tmp_all = tmp_tableBlanksFilter.Reverse<ResultModel>().ToList();
                System.Diagnostics.Debug.WriteLine("COUNT------------==================>" + count);
                if (count_all != 0)
                {
                    System.Diagnostics.Debug.WriteLine("t1------------==================>" + TimeSpan.Parse(tmp_tableBlanksFilter.First().Data.ToLongTimeString()));
                    System.Diagnostics.Debug.WriteLine("t1------------==================>" + TimeSpan.Parse(t_tmp_all.First().Data.ToLongTimeString()));
                    TimeSpan t1 = TimeSpan.Parse(t_tmp_all.First().Data.ToLongTimeString()).Subtract(TimeSpan.Parse(tmp_tableBlanksFilter.First().Data.ToLongTimeString()));
                    Double t2 = t1.TotalHours;
                    System.Diagnostics.Debug.WriteLine("t1------------==================>" + t1);
                    System.Diagnostics.Debug.WriteLine("t2------------==================>" + t2);
                    count_ch_all = count_all / t2;
                }

                //
                //
                while (BeginData <= EndData)
                {
                    keys.Add(BeginData, tmp_tableBlanksFilter.Where(u => u.Data.Date == BeginData.Date).ToList());
                    BeginData = BeginData.AddDays(1);
                }

                foreach (var i in keys)
                {
                    List<ResultModel> tmp_list = i.Value;
                    //Количество анкет
                    count = tmp_list.Count();
                    //Вычисляем Срею время, Мин. - Макс. время, Сре. отклонение
                    min = new TimeSpan(23, 59, 59); max = new TimeSpan(0, 0, 0);
                    foreach (ResultModel item in tmp_list)
                    {
                        if (min >= DateTime.Parse(item.Time).Subtract(item.Data)) { min = DateTime.Parse(item.Time).Subtract(item.Data); }
                        if (max <= DateTime.Parse(item.Time).Subtract(item.Data)) { max = DateTime.Parse(item.Time).Subtract(item.Data); }
                    }
                    if (count > 1)
                    {
                        double seconds = 0; double secondsOT = 0;
                        foreach (ResultModel item in tmp_list)
                        {
                            seconds += DateTime.Parse(item.Time).Subtract(item.Data).TotalSeconds;
                        }
                        seconds = seconds / tmp_list.Count;
                        foreach (ResultModel item in tmp_list)
                        {
                            secondsOT += Math.Pow(DateTime.Parse(item.Time).Subtract(item.Data).TotalSeconds - seconds, 2);
                        }
                        secondsOT /= (tmp_list.Count - 1); secondsOT = Math.Sqrt(secondsOT);
                        int minutesOt = (int)Math.Floor(secondsOT / 60);
                        int minutes = (int)Math.Floor(seconds / 60);
                        seconds -= minutes * 60;
                        secondsOT -= minutesOt * 60;
                        tmp = new DateTime(1, 1, 1, 0, minutes, (int)seconds);
                        tmp2 = new DateTime(1, 1, 1, 0, minutesOt, (int)secondsOT);
                    }
                    //
                    //Количество анкет в час
                    List<ResultModel> t_tmp = tmp_list.Reverse<ResultModel>().ToList();
                    System.Diagnostics.Debug.WriteLine("COUNT------------==================>" + count);
                    if (count != 0)
                    {
                        System.Diagnostics.Debug.WriteLine("t1------------==================>" + TimeSpan.Parse(tmp_list.First().Data.ToLongTimeString()));
                        System.Diagnostics.Debug.WriteLine("t1------------==================>" + TimeSpan.Parse(t_tmp.First().Data.ToLongTimeString()));
                        TimeSpan t1 = TimeSpan.Parse(t_tmp.First().Data.ToLongTimeString()).Subtract(TimeSpan.Parse(tmp_list.First().Data.ToLongTimeString()));
                        Double t2 = t1.TotalHours;
                        System.Diagnostics.Debug.WriteLine("t1------------==================>" + t1);
                        System.Diagnostics.Debug.WriteLine("t2------------==================>" + t2);
                        count_ch = count / t2;
                    }
                    //Время работы
                    string tmp_new = i.Key.Date.ToShortDateString();
                    session = db.SetSessionHubModel.Where(u => u.UserId == id_operator && u.Date == tmp_new).ToList();
                    string sessionStartTime = "";
                    string sessionEndTime = "";
                    string sessionTimeInSystem = "00:00:00";
                    string sessionAfkTime = "00:00:00";
                    if (session.Count != 0)
                    {
                        sessionStartTime = session.First().StartTime;
                        sessionEndTime = session.Reverse<SessionHubModel>().First().EndTime;
                        foreach (var q in session)
                        {
                            sessionTimeInSystem = (TimeSpan.Parse(sessionTimeInSystem) + TimeSpan.Parse(q.TimeInSystem)).ToString();
                            if (q.AfkTime != null)
                            {
                                sessionAfkTime = (TimeSpan.Parse(sessionAfkTime) + TimeSpan.Parse(q.AfkTime)).ToString();
                            }
                        }
                    }
                    if (count != 0)
                    {
                        list.Add(i.Key.Date + "/"
                            + count + "/"
                            + string.Format("{0:0.##}", count_ch) + "/"
                            + tmp.ToLongTimeString() + "/"
                            + min + "/"
                            + max + "/"
                            + tmp2.ToLongTimeString() + "/"
                            + sessionStartTime + "/"
                            + sessionEndTime + "/"
                            + sessionTimeInSystem + "/"
                            + sessionAfkTime
                            );
                    }
                }
            }
            else
            {
                //Создание списка 
                tmp_tableBlanksFilter = db.SetResultModels.Where(u => u.ProjectID == id_project && u.Data.CompareTo(startTimeDT) == 1 && u.Data.CompareTo(endTimeDT) == -1).ToList();
                DateTime BeginData = startTimeDT.Date;
                DateTime EndData = endTimeDT.Date;
                int count = 0;
                double count_ch = 0;
                while (BeginData <= EndData)
                {
                    keys.Add(BeginData, tmp_tableBlanksFilter.Where(u => u.Data.Date == BeginData.Date).ToList());
                    BeginData = BeginData.AddDays(1);
                }

                //
                //Вычисление общих значений
                count_all = tmp_tableBlanksFilter.Count;
                min_all = new TimeSpan(23, 59, 59); max_all = new TimeSpan(0, 0, 0);
                foreach (ResultModel item in tmp_tableBlanksFilter)
                {
                    if (min_all >= DateTime.Parse(item.Time).Subtract(item.Data)) { min_all = DateTime.Parse(item.Time).Subtract(item.Data); }
                    if (max_all <= DateTime.Parse(item.Time).Subtract(item.Data)) { max_all = DateTime.Parse(item.Time).Subtract(item.Data); }
                }
                if (count_all > 1)
                {
                    double seconds_all = 0; double secondsOT_all = 0;
                    foreach (ResultModel item in tmp_tableBlanksFilter)
                    {
                        seconds_all += DateTime.Parse(item.Time).Subtract(item.Data).TotalSeconds;
                    }
                    seconds_all = seconds_all / count_all;
                    foreach (ResultModel item in tmp_tableBlanksFilter)
                    {
                        secondsOT_all += Math.Pow(DateTime.Parse(item.Time).Subtract(item.Data).TotalSeconds - seconds_all, 2);
                    }
                    secondsOT_all /= (tmp_tableBlanksFilter.Count - 1); secondsOT_all = Math.Sqrt(secondsOT_all);
                    int minutesOt = (int)Math.Floor(secondsOT_all / 60);
                    int minutes = (int)Math.Floor(seconds_all / 60);
                    seconds_all -= minutes * 60;
                    secondsOT_all -= minutesOt * 60;
                    tmp_all = new DateTime(1, 1, 1, 0, minutes, (int)seconds_all);
                    tmp2_all = new DateTime(1, 1, 1, 0, minutesOt, (int)secondsOT_all);
                }
                //
                //
                foreach (var i in keys)
                {
                    List<ResultModel> tmp_list = i.Value;
                    //Количество анкет
                    count = tmp_list.Count();
                    //Вычисляем Срею время, Мин. - Макс. время, Сре. отклонение
                    min = new TimeSpan(23, 59, 59); max = new TimeSpan(0, 0, 0);
                    foreach (ResultModel item in tmp_list)
                    {
                        if (min >= DateTime.Parse(item.Time).Subtract(item.Data)) { min = DateTime.Parse(item.Time).Subtract(item.Data); }
                        if (max <= DateTime.Parse(item.Time).Subtract(item.Data)) { max = DateTime.Parse(item.Time).Subtract(item.Data); }
                    }
                    if (count > 1)
                    {
                        double seconds = 0; double secondsOT = 0;
                        foreach (ResultModel item in tmp_list)
                        {
                            seconds += DateTime.Parse(item.Time).Subtract(item.Data).TotalSeconds;
                        }
                        seconds = seconds / tmp_list.Count;
                        foreach (ResultModel item in tmp_list)
                        {
                            secondsOT += Math.Pow(DateTime.Parse(item.Time).Subtract(item.Data).TotalSeconds - seconds, 2);
                        }
                        secondsOT /= (tmp_list.Count - 1); secondsOT = Math.Sqrt(secondsOT);
                        int minutesOt = (int)Math.Floor(secondsOT / 60);
                        int minutes = (int)Math.Floor(seconds / 60);
                        seconds -= minutes * 60;
                        secondsOT -= minutesOt * 60;
                        tmp = new DateTime(1, 1, 1, 0, minutes, (int)seconds);
                        tmp2 = new DateTime(1, 1, 1, 0, minutesOt, (int)secondsOT);
                    }
                    //
                    //Количество анкет в час
                    List<ResultModel> t_tmp = tmp_list.Reverse<ResultModel>().ToList();
                    System.Diagnostics.Debug.WriteLine("COUNT------------==================>" + count);
                    if (count != 0)
                    {
                        System.Diagnostics.Debug.WriteLine("t1------------==================>" + TimeSpan.Parse(tmp_list.First().Data.ToLongTimeString()));
                        System.Diagnostics.Debug.WriteLine("t1------------==================>" + TimeSpan.Parse(t_tmp.First().Data.ToLongTimeString()));
                        TimeSpan t1 = TimeSpan.Parse(t_tmp.First().Data.ToLongTimeString()).Subtract(TimeSpan.Parse(tmp_list.First().Data.ToLongTimeString()));
                        Double t2 = t1.TotalHours;
                        System.Diagnostics.Debug.WriteLine("t1------------==================>" + t1);
                        System.Diagnostics.Debug.WriteLine("t2------------==================>" + t2);
                        count_ch = count / t2;
                    }
                    if (count != 0)
                    {
                        list.Add(i.Key.Date + "/"
                            + count + "/"
                            + string.Format("{0:0.##}", count_ch) + "/"
                            + tmp.ToLongTimeString() + "/"
                            + min + "/"
                            + max + "/"
                            + tmp2.ToLongTimeString() + "/"
                            + " /"
                            + " /"
                            + " /"
                            + " "
                            );
                    }
                }
            }

            if (count_all != 0)
            {
                list_all.Add("ИТОГО : /"
                    + count_all + "/"
                    + string.Format("{0:0.##}", count_ch_all) + "/"
                    + tmp_all.ToLongTimeString() + "/"
                    + min_all + "/"
                    + max_all + "/"
                    + tmp2_all.ToLongTimeString()
                    );
            }
            ViewBag.list_all = list_all;
            ViewBag.list = list;
            int pageSize = 15;
            int pageNumber = (page ?? 1);
            return PartialView(tmp_tableBlanksFilter.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Monitoring()
        {
            return PartialView();
        }

        [HttpGet]
        public JsonResult getMonitoring()
        {
            String dateNow = DateTime.Now.ToLongDateString();
            ApplicationContext context = new ApplicationContext();
            List<SessionHubModel> date = context.SetSessionHubModel.ToList();
            List<DataUser> dataUsers = context.SetDataUsers.ToList();
            List<string> monitoring = new List<string>();
            foreach (var item in dataUsers)
            {
                if (date.FirstOrDefault(u => u.UserId == item.UserId) != null)
                {
                    List<SessionHubModel> tmp_date = date.Where(u => u.UserId == item.UserId).ToList();
                    SessionHubModel i;
                    if ((i = tmp_date.FirstOrDefault(u => u.Date == dateNow)) != null)
                    {
                        monitoring.Add(item.Family + " " + item.Name + "_" + i.IsAction + "_" + item.UserId);
                    }
                }
            }
            return Json(monitoring, JsonRequestBehavior.AllowGet);
        }

        //
        //Вывод информации по проекту
        //
        [HttpGet]
        public JsonResult getInfoProject(int id_project)
        {
            string info = "Кол-во анкет : _";
            if (db.SetResultModels.Where(u => u.ProjectID == id_project).Count() != 0)
            {
                info = info
                    + db.SetResultModels.Where(u => u.ProjectID == id_project).Count().ToString() + "_"
                    + "  Дата : _"
                    + db.SetResultModels.Where(u => u.ProjectID == id_project).First().Data.ToShortDateString()
                    + " - "
                    + db.SetResultModels.Where(u => u.ProjectID == id_project).ToList().Reverse<ResultModel>().First().Data.ToShortDateString();
            }
            else
            {
                info = "null";
            }
            System.Diagnostics.Debug.WriteLine("-==============>" + info);
            return Json(info, JsonRequestBehavior.AllowGet);
        }
        //
        //Личный кабинет
        //
        public ActionResult PersonalAccount()
        {
            HttpCookie cookieReq = Request.Cookies["cookieAuth"];
            string cookieString = null;
            if (cookieReq != null)
            {
                cookieString = CryporEngine.Decrypt(cookieReq["Login"], true);
            }
            IQueryable<UserViewModel> result = (
            from User in db.SetUser
            join DataUsers in db.SetDataUsers on User.Id equals DataUsers.UserId
            join Role in db.SetRoles on User.RoleId equals Role.Id
            where User.Login == cookieString
            select new UserViewModel
            {
                IdView = User.Id,
                LoginView = User.Login,
                PasswordView = User.Password,
                NameView = DataUsers.Name,
                FamilyView = DataUsers.Family,
                AgeView = DataUsers.Age,
                FoolView = DataUsers.Fool,
                EmailView = DataUsers.Email,
                SchoolDayView = User.SchoolDay,
                RoleView = Role.Name
            }
            );
            return PartialView(result);
        }
        public JsonResult listproject()
        {
            return Json(db2.SetProjectModels.Where(u => u.ActionProject == true).ToList(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult getResultListProject(int id_project, int id_operator)
        {
            string info = "Кол-во анкет за проект : _";
            DateTime BeginData = DateTime.Now.Date;
            List<ResultModel> tmp_list = db.SetResultModels.ToList();

            if (db.SetResultModels.Where(u => u.ProjectID == id_project && u.UserID == id_operator).Count() != 0)
            {
                info = info
                    + db.SetResultModels.Where(u => u.ProjectID == id_project && u.UserID == id_operator).Count().ToString() + "_"
                    + "Кол-во анкет за сегодня: _";
                if (tmp_list.Where(u => u.ProjectID == id_project && u.UserID == id_operator && u.Data.Date == BeginData.Date).Count() != 0)
                {
                    info += tmp_list.Where(u => u.ProjectID == id_project && u.UserID == id_operator && u.Data.Date == BeginData.Date).Count().ToString();
                }
                else
                {
                    info += 0;
                }
            }
            return Json(info, JsonRequestBehavior.AllowGet);
        }
        //
    }
}