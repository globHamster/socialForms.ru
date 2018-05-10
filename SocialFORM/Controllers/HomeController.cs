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

            //var privileges = db.SetPriveleges.Include(a => a.User);
            //ViewBag.Privileges = privileges;
            //var datasets = db.SetDataUsers.Include(a => a.User);
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
                RoleView = Role.Name
            }
            );
            ViewBag.operator_id = result.First().IdView;
            ViewData["Name"] = result.First().NameView;
            ViewData["Family"] = result.First().FamilyView;
            ViewData["Role"] = result.First().RoleView;

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

        ProjectContext db4 = new ProjectContext();
        [Authorize(Roles = "Admin")]
        public ActionResult Project()
        {
            return PartialView(db4.SetProjectModels);
        }

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

        public ActionResult Users()
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
            return PartialView(result.ToList());
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


        public void ExportToEXCEL(int id_p)
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
                listGroupExport.AddRange(group_context.SetGroupModels.Where(u => u.ProjectID == id_p && u.GroupID != null).OrderBy(u => u.IndexQuestion).ToList());
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
            foreach (var item in listQuestionExport)
            {
                using (QuestionContext q_context = new QuestionContext())
                {
                    listAnswerAllExport.Add(item.Key, q_context.SetAnswers.Where(u => u.QuestionID == item.Key).ToList());
                }
            }

            var products = new System.Data.DataTable();

            products.Columns.Add("Номер");
            products.Columns.Add("ФИО");
            products.Columns.Add("Номер телефона");
            products.Columns.Add("Начало анкеты");
            products.Columns.Add("Конец анкеты");

            foreach (var item in listGroupExport)
            {
                QuestionModel tmp = listQuestionExport[(int)item.QuestionID];
                switch (tmp.TypeQuestion)
                {
                    case Models.Question.Type.Single:
                        products.Columns.Add(item.GroupName);
                        if (listAnswerAllExport[(int)item.QuestionID].Where(u => u.isFreeArea == true).Count() > 0)
                        {
                            products.Columns.Add(item.GroupName + "_др");
                        }
                        break;
                    case Models.Question.Type.Multiple:
                        int tmp_count = listAnswerAllExport[(int)item.QuestionID].Count();
                        for (int i = 1; i <= tmp_count; i++)
                        {
                            products.Columns.Add(item.GroupName + "_" + i);
                        }
                        if (listAnswerAllExport[(int)item.QuestionID].Where(u => u.isFreeArea == true).Count() == 1)
                        {
                            products.Columns.Add(item.GroupName + "_др");
                        }
                        else if (listAnswerAllExport[(int)item.QuestionID].Where(u => u.isFreeArea == true).Count() > 1)
                        {
                            int count = 1;
                            foreach (var item_answer in listAnswerAllExport[(int)item.QuestionID].Where(u => u.isFreeArea == true))
                            {
                                products.Columns.Add(item.GroupName + "_др_" + count);
                                count++;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            List<string> tmp_str = new List<string>();

            foreach (var item in listResultExport)
            {
                tmp_str.Add(item.Id.ToString());
                tmp_str.Add(item.UserName);
                tmp_str.Add(item.PhoneNumber);
                tmp_str.Add(item.Data.ToString());
                tmp_str.Add(item.Time);
                List<BlankModel> tmp_blank = listBlankExport[item.Id];
                System.Diagnostics.Debug.WriteLine("Count blanks --- > " + tmp_blank.Count());
                
                for (int j=0; j< tmp_blank.Count -1; j++ )
                {
                    
                    QuestionModel tmp = listQuestionExport[tmp_blank[j].QuestionID];
                    switch (tmp.TypeQuestion)
                    {
                        case Models.Question.Type.Single:
                            tmp_str.Add(tmp_blank[j].AnswerIndex.ToString());
                            if (listAnswerAllExport[tmp_blank[j].QuestionID].Where(u => u.isFreeArea == true).Count() > 0)
                            {
                                if (tmp_blank[j].Text != null)
                                {
                                    tmp_str.Add(tmp_blank[j].Text);
                                }
                                else
                                {
                                    tmp_str.Add(" ");
                                }
                            }
                            break;
                        case Models.Question.Type.Multiple:
                            int count_all_answer = listAnswerAllExport[tmp.Id].Count()+j-1;
                            int count_all_result = tmp_blank.Where(u=>u.QuestionID == tmp.Id).Count() + j - 1;
                            int count_other_column = listAnswerAllExport[tmp.Id].Where(u => u.isFreeArea == true).Count();
                            List<string> other_column = new List<string>(); 
                            for (int i = j; i<=count_all_answer; i++)
                            {

                                if (i <= count_all_result)
                                {
                                    tmp_str.Add(tmp_blank[i].AnswerIndex.ToString());
                                    if (tmp_blank[i].Text != null)
                                    {
                                        other_column.Add(tmp_blank[i].Text);
                                    }
                                }
                                else
                                    tmp_str.Add(" ");

                            }

                            j = count_all_result;
                        
                            if (other_column.Count > 0)
                            {
                                tmp_str.AddRange(other_column);
                                j += count_other_column - 1;
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
            //grid.DataSource = from code in listBlankExport 
            //                  select new
            //                  {
            //                    q1 = code.AnswerIndex

            //                  };
            grid.DataBind();

            Response.ClearContent();
            Response.AddHeader("content-disposition", "attachment; filename=Exported_Diners.xls");
            Response.ContentType = "application/excel";
            Response.ContentEncoding = System.Text.Encoding.GetEncoding("UTF-8");
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
        public ActionResult TableBlanksFilter(int id_project,int id_operator,string startTime, string endTime, int? page)
        {
            ViewBag.Id_Project_Next = id_project;
            ViewBag.Id_Operator_Next = id_operator;
            ViewBag.startTimeNext = startTime;
            ViewBag.endTimeNext = endTime;
            DateTime startTimeDT = Convert.ToDateTime(startTime.Replace("_"," "));
            DateTime endTimeDT = Convert.ToDateTime(endTime.Replace("_"," "));
            System.Diagnostics.Debug.WriteLine(startTimeDT);
            System.Diagnostics.Debug.WriteLine(endTimeDT);

            using (ProjectContext project_db = new ProjectContext())
            {
                ViewBag.ProjectName = project_db.SetProjectModels.First(u => u.Id == id_project).NameProject;
            }
            if (id_operator > 0)
            {
                
                tmp_tableBlanksFilter = db.SetResultModels.Where(u => u.ProjectID == id_project && u.UserID == id_operator && u.Data.CompareTo(startTimeDT) == 1 && u.Data.CompareTo(endTimeDT) == -1).ToList();
            }
            else
            {
                tmp_tableBlanksFilter = db.SetResultModels.Where(u => u.ProjectID == id_project && u.Data.CompareTo(startTimeDT) == 1 && u.Data.CompareTo(endTimeDT) == -1).ToList();
            }
            int pageSize = 15;
            int pageNumber = (page ?? 1);
            return PartialView(tmp_tableBlanksFilter.ToPagedList(pageNumber, pageSize));
            //return PartialView(tmp);
        }

        //
        //Отрисовка блока с результатами (ПО ПАНЕЛИ)
        //
        [HttpGet]
        public ActionResult _TableBlanksFilter(string id_project, string id_operator, string startTime, string endTime, int? page)
        {
            ViewBag.Id_Project_Next = id_project;
            int id_pr = Convert.ToInt32(id_project);
            int id_op = Convert.ToInt32(id_operator);
            DateTime startTimeDT = Convert.ToDateTime(startTime.Replace("_", " "));
            DateTime endTimeDT = Convert.ToDateTime(endTime.Replace("_", " "));
            System.Diagnostics.Debug.WriteLine(startTimeDT);
            System.Diagnostics.Debug.WriteLine(endTimeDT);
            using (ProjectContext project_db = new ProjectContext())
            {
                ViewBag.ProjectName = project_db.SetProjectModels.First(u => u.Id == id_pr).NameProject;
            }
            if (id_op > 0)
            {
                tmp_tableBlanksFilter = db.SetResultModels.Where(u => u.ProjectID == id_pr && u.UserID == id_op && u.Data.CompareTo(startTimeDT) == 1 && u.Data.CompareTo(endTimeDT) == -1).ToList();
            }
            else
            {
                tmp_tableBlanksFilter = db.SetResultModels.Where(u => u.ProjectID == id_pr && u.Data.CompareTo(startTimeDT) == 1 && u.Data.CompareTo(endTimeDT) == -1).ToList();
            }
            int pageSize = 15;
            int pageNumber = (page ?? 1);
            System.Diagnostics.Debug.WriteLine(tmp_tableBlanksFilter.Count());
            return PartialView("_TableBlanksFilter", tmp_tableBlanksFilter.ToPagedList(pageNumber, pageSize));

        }
    }
}