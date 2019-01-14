using LumenWorks.Framework.IO.Csv;
using Newtonsoft.Json.Linq;
using SocialFORM.Models;
using SocialFORM.Models.Form;
using SocialFORM.Models.Number;
using SocialFORM.Models.Question;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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

        public ActionResult DataBaseView()
        {
            return PartialView();
        }

        public ActionResult PrimaryTableView()
        {
            return PartialView();
        }

        public ActionResult SyncWithBlanksView()
        {
            return PartialView();
        }

        public ActionResult GeneratorView()
        {
            return PartialView();
        }

        [HttpPost]
        public void TestImport(string table)
        {
            Dictionary<string, int> callback = new Dictionary<string, int>();
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
                    int count = 1;
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
                                if (tmp.Phone.ElementAt(0) != '8')
                                {
                                    tmp.Phone = tmp.Phone.Remove(0, 1).Insert(0, "8");
                                }
                                if (db.SetFormNumbers.FirstOrDefault(u => u.Phone == tmp.Phone) != null)
                                {
                                    continue;
                                }
                            }
                            else continue;
                            tmp.Sex = tmp_str[7];
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
                            lst_phone.Add(tmp);
                            count++;
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
                    System.Diagnostics.Debug.WriteLine("Count >>> " + lst_phone.Count());
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
                foreach(var item in tmp_list)
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
                return Json(null, JsonRequestBehavior.AllowGet);
            }

        }

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
                row_lst.Add(item.Sex);
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
                    tmp.Phone = "8" + numb.ToString();
                    if (db.SetPTs.FirstOrDefault(u => u.Phone == tmp.Phone) != null) continue;
                    tmp.Status = "0";
                    tmp.Type = Int64.Parse(tmp.Phone) < 89000000000 ? 0 : 1;
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
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
        }

        class TmpClass
        {
            public int Id { get; set; }
            public string Number { get; set; }
            public string Status { get; set; }
        }

        [HttpGet]
        public JsonResult GetNumberStatus(string FO, string OB, string GOR, string settings)
        {
            List<bool> lst_setting = new List<bool>();
            foreach (var item in settings.Split(','))
            {
                lst_setting.Add(item == "false" ? false : true);
            }
            List<PT> lst_PT = db.SetPTs.ToList();
            if (FO != "")
            {
                lst_PT = lst_PT.Where(u => u.FO == FO).ToList();
                if (OB != "")
                {
                    lst_PT = lst_PT.Where(u => u.OB == OB).ToList();
                    if (GOR != "")
                    {
                        lst_PT = lst_PT.Where(u => u.GOR == GOR).ToList();
                    }
                }
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

                if (lst_setting[0])
                {
                    tmp_lst_PT.AddRange(lst_PT.Where(u => u.Status == "0").ToList());
                }

                if (lst_setting[1])
                {
                    tmp_lst_PT.AddRange(lst_PT.Where(u => u.Status == "занято").ToList());
                }

                if (lst_setting[2])
                {
                    tmp_lst_PT.AddRange(lst_PT.Where(u => u.Status == "нет ответа").ToList());
                }

                if (lst_setting[3])
                {
                    tmp_lst_PT.AddRange(lst_PT.Where(u => u.Status == "connect" || u.Status == "1" || u.Status== "завершено").ToList());
                }

                if (lst_setting[6])
                {
                    tmp_lst_PT.AddRange(lst_PT.Where(u => u.Status == "линия не найдена").ToList());
                }

                if (lst_setting[7])
                {
                    tmp_lst_PT.AddRange(lst_PT.Where(u => u.Status == "перезвонить").ToList());
                }

                return Json(tmp_lst_PT, JsonRequestBehavior.AllowGet);
            }

            return Json(lst_PT, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void ImportNumberToOktell(string FO, string OB, string GOR, int id_table, string settings)
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

            if (lst_setting[4])
            {
                tmp_lst_PT = tmp_lst_PT.Where(u => u.Type == 0).ToList();
            }

            if (lst_setting[5])
            {
                tmp_lst_PT = tmp_lst_PT.Where(u => u.Type == 1).ToList();
            }

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
                    tmp_lst_PT_.AddRange(tmp_lst_PT.Where(u=>u.Status == "connect" || u.Status == "1" || u.Status == "завершено").ToList());
                }

                if (lst_setting[6])
                {
                    tmp_lst_PT_.AddRange(tmp_lst_PT.Where(u => u.Status == "линия не найдена").ToList());
                }

                if (lst_setting[7])
                {
                    tmp_lst_PT_.AddRange(tmp_lst_PT.Where(u => u.Status == "перезвонить").ToList());
                }

                tmp_lst_PT = tmp_lst_PT_;
            }
            System.Diagnostics.Debug.WriteLine("In here");
            if (tmp_lst_PT != null && tmp_lst_PT.Count() >= 1)
            {
                System.Diagnostics.Debug.WriteLine("In here2");
                //List<TmpClass> tmp_lst_numb_phone = new List<TmpClass>();
                int count = 1;
                try
                {
                    foreach (var item in tmp_lst_PT)
                    {
                        TmpClass tmp_arg = new TmpClass();
                        tmp_arg.Number = item.Phone;
                        tmp_arg.Status = item.Status;
                        //tmp_lst_numb_phone.Add(tmp_arg);
                        mysql_db.Database.ExecuteSqlCommand("INSERT INTO table" + id_table + " (Id, Number, Status) VALUES ('" + (count++) + "','" + item.Phone + "','0')");
                    }
                    mysql_db.Database.ExecuteSqlCommand("UPDATE name_table SET Name=\"" + name_table + ".csv\" WHERE Id=" + id_table);
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

        [HttpGet]
        public JsonResult GetFreeNameTable()
        {
            List<string> tmp = mysql_db.Database.SqlQuery<string>("SELECT Name FROM name_table").ToList();
            return Json(tmp, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void SynchNumbers(int id_table)
        {
            List<TmpClass> sync_status_lst = new List<TmpClass>();
            sync_status_lst = mysql_db.Database.SqlQuery<TmpClass>("SELECT * FROM table" + id_table).ToList();
            List<PT> pt_lst_tmp = db.SetPTs.ToList();
            foreach (var item in sync_status_lst)
            {
                //PT tmp_item = db.SetPTs.FirstOrDefault(u => u.Phone == item.Number);
                if (pt_lst_tmp.FirstOrDefault(u => u.Phone == item.Number) != null)
                {
                    pt_lst_tmp.FirstOrDefault(u => u.Phone == item.Number).Status = item.Status;
                }
            }
            db.SaveChanges();
        }

        [HttpPost]
        public void TestFuncTest(string name, int id_table, string[] tmp)
        {
            System.Diagnostics.Debug.WriteLine("count --- ");
            List<TmpClass> lst_to_load = new List<TmpClass>();
            foreach (var item in tmp)
            {
                TmpClass item_load = new TmpClass();
                item_load.Number = item;
                item_load.Status = "0";
                lst_to_load.Add(item_load);
            }

            int count = 1;
            try
            {
                foreach (var item in lst_to_load)
                {
                    mysql_db.Database.ExecuteSqlCommand("INSERT INTO table" + id_table + " (Id, Number, Status) VALUES ('" + (count++) + "','" + item.Number + "','0')");
                }
                mysql_db.Database.ExecuteSqlCommand("UPDATE name_table SET Name=\"" + name + ".csv\" WHERE Id=" + id_table);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }

            System.Diagnostics.Debug.WriteLine("Count >>> " + lst_to_load.Count + " " + name);
        }

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
                    int count = 1;
                    do
                    {
                        str = streamReader.ReadLine();
                        if (str != null)
                        {
                            List<string> tmp_str = str.Split(';').ToList();
                            PT tmp = new PT();
                           
                            if (tmp_str[0] == null || tmp_str[0] == "")
                            {
                                break;
                            }
                            
                            tmp.FO = FO;
                            tmp.OB = OB;
                            tmp.GOR = GOR;
                            if (tmp_str[1] != "")
                            {
                               
                                tmp.Phone = tmp_str[1];
                                if (tmp.Phone.ElementAt(0) != '8')
                                {
                                    tmp.Phone = tmp.Phone.Remove(0, 1).Insert(0, "8");
                                }
                                if (db.SetPTs.FirstOrDefault(u => u.Phone == tmp.Phone) != null)
                                {
                                    continue;
                                }
                            }
                            else continue;
                            
                            tmp.Status = tmp_str[2];
                            if (tmp.Phone.CompareTo("89000000000") == -1)
                            {
                                tmp.Type = 0;
                            }
                            else
                            {
                                tmp.Type = 1;
                            }
                            lst_phone.Add(tmp);
                            count++;
                            System.Diagnostics.Debug.WriteLine("In HERE");
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
                            List<PT> tmp_lst = lst_phone.Where(u => u.Phone == lst_phone[iter].Phone).ToList();
                            tmp_lst.Remove(tmp_lst.First());
                            foreach (var rem_item in tmp_lst)
                            {
                                lst_phone.Remove(rem_item);
                            }
                            count_lst = lst_phone.Count();
                        }
                        iter++;
                    }
                    System.Diagnostics.Debug.WriteLine("Count >>> " + lst_phone.Count());
                    db.SetPTs.AddRange(lst_phone);
                    db.SaveChanges();

                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.StackTrace.ToString());
            }
        }

        [HttpGet]
        public async Task<JsonResult> SyncBlankWithDB(int p_id, int gor_id ,int s_id = 0, int a_id = 0, int np_id = 0)
        {
            List<ResultModel> lst_blank = await form_db.SetResultModels.Where(u => u.ProjectID == p_id).ToListAsync();
            List<AnswerModel> lst_answer = await q_db.SetAnswers.Where(u => u.QuestionID == s_id).ToListAsync();
            List<BlankModel> lst_answer_gor = await form_db.SetBlankModels.Where(u => u.QuestionID == gor_id).ToListAsync();
            List<BlankModel> lst_sex = new List<BlankModel>();
            if (s_id != 0) {
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
            List<FormNumber> lst_form_numbers = new List<FormNumber>();
            List<FormNumber> skiping_numbers = new List<FormNumber>();
            string FO = "";
            string OB = "";
            foreach(var item in lst_blank)
            {
                PT tmp = await db.SetPTs.FirstOrDefaultAsync(u => u.Phone == item.PhoneNumber);
                if (tmp != null)
                {
                    FO = tmp.FO;
                    OB = tmp.OB;
                    break;
                }
            }
            foreach(var item in lst_blank) {
                PT tmp = await db.SetPTs.FirstOrDefaultAsync(u => u.Phone == item.PhoneNumber);
                if (lst_form_numbers.FirstOrDefault(u=>u.Phone == item.PhoneNumber) != null)
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
                        tmp_form_number.Sex = lst_answer.First(u => u.Index == lst_sex.First(x => x.BlankID == item.Id).AnswerIndex).AnswerText;
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
                    lst_form_numbers.Add(tmp_form_number);
                } else
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
                        skip_tmp_form_number.Sex = lst_answer.First(u => u.Index == lst_sex.First(x => x.BlankID == item.Id).AnswerIndex).AnswerText;
                    }
                    if (a_id != 0)
                    {
                        skip_tmp_form_number.Age = DateTime.Now.Year - Int32.Parse(lst_age.First(u => u.BlankID == item.Id).Text);
                    }
                    if (np_id != 0) {
                        skip_tmp_form_number.NP = lst_np.First(u => u.BlankID == item.Id).Text;
                    }
                    skip_tmp_form_number.Type = item.PhoneNumber.CompareTo("89000000000") == -1 ? 0 : 1;
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
            }catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.InnerException);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
                System.Diagnostics.Debug.WriteLine(e.Data);
                System.Diagnostics.Debug.WriteLine(e.Data);
            }
            try
            {
                db.SetFormNumbers.AddRange(lst_form_numbers);
                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.InnerException);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
                System.Diagnostics.Debug.WriteLine(e.Data);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            return Json(lst_form_numbers, JsonRequestBehavior.AllowGet);
        }
    }
}
