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
    public class DataBaseController : Controller
    {
        ApplicationContext db = new ApplicationContext();
        ProjectContext db2 = new ProjectContext();

        // GET: DataBase
        public ActionResult Index()
        {
            return View();
        }

        public class diapList
        {
            public int sd { get; set; }
            public int ed { get; set; }
        }

        [HttpGet]
        public JsonResult GetListDIAP(string KodFO, string KodOB, string KodGOR)
        {
            NumberAppContext context = new NumberAppContext();
            List<diapList> diapLists = new List<diapList>();
            List<Diap> diaps = new List<Diap>();
            diaps = context.SetDiap.Where(u => u.KodFO == KodFO && u.KodGOR == KodGOR && u.KodOB == KodOB).ToList();
            return Json(diaps, JsonRequestBehavior.AllowGet);
        }
    }
}