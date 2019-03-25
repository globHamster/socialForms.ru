using MySql.Data.MySqlClient;
using SocialFORM.Models;
using SocialFORM.Models.Form;
using SocialFORM.Models.Number;
using SocialFORM.Models.Question;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SocialFORM.Controllers
{
    public class PhoneController : Controller
    {
        NumberAppContext db = new NumberAppContext();
        PhoneContext mysql_db = new PhoneContext();
        ApplicationContext form_db = new ApplicationContext();
        QuestionContext q_db = new QuestionContext();
        // GET: Phone
        public ActionResult Index()
        {
            return View();
        }
        //Представление вторичной базы
        public ActionResult DataBaseView()
        {
            return PartialView();
        }
        //Представление первичной базы
        public ActionResult PrimaryTableView()
        {
            return PartialView();
        }
        //Представление синхронизации информации с номером
        public ActionResult SyncWithBlanksView()
        {
            return PartialView();
        }
        //Представление генератора номеров
        public ActionResult GeneratorView()
        {
            return PartialView();
        }

        //Функция загрузки номеров с информацией во вторичную базу (Временно отключено из view)
        [HttpPost]
        public void TestImport(string table)
        {
            try
            {
                var fileContent = Request.Files[0];
                if (fileContent != null && fileContent.ContentLength > 0)
                {
                    string name_file = fileContent.FileName;

                    Stream stream = fileContent.InputStream;
                    DataTable csvTable = new DataTable();
                    StreamReader streamReader = new StreamReader(stream, Encoding.GetEncoding(1251));
                    String str;
                    List<FormNumber> lst_phone = new List<FormNumber>();
                    do
                    {
                        str = streamReader.ReadLine();
                        if (str != null)
                        {
                            List<string> tmp_str = str.Split(';').ToList();
                            FormNumber tmp = new FormNumber();
                            if (tmp_str[0] == null || tmp_str[0] == "")
                            {
                                break;
                            }
                            tmp.FO = tmp_str[0];
                            tmp.OB = tmp_str[1];
                            tmp.GOR = tmp_str[2];
                            tmp.NP = tmp_str[3];
                            tmp.VGOR = tmp_str[4];
                            if (tmp_str[6] != "")
                            {
                                tmp.Phone = tmp_str[6];
                                if (tmp.Phone.ElementAt(0) != '7')
                                {
                                    tmp.Phone = tmp.Phone.Remove(0, 1).Insert(0, "7");
                                }
                                if (db.SetFormNumbers.FirstOrDefault(u => u.Phone == tmp.Phone) != null)
                                {
                                    continue;
                                }
                            }
                            else continue;
                            short tmp_file_sex = 0;
                            short.TryParse(tmp_str[7], out tmp_file_sex);
                            tmp.Sex = tmp_file_sex == 0 ? false : true;
                            try
                            {
                                tmp.Age = Int32.Parse(tmp_str[8]);
                            }
                            catch (FormatException e)
                            {
                                tmp.Age = 0;
                            }
                            tmp.Address = tmp_str[5];
                            tmp.Education = "";
                            tmp.Type = tmp.Phone.ElementAt(1) == '9' ? 1 : 0;
                            tmp.TypeNP = false;
                            lst_phone.Add(tmp);
                        }
                        else
                        {
                            break;
                        }

                    } while (true);
                    int iter = 0;
                    int count_lst = lst_phone.Count();
                    while (iter < count_lst)
                    {
                        if (lst_phone.Where(u => u.Phone == lst_phone[iter].Phone).Count() > 1)
                        {
                            List<FormNumber> tmp_lst = lst_phone.Where(u => u.Phone == lst_phone[iter].Phone).ToList();
                            tmp_lst.Remove(tmp_lst.First());
                            foreach (var rem_item in tmp_lst)
                            {
                                lst_phone.Remove(rem_item);
                            }
                            count_lst = lst_phone.Count();
                        }
                        iter++;
                    }
                    db.SetFormNumbers.AddRange(lst_phone);
                    db.SaveChanges();

                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.StackTrace.ToString());
            }
        }

        [HttpGet]
        public JsonResult getListFormNumber(string KodFO, string KodOB, string KodGOR)
        {
            try
            {
                List<FormNumber> tmp_list = db.SetFormNumbers.Where(u => u.FO == KodFO).ToList();
                if (KodOB != "")
                {
                    tmp_list = tmp_list.Where(u => u.OB == KodOB).ToList();
                    if (KodGOR != "")
                    {
                        tmp_list = tmp_list.Where(u => u.GOR == KodGOR).ToList();
                    }
                }
                foreach (var item in tmp_list)
                {
                    if (item.Age > 1000)
                    {
                        item.Age = DateTime.Now.Year - item.Age;
                    }
                }
                var jsonResult = Json(tmp_list, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception >>> " + e.StackTrace);
                System.Diagnostics.Debug.WriteLine("Exception >>> " + e.Message);
                System.Diagnostics.Debug.WriteLine("Exception >>> " + e.Source);
                System.Diagnostics.Debug.WriteLine("Exception >>> " + e.Data);
                return Json(null, JsonRequestBehavior.AllowGet);
            }

        }

        //Выгрузка листа с кодом FO
        [HttpGet]
        public JsonResult getListFO()
        {
            List<FO> lst_FO = new List<FO>();
            lst_FO = db.SetFO.ToList();
            if (lst_FO != null)
            {
                return Json(lst_FO, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        //Выгрузка листа с кодом OB
        [HttpGet]
        public JsonResult getListOB(string code)
        {
            List<OB> lst_OB = db.SetOB.Where(u => u.KodFO == code).ToList();
            if (lst_OB != null)
            {
                return Json(lst_OB, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        //Выгрузка листа с кодом GOR
        [HttpGet]
        public JsonResult getListGOR(string codeFO, string codeOB)
        {
            List<GOR> lst_GOR = db.SetGOR.Where(u => u.KodFO == codeFO && u.KodOB == codeOB).ToList();
            if (lst_GOR != null)
            {
                return Json(lst_GOR, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        //Выгрузка листа номеров в формате EXCEL из вторичной базы
        [HttpGet]
        public void ImportFile(string FO, string OB, string GOR)
        {
            List<FO> lst_FO = db.SetFO.ToList();
            List<OB> lst_OB = db.SetOB.ToList();
            List<GOR> lst_GOR = db.SetGOR.ToList();

            var products = new System.Data.DataTable();

            products.Columns.Add("Федеральный округ");
            products.Columns.Add("ntcn");
            products.Columns.Add("text");
            products.Columns.Add("Населеный пункт");
            products.Columns.Add("Внутригородской район");
            products.Columns.Add("Телефон");
            products.Columns.Add("Пол");
            products.Columns.Add("Возраст");
            products.Columns.Add("Адрес");
            products.Columns.Add("Образование");

            List<FormNumber> tmp_list = db.SetFormNumbers.Where(u => u.FO == FO).ToList();
            if (OB != "")
            {
                tmp_list = tmp_list.Where(u => u.OB == OB).ToList();
                if (GOR != "")
                {
                    tmp_list = tmp_list.Where(u => u.GOR == GOR).ToList();
                }
            }

            foreach (var item in tmp_list)
            {
                List<string> row_lst = new List<string>();
                row_lst.Add(lst_FO.Find(u => u.KodFO == item.FO).NameFO.ToString());
                row_lst.Add(lst_OB.Find(u => u.KodFO == item.FO && u.KodOB == item.OB).NameOB.ToString());
                row_lst.Add(lst_GOR.Find(u => u.KodFO == item.FO && u.KodOB == item.OB && u.KodGOR == item.GOR).NameGOR.ToString());
                row_lst.Add(item.NP);
                row_lst.Add(item.VGOR);
                row_lst.Add(item.Phone);
                row_lst.Add((item.Sex == false ? "М" : "Ж"));
                row_lst.Add(item.Age.ToString());
                row_lst.Add(item.Address);
                row_lst.Add(item.Education);
                products.Rows.Add(row_lst.ToArray());
                row_lst.Clear();
            }


            var grid = new GridView();

            grid.DataSource = products;
            grid.DataBind();

            Response.ClearContent();
            Response.Clear();
            Response.ContentEncoding = Encoding.GetEncoding(1251);
            Response.ContentType = "application/ms-excel";
            Response.AddHeader("Content-Disposition", "attachment; filename=phones.xls");
            Response.Charset = "";
            StringWriter sw = new StringWriter();
            HtmlTextWriter htmlTextWriter = new HtmlTextWriter(sw);
            grid.RenderControl(htmlTextWriter);
            //Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.Write(sw.ToString());
            Response.Flush();
            Response.End();

        }
        //Выгрузка листа номеров в фомате CSV из вторичной базы
        [HttpGet]
        public void ImportFileCSV(string FO, string OB, string GOR)
        {
            List<FO> lst_FO = db.SetFO.ToList();
            List<OB> lst_OB = db.SetOB.ToList();
            List<GOR> lst_GOR = db.SetGOR.ToList();
            string csv = "";
            string name_file = lst_FO.FirstOrDefault(u => u.KodFO == FO).NameFO;

            List<FormNumber> tmp_list = db.SetFormNumbers.Where(u => u.FO == FO).ToList();
            if (OB != "")
            {
                name_file = lst_OB.FirstOrDefault(u => u.KodFO == FO && u.KodOB == OB).NameOB;
                tmp_list = tmp_list.Where(u => u.OB == OB).ToList();
                if (GOR != "")
                {
                    name_file = lst_GOR.FirstOrDefault(u => u.KodFO == FO && u.KodOB == OB && u.KodGOR == GOR).NameGOR;
                    tmp_list = tmp_list.Where(u => u.GOR == GOR).ToList();
                }
            }
            int count = 1;
            var sw = new StringWriter();

            foreach (var item in tmp_list)
            {
                sw.WriteLine(String.Format("{0};{1};{2}", count, item.Phone, "0"));
                count++;
            }

            Response.Clear();
            Response.AddHeader("Content-Disposition", "attachment; filename=" + name_file + ".csv");
            Response.ContentType = "text/csv";
            Response.Write(sw);
            Response.Flush();
            Response.End();

        }

        [HttpGet]
        public JsonResult GetFormNumber(string numb)
        {
            FormNumber tmp = db.SetFormNumbers.FirstOrDefault(u => u.Phone == numb);
            if (tmp != null)
            {
                return Json(tmp, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public int SetFormNumber(FormNumber _fn)
        {
            try
            {
                db.Entry(_fn).State = EntityState.Modified;
                db.SaveChanges();
                return 200;
            }
            catch (Exception e)
            {
                Response.AppendToLog(e.StackTrace);
                return 0;
            }
        }

        [HttpGet]
        public JsonResult GetInfoNumber()
        {
            string str = "{";
            List<FO> lst_FO = db.SetFO.ToList();
            List<OB> lst_OB = db.SetOB.ToList();
            List<GOR> lst_GOR = db.SetGOR.ToList();
            int count_FO = 0;
            //по списку FO
            foreach (var item_FO in lst_FO)
            {
                int length_FO = db.SetFormNumbers.Where(u => u.FO == item_FO.KodFO).Count();
                string str_FO = item_FO.KodFO;
                string name_FO = item_FO.NameFO;
                if (length_FO == 0) continue;
                str += "'" + str_FO + "': {";
                int count_OB = 0;
                //по списку OB
                foreach (var item_OB in lst_OB.Where(u => u.KodFO == str_FO).ToList())
                {
                    int length_OB = db.SetFormNumbers.Where(u => u.FO == str_FO && u.OB == item_OB.KodOB).Count();
                    string str_OB = item_OB.KodOB;
                    string name_OB = item_OB.NameOB;
                    if (length_OB == 0) continue;
                    str += "'" + str_OB + "': {";
                    int count_GOR = 0;
                    //по списку GOR
                    foreach (var item_GOR in lst_GOR.Where(u => u.KodFO == str_FO && u.KodOB == str_OB).ToList())
                    {
                        int length_GOR = db.SetFormNumbers.Where(u => u.FO == str_FO && u.OB == str_OB && u.GOR == item_GOR.KodGOR).Count();
                        string str_GOR = item_GOR.KodGOR;
                        string name_GOR = item_GOR.NameGOR;
                        if (length_GOR == 0) continue;
                        str += "'" + str_GOR + "': { 'name': '" + name_GOR + "','count' :" + length_GOR + ", 'length': 0},";
                        count_GOR++;
                    }
                    str += " 'name': '" + name_OB + "', 'count' :" + length_OB + ", 'length': " + count_GOR + "},";
                    count_OB++;
                }
                str += " 'name': '" + name_FO + "', 'count' : " + length_FO + ", 'length': " + count_OB + "},";
                count_FO++;
            }
            //str = str.Remove(str.Length - 1, 1);
            str += "'length':" + count_FO + "}";
            JavaScriptSerializer j = new JavaScriptSerializer();
            object obj = j.Deserialize(str, typeof(object));

            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void PushNumbersStatus(string KodFO, string KodOB, string KodGOR, List<string> mas_numb)
        {
            List<PT> lst_numb_with_status = new List<PT>();
            foreach (var item in mas_numb)
            {
                long start_diap = Int64.Parse(item.Split('-')[0]);
                long end_diap = Int64.Parse(item.Split('-')[1]);
                for (long numb = start_diap; numb <= end_diap; numb++)
                {
                    PT tmp = new PT();
                    tmp.FO = KodFO;
                    tmp.OB = KodOB;
                    tmp.GOR = KodGOR;
                    tmp.Phone = "7" + numb.ToString();
                    if (db.SetPTs.FirstOrDefault(u => u.Phone == tmp.Phone) != null) continue;
                    tmp.Status = "0";
                    tmp.Type = Int64.Parse(tmp.Phone) < 79000000000 ? 0 : 1;
                    tmp.isActual = false;
                    tmp.TimeCall = new DateTime(2000, 1, 1);
                    lst_numb_with_status.Add(tmp);
                }
            }
            try
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                db.SetPTs.AddRange(lst_numb_with_status);
                db.SaveChanges();
                db.Configuration.AutoDetectChangesEnabled = true;
            }
            catch (Exception e)
            {
                Response.AppendToLog(e.StackTrace);
            }
        }

        class TmpClass
        {
            public int Id { get; set; }
            public string Number { get; set; }
            public string Status { get; set; }
        }
        //Получение номеров для первичной базы в соответствии с настройками
        [HttpGet]
        public JsonResult GetNumberStatus(string FO, string OB, string GOR, string settings, short type_select, string mass_time, byte? iterval, bool? invers)
        {
            List<bool> lst_setting = new List<bool>();
            foreach (var item in settings.Split(','))
            {
                lst_setting.Add(item == "false" ? false : true);
            }
            List<PT> lst_PT;

            if (GOR != "")
            {
                lst_PT = db.SetPTs.Where(u => u.FO == FO && u.OB == OB && u.GOR == GOR).ToList();
            } else if ( OB != "")
            {
                lst_PT = db.SetPTs.Where(u => u.FO == FO && u.OB == OB).ToList();
            } else if (FO != "")
            {
                lst_PT = db.SetPTs.Where(u => u.FO == FO).ToList();
            } else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }

            if (lst_setting[4])
            {
                lst_PT = lst_PT.Where(u => u.Type == 0).ToList();
            }

            if (lst_setting[5])
            {
                lst_PT = lst_PT.Where(u => u.Type == 1).ToList();
            }

            if (lst_setting != null)
            {
                List<PT> tmp_lst_PT = new List<PT>();

                if (!lst_setting[0])
                {
                    lst_PT = lst_PT.Except(lst_PT.Where(u => u.Status == "0")).ToList();
                }

                if (!lst_setting[1])
                {
                    lst_PT = lst_PT.Except(lst_PT.Where(u => u.Status == "занято")).ToList();
                }

                if (!lst_setting[2])
                {
                    lst_PT = lst_PT.Except(lst_PT.Where(u => u.Status == "нет ответа")).ToList();
                }

                if (!lst_setting[3])
                {
                    lst_PT = lst_PT.Except(lst_PT.Where(u => u.Status == "1" || u.Status == "завершено")).ToList();
                }

                if (!lst_setting[6])
                {
                    lst_PT = lst_PT.Except(lst_PT.Where(u => u.Status == "линия не найдена")).ToList();
                }

                if (!lst_setting[7])
                {
                    lst_PT = lst_PT.Except(lst_PT.Where(u => u.Status == "перезвонить")).ToList();
                }

                if (!lst_setting[8])
                {
                    lst_PT = lst_PT.Except(lst_PT.Where(u => u.Status == "connect")).ToList();
                }

                switch (type_select)
                {
                    case 0:
                    case 1:
                        lst_PT = lst_PT.Where(u => u.isActual == (type_select == 0 ? false : true)).ToList();
                        break;
                    default:
                        break;
                }

                List<string> time_arg_tmp = mass_time?.Split('|').ToList() ?? null;
                if (time_arg_tmp != null)
                {
                    if (iterval > 0)
                    {
                        DateTime time_tmp_ = DateTime.Parse(time_arg_tmp[0]);
                        if (invers == true)
                        {
                            lst_PT = lst_PT.Where(u => u.TimeCall < time_tmp_).ToList();
                        }
                        else
                        {
                            lst_PT = lst_PT.Where(u => u.TimeCall >= time_tmp_).ToList();
                        }
                    }
                    else
                    {
                        List<PT> lst_tmp_list_time = new List<PT>();
                        time_arg_tmp.ForEach(u =>
                        {
                            DateTime time_tmp_ = DateTime.Parse(u);
                            lst_tmp_list_time.AddRange(lst_PT.Where(t => t.TimeCall == time_tmp_));
                        });
                        lst_PT = lst_tmp_list_time;
                    }
                }
            }
            var jsonResult = Json(lst_PT, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        //Загрузка номеров в сценарий через первичную базу
        [HttpPost]
        public void ImportNumberToOktell(string FO, string OB, string GOR, int id_table, string settings, short type_load, List<string> time, short type_select)
        {
            List<bool> lst_setting = new List<bool>();
            foreach (var item in settings.Split(','))
            {
                lst_setting.Add(item == "false" ? false : true);
            }
            List<PT> tmp_lst_PT = new List<PT>();
            string name_table = "";
            if (FO != "")
            {
                tmp_lst_PT = db.SetPTs.Where(u => u.FO == FO).ToList();
                name_table = db.SetFO.First(u => u.KodFO == FO).NameFO;
                if (OB != "")
                {
                    tmp_lst_PT = tmp_lst_PT.Where(u => u.OB == OB).ToList();
                    name_table = db.SetOB.First(u => u.KodFO == FO && u.KodOB == OB).NameOB;
                    if (GOR != "")
                    {
                        tmp_lst_PT = tmp_lst_PT.Where(u => u.GOR == GOR).ToList();
                        name_table = db.SetGOR.First(u => u.KodFO == FO && u.KodOB == OB && u.KodGOR == GOR).NameGOR;
                    }
                }
            }
            string name_type = "(станц + моб)";
            if (lst_setting[4])
            {
                tmp_lst_PT = tmp_lst_PT.Where(u => u.Type == 0).ToList();
                name_type = "(станц)";
            }

            if (lst_setting[5])
            {
                tmp_lst_PT = tmp_lst_PT.Where(u => u.Type == 1).ToList();
                name_type = "(моб)";
            }
            try
            {
                if (lst_setting != null)
                {
                    List<PT> tmp_lst_PT_ = new List<PT>();

                    if (lst_setting[0])
                    {
                        tmp_lst_PT_.AddRange(tmp_lst_PT.Where(u => u.Status == "0").ToList());
                    }

                    if (lst_setting[1])
                    {
                        tmp_lst_PT_.AddRange(tmp_lst_PT.Where(u => u.Status == "занято").ToList());
                    }

                    if (lst_setting[2])
                    {
                        tmp_lst_PT_.AddRange(tmp_lst_PT.Where(u => u.Status == "нет ответа").ToList());
                    }

                    if (lst_setting[3])
                    {
                        tmp_lst_PT_.AddRange(tmp_lst_PT.Where(u => u.Status == "1" || u.Status == "завершено").ToList());
                    }

                    if (lst_setting[6])
                    {
                        tmp_lst_PT_.AddRange(tmp_lst_PT.Where(u => u.Status == "линия не найдена").ToList());
                    }

                    if (lst_setting[7])
                    {
                        tmp_lst_PT_.AddRange(tmp_lst_PT.Where(u => u.Status == "перезвонить").ToList());
                    }

                    if (lst_setting[8])
                    {
                        tmp_lst_PT_.AddRange(tmp_lst_PT.Where(u => u.Status == "connect").ToList());
                    }

                    tmp_lst_PT = tmp_lst_PT_;
                    tmp_lst_PT_ = new List<PT>();
                    tmp_lst_PT_.Clear();
                    foreach (var item in time)
                    {
                        DateTime time_tmp = DateTime.Parse(item);
                        tmp_lst_PT_.AddRange(tmp_lst_PT.Where(u => u.TimeCall == time_tmp).ToList());
                    }
                    tmp_lst_PT = tmp_lst_PT_;
                    if (type_select != 2)
                    {
                        tmp_lst_PT = tmp_lst_PT.Where(u => u.isActual == (type_select == 0 ? false : true)).ToList();
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(">>>>> " + e.StackTrace);
                System.Diagnostics.Debug.WriteLine(">>>>> " + e.Message);
                System.Diagnostics.Debug.WriteLine(">>>>> " + e.Source);
                System.Diagnostics.Debug.WriteLine(">>>>> " + e.InnerException);
            }

            if (tmp_lst_PT != null && tmp_lst_PT.Count() >= 1)
            {
                int count = 1;
                if (tmp_lst_PT.Count() > 5000)
                {
                    tmp_lst_PT = tmp_lst_PT.GetRange(0, 5000);
                }
                if (lst_setting[9])
                {
                    Random RDM = new Random();
                    for (int i = 0; i < tmp_lst_PT.Count; i++)
                    {
                        PT tmp_element = tmp_lst_PT[0];
                        tmp_lst_PT.RemoveAt(0);
                        tmp_lst_PT.Insert(RDM.Next(tmp_lst_PT.Count()), tmp_element);
                    }
                }
                try
                {
                    List<string> cmd_str_mysql = new List<string>();
                    List<string> cmd_str_mssql = new List<string>();
                    tmp_lst_PT.ForEach(u =>
                    {
                        TmpClass tmp_arg = new TmpClass();
                        tmp_arg.Number = u.Phone;
                        tmp_arg.Status = u.Status;
                        if (type_load == 1)
                        {
                            //db.Database.ExecuteSqlCommand("UPDATE dbo.PTs Set isActual='1' WHERE Phone='" + u.Phone + "'");
                            cmd_str_mssql.Add("UPDATE dbo.PTs Set isActual='1' WHERE Phone='" + u.Phone + "'");
                        }
                        else
                        {
                            //db.Database.ExecuteSqlCommand("UPDATE dbo.PTs Set isActual='0' WHERE Phone='" + u.Phone + "'");
                            cmd_str_mssql.Add("UPDATE dbo.PTs Set isActual='0' WHERE Phone='" + u.Phone + "'");
                        }
                        //mysql_db.Database.ExecuteSqlCommand("INSERT INTO table" + id_table + " (Id, Number, Status) VALUES ('" + (count++) + "','" + u.Phone + "','0')");
                        cmd_str_mysql.Add("INSERT INTO table" + id_table + " (Id, Number, Status) VALUES ('" + (count++) + "','" + u.Phone + "','0')");
                    });
                    db.Database.ExecuteSqlCommand(String.Join(";", cmd_str_mssql));
                    mysql_db.Database.ExecuteSqlCommand(String.Join(";", cmd_str_mysql));
                    mysql_db.Database.ExecuteSqlCommand("UPDATE name_table SET Name=\"" + name_table + " " + name_type + ".csv\" WHERE Id=" + id_table);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(">>>>> " + e.StackTrace);
                    System.Diagnostics.Debug.WriteLine(">>>>> " + e.Message);
                    System.Diagnostics.Debug.WriteLine(">>>>> " + e.Source);
                    System.Diagnostics.Debug.WriteLine(">>>>> " + e.InnerException);
                }
            }
        }
        //Получение списка свободных таблиц сценриев
        [HttpGet]
        public JsonResult GetFreeNameTable()
        {
            List<string> tmp = mysql_db.Database.SqlQuery<string>("SELECT Name FROM name_table").ToList();
            return Json(tmp, JsonRequestBehavior.AllowGet);
        }
        //Функция очистки сценария 
        [HttpPost]
        public void SynchNumbers(int id_table)
        {
            List<TmpClass> sync_status_lst = new List<TmpClass>();
            sync_status_lst = mysql_db.Database.SqlQuery<TmpClass>("SELECT * FROM table" + id_table).ToList();
            mysql_db.Database.ExecuteSqlCommand("UPDATE name_table SET Name=null WHERE Id=" + id_table);
            mysql_db.Database.ExecuteSqlCommand("TRUNCATE TABLE table" + id_table);
            SyncNumbSThread(sync_status_lst);
            //Thread mThread = new Thread(new ParameterizedThreadStart(SyncNumbSThread));
            //mThread.Start(sync_status_lst);
        }

        //Функция синхронизации номеров с первичной базой
        private void SyncNumbSThread(object _sync_lst)
        {
            List<TmpClass> sync_status_lst = (List<TmpClass>)_sync_lst;
            List<PT> lst_tmp_phone_status = new List<PT>();
            sync_status_lst.ForEach(u =>
            {
                PT tmp = db.SetPTs.FirstOrDefault(t => t.Phone == u.Number);
                if (tmp != null)
                    lst_tmp_phone_status.Add(tmp);
            });
            Dictionary<string, PT> db_PT_list_number = lst_tmp_phone_status.ToDictionary(u => u.Phone, u => u);
            DateTime dateTime = DateTime.Parse(DateTime.Now.ToLongDateString());
            List<string> cmd_string_lst = new List<string>();
            sync_status_lst.ForEach(el =>
            {
                if (db_PT_list_number.ContainsKey(el.Number))
                {
                    PT item_from_PTs = db_PT_list_number[el.Number];
                    if (item_from_PTs.Status == "завершено")
                    {
                        cmd_string_lst.Add("UPDATE dbo.PTs SET TimeCall='" + dateTime + "' WHERE Phone='" + el.Number + "'");
                    }
                    else if (item_from_PTs.Status == "connect" & (el.Status == "0" || el.Status == "занято" || el.Status == "нет ответа" || el.Status == "линия не найдена" || el.Status == "перезвонить"))
                    {
                        cmd_string_lst.Add("UPDATE dbo.PTs SET TimeCall='" + dateTime + "' WHERE Phone='" + el.Number + "'");
                    }
                    else
                    {
                        cmd_string_lst.Add("UPDATE dbo.PTs SET Status='" + el.Status + "', TimeCall='" + dateTime + "' WHERE Phone='" + el.Number + "'");
                    }
                }
            });
            if (cmd_string_lst.Count > 1)
            {
                db.Database.ExecuteSqlCommand(String.Join(";", cmd_string_lst));
            }
        }

        //Загрузка номеров в сценарий через вторичную базу
        [HttpPost]
        public void TestFuncTest(string name_FO, string name_OB, string name_GOR, int type, string name, int id_table, List<string> tmp)
        {
            List<TmpClass> lst_to_load = new List<TmpClass>();
            string type_load = type == 1 ? "all" : (type == 2 ? "connect" : (type == 3 ? "завершено" : "all"));
            List<PT> lst_db_phone_pt = new List<PT>();
            //Проверка соответствующей области
            if (name_GOR != "empty")
            {
                lst_db_phone_pt = db.SetPTs.Where(u => u.FO == name_FO && u.OB == name_OB && u.GOR == name_GOR).ToList();
            }
            else if (name_OB != "empty")
            {
                lst_db_phone_pt = db.SetPTs.Where(u => u.FO == name_FO && u.OB == name_OB).ToList();
            }
            else
            {
                lst_db_phone_pt = db.SetPTs.Where(u => u.FO == name_FO).ToList();
            }
            //Проверка статуса номера телефона
            if (type_load != "all")
            {
                lst_db_phone_pt = lst_db_phone_pt.Where(u => u.Status == type_load).ToList();
            }
            List<string> tt_ = lst_db_phone_pt.Select(u => u.Phone).Intersect(tmp).ToList();
            List<string> cmd_phone_lst = new List<string>();
            if (type_load == "завершено" || type_load == "all")
            {
                tt_.ForEach(u =>
                {
                    cmd_phone_lst.Add("UPDATE dbo.PTs SET Status='connect' WHERE Phone='" + u + "'");
                });
                db.Database.ExecuteSqlCommand(String.Join(";", cmd_phone_lst));
            }
            tmp.ForEach(u =>
            {
                lst_to_load.Add(new TmpClass { Number = u, Status = "0" });
            });
            using (MySqlConnection connection = new MySqlConnection("server=192.168.0.3;UserId=testuser;Password=12345;port=3306;database=phone;sslmode=none;persistsecurityinfo=True;"))
            {
                try
                {
                    connection.Open();
                    int count_i = 1;
                    tt_.ForEach(u =>
                    {
                        new MySqlCommand("INSERT INTO table" + id_table + " (Id, Number, Status) VALUES ('" + (count_i++) + "','" + u + "','0')", connection).ExecuteNonQuery();
                    });
                    mysql_db.Database.ExecuteSqlCommand("UPDATE name_table SET Name=\"" + name + ".csv\" WHERE Id=" + id_table);
                }
                catch (Exception e)
                {
                    Response.AppendToLog(e.StackTrace);
                }
            }
        }

        //Загрузка номеров в базу данных первичную
        [HttpPost]
        public void ImportOurNumbers(string FO, string OB, string GOR)
        {
            try
            {
                var fileContent = Request.Files[0];
                if (fileContent != null && fileContent.ContentLength > 0)
                {

                    Stream stream = fileContent.InputStream;
                    DataTable csvTable = new DataTable();
                    StreamReader streamReader = new StreamReader(stream, Encoding.GetEncoding(1251));
                    String str;
                    List<PT> lst_phone = new List<PT>();
                    List<string> db_lst_numbers = db.SetPTs.Select(u => u.Phone).ToList();
                    Dictionary<string, string> hashPhoneAndStatus = new Dictionary<string, string>();
                    List<string> file_phone_number = new List<string>();
                    do
                    {
                        str = streamReader.ReadLine();
                        if (str != null)
                        {
                            string[] tmp_str = str.Split(';');
                            file_phone_number.Add(tmp_str[1]);
                            if (!hashPhoneAndStatus.ContainsKey(tmp_str[1]))
                            {
                                hashPhoneAndStatus.Add(tmp_str[1], tmp_str[2]);
                            }
                        }
                        else
                        {
                            break;
                        }

                    } while (true);
                    file_phone_number = file_phone_number.Distinct().ToList();
                    file_phone_number = file_phone_number.Except(file_phone_number.Intersect(db_lst_numbers)).ToList();

                    file_phone_number.ForEach(u =>
                    {
                        lst_phone.Add(new PT { FO = FO, OB = OB, GOR = GOR, Phone = u, Type = 0, Status = hashPhoneAndStatus[u], isActual = false, TimeCall = new DateTime(2000, 1, 1) });
                    });

                    lst_phone = lst_phone.Where(u => u.Phone.Length == 11).AsParallel().Select(u =>
                      {
                          ulong tmp_number_phone = 0;
                          ulong.TryParse(u.Phone, out tmp_number_phone);
                          if (tmp_number_phone >= 80000000000)
                          {
                              tmp_number_phone -= 10000000000;
                          }
                          if (tmp_number_phone >= 79000000000)
                          {
                              u.Type = 1;
                          }
                          else
                          {
                              u.Type = 0;
                          }
                          u.Phone = tmp_number_phone.ToString();
                          return u;
                      }).ToList();

                    lst_phone = lst_phone.Where(u => u.Phone.Length == 11).ToList();

                    DataTable dtPhone = new DataTable();
                    DataColumn dcNameFO = new DataColumn("FO", System.Type.GetType("System.String"));
                    DataColumn dcNameOB = new DataColumn("OB", System.Type.GetType("System.String"));
                    DataColumn dcNameGOR = new DataColumn("GOR", System.Type.GetType("System.String"));
                    DataColumn dcPhone = new DataColumn("Phone", System.Type.GetType("System.String"));
                    DataColumn dcStatus = new DataColumn("Status", System.Type.GetType("System.String"));
                    DataColumn dcType = new DataColumn("Type", System.Type.GetType("System.Int32"));
                    DataColumn dcIsAction = new DataColumn("isAction", System.Type.GetType("System.Boolean"));
                    DataColumn dcTimeCall = new DataColumn("TimeCall", System.Type.GetType("System.DateTime"));
                    dtPhone.PrimaryKey = new DataColumn[] { dtPhone.Columns["Phone"] };
                    dtPhone.Columns.Add(dcNameFO);
                    dtPhone.Columns.Add(dcNameOB);
                    dtPhone.Columns.Add(dcNameGOR);
                    dtPhone.Columns.Add(dcPhone);
                    dtPhone.Columns.Add(dcStatus);
                    dtPhone.Columns.Add(dcType);
                    dtPhone.Columns.Add(dcIsAction);
                    dtPhone.Columns.Add(dcTimeCall);
                    lst_phone.ForEach(u =>
                    {
                        dtPhone.Rows.Add(new object[] { u.FO, u.OB, u.GOR, u.Phone, u.Status, u.Type, u.isActual, u.TimeCall });
                    });

                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy("Data Source=192.168.0.4, 55501 ;Network Library=DBMSSOCN;Initial Catalog=BD_IFsocialforms_Number;User ID=sa;Password=7oDK35jqS;",
                        SqlBulkCopyOptions.TableLock))
                    {
                        bulkCopy.DestinationTableName = "dbo.PTs";
                        try
                        {
                            bulkCopy.WriteToServer(dtPhone);
                        }
                        catch (Exception e)
                        {
                            Response.AppendToLog(e.StackTrace);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Response.AppendToLog(e.StackTrace);
            }
        }

        protected PT SendObjectToDB(PT el)
        {
            db.Database.ExecuteSqlCommand("INSERT dbo.PTs VALUES ({0},{1},{2},{3},{4},{5},{6},{7})", el.FO, el.OB, el.GOR, el.Phone, el.Status, el.Type, el.isActual, el.TimeCall);
            return el;
        }

        //Загрузка пола в соответствии с номером
        [HttpPost]
        public void LoadBackup()
        {
            Dictionary<string, bool> hashTypeSex = new Dictionary<string, bool>();
            List<FormNumber> lst_form_numbers = db.SetFormNumbers.ToList();
            try
            {
                var fileContent = Request.Files[0];
                if (fileContent != null && fileContent.ContentLength > 0)
                {

                    Stream stream = fileContent.InputStream;
                    StreamReader streamReader = new StreamReader(stream, Encoding.GetEncoding(1251));
                    String str;
                    do
                    {
                        str = streamReader.ReadLine();
                        if (str != null)
                        {
                            string[] tmp_str = str.Split(';');
                            bool type_sex = tmp_str[3] == "1" ? true : false;

                            ulong phone_sex = 0;
                            ulong.TryParse(tmp_str[1], out phone_sex);
                            if (phone_sex != 0)
                            {

                                hashTypeSex.Add(phone_sex.ToString(), type_sex);
                            }

                        }
                        else
                        {
                            break;
                        }

                    } while (true);
                }
                System.Diagnostics.Debug.WriteLine("Count hash array ---- {0}", hashTypeSex.Count);
                lst_form_numbers.ForEach(u =>
                {
                    if (hashTypeSex.ContainsKey(u.Phone))
                    {
                        System.Diagnostics.Debug.WriteLine(hashTypeSex[u.Phone]);
                        u.Sex = hashTypeSex[u.Phone];
                    }
                });

                db.SaveChanges();
            }

            catch (Exception e)
            {
                Response.AppendToLog(e.StackTrace);
            }
        }

        //Сбор информации по номеру из базы анкет
        [HttpGet]
        public async Task<JsonResult> SyncBlankWithDB(int p_id, int gor_id, int s_id = 0, int a_id = 0, int np_id = 0, int typeNP_id = 0)
        {
            List<ResultModel> lst_blank = await form_db.SetResultModels.Where(u => u.ProjectID == p_id).ToListAsync();
            List<AnswerModel> lst_answer = await q_db.SetAnswers.Where(u => u.QuestionID == s_id).ToListAsync();
            List<BlankModel> lst_answer_gor = await form_db.SetBlankModels.Where(u => u.QuestionID == gor_id).ToListAsync();
            List<BlankModel> lst_sex = new List<BlankModel>();
            if (s_id != 0)
            {
                lst_sex = await form_db.SetBlankModels.Where(u => u.QuestionID == s_id).ToListAsync();
            }
            List<BlankModel> lst_age = new List<BlankModel>();
            if (a_id != 0)
            {
                lst_age = await form_db.SetBlankModels.Where(u => u.QuestionID == a_id).ToListAsync();
            }
            List<BlankModel> lst_np = new List<BlankModel>();
            if (np_id != 0)
            {
                lst_np = await form_db.SetBlankModels.Where(u => u.QuestionID == np_id).ToListAsync();
            }
            List<BlankModel> lst_typeNP = new List<BlankModel>();
            if (typeNP_id != 0)
            {
                lst_typeNP = await form_db.SetBlankModels.Where(u => u.QuestionID == typeNP_id).ToListAsync();
            }
            List<FormNumber> lst_form_numbers = new List<FormNumber>();
            List<FormNumber> skiping_numbers = new List<FormNumber>();
            string FO = "";
            string OB = "";
            foreach (var item in lst_blank)
            {
                PT tmp = await db.SetPTs.FirstOrDefaultAsync(u => u.Phone == item.PhoneNumber);
                if (tmp != null)
                {
                    FO = tmp.FO;
                    OB = tmp.OB;
                    break;
                }
            }
            foreach (var item in lst_blank)
            {
                PT tmp = await db.SetPTs.FirstOrDefaultAsync(u => u.Phone == item.PhoneNumber);
                if (lst_form_numbers.FirstOrDefault(u => u.Phone == item.PhoneNumber) != null)
                {
                    continue;
                }
                if (tmp != null)
                {
                    FormNumber tmp_form_number = new FormNumber();
                    tmp_form_number.FO = tmp.FO;
                    if (FO == "")
                    {
                        FO = tmp.FO;
                    }
                    tmp_form_number.OB = tmp.OB;
                    if (OB == "")
                    {
                        OB = tmp.OB;
                    }
                    tmp_form_number.GOR = tmp.GOR;
                    tmp_form_number.Phone = tmp.Phone;
                    if (s_id != 0)
                    {
                        tmp_form_number.Sex = lst_answer.First(u => u.Index == lst_sex.First(x => x.BlankID == item.Id).AnswerIndex).Index == 0 ? false : true;
                    }
                    if (a_id != 0)
                    {
                        tmp_form_number.Age = DateTime.Now.Year - Int32.Parse(lst_age.First(u => u.BlankID == item.Id).Text);
                    }
                    tmp_form_number.Type = tmp.Type;
                    if (np_id != 0)
                    {
                        tmp_form_number.NP = lst_np.First(u => u.BlankID == item.Id).Text;
                    }
                    if (typeNP_id != 0)
                    {
                        tmp_form_number.TypeNP = lst_typeNP.First(u => u.BlankID == item.Id).AnswerIndex == 1 ? false : true;
                    }
                    lst_form_numbers.Add(tmp_form_number);
                }
                else
                {
                    FormNumber skip_tmp_form_number = new FormNumber();
                    int GOR_index = lst_answer_gor.First(u => u.BlankID == item.Id).AnswerIndex;
                    string GOR = "GOR" + GOR_index;
                    skip_tmp_form_number.FO = FO;
                    skip_tmp_form_number.OB = OB;
                    skip_tmp_form_number.GOR = GOR;
                    skip_tmp_form_number.Phone = item.PhoneNumber;
                    if (s_id != 0)
                    {
                        skip_tmp_form_number.Sex = lst_answer.First(u => u.Index == lst_sex.First(x => x.BlankID == item.Id).AnswerIndex).Index == 0 ? false : true;
                    }
                    if (a_id != 0)
                    {
                        skip_tmp_form_number.Age = DateTime.Now.Year - Int32.Parse(lst_age.First(u => u.BlankID == item.Id).Text);
                    }
                    if (np_id != 0)
                    {
                        skip_tmp_form_number.NP = lst_np.First(u => u.BlankID == item.Id).Text;
                    }
                    if (typeNP_id != 0)
                    {
                        skip_tmp_form_number.TypeNP = lst_typeNP.First(u => u.BlankID == item.Id).AnswerIndex == 1 ? false : true;
                    }
                    skip_tmp_form_number.Type = item.PhoneNumber.CompareTo("79000000000") == -1 ? 0 : 1;
                    skiping_numbers.Add(skip_tmp_form_number);
                }
            }
            lst_form_numbers.AddRange(skiping_numbers);
            int count_lst_form_numbers = lst_form_numbers.Count();
            int iter = 0;

            try
            {
                do
                {
                    FormNumber tmp = await db.SetFormNumbers.FirstOrDefaultAsync(u => u.Phone == lst_form_numbers[iter].Phone);
                    if (tmp != null)
                    {
                        lst_form_numbers.Remove(lst_form_numbers[iter]);
                        count_lst_form_numbers--;
                    }
                    else
                    {
                        iter++;
                    }
                } while (iter <= count_lst_form_numbers);
            }
            catch (Exception e)
            {
                Response.AppendToLog(e.StackTrace);
            }

            foreach (var item in lst_form_numbers)
            {
                PT tmp = await db.SetPTs.FirstOrDefaultAsync(u => u.Phone == item.Phone);
                if (tmp != null)
                {
                    if (item.Type == 1)
                    {
                        tmp.GOR = item.GOR;
                        tmp.Status = "завершено";
                        await db.SaveChangesAsync();
                    }
                }
                else
                {
                    tmp.FO = item.FO;
                    tmp.OB = item.OB;
                    tmp.GOR = item.GOR;
                    tmp.Phone = item.Phone;
                    tmp.Status = "завершено";
                    tmp.Type = item.Type;
                    tmp.isActual = false;
                    tmp.TimeCall = DateTime.Parse(DateTime.Now.ToLongDateString());
                    db.SetPTs.Add(tmp);
                    await db.SaveChangesAsync();
                }
            }

            try
            {
                db.SetFormNumbers.AddRange(lst_form_numbers);
                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Response.AppendToLog(e.StackTrace);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            return Json(lst_form_numbers, JsonRequestBehavior.AllowGet);
        }

    }
}
