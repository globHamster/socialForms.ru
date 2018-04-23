using SocialFORM.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;

namespace SocialFORM.Controllers.Delete
{
    public class DeleteController : Controller
    {
        ApplicationContext db = new ApplicationContext();

        // GET: Delete
        [HttpPost]
        public void Delete(int Id)
        {
            DataUser resultDataUser = db.SetDataUsers.FirstOrDefault(u => u.UserId == Id);
            db.SetDataUsers.Remove(resultDataUser);
            db.SaveChanges();

            User resultUser = db.SetUser.FirstOrDefault(u => u.Id == Id);
            db.SetUser.Remove(resultUser);
            db.SaveChanges();
        }
    }
}