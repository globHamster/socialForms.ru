using SocialFORM.Models;
using SocialFORM.Models.Authentication;
using SocialFORM.Models.Session;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Security;

namespace SocialFORM.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model)
        {
            DateTime someDateTime = DateTime.Now;
            if (ModelState.IsValid)
            {
                // поиск пользователя в бд
                User user = null;
                using (ApplicationContext db = new ApplicationContext())
                {
                    string password = CodePass(model.Password);
                    user = db.SetUser.FirstOrDefault(u => u.Login == model.Login && u.Password == password);

                }
                if (user != null)
                {
                    using (ApplicationContext db = new ApplicationContext())
                    {
                        SessionModel date = db.SetSession.FirstOrDefault(u => u.UserId == user.Id && u.Date == someDateTime.Date);
                        if (date == null)
                        {
                            db.SetSession.Add(new Models.Session.SessionModel { UserId = user.Id, Date = someDateTime.Date, TimeUp = someDateTime.ToLongTimeString(), SetTimeUp = someDateTime.ToLongTimeString(), TimeOut = someDateTime.ToLongTimeString(), AllTime = "00:00:00", StatusTime = 1 });
                            db.SaveChanges();
                        }
                        if (date != null)
                        {
                            SessionModel UPSetTimeUp = db.SetSession.Where(u => u.UserId == user.Id && u.Date == someDateTime.Date).First();
                            UPSetTimeUp.SetTimeUp = someDateTime.ToLongTimeString();
                            UPSetTimeUp.TimeOut = someDateTime.ToLongTimeString();
                            UPSetTimeUp.StatusTime = 1;
                            db.Entry(UPSetTimeUp).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                    FormsAuthentication.SetAuthCookie(model.Login, true);
                    // Создать объект cookie-набора
                    HttpCookie cookie = new HttpCookie("cookieAuth");

                    // Установить значения в нем
                    cookie["Login"] = CryporEngine.Encrypt(model.Login, true);
                    cookie.Expires = DateTime.Now.AddYears(1);

                    // Добавить куки в ответ
                    Response.Cookies.Add(cookie);
                    return RedirectToAction("_Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Пользователя с таким логином и паролем нет");
                }
            }
            
            return View(model);
        }

        ApplicationContext db = new ApplicationContext();
        [HttpGet]
        public ActionResult Register()
        {

            SelectList roles = new SelectList(db.SetRoles, "Id", "Name");
            ViewBag.Role = roles;
            return PartialView();
        }

        [HttpPost]
        public void RegisterUser(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                User user = null;
                using (ApplicationContext db = new ApplicationContext())
                {
                    user = db.SetUser.FirstOrDefault(u => u.Login == model.Login);
                }
                if (user == null)
                {
                    // создаем нового пользователя
                    using (ApplicationContext db = new ApplicationContext())
                    {
                        string password = CodePass(model.Password);
                        db.SetUser.Add(new User { Login = model.Login, Password = password, RoleId = model.RoleId, SchoolDay = model.SchoolDay });
                        db.SaveChanges();
                        db.SetDataUsers.Add(new DataUser { Name = model.Name, Family = model.Family, Age = model.Age, Fool = model.Fool, Email = model.Email, UserId = db.SetUser.First(u => u.Login == model.Login).Id });
                        db.SaveChanges();

                        user = db.SetUser.Where(u => u.Login == model.Login && u.Password == model.Password).FirstOrDefault();
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Пользователь с таким логином уже существует");
                }
            }
        }
        [HttpPost]
        public void UpdateUser(UserViewModel model)
        {

            //if (ModelState.IsValid)
            //{
            //User user = null;
            //using (ApplicationContext db = new ApplicationContext())
            //{
            //    user = db.SetUser.FirstOrDefault(u => u.Login == model.LoginView);
            //}
            //if (user != null)
            //{
            // обновляем пользователя
            using (ApplicationContext db = new ApplicationContext())
            {
                User UPuser = db.SetUser.Where(c => c.Id == model.IdView).First();
                DataUser UPdataUser = db.SetDataUsers.Where(c => c.UserId == model.IdView).First();

                UPuser.Login = model.LoginView;

                UPuser.Password = CodePass(model.PasswordView);
                UPuser.RoleId = model.RoleIdView;
                UPuser.SchoolDay = model.SchoolDayView;

                UPdataUser.Name = model.NameView;
                UPdataUser.Family = model.FamilyView;
                UPdataUser.Age = model.AgeView;
                UPdataUser.Fool = model.FoolView;

                db.Entry(UPuser).State = EntityState.Modified;
                db.Entry(UPdataUser).State = EntityState.Modified;
                db.SaveChanges();
            }

            //    user = db.SetUser.Where(u => u.Login == model.LoginView && u.Password == model.PasswordView).FirstOrDefault();
            //}
            //}
        }

        public ActionResult Logoff()
        {
            DateTime someDateTime = DateTime.Now;
            HttpCookie cookieReq = Request.Cookies["cookieAuth"];
            string cookieString = null;
            if (cookieReq != null)
            {
                cookieString = CryporEngine.Decrypt(cookieReq["Login"], true);
                System.Diagnostics.Debug.WriteLine(cookieString);

                HttpCookie cookie = new HttpCookie("cookieAuth");
                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookie);
                if (ModelState.IsValid)
                {
                    // поиск пользователя в бд
                    User user = null;
                    using (ApplicationContext db = new ApplicationContext())
                    {
                        user = db.SetUser.Where(u => u.Login == cookieString).First();

                    }
                    if (user != null)
                    {
                        using (ApplicationContext db = new ApplicationContext())
                        {
                            SessionModel date = db.SetSession.FirstOrDefault(u => u.UserId == user.Id && u.Date == someDateTime.Date);
                            if (date != null)
                            {
                                if (date.TimeUp == date.SetTimeUp)
                                {
                                    SessionModel UPSetTimeUp = db.SetSession.Where(u => u.UserId == user.Id && u.Date == someDateTime.Date).First();
                                    UPSetTimeUp.TimeOut = someDateTime.ToLongTimeString();
                                    UPSetTimeUp.AllTime = Convert.ToDateTime((DateTime.Parse(UPSetTimeUp.AllTime) + (DateTime.Parse(DateTime.Now.ToLongTimeString()) - DateTime.Parse(UPSetTimeUp.TimeUp))).ToString()).ToLongTimeString();
                                    UPSetTimeUp.StatusTime = 0;
                                    db.Entry(UPSetTimeUp).State = EntityState.Modified;
                                }
                                else
                                {
                                    SessionModel UPSetTimeUp = db.SetSession.Where(u => u.UserId == user.Id && u.Date == someDateTime.Date).First();
                                    UPSetTimeUp.TimeOut = someDateTime.ToShortTimeString();
                                    UPSetTimeUp.AllTime = Convert.ToDateTime((DateTime.Parse(UPSetTimeUp.AllTime) + (DateTime.Parse(DateTime.Now.ToLongTimeString()) - DateTime.Parse(UPSetTimeUp.SetTimeUp))).ToString()).ToLongTimeString();
                                    UPSetTimeUp.StatusTime = 0;
                                    db.Entry(UPSetTimeUp).State = EntityState.Modified;
                                }
                                db.SaveChanges();
                            }
                        }
                    }
                }
                FormsAuthentication.SignOut();
            }
            return RedirectToAction("_Index", "Home");
        }

        [HttpPost]
        public void Logofff()
        {
            DateTime someDateTime = DateTime.Now;
            HttpCookie cookieReq = Request.Cookies["cookieAuth"];
            string cookieString = null;
            if (cookieReq != null)
            {
                cookieString = CryporEngine.Decrypt(cookieReq["Login"], true);
            }

            System.Diagnostics.Debug.WriteLine(cookieString);

            HttpCookie cookie = new HttpCookie("cookieAuth");
            cookie.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Add(cookie);
            
            FormsAuthentication.SignOut();
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
    }

    public class CryporEngine
    {
        public static string Encrypt(string ToEncrypt, bool useHasing)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(ToEncrypt);
            string Key = "socialForm";
            if (useHasing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(Key));
                hashmd5.Clear();
            }
            else
            {
                keyArray = UTF8Encoding.UTF8.GetBytes(Key);
            }
            TripleDESCryptoServiceProvider tDes = new TripleDESCryptoServiceProvider();
            tDes.Key = keyArray;
            tDes.Mode = CipherMode.ECB;
            tDes.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tDes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tDes.Clear();
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public static string Decrypt(string cypherString, bool useHasing)
        {
            byte[] keyArray;
            byte[] toDecryptArray = Convert.FromBase64String(cypherString);
            string key = "socialForm";
            if (useHasing)
            {
                MD5CryptoServiceProvider hashmd = new MD5CryptoServiceProvider();
                keyArray = hashmd.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                hashmd.Clear();
            }
            else
            {
                keyArray = UTF8Encoding.UTF8.GetBytes(key);
            }
            TripleDESCryptoServiceProvider tDes = new TripleDESCryptoServiceProvider();
            tDes.Key = keyArray;
            tDes.Mode = CipherMode.ECB;
            tDes.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tDes.CreateDecryptor();
            try
            {
                byte[] resultArray = cTransform.TransformFinalBlock(toDecryptArray, 0, toDecryptArray.Length);

                tDes.Clear();
                return UTF8Encoding.UTF8.GetString(resultArray, 0, resultArray.Length);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }


}