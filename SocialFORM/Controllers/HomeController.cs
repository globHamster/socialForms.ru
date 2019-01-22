using SocialFORM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using SocialFORM.Models.Project;
using SocialFORM.Models.Question;
using SocialFORM.Models.Group;
using SocialFORM.Models.Form;
using SocialFORM.Models.Session;
using SocialFORM.Models.Statistick;
using SocialFORM.Models.Number;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using PagedList;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using ClosedXML.Excel;
using System.Data;
using LumenWorks.Framework.IO.Csv;
using System.Net;
using SocialFORM.Models.DB;

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

        [Authorize(Roles = "Admin , Operator, Customer")]
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

            SetTimeAfk tmp = db.SetTimeAfk.Where(u => u.Id == 1).First();
            System.Diagnostics.Debug.WriteLine("=====>>>>> time " + Convert.ToInt32(tmp.Time) * 1000);
            ViewBag.TimeID = (Convert.ToInt32(tmp.Time) * 1000).ToString();

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
        //[Authorize(Roles = "Admin")]
        public ActionResult Project(int? page)
        {
            tmp_listProject = db4.SetProjectModels.ToList();
            listProject = tmp_listProject.Reverse<ProjectModel>().ToList();
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return PartialView("_Project", listProject.ToPagedList(pageNumber, pageSize));
        }

        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public ActionResult _Project(int? page)
        {
            tmp_listProject = db4.SetProjectModels.ToList();
            listProject = tmp_listProject.Reverse<ProjectModel>().ToList();
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return PartialView("_Project", listProject.ToPagedList(pageNumber, pageSize));
        }
        //

        //[Authorize(Roles = "Operator")]
        public ActionResult ProjectOperator()
        {
            return PartialView(db4.SetProjectModels.Where(u => u.ActionProject == true));
        }

        //[Authorize(Roles = "Operator")]
        public ActionResult BeginForm(int id_p)
        {
            ViewBag.ProjectID = id_p;
            using (ProjectContext p_context = new ProjectContext())
            {
                ViewBag.NameProject = p_context.SetProjectModels.First(u => u.Id == id_p).NameProject;
            }
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
            listUsers = result.ToList().OrderBy(u => u.FamilyView).ToList();
            int pageSize = 14;
            int pageNumber = (page ?? 1);
            return PartialView("_Users", listUsers.ToPagedList(pageNumber, pageSize));
        }
        //
        //Таблица Пользователей
        //
        public string GetData()
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
                SchoolDayView = User.SchoolDay,
                RoleView = Role.Name
            }
            );
            listUsers = result.ToList();
            return JsonConvert.SerializeObject(listUsers);
        }

        [HttpPost]
        public void Edit(UserViewModel model)
        {
            string password = CodePass(model.PasswordView);
            // действия по редактированию
            using (ApplicationContext db = new ApplicationContext())
            {
                User UPuser = db.SetUser.Where(c => c.Id == model.IdView).First();
                DataUser UPdataUser = db.SetDataUsers.Where(c => c.UserId == model.IdView).First();

                UPuser.Login = model.LoginView;

                UPuser.Password = CodePass(model.PasswordView);
                UPuser.RoleId = Convert.ToInt32(model.RoleView);
                UPuser.SchoolDay = model.SchoolDayView;

                UPdataUser.Name = model.NameView;
                UPdataUser.Family = model.FamilyView;
                UPdataUser.Age = model.AgeView;
                UPdataUser.Fool = model.FoolView;

                db.Entry(UPuser).State = EntityState.Modified;
                db.Entry(UPdataUser).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        [HttpPost]
        public void Create(UserViewModel model)
        {
            string password = CodePass(model.PasswordView);
            // действия по созданию
            using (ApplicationContext db = new ApplicationContext())
            {
                db.SetUser.Add(new User { Login = model.LoginView, Password = CodePass(model.PasswordView), RoleId = Convert.ToInt32(model.RoleView), SchoolDay = model.SchoolDayView });
                db.SaveChanges();
                db.SetDataUsers.Add(new DataUser { Name = model.NameView, Family = model.FamilyView, Age = model.AgeView, Fool = model.FoolView, Email = model.EmailView, UserId = db.SetUser.First(u => u.Login == model.LoginView).Id });
                db.SaveChanges();
            }
        }

        [HttpPost]
        public void Delete(int Id)
        {
            // действия по удалению
            User UPuser = db.SetUser.Where(c => c.Id == Id).First();
            DataUser UPdataUser = db.SetDataUsers.Where(c => c.UserId == Id).First();
            db.SetUser.Remove(UPuser);
            db.SetDataUsers.Remove(UPdataUser);
            db.SaveChanges();
        }

        public static string CodePass(string pass)
        {
            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(pass);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            byte[] hash = Encoding.ASCII.GetBytes(returnValue);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] hashenc = md5.ComputeHash(hash);
            string result = "";
            foreach (var b in hashenc)
            {
                result += b.ToString("x2");
            }
            return result;
        }
        //
        //Конец таблицы пользователей
        //

        [HttpGet]
        public JsonResult AddProject(string name_project)
        {
            System.Diagnostics.Debug.WriteLine("Adding new project in table ..." + name_project);
            ProjectModel tmp = new ProjectModel();
            tmp.NameProject = name_project;
            tmp.SettingEncode = "UTF-8";
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

        //Отрисовка блока с результатами (НАЧАЛО)
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

        //Отрисовка блока с результатами (ПО ПАНЕЛИ)
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

        public void ExportToEXCEL(int id_p, string name_file, string encode)
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

            List<RangeModel> listRangeExport = new List<RangeModel>();
            using (QuestionContext q_context = new QuestionContext())
            {
                listRangeExport = q_context.SetRangeModels.Where(u => u.ProjectID == id_p).ToList();
            }

            var products = new System.Data.DataTable();

            products.Columns.Add("Номер");
            products.Columns.Add("ФИО");
            products.Columns.Add("Номер телефона");
            products.Columns.Add("Начало анкеты");
            products.Columns.Add("Конец анкеты");
            products.Columns.Add("Учебный день");
            products.Columns.Add("Координаты");
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
                            if (listQuestionExport[(int)item.QuestionID].LimitCount > 1)
                            {
                                tmp_count = (int)listQuestionExport[(int)item.QuestionID].LimitCount;
                            }
                            //System.Diagnostics.Debug.WriteLine("Limit count  >>>>> " + (int)listQuestionExport[(int)item.QuestionID].LimitCount);
                            for (int i = 1; i <= tmp_count; i++)
                            {
                                if (item.GroupID > 0)
                                    products.Columns.Add("Г" + item.GroupID + " " + item.GroupName + "_в" + i);
                                else
                                    products.Columns.Add(item.GroupName + "_в" + i);
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
                                        products.Columns.Add("Г" + item.GroupID + " " + item.GroupName + "_др_в" + count);
                                    else
                                        products.Columns.Add(item.GroupName + "_др_в" + count);
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
                            if (listQuestionExport[(int)item.QuestionID].IsKvot == true)
                            {
                                products.Columns.Add("Диапозон");
                            }
                        }
                        break;
                    case Models.Question.Type.Table:
                        {
                            int count_row = listTableRow[(int)item.QuestionID].Count();
                            int null_count_row = listTableRow[(int)item.QuestionID].Where(u => u.IndexRow == 0).Count();
                            int max_cont_row = 0;
                            foreach (var item_row in listTableRow[(int)item.QuestionID])
                            {
                                if (item_row.IndexRow != null && item_row.IndexRow != 0)
                                {
                                    int tmp_max = listTableRow[(int)item.QuestionID].Where(u => u.IndexRow == item_row.IndexRow).Count();
                                    if (max_cont_row < tmp_max)
                                    {
                                        max_cont_row = tmp_max;
                                    }
                                }
                            }
                            if (listQuestionExport[(int)item.QuestionID].Bind != null)
                            {
                                count_row = (null_count_row + max_cont_row);
                                System.Diagnostics.Debug.WriteLine("ALL count Rows >>>> " + (null_count_row + max_cont_row));
                            }
                            for (int i = 1; i <= count_row; i++)
                            {
                                if (item.GroupID > 0)
                                    products.Columns.Add("Г" + item.GroupID + " " + item.GroupName + "_стр" + i);
                                else
                                    products.Columns.Add(item.GroupName + "_стр" + i);
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
                tmp_str.Add(item.CoordWidth.ToString() + "," + item.CoordHeight.ToString());
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
                                    {
                                        if (tmp_blank.FirstOrDefault(u => u.QuestionID == group_item.QuestionID).AnswerIndex < 0)
                                        {
                                            tmp_str.Add((-1 * tmp_blank.FirstOrDefault(u => u.QuestionID == group_item.QuestionID).AnswerIndex).ToString());
                                        }
                                        else
                                        {
                                            tmp_str.Add(tmp_blank.FirstOrDefault(u => u.QuestionID == group_item.QuestionID).AnswerIndex.ToString());
                                        }
                                    }
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
                                    List<AnswerModel> list_answer = new QuestionContext().SetAnswers.Where(u => u.QuestionID == group_item.QuestionID).ToList();
                                    bool is_non_limited = true;
                                    int count_all_answer = listAnswerAllExport[(int)group_item.QuestionID].Count();
                                    if (listQuestionExport[(int)group_item.QuestionID].LimitCount > 1)
                                    {
                                        count_all_answer = (int)listQuestionExport[(int)group_item.QuestionID].LimitCount;
                                        is_non_limited = false;
                                    }
                                    int count_other_column = listAnswerAllExport[(int)group_item.QuestionID].Where(u => u.isFreeArea == true).Count();
                                    List<string> other_column = new List<string>();
                                    if (is_non_limited & name_file.Contains("население"))
                                    {
                                        foreach (var item_answer in list_answer)
                                        {
                                            BlankModel blank_tmp_arg = tmp_list_blank.FirstOrDefault(u => u.AnswerIndex == item_answer.Index & u.AnswerID == item_answer.Id);
                                            if (blank_tmp_arg != null)
                                            {
                                                System.Diagnostics.Debug.WriteLine("AnswerID : " + item_answer.Id + " isFreeArea :" + item_answer.isFreeArea + " Text : " + blank_tmp_arg.Text + " ID: " + blank_tmp_arg.AnswerID);
                                                tmp_str.Add(blank_tmp_arg.AnswerIndex.ToString());
                                                if (item_answer.isFreeArea)
                                                {
                                                    System.Diagnostics.Debug.WriteLine("Text >> " + blank_tmp_arg.Text);
                                                    other_column.Add(blank_tmp_arg.Text);
                                                }
                                            }
                                            else
                                            {
                                                tmp_str.Add(" ");
                                                if (item_answer.isFreeArea)
                                                {
                                                    other_column.Add(" ");
                                                }
                                            }
                                        }
                                        for (int iter = 0; iter < other_column.Count(); iter++)
                                        {
                                            tmp_str.Add(other_column[iter]);
                                        }
                                    }
                                    else
                                    {
                                        foreach (var blank_item in tmp_list_blank)
                                        {
                                            if (count_all_answer == 0) break;
                                            if (blank_item.AnswerIndex != -404)
                                            {
                                                if (blank_item.AnswerIndex < 0)
                                                {
                                                    tmp_str.Add((-1 * blank_item.AnswerIndex).ToString());
                                                }
                                                else
                                                {
                                                    tmp_str.Add(blank_item.AnswerIndex.ToString());
                                                }
                                            }
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
                                else
                                {
                                    int count_all_answer = listAnswerAllExport[(int)group_item.QuestionID].Count();
                                    if (listQuestionExport[(int)group_item.QuestionID].LimitCount > 1)
                                    {
                                        count_all_answer = (int)listQuestionExport[(int)group_item.QuestionID].LimitCount;
                                    }
                                    int count_other_column = listAnswerAllExport[(int)group_item.QuestionID].Where(u => u.isFreeArea == true).Count();
                                    for (int i = 0; i < count_all_answer; i++)
                                    {
                                        tmp_str.Add(" ");
                                    }
                                    for (int i = 0; i < count_other_column; i++)
                                    {
                                        tmp_str.Add(" ");
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
                                if (listQuestionExport[(int)group_item.QuestionID].IsKvot == true)
                                {
                                    List<RangeModel> tmp_listRange = listRangeExport.Where(u => u.BindQuestion == (int)group_item.QuestionID).ToList();
                                    if (_blank_list.Count() == 0) { System.Diagnostics.Debug.WriteLine("Count = 0"); tmp_str.Add(" "); break; }
                                    if (_blank_list[0].Text == null) { System.Diagnostics.Debug.WriteLine("Text is null"); tmp_str.Add(" "); break; }
                                    int age = Int32.Parse(_blank_list[0].Text);
                                    foreach (var item_range in tmp_listRange)
                                    {
                                        int lower_limit;
                                        if (!Int32.TryParse(item_range.RangeString.Split('-')[0], out lower_limit))
                                        {
                                            lower_limit = Int32.MinValue;
                                        }
                                        int upper_limit;
                                        if (!Int32.TryParse(item_range.RangeString.Split('-')[1], out upper_limit))
                                        {
                                            upper_limit = Int32.MaxValue;
                                        }

                                        if ((lower_limit <= age) && (age <= upper_limit))
                                        {
                                            tmp_str.Add(item_range.IndexRange.ToString());

                                            break;
                                        }
                                    }
                                }

                            }
                            break;
                        case Models.Question.Type.Table:
                            {
                                int count_row = listTableRow[(int)group_item.QuestionID].Count();
                                int count_row_result = tmp_blank.Where(u => u.QuestionID == group_item.QuestionID).Count();
                                int null_count_row = listTableRow[(int)group_item.QuestionID].Where(u => u.IndexRow == 0).Count();
                                int max_cont_row = 0;
                                foreach (var item_row in listTableRow[(int)group_item.QuestionID])
                                {
                                    if (item_row.IndexRow != null)
                                    {
                                        int tmp_max = listTableRow[(int)group_item.QuestionID].Where(u => u.IndexRow == item_row.IndexRow).Count();
                                        if (max_cont_row < tmp_max)
                                        {
                                            max_cont_row = tmp_max;
                                        }
                                    }
                                }
                                if (listQuestionExport[(int)group_item.QuestionID].Bind != null)
                                {
                                    count_row = (null_count_row + max_cont_row);
                                    System.Diagnostics.Debug.WriteLine("ALL count Rows >>>> " + (null_count_row + max_cont_row));
                                }
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
                        case Models.Question.Type.Filter:
                            if (tmp_blank.FirstOrDefault(u => u.QuestionID == group_item.QuestionID) != null)
                            {
                                tmp_str.Add(tmp_blank.FirstOrDefault(u => u.QuestionID == group_item.QuestionID).AnswerID.ToString());
                                tmp_str.Add(tmp_blank.FirstOrDefault(u => u.QuestionID == group_item.QuestionID).Text);
                            }
                            else
                            {
                                tmp_str.Add(" ");
                                tmp_str.Add(" ");
                            }
                            break;
                        default:
                            break;
                    }

                }

                try
                {
                    products.Rows.Add(tmp_str.ToArray());
                }
                catch (System.ArgumentException r)
                {
                    tmp_str.RemoveAt(0);
                    products.Rows.Add(tmp_str.ToArray());
                    System.Diagnostics.Debug.WriteLine(r);

                }
                tmp_str.Clear();
            }
            var grid = new GridView();

            grid.DataSource = products;
            grid.DataBind();


            Response.ClearContent();
            Response.Clear();
            Response.ContentEncoding = Encoding.GetEncoding(encode);
            Response.ContentType = "application/ms-excel";
            Response.AddHeader("Content-Disposition", "attachment; filename=" + name_file + ".xls");
            Response.Charset = "";
            StringWriter sw = new StringWriter();
            HtmlTextWriter htmlTextWriter = new HtmlTextWriter(sw);
            grid.RenderControl(htmlTextWriter);
            //Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.Write(sw.ToString());
            Response.Flush();
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

        //Статистика
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
                    string tmp_time = "";
                    if (session.Count != 0)
                    {
                        sessionStartTime = session.First().StartTime;
                        sessionEndTime = session.Reverse<SessionHubModel>().First().EndTime;
                        foreach (var q in session)
                        {
                            if (q.TimeInSystem == null) { tmp_time = DateTime.Now.ToLongTimeString(); } else { tmp_time = q.TimeInSystem; }
                            sessionTimeInSystem = (TimeSpan.Parse(sessionTimeInSystem) + TimeSpan.Parse(tmp_time)).ToString();
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
        //[HttpGet]
        //public ActionResult _TableBlanksFilter(int id_project, int id_operator, string startTime, string endTime, int? page)
        //{
        //    List<string> list = new List<string>();
        //    List<string> list_all = new List<string>();
        //    ViewBag.Id_Project_Next = id_project;
        //    ViewBag.Id_Operator_Next = id_operator;
        //    ViewBag.startTimeNext = startTime;
        //    ViewBag.endTimeNext = endTime;
        //    DateTime startTimeDT = Convert.ToDateTime(startTime.Replace("_", " "));
        //    DateTime endTimeDT = Convert.ToDateTime(endTime.Replace("_", " "));
        //    System.Diagnostics.Debug.WriteLine(startTimeDT);
        //    System.Diagnostics.Debug.WriteLine(endTimeDT);
        //    DateTime tmp = new DateTime();
        //    DateTime tmp2 = new DateTime();
        //    TimeSpan min = new TimeSpan();
        //    TimeSpan max = new TimeSpan();

        //    List<SessionHubModel> session = new List<SessionHubModel>();
        //    Dictionary<DateTime, List<ResultModel>> keys = new Dictionary<DateTime, List<ResultModel>>();

        //    using (ProjectContext project_db = new ProjectContext())
        //    {
        //        ViewBag.ProjectName = project_db.SetProjectModels.First(u => u.Id == id_project).NameProject;
        //    }
        //    //
        //    //Общие
        //    int count_all = 0;
        //    double count_ch_all = 0;
        //    TimeSpan min_all = new TimeSpan();
        //    TimeSpan max_all = new TimeSpan();
        //    DateTime tmp_all = new DateTime();
        //    DateTime tmp2_all = new DateTime();


        //    if (id_operator > 0)
        //    {
        //        tmp_tableBlanksFilter = db.SetResultModels.Where(u => u.ProjectID == id_project && u.UserID == id_operator && u.Data.CompareTo(startTimeDT) == 1 && u.Data.CompareTo(endTimeDT) == -1).ToList();
        //        DateTime BeginData = startTimeDT.Date;
        //        DateTime EndData = endTimeDT.Date;
        //        int count = 0;
        //        double count_ch = 0;
        //        count_ch_all = 0;

        //        //
        //        //Вычисление общих значений
        //        count_all = tmp_tableBlanksFilter.Count;
        //        min_all = new TimeSpan(23, 59, 59); max_all = new TimeSpan(0, 0, 0);
        //        foreach (ResultModel item in tmp_tableBlanksFilter)
        //        {
        //            if (min_all >= DateTime.Parse(item.Time).Subtract(item.Data)) { min_all = DateTime.Parse(item.Time).Subtract(item.Data); }
        //            if (max_all <= DateTime.Parse(item.Time).Subtract(item.Data)) { max_all = DateTime.Parse(item.Time).Subtract(item.Data); }
        //        }
        //        if (count_all > 1)
        //        {
        //            double seconds_all = 0; double secondsOT_all = 0;
        //            foreach (ResultModel item in tmp_tableBlanksFilter)
        //            {
        //                seconds_all += DateTime.Parse(item.Time).Subtract(item.Data).TotalSeconds;
        //            }
        //            seconds_all = seconds_all / count_all;
        //            foreach (ResultModel item in tmp_tableBlanksFilter)
        //            {
        //                secondsOT_all += Math.Pow(DateTime.Parse(item.Time).Subtract(item.Data).TotalSeconds - seconds_all, 2);
        //            }
        //            secondsOT_all /= (tmp_tableBlanksFilter.Count - 1); secondsOT_all = Math.Sqrt(secondsOT_all);
        //            int minutesOt = (int)Math.Floor(secondsOT_all / 60);
        //            int minutes = (int)Math.Floor(seconds_all / 60);
        //            seconds_all -= minutes * 60;
        //            secondsOT_all -= minutesOt * 60;
        //            tmp_all = new DateTime(1, 1, 1, 0, minutes, (int)seconds_all);
        //            tmp2_all = new DateTime(1, 1, 1, 0, minutesOt, (int)secondsOT_all);
        //        }
        //        //
        //        //Количество анкет в час
        //        List<ResultModel> t_tmp_all = tmp_tableBlanksFilter.Reverse<ResultModel>().ToList();
        //        System.Diagnostics.Debug.WriteLine("COUNT------------==================>" + count);
        //        if (count_all != 0)
        //        {
        //            System.Diagnostics.Debug.WriteLine("t1------------==================>" + TimeSpan.Parse(tmp_tableBlanksFilter.First().Data.ToLongTimeString()));
        //            System.Diagnostics.Debug.WriteLine("t1------------==================>" + TimeSpan.Parse(t_tmp_all.First().Data.ToLongTimeString()));
        //            TimeSpan t1 = TimeSpan.Parse(t_tmp_all.First().Data.ToLongTimeString()).Subtract(TimeSpan.Parse(tmp_tableBlanksFilter.First().Data.ToLongTimeString()));
        //            Double t2 = t1.TotalHours;
        //            System.Diagnostics.Debug.WriteLine("t1------------==================>" + t1);
        //            System.Diagnostics.Debug.WriteLine("t2------------==================>" + t2);
        //            count_ch_all = count_all / t2;
        //        }

        //        //
        //        //
        //        while (BeginData <= EndData)
        //        {
        //            keys.Add(BeginData, tmp_tableBlanksFilter.Where(u => u.Data.Date == BeginData.Date).ToList());
        //            BeginData = BeginData.AddDays(1);
        //        }

        //        foreach (var i in keys)
        //        {
        //            List<ResultModel> tmp_list = i.Value;
        //            //Количество анкет
        //            count = tmp_list.Count();
        //            //Вычисляем Срею время, Мин. - Макс. время, Сре. отклонение
        //            min = new TimeSpan(23, 59, 59); max = new TimeSpan(0, 0, 0);
        //            foreach (ResultModel item in tmp_list)
        //            {
        //                if (min >= DateTime.Parse(item.Time).Subtract(item.Data)) { min = DateTime.Parse(item.Time).Subtract(item.Data); }
        //                if (max <= DateTime.Parse(item.Time).Subtract(item.Data)) { max = DateTime.Parse(item.Time).Subtract(item.Data); }
        //            }
        //            if (count > 1)
        //            {
        //                double seconds = 0; double secondsOT = 0;
        //                foreach (ResultModel item in tmp_list)
        //                {
        //                    seconds += DateTime.Parse(item.Time).Subtract(item.Data).TotalSeconds;
        //                }
        //                seconds = seconds / tmp_list.Count;
        //                foreach (ResultModel item in tmp_list)
        //                {
        //                    secondsOT += Math.Pow(DateTime.Parse(item.Time).Subtract(item.Data).TotalSeconds - seconds, 2);
        //                }
        //                secondsOT /= (tmp_list.Count - 1); secondsOT = Math.Sqrt(secondsOT);
        //                int minutesOt = (int)Math.Floor(secondsOT / 60);
        //                int minutes = (int)Math.Floor(seconds / 60);
        //                seconds -= minutes * 60;
        //                secondsOT -= minutesOt * 60;
        //                tmp = new DateTime(1, 1, 1, 0, minutes, (int)seconds);
        //                tmp2 = new DateTime(1, 1, 1, 0, minutesOt, (int)secondsOT);
        //            }
        //            //
        //            //Количество анкет в час
        //            List<ResultModel> t_tmp = tmp_list.Reverse<ResultModel>().ToList();
        //            System.Diagnostics.Debug.WriteLine("COUNT------------==================>" + count);
        //            if (count != 0)
        //            {
        //                System.Diagnostics.Debug.WriteLine("t1------------==================>" + TimeSpan.Parse(tmp_list.First().Data.ToLongTimeString()));
        //                System.Diagnostics.Debug.WriteLine("t1------------==================>" + TimeSpan.Parse(t_tmp.First().Data.ToLongTimeString()));
        //                TimeSpan t1 = TimeSpan.Parse(t_tmp.First().Data.ToLongTimeString()).Subtract(TimeSpan.Parse(tmp_list.First().Data.ToLongTimeString()));
        //                Double t2 = t1.TotalHours;
        //                System.Diagnostics.Debug.WriteLine("t1------------==================>" + t1);
        //                System.Diagnostics.Debug.WriteLine("t2------------==================>" + t2);
        //                count_ch = count / t2;
        //            }
        //            //Время работы
        //            string tmp_new = i.Key.Date.ToShortDateString();
        //            session = db.SetSessionHubModel.Where(u => u.UserId == id_operator && u.Date == tmp_new).ToList();
        //            string sessionStartTime = "";
        //            string sessionEndTime = "";
        //            string sessionTimeInSystem = "00:00:00";
        //            string sessionAfkTime = "00:00:00";
        //            if (session.Count != 0)
        //            {
        //                sessionStartTime = session.First().StartTime;
        //                sessionEndTime = session.Reverse<SessionHubModel>().First().EndTime;
        //                foreach (var q in session)
        //                {
        //                    sessionTimeInSystem = (TimeSpan.Parse(sessionTimeInSystem) + TimeSpan.Parse(q.TimeInSystem)).ToString();
        //                    if (q.AfkTime != null)
        //                    {
        //                        sessionAfkTime = (TimeSpan.Parse(sessionAfkTime) + TimeSpan.Parse(q.AfkTime)).ToString();
        //                    }
        //                }
        //            }
        //            if (count != 0)
        //            {
        //                list.Add(i.Key.Date + "/"
        //                    + count + "/"
        //                    + string.Format("{0:0.##}", count_ch) + "/"
        //                    + tmp.ToLongTimeString() + "/"
        //                    + min + "/"
        //                    + max + "/"
        //                    + tmp2.ToLongTimeString() + "/"
        //                    + sessionStartTime + "/"
        //                    + sessionEndTime + "/"
        //                    + sessionTimeInSystem + "/"
        //                    + sessionAfkTime
        //                    );
        //            }
        //        }
        //    }
        //    else
        //    {
        //        //Создание списка 
        //        tmp_tableBlanksFilter = db.SetResultModels.Where(u => u.ProjectID == id_project && u.Data.CompareTo(startTimeDT) == 1 && u.Data.CompareTo(endTimeDT) == -1).ToList();
        //        DateTime BeginData = startTimeDT.Date;
        //        DateTime EndData = endTimeDT.Date;
        //        int count = 0;
        //        double count_ch = 0;
        //        while (BeginData <= EndData)
        //        {
        //            keys.Add(BeginData, tmp_tableBlanksFilter.Where(u => u.Data.Date == BeginData.Date).ToList());
        //            BeginData = BeginData.AddDays(1);
        //        }

        //        //
        //        //Вычисление общих значений
        //        count_all = tmp_tableBlanksFilter.Count;
        //        min_all = new TimeSpan(23, 59, 59); max_all = new TimeSpan(0, 0, 0);
        //        foreach (ResultModel item in tmp_tableBlanksFilter)
        //        {
        //            if (min_all >= DateTime.Parse(item.Time).Subtract(item.Data)) { min_all = DateTime.Parse(item.Time).Subtract(item.Data); }
        //            if (max_all <= DateTime.Parse(item.Time).Subtract(item.Data)) { max_all = DateTime.Parse(item.Time).Subtract(item.Data); }
        //        }
        //        if (count_all > 1)
        //        {
        //            double seconds_all = 0; double secondsOT_all = 0;
        //            foreach (ResultModel item in tmp_tableBlanksFilter)
        //            {
        //                seconds_all += DateTime.Parse(item.Time).Subtract(item.Data).TotalSeconds;
        //            }
        //            seconds_all = seconds_all / count_all;
        //            foreach (ResultModel item in tmp_tableBlanksFilter)
        //            {
        //                secondsOT_all += Math.Pow(DateTime.Parse(item.Time).Subtract(item.Data).TotalSeconds - seconds_all, 2);
        //            }
        //            secondsOT_all /= (tmp_tableBlanksFilter.Count - 1); secondsOT_all = Math.Sqrt(secondsOT_all);
        //            int minutesOt = (int)Math.Floor(secondsOT_all / 60);
        //            int minutes = (int)Math.Floor(seconds_all / 60);
        //            seconds_all -= minutes * 60;
        //            secondsOT_all -= minutesOt * 60;
        //            tmp_all = new DateTime(1, 1, 1, 0, minutes, (int)seconds_all);
        //            tmp2_all = new DateTime(1, 1, 1, 0, minutesOt, (int)secondsOT_all);
        //        }
        //        //
        //        //
        //        foreach (var i in keys)
        //        {
        //            List<ResultModel> tmp_list = i.Value;
        //            //Количество анкет
        //            count = tmp_list.Count();
        //            //Вычисляем Срею время, Мин. - Макс. время, Сре. отклонение
        //            min = new TimeSpan(23, 59, 59); max = new TimeSpan(0, 0, 0);
        //            foreach (ResultModel item in tmp_list)
        //            {
        //                if (min >= DateTime.Parse(item.Time).Subtract(item.Data)) { min = DateTime.Parse(item.Time).Subtract(item.Data); }
        //                if (max <= DateTime.Parse(item.Time).Subtract(item.Data)) { max = DateTime.Parse(item.Time).Subtract(item.Data); }
        //            }
        //            if (count > 1)
        //            {
        //                double seconds = 0; double secondsOT = 0;
        //                foreach (ResultModel item in tmp_list)
        //                {
        //                    seconds += DateTime.Parse(item.Time).Subtract(item.Data).TotalSeconds;
        //                }
        //                seconds = seconds / tmp_list.Count;
        //                foreach (ResultModel item in tmp_list)
        //                {
        //                    secondsOT += Math.Pow(DateTime.Parse(item.Time).Subtract(item.Data).TotalSeconds - seconds, 2);
        //                }
        //                secondsOT /= (tmp_list.Count - 1); secondsOT = Math.Sqrt(secondsOT);
        //                int minutesOt = (int)Math.Floor(secondsOT / 60);
        //                int minutes = (int)Math.Floor(seconds / 60);
        //                seconds -= minutes * 60;
        //                secondsOT -= minutesOt * 60;
        //                tmp = new DateTime(1, 1, 1, 0, minutes, (int)seconds);
        //                tmp2 = new DateTime(1, 1, 1, 0, minutesOt, (int)secondsOT);
        //            }
        //            //
        //            //Количество анкет в час
        //            List<ResultModel> t_tmp = tmp_list.Reverse<ResultModel>().ToList();
        //            System.Diagnostics.Debug.WriteLine("COUNT------------==================>" + count);
        //            if (count != 0)
        //            {
        //                System.Diagnostics.Debug.WriteLine("t1------------==================>" + TimeSpan.Parse(tmp_list.First().Data.ToLongTimeString()));
        //                System.Diagnostics.Debug.WriteLine("t1------------==================>" + TimeSpan.Parse(t_tmp.First().Data.ToLongTimeString()));
        //                TimeSpan t1 = TimeSpan.Parse(t_tmp.First().Data.ToLongTimeString()).Subtract(TimeSpan.Parse(tmp_list.First().Data.ToLongTimeString()));
        //                Double t2 = t1.TotalHours;
        //                System.Diagnostics.Debug.WriteLine("t1------------==================>" + t1);
        //                System.Diagnostics.Debug.WriteLine("t2------------==================>" + t2);
        //                count_ch = count / t2;
        //            }
        //            if (count != 0)
        //            {
        //                list.Add(i.Key.Date + "/"
        //                    + count + "/"
        //                    + string.Format("{0:0.##}", count_ch) + "/"
        //                    + tmp.ToLongTimeString() + "/"
        //                    + min + "/"
        //                    + max + "/"
        //                    + tmp2.ToLongTimeString() + "/"
        //                    + " /"
        //                    + " /"
        //                    + " /"
        //                    + " "
        //                    );
        //            }
        //        }
        //    }

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
        
        public ActionResult Options()
        {
            return PartialView();
        }
        
        //DataBase
        public ActionResult DataBeseAll()
        {
            return PartialView();
        }

        [HttpGet]
        public JsonResult GetDiapazon(string NameD, string KodFO, string KodOB, string KodGOR, string str_dip)
        {
            bool flag = false;
            NumberAppContext context = new NumberAppContext();
            Diap res = new Diap();
            var diap = str_dip.Split('#');
            foreach (var i in diap)
            {
                System.Diagnostics.Debug.WriteLine(i);
            }
            foreach (var item in diap)
            {
                if (item != "")
                {
                    var diap_tmp = item.Split('|');
                    res.Kod = diap_tmp[0];
                    res.KodFO = KodFO;
                    res.KodOB = KodOB;
                    res.KodGOR = KodGOR;
                    res.NameD = NameD;
                    res.Num1 = diap_tmp[1];
                    res.NumN = diap_tmp[2];
                    context.SetDiap.Add(res);
                    context.SaveChanges();
                    flag = true;
                }
            }
            return Json(flag, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetListFO()
        {
            NumberAppContext dbN = new NumberAppContext();
            List<FO> fo = dbN.SetFO.ToList();
            return Json(fo, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetListOB()
        {
            NumberAppContext dbN = new NumberAppContext();
            List<OB> ob = dbN.SetOB.ToList();

            return Json(ob, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetListGOR()
        {
            NumberAppContext dbN = new NumberAppContext();
            List<GOR> gor = dbN.SetGOR.ToList();

            return Json(gor, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Kvot(int id_p)
        {
            ViewBag.ProjectID = id_p;
            return PartialView();
        }

        [HttpPost]
        public async Task<int> postSaveOp(List<string> mass)
        {
            System.Diagnostics.Debug.WriteLine(mass.Count());
            ApplicationContext context = new ApplicationContext();
            Opros list = new Opros();
            list.q1 = mass[0];
            list.q2 = mass[1];
            list.q3 = mass[2];
            list.q4 = mass[3];
            list.q5 = mass[4];
            list.q6 = mass[5];
            list.q7 = mass[6];
            list.q8 = mass[7];
            list.q9 = mass[8];
            list.q10 = mass[9];
            list.q11 = mass[10];
            list.q12 = mass[11];
            list.q13 = mass[12];
            list.q14 = mass[13];
            list.q15 = mass[14];
            list.q16 = mass[15];
            list.q17 = mass[16];
            list.q18 = mass[17];
            list.q19 = mass[18];
            list.q20 = mass[19];
            list.q21 = mass[20];
            list.q22 = mass[21];
            list.q23 = mass[22];
            list.q24 = mass[23];
            list.q25 = mass[24];
            list.q26 = mass[25];
            list.q27 = mass[26];
            list.q28 = mass[27];
            list.q29 = mass[28];
            list.q30 = mass[29];
            list.q31 = mass[30];
            list.q32 = mass[31];
            list.q33 = mass[32];
            list.q34 = mass[33];
            list.q35 = mass[34];
            list.q36 = mass[35];
            list.q37 = mass[36];
            list.q38 = mass[37];
            list.q39 = mass[38];
            list.q40 = mass[39];
            list.q41 = mass[40];
            list.q42 = mass[41];
            list.q43 = mass[42];
            list.q44 = mass[43];
            list.q45 = mass[44];
            list.q46 = mass[45];
            context.SetOpros.Add(list);
            await context.SaveChangesAsync();
            return list.id;
        }

        public ActionResult SuccessOpros(int id_op)
        {
            ViewBag.IDopros = id_op;
            return PartialView();
        }

        [HttpGet]
        public JsonResult GetListProject()
        {
            return Json(db2.SetProjectModels.ToList(), JsonRequestBehavior.AllowGet);
        }
        
        //Таблица Статистика PATH 1
        public string GetDataStat(int id, int idop, string sd, string ed, string st, string et)
        {
            List<ProjectViewModel> res = new List<ProjectViewModel>();
            List<ResultModel> tmp1 = db.SetResultModels.ToList();
            if (id > 0)
            {
                tmp1 = tmp1.Where(u => u.ProjectID == id).ToList();
            }
            if (idop > 0)
            {
                tmp1 = tmp1.Where(u => u.UserID == idop).ToList();
            }
            if (sd != "-1" && ed != "-1")
            {
                System.Diagnostics.Debug.WriteLine("Nice");
                tmp1 = tmp1.Where(u => Convert.ToDateTime(u.Data.ToShortDateString()).CompareTo(Convert.ToDateTime(sd)) == 1 && Convert.ToDateTime(u.Data.ToShortDateString()).CompareTo(Convert.ToDateTime(ed)) == -1 || Convert.ToDateTime(u.Data.ToShortDateString()).CompareTo(Convert.ToDateTime(sd)) == 0 || Convert.ToDateTime(u.Data.ToShortDateString()).CompareTo(Convert.ToDateTime(ed)) == 0).ToList();
            }
            if (st != "-1" && et != "-1")
            {
                tmp1 = tmp1.Where(u => Convert.ToDateTime(u.Time).Ticks - ((Convert.ToDateTime(u.Time).Ticks - Convert.ToDateTime(u.Data).Ticks) / 2) >= Convert.ToDateTime(sd + " " + st).Ticks && Convert.ToDateTime(u.Time).Ticks - ((Convert.ToDateTime(u.Time).Ticks - Convert.ToDateTime(u.Data).Ticks) / 2) <= Convert.ToDateTime(ed + " " + et).Ticks).ToList();

                //tmp1 = tmp1.Where(u => Convert.ToDateTime(u.Time) >= Convert.ToDateTime(sd + " " + st) && Convert.ToDateTime(u.Time) <= Convert.ToDateTime(ed + " " + et) && Convert.ToDateTime(u.Data) >= Convert.ToDateTime(sd + " " + st) && Convert.ToDateTime(u.Data) <= Convert.ToDateTime(ed + " " + et)).ToList();
                //tmp1_1 = tmp1.Where(u => Convert.ToDateTime(u.Data.ToShortDateString()).CompareTo(Convert.ToDateTime(sd)) == 0 && TimeSpan.Parse(u.Data.ToLongTimeString()).CompareTo(TimeSpan.Parse(st)) == 1 && TimeSpan.Parse(u.Data.ToLongTimeString()).CompareTo(TimeSpan.Parse("23:59:59")) == -1).ToList();
                //tmp1_2 = tmp1.Where(u => Convert.ToDateTime(u.Data.ToShortDateString()).CompareTo(Convert.ToDateTime(sd)) == 1 && Convert.ToDateTime(u.Data.ToShortDateString()).CompareTo(Convert.ToDateTime(ed)) == 1 && TimeSpan.Parse(u.Data.ToLongTimeString()).CompareTo(TimeSpan.Parse("00:00:00")) == 1 && TimeSpan.Parse(u.Data.ToLongTimeString()).CompareTo(TimeSpan.Parse("23:59:59")) == -1).ToList();
                //tmp1_3 = tmp1.Where(u => Convert.ToDateTime(u.Data.ToShortDateString()).CompareTo(Convert.ToDateTime(ed)) == 0 && TimeSpan.Parse(u.Data.ToLongTimeString()).CompareTo(TimeSpan.Parse("00:00:00")) == 1 && TimeSpan.Parse(u.Data.ToLongTimeString()).CompareTo(TimeSpan.Parse(et)) == -1).ToList();
            }
            foreach (var item in tmp1)
            {
                ProjectViewModel tmp2 = new ProjectViewModel();
                tmp2.IDView = item.BlankID;
                tmp2.ProjectIDView = item.ProjectID;
                tmp2.UserIDView = item.UserID;
                tmp2.UserNameView = item.UserName.Trim();
                tmp2.PhoneView = item.PhoneNumber;
                tmp2.DateView = item.Data.ToShortDateString();
                tmp2.StartTimeView = item.Data.ToLongTimeString();
                tmp2.EndTimeView = DateTime.Parse(item.Time).ToLongTimeString();
                tmp2.LenghtTimeView = (DateTime.Parse(item.Time) - item.Data).ToString();
                res.Add(tmp2);
            }
            return JsonConvert.SerializeObject(res);
        }

        //Таблица Статистика PATH 2
        public string GetDataStatResult(int id, int idop, string sd, string ed, string st, string et)
        {
            List<StatResViewModel> StatRes = new List<StatResViewModel>();
            List<ResultModel> tmp1 = db.SetResultModels.ToList();
            List<ResultModel> tmp1_k = db.SetResultModels.Where(u => u.ProjectID == id).ToList();
            List<ResultModel> tmp1_1 = db.SetResultModels.ToList();
            List<ResultModel> tmp1_2 = db.SetResultModels.ToList();
            List<ResultModel> tmp1_3 = db.SetResultModels.ToList();
            List<ResultModel> tmp1_t = db.SetResultModels.ToList();
            List<SessionHubModel> tmp2 = new List<SessionHubModel>();
            tmp2 = db.SetSessionHubModel.Where(u => u.UserRole == "2").ToList();

            //
            //Обработка списка результатов по фильтрам
            //
            if (id > 0)
            {
                tmp1 = tmp1.Where(u => u.ProjectID == id).ToList();
                tmp1_t = tmp1.Where(u => u.ProjectID == id).ToList();
            }
            if (idop > 0)
            {
                tmp1 = tmp1.Where(u => u.UserID == idop).ToList();
            }
            if (sd != "-1" && ed != "-1")
            {
                tmp1 = tmp1.Where(u => Convert.ToDateTime(u.Data.ToShortDateString()).CompareTo(Convert.ToDateTime(sd)) == 1 && Convert.ToDateTime(u.Data.ToShortDateString()).CompareTo(Convert.ToDateTime(ed)) == -1 || Convert.ToDateTime(u.Data.ToShortDateString()).CompareTo(Convert.ToDateTime(sd)) == 0 || Convert.ToDateTime(u.Data.ToShortDateString()).CompareTo(Convert.ToDateTime(ed)) == 0).ToList();
            }
            if (st != "-1" && et != "-1")
            {
                //(TimeSpan.Parse(u.Time).Ticks - ((TimeSpan.Parse(u.Time).Ticks - (TimeSpan.Parse(Convert.ToDateTime(u.Data).ToLongTimeString()).Ticks / 2)))))
                tmp1 = tmp1.Where(u => Convert.ToDateTime(u.Time).Ticks - ((Convert.ToDateTime(u.Time).Ticks - Convert.ToDateTime(u.Data).Ticks) / 2) >= Convert.ToDateTime(sd + " " + st).Ticks && Convert.ToDateTime(u.Time).Ticks - ((Convert.ToDateTime(u.Time).Ticks - Convert.ToDateTime(u.Data).Ticks) / 2) <= Convert.ToDateTime(ed + " " + et).Ticks).ToList();
                //tmp1 = tmp1.Where(u => Convert.ToDateTime(u.Time) >= Convert.ToDateTime(sd + " " + st) && Convert.ToDateTime(u.Time) <= Convert.ToDateTime(ed + " " + et) && Convert.ToDateTime(u.Data) >= Convert.ToDateTime(sd + " " + st) && Convert.ToDateTime(u.Data) <= Convert.ToDateTime(ed + " " + et)).ToList();
            }

            //
            //Определение выбрана ли дата или обработка всего списка 
            //
            if (tmp1.Count != 0)
            {
                Dictionary<DateTime, List<ResultModel>> keys = new Dictionary<DateTime, List<ResultModel>>();
                string startData, endData, startTime, endTime = "";

                if (sd != "-1" && ed != "-1")
                {
                    startData = sd;
                    startTime = st;
                    endData = ed;
                    endTime = et;
                }
                else
                {
                    startData = tmp1.First().Data.ToShortDateString();
                    startTime = tmp1.First().Data.ToLongTimeString();
                    List<ResultModel> tmp1t = tmp1.Reverse<ResultModel>().ToList();
                    endData = tmp1t.First().Data.ToShortDateString();
                    endTime = tmp1t.First().Data.ToLongTimeString();
                }

                DateTime BeginData = Convert.ToDateTime(startData).Date;
                DateTime EndData = Convert.ToDateTime(endData).Date;
                DateTime ttmp = new DateTime();
                DateTime ttmp2 = new DateTime();
                TimeSpan min = new TimeSpan();
                TimeSpan max = new TimeSpan();
                List<SchoolDay> list_schoolday = db.SetSchoolDay.ToList();

                int count;
                int countID = 0;
                double count_ch = 0;

                while (BeginData <= EndData)
                {
                    keys.Add(BeginData, tmp1.Where(u => u.Data.Date == BeginData.Date).ToList());
                    BeginData = BeginData.AddDays(1);
                }
                foreach (var i in keys)
                {
                    List<ResultModel> tmp_list = i.Value;
                    if (tmp_list.Count != 0)
                    {
                        int countlist = tmp_list.Count;
                        int iter = 0;
                        while( iter < countlist)
                        {
                            ResultModel item = tmp_list[iter];
                            if (list_schoolday.Where(u => u.Date.ToShortDateString() == item.Data.ToShortDateString() && u.UserId == item.UserID).Count() != 0)
                            {
                                tmp_list.Remove(item);
                                System.Diagnostics.Debug.WriteLine(item.UserName.ToString());
                                countlist--;
                            }
                            else
                            {
                                iter++;
                            }
                        }
                        //Количество анкет
                        count = tmp_list.Count();
                        //Вычисляем Срею время, Мин. - Макс. время, Сре. отклонение
                        min = new TimeSpan(23, 59, 59); max = new TimeSpan(0, 0, 0);
                        foreach (ResultModel item in tmp_list)
                        {
                            if (min >= DateTime.Parse(item.Time).Subtract(item.Data)) { min = DateTime.Parse(item.Time).Subtract(item.Data); }
                            if (max <= DateTime.Parse(item.Time).Subtract(item.Data)) { max = DateTime.Parse(item.Time).Subtract(item.Data); }
                        }
                        if (count == 1)
                        {
                            double seconds = 0;
                            foreach (ResultModel item in tmp_list)
                            {
                                seconds += DateTime.Parse(item.Time).Subtract(item.Data).TotalSeconds;
                            }
                            seconds = seconds / tmp_list.Count;
                            int minutes = (int)Math.Floor(seconds / 60);
                            seconds -= minutes * 60;
                            ttmp = new DateTime(1, 1, 1, 0, minutes, (int)seconds);
                            ttmp2 = new DateTime(1, 1, 1, 0, 0, 0);
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
                            secondsOT /= (tmp_list.Count - 1);
                            secondsOT = Math.Sqrt(secondsOT);
                            int minutesOt = (int)Math.Floor(secondsOT / 60);
                            int minutes = (int)Math.Floor(seconds / 60);
                            seconds -= minutes * 60;
                            secondsOT -= minutesOt * 60;
                            ttmp = new DateTime(1, 1, 1, 0, minutes, (int)seconds);
                            ttmp2 = new DateTime(1, 1, 1, 0, minutesOt, (int)secondsOT);
                        }

                        //Время работы
                        string tmp_new = i.Key.Date.ToShortDateString();
                        tmp2 = db.SetSessionHubModel.Where(u => u.UserId == idop && u.Date == tmp_new).ToList();
                        List<SessionHubModel> sessionHubs = db.SetSessionHubModel.Where(u => u.Date == tmp_new && u.UserRole == "2").OrderBy(s => s.UserId).ToList();
                        int countUser = 0;

                        //if (st == "-1" && et == "-1")
                        //{
                        //    countUser = sessionHubs.GroupBy(u => u.UserId).Select(group => group.First()).Count();

                        //}
                        //else
                        //{
                        //    countUser = tmp_list.Where(q => q.Data == null || TimeSpan.Parse(q.Data.ToLongTimeString()).CompareTo(TimeSpan.Parse(st)) == 1).GroupBy(u => u.UserID).Select(group => group.First()).Count();
                        //    //countUser = sessionHubs.Where(q => q.EndTime == null || TimeSpan.Parse(q.EndTime).CompareTo(TimeSpan.Parse(st)) == 1).GroupBy(u => u.UserId).Select(group => group.First()).Count();
                        //}

                        //Количество анкет в час
                        List<ResultModel> t_tmp = tmp_list.Reverse<ResultModel>().ToList();
                        List<ResultModel> tt_tmp1 = new List<ResultModel>();
                        List<ResultModel> tt_tmp2 = new List<ResultModel>();
                        List<ResultModel> tt_tmp3 = new List<ResultModel>();
                        List<User> tmp_users = db.SetUser.Where(u => u.RoleId == 2).ToList();
                        TimeSpan t1 = new TimeSpan();
                        TimeSpan t1_go = new TimeSpan();
                        countUser = 0;
                        if (count != 0)
                        {
                            if (tt_tmp2.Count != 0)
                            {
                                t1_go = TimeSpan.Parse(DateTime.Parse(tmp_list.Reverse<ResultModel>().First().Time).ToLongTimeString()).Subtract(TimeSpan.Parse(tmp_list.First().Data.ToLongTimeString()));
                                //КОЛИЧЕСТВО ПОЛЬЗОВАТЕЛЕЙ!!
                                //countUser = tt_tmp2.Where(q => q.Data == null || TimeSpan.Parse(q.Data.ToLongTimeString()).CompareTo(TimeSpan.Parse(st)) == 1).GroupBy(u => u.UserID).Select(group => group.First()).Count();
                            }
                            Double t2 = 0;
                            count_ch = 0;
                            foreach (var tu in tmp_users)
                            {
                                if (t_tmp.Where(c => c.UserID == tu.Id).ToList().Count != 0)
                                {
                                    if (list_schoolday.Where(u => u.UserId == tu.Id && u.Date.ToShortDateString() == t_tmp.Where(c => c.UserID == tu.Id).ToList().First().Data.ToShortDateString()).Count() < 1)
                                    {
                                        tt_tmp1 = t_tmp.Where(c => c.UserID == tu.Id).ToList();
                                        tt_tmp2 = tmp_list.Where(c => c.UserID == tu.Id).ToList();
                                        tt_tmp3 = tmp_list.Where(c => c.UserID == tu.Id).ToList();
                                        if (tt_tmp2.Count != 0)
                                        {
                                            string tstr1 = tt_tmp2.First().Data.ToLongDateString();
                                            string tstr1_tt_tmp1 = tt_tmp1.First().Data.ToLongDateString();
                                            int tstr2 = tt_tmp2.First().BlankID;
                                            int tstr2_tt_tmp1 = tt_tmp1.First().BlankID;
                                            int tstr3 = tt_tmp2.First().UserID;
                                            int tstr3_tt_tmp1 = tt_tmp1.First().UserID;

                                            if (tmp1_k.Where(u => u.Data.ToLongDateString() == tstr1 && u.UserID == tstr3 && u.BlankID < tstr2).Count() != 0 && tmp1_k.Where(u => u.Data.ToLongDateString() == tstr1 && u.UserID == tstr3 && u.BlankID > tstr2_tt_tmp1).Count() != 0)
                                            {
                                                t1 = TimeSpan.Parse(et).Subtract(TimeSpan.Parse(st));
                                                System.Diagnostics.Debug.WriteLine("et - st => " + et + "  " + st);
                                                countUser++;
                                            }
                                            if (tmp1_k.Where(u => u.Data.ToLongDateString() == tstr1 && u.UserID == tstr3 && u.BlankID < tstr2).Count() != 0 && tmp1_k.Where(u => u.Data.ToLongDateString() == tstr1 && u.UserID == tstr3 && u.BlankID > tstr2_tt_tmp1).Count() == 0)
                                            {
                                                t1 = TimeSpan.Parse(DateTime.Parse(tt_tmp1.First().Time).ToLongTimeString()).Subtract(TimeSpan.Parse(st));
                                                System.Diagnostics.Debug.WriteLine("n - st => " + DateTime.Parse(tt_tmp1.First().Time).ToLongTimeString() + "  " + st);
                                                countUser++;
                                            }
                                            if (tmp1_k.Where(u => u.Data.ToLongDateString() == tstr1 && u.UserID == tstr3 && u.BlankID < tstr2).Count() == 0 && tmp1_k.Where(u => u.Data.ToLongDateString() == tstr1 && u.UserID == tstr3 && u.BlankID > tstr2_tt_tmp1).Count() != 0)
                                            {
                                                t1 = TimeSpan.Parse(et).Subtract(TimeSpan.Parse(tt_tmp2.Where(u => u.ProjectID == id).First().Data.ToLongTimeString()));
                                                System.Diagnostics.Debug.WriteLine("et - 1 => " + et + "  " + tt_tmp2.First().Data.ToLongTimeString());
                                                countUser++;
                                            }
                                            if (tmp1_k.Where(u => u.Data.ToLongDateString() == tstr1 && u.UserID == tstr3 && u.BlankID < tstr2).Count() == 0 && tmp1_k.Where(u => u.Data.ToLongDateString() == tstr1 && u.UserID == tstr3 && u.BlankID > tstr2_tt_tmp1).Count() == 0)
                                            {
                                                t1 = TimeSpan.Parse(DateTime.Parse(tt_tmp1.First().Time).ToLongTimeString()).Subtract(TimeSpan.Parse(tt_tmp2.First().Data.ToLongTimeString()));
                                                System.Diagnostics.Debug.WriteLine("n - 1 => " + DateTime.Parse(tt_tmp1.First().Time).ToLongTimeString() + "  " + tt_tmp2.First().Data.ToLongTimeString());
                                                countUser++;
                                            }

                                            t2 = t1.TotalHours;
                                            count_ch += tt_tmp2.Count / t2;

                                        }
                                    }
                                }
                            }
                            count_ch = count_ch / countUser;

                            t1_go = TimeSpan.Parse(DateTime.Parse(tmp_list.Reverse<ResultModel>().First().Time).ToLongTimeString()).Subtract(TimeSpan.Parse(tmp_list.First().Data.ToLongTimeString()));

                        }

                        //string sessionStartTime = "";
                        //string sessionEndTime = "";
                        //string sessionTimeInSystem = "00:00:00";
                        //string sessionAfkTime = "00:00:00";
                        //if (tmp2.Count != 0)
                        //{
                        //    sessionStartTime = tmp2.First().StartTime;
                        //    sessionEndTime = tmp2.Reverse<SessionHubModel>().First().EndTime;
                        //    if (sessionEndTime == null) sessionEndTime = DateTime.Now.ToLongTimeString();
                        //    foreach (var q in tmp2)
                        //    {
                        //        if (q.TimeInSystem == null)
                        //        {
                        //            sessionTimeInSystem = (TimeSpan.Parse(sessionTimeInSystem) + TimeSpan.Parse(DateTime.Now.ToLongTimeString()) - TimeSpan.Parse(q.StartTime)).ToString();
                        //        }
                        //        else
                        //        {
                        //            sessionTimeInSystem = (TimeSpan.Parse(sessionTimeInSystem) + TimeSpan.Parse(q.TimeInSystem)).ToString();
                        //        }
                        //        if (q.AfkTime == null)
                        //        {
                        //            sessionAfkTime = (TimeSpan.Parse(sessionAfkTime) + TimeSpan.Parse("00:00:00")).ToString();
                        //        }
                        //        else
                        //        {
                        //            sessionAfkTime = (TimeSpan.Parse(sessionAfkTime) + TimeSpan.Parse(q.AfkTime)).ToString();
                        //        }
                        //    }
                        //}

                        StatResViewModel tmp_statResViewModel = new StatResViewModel();
                        tmp_statResViewModel.IdView = countID;
                        tmp_statResViewModel.DataView = i.Key.ToShortDateString();
                        tmp_statResViewModel.CountUserView = idop == -1 ? countUser.ToString() : "0";
                        tmp_statResViewModel.CountProjectView = tmp_list.Count().ToString();
                        tmp_statResViewModel.CountHourView = string.Format("{0:0.##}", count_ch);
                        tmp_statResViewModel.MediumTimeView = ttmp.ToLongTimeString();
                        tmp_statResViewModel.MinLenghtView = min.ToString();
                        tmp_statResViewModel.MaxLenghtView = max.ToString();
                        tmp_statResViewModel.MediumView = ttmp2.ToLongTimeString();
                        //tmp_statResViewModel.TimeUpView = tmp2.Count != 0 ? sessionStartTime : "0";
                        //tmp_statResViewModel.TimeOutView = tmp2.Count != 0 ? sessionEndTime : "0";
                        //tmp_statResViewModel.TimeWorkView = tmp2.Count != 0 ? sessionTimeInSystem : "0";
                        tmp_statResViewModel.OneNView = t1_go.ToString(); ;
                        //tmp_statResViewModel.TimeAfkView = tmp2.Count != 0 ? sessionAfkTime : "0";

                        StatRes.Add(tmp_statResViewModel);
                        countID++;
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("===============================================>>>>>>>>>>>>>>>>");
                Dictionary<DateTime, List<ResultModel>> keys = new Dictionary<DateTime, List<ResultModel>>();
                string startData, endData, startTime, endTime = "";

                if (sd != "-1" && ed != "-1")
                {
                    startData = sd;
                    startTime = st;
                    endData = ed;
                    endTime = et;
                }
                else
                {
                    startData = tmp1_t.First().Data.ToShortDateString();
                    startTime = tmp1_t.First().Data.ToLongTimeString();
                    List<ResultModel> tmp1t = tmp1_t.Reverse<ResultModel>().ToList();
                    endData = tmp1t.First().Data.ToShortDateString();
                    endTime = tmp1t.First().Data.ToLongTimeString();
                }

                DateTime BeginData = Convert.ToDateTime(startData).Date;
                DateTime EndData = Convert.ToDateTime(endData).Date;
                int countID = 0;

                while (BeginData <= EndData)
                {
                    keys.Add(BeginData, tmp1.Where(u => u.Data.Date == BeginData.Date).ToList());
                    BeginData = BeginData.AddDays(1);
                }
                foreach (var i in keys)
                {
                    List<ResultModel> tmp_list = i.Value;

                    //Время работы
                    string tmp_new = i.Key.Date.ToShortDateString();
                    tmp2 = db.SetSessionHubModel.Where(u => u.UserId == idop && u.Date == tmp_new).ToList();
                    List<SessionHubModel> sessionHubs = db.SetSessionHubModel.Where(u => u.Date == tmp_new).OrderBy(s => s.UserId).ToList();
                    int countUser = 0;
                    //КОЛИЧЕСТВО ПОЛЬЗОВАТЕЛЕЙ!!
                    if (st == "-1" && et == "-1")
                    {
                        countUser = sessionHubs.GroupBy(u => u.UserId).Select(group => group.First()).Count();
                    }
                    else
                    {
                        countUser = sessionHubs.Where(q => q.EndTime == null || TimeSpan.Parse(q.EndTime).CompareTo(TimeSpan.Parse(st)) == 1).GroupBy(u => u.UserId).Select(group => group.First()).Count();
                    }
                    //string sessionStartTime = "";
                    //string sessionEndTime = "";
                    //string sessionTimeInSystem = "00:00:00";
                    //string sessionAfkTime = "00:00:00";
                    //if (tmp2.Count != 0)
                    //{
                    //    sessionStartTime = tmp2.First().StartTime;
                    //    sessionEndTime = tmp2.Reverse<SessionHubModel>().First().EndTime;
                    //    if (sessionEndTime == null) sessionEndTime = DateTime.Now.ToLongTimeString();
                    //    foreach (var q in tmp2)
                    //    {
                    //        if (q.TimeInSystem == null)
                    //        {
                    //            sessionTimeInSystem = (TimeSpan.Parse(sessionTimeInSystem) + TimeSpan.Parse(DateTime.Now.ToLongTimeString()) - TimeSpan.Parse(q.StartTime)).ToString();
                    //        }
                    //        else
                    //        {
                    //            sessionTimeInSystem = (TimeSpan.Parse(sessionTimeInSystem) + TimeSpan.Parse(q.TimeInSystem)).ToString();
                    //        }
                    //        if (q.AfkTime == null)
                    //        {
                    //            sessionAfkTime = (TimeSpan.Parse(sessionAfkTime) + TimeSpan.Parse("00:00:00")).ToString();
                    //        }
                    //        else
                    //        {
                    //            sessionAfkTime = (TimeSpan.Parse(sessionAfkTime) + TimeSpan.Parse(q.AfkTime)).ToString();
                    //        }
                    //    }
                    //}

                    StatResViewModel tmp_statResViewModel = new StatResViewModel();
                    tmp_statResViewModel.IdView = countID;
                    tmp_statResViewModel.DataView = i.Key.ToShortDateString();
                    tmp_statResViewModel.CountUserView = idop == -1 ? countUser.ToString() : "0";
                    tmp_statResViewModel.CountProjectView = "0";
                    tmp_statResViewModel.CountHourView = "0";
                    tmp_statResViewModel.MediumTimeView = "00:00:00";
                    tmp_statResViewModel.MinLenghtView = "00:00:00";
                    tmp_statResViewModel.MaxLenghtView = "00:00:00";
                    tmp_statResViewModel.MediumView = "00:00:00";
                    //tmp_statResViewModel.TimeUpView = tmp2.Count != 0 ? sessionStartTime : "0";
                    //tmp_statResViewModel.TimeOutView = tmp2.Count != 0 ? sessionEndTime : "0";
                    //tmp_statResViewModel.TimeWorkView = tmp2.Count != 0 ? sessionTimeInSystem : "0";
                    tmp_statResViewModel.OneNView = "00:00:00";
                    //tmp_statResViewModel.TimeAfkView = tmp2.Count != 0 ? sessionAfkTime : "0";

                    StatRes.Add(tmp_statResViewModel);
                    countID++;
                }
            }

            //ИТОГ
            double ccount = 0;
            double ccountMAX = 0;
            double ccountMIN = 0;
            double tmpd1 = 0;
            double tmpd1MAX = 0;
            double tmpd1MIN = 0;
            double tmpd2 = 0;
            double tmpd3 = 0;
            double tmpd3MAX = 0;
            double tmpd3MIN = 0;
            long ticks1 = 0;
            long ticks1MAX = 0;
            long ticks1MIN = 0;
            long ticks2 = 0;
            long ticks3 = 0;
            long ticks4 = 0;
            long ticks4MAX = 0;
            long ticks4MIN = 0;
            long ticks5 = 0;
            long ticks5MAX = 0;
            long ticks5MIN = 0;
            long ticksmax = 0;
            long ticksmin = 0;

            StatResItogViewModel model = new StatResItogViewModel();
            ticksmax = TimeSpan.Parse(StatRes.First().MaxLenghtView).Ticks;
            ticksmin = TimeSpan.Parse(StatRes.First().MinLenghtView).Ticks;
            ccountMAX = ccountMIN = Convert.ToDouble(StatRes.First().CountUserView);
            tmpd1MAX = tmpd1MIN = Convert.ToDouble(StatRes.First().CountProjectView);
            tmpd3MAX = tmpd3MIN = Convert.ToDouble(StatRes.First().CountHourView);
            ticks1MAX = ticks1MIN = TimeSpan.Parse(StatRes.First().MediumTimeView).Ticks;
            ticks4MAX = ticks4MIN = TimeSpan.Parse(StatRes.First().MediumView).Ticks;
            ticks5MAX = ticks5MIN = TimeSpan.Parse(StatRes.First().OneNView).Ticks;
            foreach (var i in StatRes)
            {
                ccount += Convert.ToDouble(i.CountUserView);
                if (ccountMAX < Convert.ToDouble(i.CountUserView)) ccountMAX = Convert.ToDouble(i.CountUserView);
                if (ccountMIN > Convert.ToDouble(i.CountUserView)) ccountMIN = Convert.ToDouble(i.CountUserView);
                tmpd1 += Convert.ToDouble(i.CountProjectView);
                if (tmpd1MAX < Convert.ToDouble(i.CountProjectView)) tmpd1MAX = Convert.ToDouble(i.CountProjectView);
                if (tmpd1MIN > Convert.ToDouble(i.CountProjectView)) tmpd1MIN = Convert.ToDouble(i.CountProjectView);
                tmpd2 += Convert.ToDouble(i.CountProjectView);
                tmpd3 += Convert.ToDouble(i.CountHourView);
                if (tmpd3MAX < Convert.ToDouble(i.CountHourView)) tmpd3MAX = Convert.ToDouble(i.CountHourView);
                if (tmpd3MIN > Convert.ToDouble(i.CountHourView)) tmpd3MIN = Convert.ToDouble(i.CountHourView);
                ticks1 += TimeSpan.Parse(i.MediumTimeView).Ticks;
                if (ticks1MAX < TimeSpan.Parse(i.MediumTimeView).Ticks) ticks1MAX = TimeSpan.Parse(i.MediumTimeView).Ticks;
                if (ticks1MIN > TimeSpan.Parse(i.MediumTimeView).Ticks) ticks1MIN = TimeSpan.Parse(i.MediumTimeView).Ticks;
                ticks2 += TimeSpan.Parse(i.MinLenghtView).Ticks;
                if (ticksmin > TimeSpan.Parse(i.MinLenghtView).Ticks) ticksmin = TimeSpan.Parse(i.MinLenghtView).Ticks;
                ticks3 += TimeSpan.Parse(i.MaxLenghtView).Ticks;
                if (ticksmax < TimeSpan.Parse(i.MaxLenghtView).Ticks) ticksmax = TimeSpan.Parse(i.MaxLenghtView).Ticks;
                ticks4 += TimeSpan.Parse(i.MediumView).Ticks;
                if (ticks4MAX < TimeSpan.Parse(i.MediumView).Ticks) ticks4MAX = TimeSpan.Parse(i.MediumView).Ticks;
                if (ticks4MIN > TimeSpan.Parse(i.MediumView).Ticks) ticks4MIN = TimeSpan.Parse(i.MediumView).Ticks;
                ticks5 += TimeSpan.Parse(i.OneNView).Ticks;
                if (ticks5MAX < TimeSpan.Parse(i.OneNView).Ticks) ticks5MAX = TimeSpan.Parse(i.OneNView).Ticks;
                if (ticks5MIN > TimeSpan.Parse(i.OneNView).Ticks) ticks5MIN = TimeSpan.Parse(i.OneNView).Ticks;
            }

            StatResViewModel II = new StatResViewModel();
            StatResViewModel SR = new StatResViewModel();
            StatResViewModel MAX = new StatResViewModel();
            StatResViewModel MIN = new StatResViewModel();
            //ИТОГ
            II.DataView = "ИТОГ:";
            //Количество пользователей
            II.CountUserView = "";
            //Кол-во анкет
            II.CountProjectView = tmpd1.ToString("0");
            //Количество анкет в час
            II.CountHourView = "";
            //Среднее время 
            II.MediumTimeView = "";
            //Минимально
            II.MinLenghtView = "";
            //Максимальное
            II.MaxLenghtView = "";
            //Среднее
            II.MediumView = TimeSpan.FromTicks(ticks4).ToString(@"hh\:mm\:ss");
            //Общее время
            II.OneNView = TimeSpan.FromTicks(ticks5).ToString();

            //СРЕДНЕЕ
            SR.DataView = "СРЕДНЕЕ:";
            //Количество пользователей
            SR.CountUserView = (ccount / StatRes.Count).ToString("0");
            //Кол-во анкет
            SR.CountProjectView = (tmpd2 / StatRes.Count).ToString("0");
            //Количество анкет в час
            SR.CountHourView = (tmpd3 / StatRes.Count).ToString("0.00");
            //Среднее время
            SR.MediumTimeView = TimeSpan.FromTicks(ticks1 / (long)StatRes.Count).ToString(@"hh\:mm\:ss");
            //Минимально
            SR.MinLenghtView = TimeSpan.FromTicks(ticks2 / (long)StatRes.Count).ToString(@"hh\:mm\:ss");
            //Максимальное
            SR.MaxLenghtView = TimeSpan.FromTicks(ticks3 / (long)StatRes.Count).ToString(@"hh\:mm\:ss");
            //Среднее
            SR.MediumView = TimeSpan.FromTicks(ticks4 / (long)StatRes.Count).ToString(@"hh\:mm\:ss");
            //Общее время
            SR.OneNView = TimeSpan.FromTicks(ticks5 / (long)StatRes.Count).ToString(@"hh\:mm\:ss");

            //MAX
            MAX.DataView = "MAX:";
            //Количество пользователей
            MAX.CountUserView = ccountMAX.ToString("0");
            //Кол-во анкет
            MAX.CountProjectView = (tmpd1MAX).ToString("0");
            //Количество анкет в час
            MAX.CountHourView = (tmpd3MAX).ToString("0.00");
            //Среднее время
            MAX.MediumTimeView = TimeSpan.FromTicks(ticks1MAX).ToString(@"hh\:mm\:ss");
            //Минимально
            MAX.MinLenghtView = "";
            //Максимальное
            MAX.MaxLenghtView = TimeSpan.FromTicks(ticksmax).ToString(@"hh\:mm\:ss");
            //Среднее
            MAX.MediumView = TimeSpan.FromTicks(ticks4MAX).ToString(@"hh\:mm\:ss");
            //Общее время
            MAX.OneNView = TimeSpan.FromTicks(ticks5MAX).ToString(@"hh\:mm\:ss");

            //MIN
            MIN.DataView = "MIN:";
            //Количество пользователей
            MIN.CountUserView = ccountMIN.ToString("0");
            //Кол-во анкет
            MIN.CountProjectView = (tmpd1MIN).ToString("0");
            //Количество анкет в час
            MIN.CountHourView = (tmpd3MIN).ToString("0.00");
            //Среднее время
            MIN.MediumTimeView = TimeSpan.FromTicks(ticks1MIN).ToString(@"hh\:mm\:ss");
            //Минимально
            MIN.MinLenghtView = TimeSpan.FromTicks(ticksmin).ToString(@"hh\:mm\:ss");
            //Максимальное
            MIN.MaxLenghtView = "";
            //Среднее
            MIN.MediumView = TimeSpan.FromTicks(ticks4MIN).ToString(@"hh\:mm\:ss");
            //Общее время
            MIN.OneNView = TimeSpan.FromTicks(ticks5MIN).ToString(@"hh\:mm\:ss");

            StatRes.Add(II);
            StatRes.Add(SR);
            StatRes.Add(MAX);
            StatRes.Add(MIN);
            return JsonConvert.SerializeObject(StatRes);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Test()
        {
            return PartialView();
        }

        [HttpGet]
        public JsonResult GetListMySQL()
        {
            using (PhoneContext pc = new PhoneContext())
            {
                return Json(pc.Phones.ToList(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public void SetNumber(string str_numb)
        {
            using (PhoneContext pc = new PhoneContext())
            {
                Phone tmp = new Phone();
                tmp.Number = str_numb;
                pc.Phones.Add(tmp);
                pc.SaveChanges();
            }
        }

        [HttpPost]
        public void AddRow()
        {
            using (PhoneContext pc = new PhoneContext())
            {
                for (int i = 1; i <= 10; i++)
                {
                    pc.Database.ExecuteSqlCommand("INSERT INTO testtable VALUES (" + i + "," + (i * 100) + ")");
                }
            }
        }

        class TmpClass
        {
            public int Id { get; set; }
            public string Number { get; set; }
            public string Status { get; set; }
        }

        private async Task SetCell(TestTable item)
        {
            PhoneContext pc = new PhoneContext();
            await pc.Database.ExecuteSqlCommandAsync("INSERT INTO testtable VALUES (" + item.Id + "," + item.Name + ")");
        }

        [HttpPost]
        public JsonResult Import(string table)
        {
            Dictionary<string, int> callback = new Dictionary<string, int>();
            try
            {
                var fileContent = Request.Files[0];
                if (fileContent != null && fileContent.ContentLength > 0)
                {
                    string name_file = fileContent.FileName;
                    using (PhoneContext pc = new PhoneContext())
                    {
                        pc.Database.ExecuteSqlCommand("LOAD DATA INFILE \"D:/number/" + name_file + "\" INTO TABLE table" + table + " character set cp1251 FIELDS TERMINATED BY  \";\" ENCLOSED BY  \"\" ESCAPED BY  \"\\\\\" LINES TERMINATED BY  \"\\r\\n\"");

                        pc.Database.ExecuteSqlCommand("UPDATE name_table SET Name=\"" + name_file + "\" WHERE Id=" + table);

                        var tmp = pc.Database.SqlQuery<TmpClass>("SELECT * FROM table" + table);
                        callback.Add("Common", tmp.Count());
                        callback.Add("Not_answer", tmp.Where(u => u.Status == "нет ответа" || u.Status == "линия не найдена").ToList().Count());
                        callback.Add("Connect", tmp.Where(u => u.Status == "connect" || u.Status == "завершено").ToList().Count());
                        callback.Add("Balance", tmp.Where(u => u.Status == "0").ToList().Count());
                        callback.Add("Busy", tmp.Where(u => u.Status == "занято").ToList().Count());
                        callback.Add("Recall", tmp.Where(u => u.Status == "перезвонить").ToList().Count());
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                Response.AppendToLog(e.Message);
                Response.AppendToLog(e.TargetSite.ToString());
                Response.AppendToLog(e.ToString());
            }
            return Json(callback);
        }

        class TableInfo
        {
            public string Tables_in_phone { get; set; }
        }


        [HttpGet]
        public JsonResult GetListTable()
        {

            using (PhoneContext pc = new PhoneContext())
            {
                var tmp = pc.Database.SqlQuery<TableInfo>("SHOW TABLES FROM phone").ToList();
                System.Diagnostics.Debug.WriteLine("Count >>> " + tmp.Count());
                if (tmp == null) { throw new Exception("tmp is null"); }
                List<string> send_list = new List<string>();
                foreach (var item in tmp)
                {
                    System.Diagnostics.Debug.WriteLine(">>> " + item.Tables_in_phone);
                    send_list.Add(item.Tables_in_phone);
                }
                return Json(send_list, JsonRequestBehavior.AllowGet);
            }
        }

        class NameTable
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [HttpGet]
        public JsonResult GetNameTable()
        {
            List<NameTable> list_name = new List<NameTable>();
            using (PhoneContext pc = new PhoneContext())
            {
                var tmp_list = pc.Database.SqlQuery<NameTable>("SELECT * FROM name_table");
                System.Diagnostics.Debug.WriteLine(tmp_list.Count());
                foreach (var item in tmp_list)
                {
                    list_name.Add(item);
                }
                return Json(list_name, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetNumbers(int id)
        {
            List<TmpClass> send_list = new List<TmpClass>();
            Dictionary<string, int> callback = new Dictionary<string, int>();
            using (PhoneContext pc = new PhoneContext())
            {
                var tmp = pc.Database.SqlQuery<TmpClass>("SELECT * FROM table" + id).ToList();
                callback.Add("Common", tmp.Count());
                callback.Add("Not_answer", tmp.Where(u => u.Status == "нет ответа" || u.Status == "линия не найдена").ToList().Count());
                callback.Add("Connect", tmp.Where(u => u.Status == "connect" || u.Status == "завершено").ToList().Count());
                callback.Add("Balance", tmp.Where(u => u.Status == "0").ToList().Count());
                callback.Add("Busy", tmp.Where(u => u.Status == "занято").ToList().Count());
                callback.Add("Recall", tmp.Where(u => u.Status == "перезвонить").ToList().Count());
            }
            return Json(callback, JsonRequestBehavior.AllowGet);
        }

        public FileResult ExportToCSV(int id)
        {
            using (PhoneContext pc = new PhoneContext())
            {
                var tmp = pc.Database.SqlQuery<TmpClass>("SELECT * FROM table" + id);
                var name_file = pc.Database.SqlQuery<NameTable>("SELECT * FROM name_table WHERE Id=" + id + " ").ToList();
                StringWriter sw = new StringWriter();
                foreach (var item in tmp)
                {
                    sw.WriteLine(string.Format("\"{0}\";\"{1}\";\"{2}\"", item.Id, item.Number, item.Status));
                }

                return File(Encoding.GetEncoding(1251).GetBytes(sw.ToString()), "text/csv", name_file[0].Name);
            }
        }

        [HttpPost]
        public void CreateNewTable(int id)
        {
            using (PhoneContext pc = new PhoneContext())
            {
                pc.Database.ExecuteSqlCommand("INSERT INTO name_table VALUES (" + (id + 1) + ", null)");
                pc.Database.ExecuteSqlCommand("CREATE TABLE table" + (id + 1) + " (Id int not null primary key, Number varchar(45), Status varchar(45))");
            }
        }

        [HttpPost]
        public void DeleteTable(int id)
        {
            using (PhoneContext pc = new PhoneContext())
            {
                pc.Database.ExecuteSqlCommand("DELETE FROM name_table WHERE Id=" + id);
                pc.Database.ExecuteSqlCommand("DROP TABLE table" + id);
            }
        }

        [HttpPost]
        public void ClearTable(int id)
        {
            System.Diagnostics.Debug.WriteLine("ID >>> " + id);
            using (PhoneContext pc = new PhoneContext())
            {
                pc.Database.ExecuteSqlCommand("UPDATE name_table SET Name=null WHERE Id=" + id);
                pc.Database.ExecuteSqlCommand("TRUNCATE TABLE table" + id);
            }
        }
        public class StatResItogViewModel
        {
            public string CountViewI { get; set; }
            public string CountDateViewI { get; set; }
            public string CountHourViewI { get; set; }
            public string MediumTimeViewI { get; set; }
            public string MinLenghtViewI { get; set; }
            public string MaxLenghtViewI { get; set; }
            public string MediumViewI { get; set; }
            public string OneNViewI { get; set; }
        }

        public void TimeAfkBase(string time)
        {

            SetTimeAfk tmp = db.SetTimeAfk.Where(u => u.Id == 1).First();
            tmp.Time = (Convert.ToInt32(time) * 60).ToString();

            db.Entry(tmp).State = EntityState.Modified;
            db.SaveChanges();
        }


        public ActionResult OprosYaroslavl()
        {
            return View();
        }

        [HttpPost]
        public async Task<int> postSaveOpYA(List<string> mass)
        {
            System.Diagnostics.Debug.WriteLine(mass.Count());
            ApplicationContext context = new ApplicationContext();
            OprosYaroslav list = new OprosYaroslav();
            list.q1 = mass[0];
            list.q2 = mass[1];
            list.q3 = mass[2];
            list.q4 = mass[3];
            list.q5 = mass[4];
            list.q6 = mass[5];
            list.q7 = mass[6];
            list.q8 = mass[7];
            list.q9 = mass[8];
            list.q10 = mass[9];
            list.q11 = mass[10];
            list.q12 = mass[11];
            list.q13 = mass[12];
            list.q14 = mass[13];
            list.q15 = mass[14];
            list.q16 = mass[15];
            list.q17 = mass[16];
            list.q18 = mass[17];
            list.q19 = mass[18];
            list.q20 = mass[19];
            list.q21 = mass[20];
            list.q22 = mass[21];
            list.q23 = mass[22];
            list.q24 = mass[23];
            list.q25 = mass[24];
            list.q26 = mass[25];
            list.q27 = mass[26];
            list.q28 = mass[27];
            list.q29 = mass[28];
            list.q30 = mass[29];
            list.q31 = mass[30];
            list.q32 = mass[31];
            list.q33 = mass[32];
            list.q34 = mass[33];
            list.q35 = mass[34];
            list.q36 = mass[35];
            list.q37 = mass[36];
            list.q38 = mass[37];
            list.q39 = mass[38];
            list.q40 = mass[39];
            list.q41 = mass[40];
            list.q42 = mass[41];
            list.q43 = mass[42];
            list.q44 = mass[43];
            list.q45 = mass[44];
            list.q46 = mass[45];
            list.q47 = mass[46];
            list.q48 = mass[47];
            list.q49 = mass[48];
            list.q50 = mass[49];
            list.q51 = mass[50];
            list.q52 = mass[51];
            list.q53 = mass[52];
            list.q54 = mass[53];
            list.q55 = mass[54];
            list.q56 = mass[55];
            list.q57 = mass[56];
            list.q58 = mass[57];
            list.q59 = mass[58];
            list.q60 = mass[59];
            list.q61 = mass[60];
            list.q62 = mass[61];
            list.q63 = mass[62];
            list.q64 = mass[63];
            list.q65 = mass[64];
            list.q66 = mass[65];
            list.q67 = mass[66];
            list.q68 = mass[67];
            list.q69 = mass[68];
            list.q70 = mass[69];
            list.q71 = mass[70];
            list.q72 = mass[71];
            list.q73 = mass[72];
            list.q74 = mass[73];
            list.q75 = mass[74];
            list.q76 = mass[75];
            list.q77 = mass[76];
            list.q78 = mass[77];
            list.q79 = mass[78];
            list.q80 = mass[79];
            list.q81 = mass[80];
            list.q82 = mass[81];
            list.q83 = mass[82];
            list.q84 = mass[83];
            list.q85 = mass[84];
            list.q86 = mass[85];
            list.q87 = mass[86];
            list.q88 = mass[87];
            list.q89 = mass[88];
            list.q90 = mass[89];
            list.q91 = mass[90];
            list.q92 = mass[91];
            list.q93 = mass[92];
            list.q94 = mass[93];
            list.q95 = mass[94];
            list.q96 = mass[95];
            list.q97 = mass[96];
            list.q98 = mass[97];
            context.SetOprosYaroslav.Add(list);
            await context.SaveChangesAsync();
            return list.id;
        }

        public ActionResult OprosNarkNas()
        {
            return View();
        }

        [HttpPost]
        public async Task<int> postSaveOpNN(List<string> mass)
        {
            System.Diagnostics.Debug.WriteLine(mass.Count());
            ApplicationContext context = new ApplicationContext();
            OprosNN list = new OprosNN();
            list.q1 = mass[0];
            list.q2 = mass[1];
            list.q3 = mass[2];
            list.q4 = mass[3];
            list.q5 = mass[4];
            list.q6 = mass[5];
            list.q7 = mass[6];
            list.q8 = mass[7];
            list.q9 = mass[8];
            list.q10 = mass[9];
            list.q11 = mass[10];
            list.q12 = mass[11];
            list.q13 = mass[12];
            list.q14 = mass[13];
            list.q15 = mass[14];
            list.q16 = mass[15];
            list.q17 = mass[16];
            list.q18 = mass[17];
            list.q19 = mass[18];
            list.q20 = mass[19];
            list.q21 = mass[20];
            list.q22 = mass[21];
            list.q23 = mass[22];
            list.q24 = mass[23];
            list.q25 = mass[24];
            list.q26 = mass[25];
            list.q27 = mass[26];
            list.q28 = mass[27];
            list.q29 = mass[28];
            list.q30 = mass[29];
            list.q31 = mass[30];
            list.q32 = mass[31];
            list.q33 = mass[32];
            list.q34 = mass[33];
            list.q35 = mass[34];
            list.q36 = mass[35];
            list.q37 = mass[36];
            list.q38 = mass[37];
            list.q39 = mass[38];
            list.q40 = mass[39];
            list.q41 = mass[40];
            list.q42 = mass[41];
            list.q43 = mass[42];
            list.q44 = mass[43];
            list.q45 = mass[44];
            list.q46 = mass[45];
            list.q47 = mass[46];
            list.q48 = mass[47];
            list.q49 = mass[48];
            list.q50 = mass[49];
            list.q51 = mass[50];
            list.q52 = mass[51];
            list.q53 = mass[52];
            list.q54 = mass[53];
            list.q55 = mass[54];
            list.q56 = mass[55];
            list.q57 = mass[56];
            list.q58 = mass[57];
            list.q59 = mass[58];
            list.q60 = mass[59];
            list.q61 = mass[60];
            list.q62 = mass[61];
            list.q63 = mass[62];
            list.q64 = mass[63];
            list.q65 = mass[64];
            list.q66 = mass[65];
            list.q67 = mass[66];
            list.q68 = mass[67];
            list.q69 = mass[68];
            list.q70 = mass[69];
            list.q71 = mass[70];
            list.q72 = mass[71];
            list.q73 = mass[72];
            list.q74 = mass[73];
            list.q75 = mass[74];
            list.q76 = mass[75];
            list.q77 = mass[76];
            list.q78 = mass[77];
            list.q79 = mass[78];
            list.q80 = mass[79];
            list.q81 = mass[80];
            list.q82 = mass[81];
            list.q83 = mass[82];
            list.q84 = mass[83];
            list.q85 = mass[84];
            list.q86 = mass[85];
            list.q87 = mass[86];
            list.q88 = mass[87];
            list.q89 = mass[88];
            list.q90 = mass[89];
            list.q91 = mass[90];
            list.q92 = mass[91];
            //list.q93 = mass[92];
            //list.q94 = mass[93];
            //list.q95 = mass[94];
            //list.q96 = mass[95];
            //list.q97 = mass[96];
            //list.q98 = mass[97];
            context.SetOprosNN.Add(list);
            await context.SaveChangesAsync();
            return list.id;
        }

        [HttpPost]
        public void ChangeEncodeProject(int id, string encode)
        {
            ProjectModel tmp = db2.SetProjectModels.FirstOrDefault(u => u.Id == id);
            if (tmp != null)
            {
                tmp.SettingEncode = encode;
                db2.SaveChanges();
            }
        }

    }
}
