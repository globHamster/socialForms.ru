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
            List<MenuItem> menuItems = db.SetMenuItems.ToList();
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

        List<ResultModel> tmp_tableBlanks = null;
        public ActionResult TableBlanks(int? page)
        {
            System.Diagnostics.Debug.WriteLine("------>" + 22 + "   " + page);
            using (ProjectContext project_db = new ProjectContext())
            {
                ViewBag.ProjectName = project_db.SetProjectModels.First(u => u.Id == 22).NameProject;
            }
            tmp_tableBlanks = db.SetResultModels.Where(u => u.ProjectID == 22).ToList();
            int pageSize = 3;
            int pageNumber = (page ?? 1);
            return PartialView(tmp_tableBlanks.ToPagedList(pageNumber, pageSize));
            //return PartialView(tmp);
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

        [HttpPost]
        public void actionProject(int id)
        {
            ProjectModel projectModel = db2.SetProjectModels.Where(u => u.Id == id).FirstOrDefault();
            if (projectModel.ActionProject == true) { projectModel.ActionProject = false; }
            else { projectModel.ActionProject = true; }
            db2.SaveChanges();
        }
    }
}