using SocialFORM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SocialFORM.Controllers
{
    public class UserController : Controller
    {
        ApplicationContext db = new ApplicationContext();

        public ActionResult ViewUser(int Id)
        {
            IQueryable<UserViewModel> result = (
            from User in db.SetUser
            join DataUsers in db.SetDataUsers on User.Id equals DataUsers.UserId
            join Role in db.SetRoles on User.RoleId equals Role.Id
            where User.Id == Id
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
            return PartialView(result.ToList());
        }

        public ActionResult EditUser(int Id)
        {
            IQueryable<UserViewModel> result = (
            from User in db.SetUser
            join DataUsers in db.SetDataUsers on User.Id equals DataUsers.UserId
            join Role in db.SetRoles on User.RoleId equals Role.Id
            where User.Id == Id
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

            SelectList roles = new SelectList(db.SetRoles, "Id", "Name", db.SetUser.First(u => u.Id == Id).RoleId);
            ViewBag.Role = roles;
            return PartialView(result.First());
        }
    }
}