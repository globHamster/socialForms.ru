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
using System.Web.Helpers;
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
                db.SetAOs.ToList().ForEach(x =>
                {
                    System.Diagnostics.Debug.WriteLine($"{x.KodFO}/{x.KodOB}/{x.KodAO}/{x.NameAO}");
                });
                System.Diagnostics.Debug.WriteLine("Start function ... ");
                List<VGMR> lst_VGMR_for_AO = db.SetVGMRs.ToList();
                System.Diagnostics.Debug.WriteLine($"Count of list VGMR : {lst_VGMR_for_AO.Count}");
                lst_VGMR_for_AO.ForEach(x =>
                {
                    System.Diagnostics.Debug.WriteLine($"{x.KodFO}/{x.KodOB}/{x.KodAO}/{x.KodVGMR}/{x.NameVGMR}");
                });
                var fileContent = Request.Files[0];
                if (fileContent != null && fileContent.ContentLength > 0)
                {
                    string name_file = fileContent.FileName;

                    Stream stream = fileContent.InputStream;
                    StreamReader streamReader = new StreamReader(stream, Encoding.GetEncoding(1251));
                    String str;
                    Dictionary<string, FormNumber> lst_phone = new Dictionary<string, FormNumber>();
                    //Dictionary<string, PT> db_phone_PT = db.SetPTs.ToDictionary(u => u.Phone, u => u);
                    //Dictionary<string, FormNumber> all_phone_FN = db.SetFormNumbers.ToDictionary(u => u.Phone, u => u);
                    uint count_write_table = 1;
                    System.Diagnostics.Debug.WriteLine("Start proccesing creating table ...");
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
                            }
                            else continue;
                            short tmp_file_sex = 0;
                            short.TryParse(tmp_str[7], out tmp_file_sex);
                            tmp.Sex = tmp_file_sex == 0 ? false : true;
                            if (tmp_str[9] != "")
                            {
                                try
                                {
                                    tmp.Age = Int32.Parse(tmp_str[9]);
                                }
                                catch (FormatException e)
                                {
                                    tmp.Age = 0;
                                }
                            }
                            else
                            {
                                try
                                {
                                    tmp.Age = Int32.Parse(tmp_str[8]);
                                }
                                catch (FormatException e)
                                {
                                    tmp.Age = 0;
                                }
                            }

                            tmp.Address = tmp_str[5];
                            tmp.AO = tmp_str[10];
                            tmp.VGMR = tmp_str[11];
                            if (tmp.VGMR!="" && tmp.VGMR != null)
                            {
                                System.Diagnostics.Debug.WriteLine($"ROW-{count_write_table} -> {tmp.FO}/{tmp.OB}/{tmp.VGMR}");
                                tmp.AO = lst_VGMR_for_AO.Find(u => u.KodFO == tmp.FO && u.KodOB == tmp.OB && u.KodVGMR == tmp.VGMR).KodAO;
                            }
                            tmp.Education = "";
                            tmp.Type = tmp.Phone.ElementAt(1) == '9' ? 1 : 0;
                            tmp.TypeNP = false;
                            if (!lst_phone.ContainsKey(tmp.Phone)) { lst_phone.Add(tmp.Phone, tmp); }
                            System.Diagnostics.Debug.WriteLine($"Row -> {(count_write_table++)}");
                        }
                        else
                        {
                            break;
                        }
                    } while (true);
                    System.Diagnostics.Debug.WriteLine("End proccesing creating table...");
                    List<string> lst_only_phone = lst_phone.Keys.ToList();
                    lst_only_phone.ForEach(u =>
                    {
                        //if (!all_phone_FN.ContainsKey(u))
                        if (!db.SetFormNumbers.Any(t => t.Phone == u))
                        {
                            db.Database.ExecuteSqlCommand("INSERT INTO dbo.FormNumbers (FO, OB, GOR, NP, VGOR, Phone, Sex, Age, Address, Education, Type, TypeNP, AO, VGMR) VALUES({0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13})",
                                lst_phone[u].FO,
                                lst_phone[u].OB,
                                lst_phone[u].GOR,
                                lst_phone[u].NP,
                                lst_phone[u].VGOR,
                                lst_phone[u].Phone,
                                lst_phone[u].Sex,
                                lst_phone[u].Age,
                                lst_phone[u].Address,
                                lst_phone[u].Education,
                                lst_phone[u].Type,
                                lst_phone[u].TypeNP,
                                lst_phone[u].AO,
                                lst_phone[u].VGMR);
                        }
                        else {
                            db.Database.ExecuteSqlCommand("UPDATE dbo.FormNumbers SET FO={0},OB={1},GOR={2},NP={3},VGOR={4},Sex={5},Age={6},Address={7},Education={8},Type={9},TypeNP={10},AO={11},VGMR={12} WHERE Phone={13}",
                                lst_phone[u].FO,
                                lst_phone[u].OB,
                                lst_phone[u].GOR,
                                lst_phone[u].NP,
                                lst_phone[u].VGOR,
                                lst_phone[u].Sex,
                                lst_phone[u].Age,
                                lst_phone[u].Address,
                                lst_phone[u].Education,
                                lst_phone[u].Type,
                                lst_phone[u].TypeNP,
                                lst_phone[u].AO,
                                lst_phone[u].VGMR,
                                lst_phone[u].Phone);
                        }
                        //if (!db_phone_PT.ContainsKey(u))
                        if (!db.SetPTs.Any(t=>t.Phone == u))
                        {
                            db.Database.ExecuteSqlCommand("INSERT INTO dbo.PTs (FO, OB, GOR, Phone, Status, Type, isActual, TimeCall) VALUES ({0},{1},{2},{3},{4},{5},{6},{7})",
                                lst_phone[u].FO,
                                lst_phone[u].OB,
                                lst_phone[u].GOR,
                                u,
                                "connect",
                                lst_phone[u].Type,
                                false,
                                new DateTime(2000, 1, 1).ToString());
                        } else
                        {
                            db.Database.ExecuteSqlCommand("UPDATE dbo.PTs SET FO={0},OB={1},GOR={2},Status={3},Type={4},isActual={5} WHERE Phone={6}",
                                lst_phone[u].FO,
                                lst_phone[u].OB,
                                lst_phone[u].GOR,
                                "connect",
                                lst_phone[u].Type,
                                false, 
                                u);
                        }
                    });
                }
            }
            catch (Exception e)
            {
                //System.Diagnostics.Debug.WriteLine(e.StackTrace.ToString());
                //System.Diagnostics.Debug.WriteLine(e.Data);
                //System.Diagnostics.Debug.WriteLine(e.Source);
                //System.Diagnostics.Debug.WriteLine(e.Message);
                Response.AppendToLog(e.StackTrace.ToString());
                Response.AppendToLog(e.Message);
                Response.AppendToLog(e.Data.ToString());
                Response.AppendToLog(e.Source);
            }
            finally
            {
                GC.Collect();
            }

        }

        [HttpGet]
        public JsonResult getListFormNumber(string KodFO, string KodOB, string KodGOR)
        {
            try
            {
                List<FormNumber> tmp_list;
                //if (KodOB != "")
                //{
                //    tmp_list = tmp_list.Where(u => u.OB == KodOB).ToList();
                //    if (KodGOR != "")
                //    {
                //        tmp_list = tmp_list.Where(u => u.GOR == KodGOR).ToList();
                //    }
                //}
                if (KodGOR != "") tmp_list = db.SetFormNumbers.Where(u => u.FO == KodFO && u.OB == KodOB && u.GOR == KodGOR).ToList();
                else if (KodOB != "") tmp_list = db.SetFormNumbers.Where(u => u.FO == KodFO && u.OB == KodOB).ToList();
                else if (KodFO != "") tmp_list = db.SetFormNumbers.Where(u => u.FO == KodFO).ToList();
                else throw new Exception("Uncorrect data for filter");


                tmp_list.AsParallel().ForAll(u => { if (u.Age > 1000) u.Age = DateTime.Now.Year - u.Age; });
                var jsonResult = Json(tmp_list, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            catch (Exception e)
            {
                //System.Diagnostics.Debug.WriteLine("Exception >>> " + e.StackTrace);
                //System.Diagnostics.Debug.WriteLine("Exception >>> " + e.Message);
                //System.Diagnostics.Debug.WriteLine("Exception >>> " + e.Source);
                //System.Diagnostics.Debug.WriteLine("Exception >>> " + e.Data);
                Response.AppendToLog(e.StackTrace);
                Response.AppendToLog(e.Data.ToString());
                Response.AppendToLog(e.Source);
                Response.AppendToLog(e.Message);
                return Json(null, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpGet]
        public JsonResult getListFormNumberOptionally(string KodFO, string KodOB, string KodGOR, string KodAO, string KodVGMR)
        {
            try
            {
                Dictionary<string, string> tmp_lstAO = null;
                Dictionary<string, string> tmp_lstVGMR = null;
                tmp_lstAO = db.SetAOs.Where(u => u.KodFO == KodFO && u.KodOB == KodOB).ToDictionary(t => t.KodAO, u => u.NameAO);
                tmp_lstVGMR = db.SetVGMRs.Where(u => u.KodFO == KodFO && u.KodOB == KodOB && u.KodAO == KodAO).ToDictionary(t => t.KodVGMR, u => u.NameVGMR);

                List<FormNumber> tmp_list;
                if (KodVGMR != "") tmp_list = db.SetFormNumbers.Where(u => u.FO == KodFO && u.OB == KodOB && u.AO == KodAO && u.VGMR == KodVGMR).ToList();
                else if (KodAO != "") tmp_list = db.SetFormNumbers.Where(u => u.FO == KodFO && u.OB == KodOB && u.AO == KodAO).ToList();
                else if (KodOB != "") tmp_list = db.SetFormNumbers.Where(u => u.FO == KodFO && u.OB == KodOB).ToList();
                else if (KodFO != "") tmp_list = db.SetFormNumbers.Where(u => u.FO == KodFO).ToList();
                else throw new Exception("Uncorrect data for filter");
                tmp_list.AsParallel().ForAll(u =>
                {
                    //System.Diagnostics.Debug.WriteLine($"AO -> {u.AO}");
                    //System.Diagnostics.Debug.WriteLine($"VGMR -> {u.VGMR}");
                    u.AO = tmp_lstAO[u.AO];
                    if(u.VGMR!=null) u.VGMR = tmp_lstVGMR[u.VGMR];
                    if (u.Age > 1000) u.Age = DateTime.Now.Year - u.Age;
                });
                var jsonResult = Json(tmp_list, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            catch (Exception e)
            {
                //System.Diagnostics.Debug.WriteLine("Exception >>> " + e.StackTrace);
                //System.Diagnostics.Debug.WriteLine("Exception >>> " + e.Message);
                //System.Diagnostics.Debug.WriteLine("Exception >>> " + e.Source);
                //System.Diagnostics.Debug.WriteLine("Exception >>> " + e.Data);
                Response.AppendToLog(e.StackTrace);
                Response.AppendToLog(e.Data.ToString());
                Response.AppendToLog(e.Source);
                Response.AppendToLog(e.Message);
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

        //Загрузка номеров из генератора
        [HttpPost]
        public void PushNumbersStatus(string KodFO, string KodOB, string KodGOR, List<string> mas_numb)
        {
            List<string> db_phone_lst = db.SetPTs.Select(u => u.Phone).ToList();
            DateTime default_date = new DateTime(2000, 1, 1);
            Dictionary<string, PT> dict_phone = new Dictionary<string, PT>();
            mas_numb.ForEach(u =>
            {
                string[] diap = u.Split('-');
                long start_diap = Int64.Parse(diap[0]);
                long end_diap = Int64.Parse(diap[1]);
                for (long numb = start_diap; numb <= end_diap; numb++)
                {
                    string tmp_phone = "7" + numb.ToString();
                    if (!dict_phone.ContainsKey(tmp_phone))
                    {
                        dict_phone.Add(tmp_phone, new PT()
                        {
                            FO = KodFO,
                            OB = KodOB,
                            GOR = KodGOR,
                            Phone = tmp_phone,
                            Status = "0",
                            Type = (Int64.Parse(tmp_phone) < 79000000000 ? 0 : 1),
                            isActual = false,
                            TimeCall = default_date
                        });
                    }
                }
            });

            List<string> lst_only_phone = dict_phone.Keys.ToList();
            lst_only_phone = lst_only_phone.Except(lst_only_phone.Intersect(db_phone_lst)).ToList();
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
            lst_only_phone.ForEach(u =>
            {
                dtPhone.Rows.Add(new object[] { dict_phone[u].FO,
                    dict_phone[u].OB,
                    dict_phone[u].GOR,
                    dict_phone[u].Phone,
                    dict_phone[u].Status,
                    dict_phone[u].Type,
                    dict_phone[u].isActual,
                    dict_phone[u].TimeCall });
            });

            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(@"Server=WEBSERVER\SQLEXPRESS;Database=BD_IFsocialforms_Number;User ID=sa;Password=7oDK35jqS;",
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

        class TmpClass
        {
            public int Id { get; set; }
            public string Number { get; set; }
            public string Status { get; set; }
        }
        //Получение номеров для первичной базы в соответствии с настройками
        [HttpGet]
        public JsonResult GetNumberStatus(string FO, string OB, string GOR, string settings, short type_select, string mass_time, byte? iterval, bool? invers, int page)
        {
            List<bool> lst_setting = new List<bool>();
            string cmd_string = "";
            foreach (var item in settings.Split(','))
            {
                lst_setting.Add(item == "false" ? false : true);
            }

            if (GOR != "")
            {
                cmd_string += $"(FO='{FO}') AND (OB='{OB}') AND (GOR='{GOR}')";
            }
            else if (OB != "")
            {
                cmd_string += $"(FO='{FO}') AND (OB='{OB}')";
            }
            else if (FO != "")
            {
                cmd_string += $"(FO='{FO}')";
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }

            List<string> tmp_cmd_string = new List<string>();
            if (lst_setting[4])
            {
                if (lst_setting[5]) { cmd_string += " AND ((Type='0')"; }
                else { cmd_string += " AND (Type='0')"; }
            }

            if (lst_setting[5])
            {
                if (lst_setting[4]) { cmd_string += " OR (Type='1'))"; }
                else { cmd_string += " AND (Type='1')"; }
            }

            byte count_setting = 0;
            bool prev_setting = false;
            lst_setting.ForEach(u =>
            {
                if (u) count_setting++;
            });

            if (count_setting != 0)
            {
                if (count_setting == 1) { cmd_string += " AND "; }
                if (count_setting > 1) { cmd_string += " AND ("; }
            }
            if (lst_setting[0])
            {
                cmd_string += " (Status='0') ";
                prev_setting = true;
            }

            if (lst_setting[1])
            {
                if (prev_setting)
                {
                    cmd_string += " OR (Status='занято')";
                }
                else
                {
                    cmd_string += "(Status='занято')";
                    prev_setting = true;
                }
            }

            if (lst_setting[2])
            {
                if (prev_setting)
                {
                    cmd_string += " OR (Status='нет ответа')";
                }
                else
                {
                    cmd_string += "(Status='нет ответа')";
                    prev_setting = true;
                }
            }

            if (lst_setting[3])
            {
                if (prev_setting)
                {
                    cmd_string += " OR ((Status='1') OR (Status='завершено'))";
                }
                else
                {
                    cmd_string += "((Status='1') OR (Status='завершено'))";
                    prev_setting = true;
                }
            }

            if (lst_setting[6])
            {
                if (prev_setting)
                {
                    cmd_string += " OR (Status='линия не найдена')";
                }
                else
                {
                    cmd_string += "(Status='линия не найдена')";
                    prev_setting = true;
                }
            }

            if (lst_setting[7])
            {
                if (prev_setting)
                {
                    cmd_string += " OR (Status='перезвонить')";
                }
                else
                {
                    cmd_string += "(Status='перезвонить')";
                    prev_setting = true;
                }
            }

            if (lst_setting[8])
            {
                if (prev_setting)
                {
                    cmd_string += " OR (Status='connect')";
                }
                else
                {
                    cmd_string += "(Status='connect')";
                }
            }

            if (count_setting > 1)
            {
                cmd_string += ")";
            }

            switch (type_select)
            {
                case 0:
                case 1:
                    {
                        cmd_string += $" AND (isActual={type_select})";
                    }
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
                        cmd_string += $" AND (TimeCall<'{time_tmp_}')";
                    }
                    else
                    {
                        cmd_string += $" AND (TimeCall>'{time_tmp_}')";
                    }
                }
                else
                {
                    if (time_arg_tmp.Count() >= 1)
                    {
                        DateTime time_tmp_;
                        if (time_arg_tmp.Count() == 1)
                        {
                            time_tmp_ = DateTime.Parse(time_arg_tmp.ElementAt(0));
                            cmd_string += $" AND (TimeCall='{time_tmp_}')";
                        }
                        else
                        {
                            cmd_string += " AND (";
                            for (short i = 0; i < time_arg_tmp.Count() - 1; i++)
                            {
                                time_tmp_ = DateTime.Parse(time_arg_tmp.ElementAt(i));
                                cmd_string += $"(TimeCall='{time_tmp_}') OR ";
                            }
                            time_tmp_ = DateTime.Parse(time_arg_tmp.ElementAt(time_arg_tmp.Count() - 1));
                            cmd_string += $"(TimeCall='{time_tmp_}'))";
                        }
                    }

                }
            }

            var jsonResult = Json(db.Database.SqlQuery<PT>("WITH tmp_data AS(" +
                                                           "SELECT ROW_NUMBER() OVER(ORDER BY Phone) AS row_id, FO, OB, GOR, Phone, Status, Type, isActual, TimeCall FROM[BD_IFsocialforms_Number].[dbo].[PTs] WHERE" + cmd_string +
                                                           ") SELECT FO, OB, GOR, Phone, Status, Type, isActual, TimeCall FROM tmp_data" +
                                                           $" WHERE row_id BETWEEN {(page * 30) + 1} AND {(30 * (page + 1))}"), JsonRequestBehavior.AllowGet);
            //jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        //Получение количество номеров для первичной базы в соответствии с настройками
        [HttpGet]
        public ulong GetCountPhone(string FO, string OB, string GOR, string settings, short type_select, string mass_time, byte? iterval, bool? invers, int page)
        {
            List<bool> lst_setting = new List<bool>();
            string cmd_string = "";
            foreach (var item in settings.Split(','))
            {
                lst_setting.Add(item == "false" ? false : true);
            }

            if (GOR != "")
            {
                cmd_string += $"(FO='{FO}') AND (OB='{OB}') AND (GOR='{GOR}')";
            }
            else if (OB != "")
            {
                cmd_string += $"(FO='{FO}') AND (OB='{OB}')";
            }
            else if (FO != "")
            {
                cmd_string += $"(FO='{FO}')";
            }
            else
            {
                return 0;
            }

            List<string> tmp_cmd_string = new List<string>();
            if (lst_setting[4])
            {
                if (lst_setting[5]) { cmd_string += " AND ((Type='0')"; }
                else { cmd_string += " AND (Type='0')"; }
            }

            if (lst_setting[5])
            {
                if (lst_setting[4]) { cmd_string += " OR (Type='1'))"; }
                else { cmd_string += " AND (Type='1')"; }
            }

            byte count_setting = 0;
            bool prev_setting = false;
            lst_setting.ForEach(u =>
            {
                if (u) count_setting++;
            });
            if (count_setting != 0)
            {
                if (count_setting == 1) { cmd_string += " AND "; }
                if (count_setting > 1) { cmd_string += " AND ("; }
            }
            if (lst_setting[0])
            {
                cmd_string += " (Status='0') ";
                prev_setting = true;
            }

            if (lst_setting[1])
            {
                if (prev_setting)
                {
                    cmd_string += " OR (Status='занято')";
                }
                else
                {
                    cmd_string += "(Status='занято')";
                    prev_setting = true;
                }
            }

            if (lst_setting[2])
            {
                if (prev_setting)
                {
                    cmd_string += " OR (Status='нет ответа')";
                }
                else
                {
                    cmd_string += "(Status='нет ответа')";
                    prev_setting = true;
                }
            }

            if (lst_setting[3])
            {
                if (prev_setting)
                {
                    cmd_string += " OR ((Status='1') OR (Status='завершено'))";
                }
                else
                {
                    cmd_string += "((Status='1') OR (Status='завершено'))";
                    prev_setting = true;
                }
            }

            if (lst_setting[6])
            {
                if (prev_setting)
                {
                    cmd_string += " OR (Status='линия не найдена')";
                }
                else
                {
                    cmd_string += "(Status='линия не найдена')";
                    prev_setting = true;
                }
            }

            if (lst_setting[7])
            {
                if (prev_setting)
                {
                    cmd_string += " OR (Status='перезвонить')";
                }
                else
                {
                    cmd_string += "(Status='перезвонить')";
                    prev_setting = true;
                }
            }

            if (lst_setting[8])
            {
                if (prev_setting)
                {
                    cmd_string += " OR (Status='connect')";
                }
                else
                {
                    cmd_string += "(Status='connect')";
                }
            }

            if (count_setting > 1)
            {
                cmd_string += ")";
            }

            switch (type_select)
            {
                case 0:
                case 1:
                    {
                        cmd_string += $" AND (isActual={type_select})";
                    }
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
                        cmd_string += $" AND (TimeCall<'{time_tmp_}')";
                    }
                    else
                    {
                        cmd_string += $" AND (TimeCall>'{time_tmp_}')";
                    }
                }
                else
                {
                    if (time_arg_tmp.Count() >= 1)
                    {
                        DateTime time_tmp_;
                        if (time_arg_tmp.Count() == 1)
                        {
                            time_tmp_ = DateTime.Parse(time_arg_tmp.ElementAt(0));
                            cmd_string += $" AND (TimeCall='{time_tmp_}')";
                        }
                        else
                        {
                            cmd_string += " AND (";
                            for (short i = 0; i < time_arg_tmp.Count() - 1; i++)
                            {
                                time_tmp_ = DateTime.Parse(time_arg_tmp.ElementAt(i));
                                cmd_string += $"(TimeCall='{time_tmp_}') OR ";
                            }
                            time_tmp_ = DateTime.Parse(time_arg_tmp.ElementAt(time_arg_tmp.Count() - 1));
                            cmd_string += $"(TimeCall='{time_tmp_}'))";
                        }
                    }

                }
            }
            string count_phone;

            try
            {
                using (SqlConnection sql_c = new SqlConnection(db.Database.Connection.ConnectionString))
                {
                    sql_c.Open();
                    SqlCommand com = new SqlCommand("SELECT COUNT(Phone) FROM [BD_IFsocialforms_Number].[dbo].[PTs] WHERE " + cmd_string, sql_c);
                    count_phone = com.ExecuteScalar().ToString();
                    sql_c.Close();
                    return ulong.Parse(count_phone);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
                Response.AppendToLog(e.StackTrace);
                Response.AppendToLog(e.Message);
                return 0;
            }
        }
        //Получение дат звонков для первичной базы в соответствии с настройками
        [HttpGet]
        public JsonResult GetTimeCallPhone(string FO, string OB, string GOR, string settings, short type_select, string mass_time, byte? iterval, bool? invers, int page)
        {
            List<bool> lst_setting = new List<bool>();
            string cmd_string = "";
            foreach (var item in settings.Split(','))
            {
                lst_setting.Add(item == "false" ? false : true);
            }

            if (GOR != "")
            {
                cmd_string += $"(FO='{FO}') AND (OB='{OB}') AND (GOR='{GOR}')";
            }
            else if (OB != "")
            {
                cmd_string += $"(FO='{FO}') AND (OB='{OB}')";
            }
            else if (FO != "")
            {
                cmd_string += $"(FO='{FO}')";
            }

            List<string> tmp_cmd_string = new List<string>();
            if (lst_setting[4])
            {
                if (lst_setting[5]) { cmd_string += " AND ((Type='0')"; }
                else { cmd_string += " AND (Type='0')"; }
            }

            if (lst_setting[5])
            {
                if (lst_setting[4]) { cmd_string += " OR (Type='1'))"; }
                else { cmd_string += " AND (Type='1')"; }
            }

            byte count_setting = 0;
            bool prev_setting = false;
            lst_setting.ForEach(u =>
            {
                if (u) count_setting++;
            });
            if (count_setting != 0)
            {
                if (count_setting == 1) { cmd_string += " AND "; }
                if (count_setting > 1) { cmd_string += " AND ("; }
            }
            if (lst_setting[0])
            {
                cmd_string += " (Status='0') ";
                prev_setting = true;
            }

            if (lst_setting[1])
            {
                if (prev_setting)
                {
                    cmd_string += " OR (Status='занято')";
                }
                else
                {
                    cmd_string += "(Status='занято')";
                    prev_setting = true;
                }
            }

            if (lst_setting[2])
            {
                if (prev_setting)
                {
                    cmd_string += " OR (Status='нет ответа')";
                }
                else
                {
                    cmd_string += "(Status='нет ответа')";
                    prev_setting = true;
                }
            }

            if (lst_setting[3])
            {
                if (prev_setting)
                {
                    cmd_string += " OR ((Status='1') OR (Status='завершено'))";
                }
                else
                {
                    cmd_string += "((Status='1') OR (Status='завершено'))";
                    prev_setting = true;
                }
            }

            if (lst_setting[6])
            {
                if (prev_setting)
                {
                    cmd_string += " OR (Status='линия не найдена')";
                }
                else
                {
                    cmd_string += "(Status='линия не найдена')";
                    prev_setting = true;
                }
            }

            if (lst_setting[7])
            {
                if (prev_setting)
                {
                    cmd_string += " OR (Status='перезвонить')";
                }
                else
                {
                    cmd_string += "(Status='перезвонить')";
                    prev_setting = true;
                }
            }

            if (lst_setting[8])
            {
                if (prev_setting)
                {
                    cmd_string += " OR (Status='connect')";
                }
                else
                {
                    cmd_string += "(Status='connect')";
                }
            }

            if (count_setting > 1)
            {
                cmd_string += ")";
            }

            switch (type_select)
            {
                case 0:
                case 1:
                    {
                        cmd_string += $" AND (isActual={type_select})";
                    }
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
                        cmd_string += $" AND (TimeCall<'{time_tmp_}')";
                    }
                    else
                    {
                        cmd_string += $" AND (TimeCall>'{time_tmp_}')";
                    }
                }
                else
                {
                    if (time_arg_tmp.Count() >= 1)
                    {
                        DateTime time_tmp_;
                        if (time_arg_tmp.Count() == 1)
                        {
                            time_tmp_ = DateTime.Parse(time_arg_tmp.ElementAt(0));
                            cmd_string += $" AND (TimeCall='{time_tmp_}')";
                        }
                        else
                        {
                            cmd_string += " AND (";
                            for (short i = 0; i < time_arg_tmp.Count() - 1; i++)
                            {
                                time_tmp_ = DateTime.Parse(time_arg_tmp.ElementAt(i));
                                cmd_string += $"(TimeCall='{time_tmp_}') OR ";
                            }
                            time_tmp_ = DateTime.Parse(time_arg_tmp.ElementAt(time_arg_tmp.Count() - 1));
                            cmd_string += $"(TimeCall='{time_tmp_}'))";
                        }
                    }

                }
            }
            try
            {
                return Json(db.Database.SqlQuery<DateTime>("WITH data AS ( SELECT TimeCall FROM [BD_IFsocialforms_Number].[dbo].[PTs] WHERE " + cmd_string +
                                                                    ") SELECT TimeCall FROM data GROUP BY TimeCall"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
                System.Diagnostics.Debug.WriteLine(e.Data);
                System.Diagnostics.Debug.WriteLine(e.Message);
                Response.AppendToLog(e.StackTrace);
                Response.AppendToLog(e.Message);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
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
            string cmd_str_ = "SELECT TOP 10000 * FROM dbo.PTs WHERE ";
            string name_table = "";
            if (FO != "")
            {
                cmd_str_ += $"(FO='{FO}')";
                name_table = db.SetFO.First(u => u.KodFO == FO).NameFO;
                if (OB != "")
                {
                    cmd_str_ += $" AND (OB='{OB}')";
                    name_table = db.SetOB.First(u => u.KodFO == FO && u.KodOB == OB).NameOB;
                    if (GOR != "")
                    {
                        cmd_str_ += $" AND (GOR='{GOR}')";
                        name_table = db.SetGOR.First(u => u.KodFO == FO && u.KodOB == OB && u.KodGOR == GOR).NameGOR;
                    }
                }
            }
            string name_type = "(стац + моб)";
            if (lst_setting[4])
            {
                cmd_str_ += " AND (Type=0)";
                name_type = "(стац)";
            }

            if (lst_setting[5])
            {
                cmd_str_ += " AND (Type=1)";
                name_type = "(моб)";
            }
            try
            {
                if (lst_setting != null)
                {
                    List<PT> tmp_lst_PT_ = new List<PT>();
                    bool prev_status = false;

                    if (lst_setting[0])
                    {
                        cmd_str_ += " AND ((Status='0')";
                        prev_status = true;
                    }

                    if (lst_setting[1])
                    {
                        if (prev_status)
                        {
                            cmd_str_ += " OR (Status='занято')";
                        }
                        else
                        {
                            cmd_str_ += " AND ((Status='занято')";
                            prev_status = true;
                        }
                    }

                    if (lst_setting[2])
                    {
                        if (prev_status)
                        {
                            cmd_str_ += " OR (Status='нет ответа')";
                        }
                        else
                        {
                            cmd_str_ += " AND ((Status='нет ответа')";
                            prev_status = true;
                        }
                    }

                    if (lst_setting[3])
                    {
                        if (prev_status)
                        {
                            cmd_str_ += " OR (Status='1') OR (Status='завершено')";
                        }
                        else
                        {
                            cmd_str_ += " AND (((Status='1') OR (Status='завершено'))";
                            prev_status = true;
                        }
                    }

                    if (lst_setting[6])
                    {
                        if (prev_status)
                        {
                            cmd_str_ += " OR (Status='линия не найдена')";
                        }
                        else
                        {
                            cmd_str_ += " AND ((Status=линия не найдена')";
                            prev_status = true;
                        }
                    }

                    if (lst_setting[7])
                    {
                        if (prev_status)
                        {
                            cmd_str_ += " OR (Status='перезвонить')";
                        }
                        else
                        {
                            cmd_str_ += " AND ((Status='перезвонить')";
                            prev_status = true;
                        }
                    }

                    if (lst_setting[8])
                    {
                        if (prev_status)
                        {
                            cmd_str_ += " OR (Status='connect')";
                        }
                        else
                        {
                            cmd_str_ += " AND ((Status='connect')";
                        }
                    }
                    cmd_str_ += ")";

                    if (time.Count == 1)
                    {
                        cmd_str_ += $" AND (TimeCall='{DateTime.Parse(time.ElementAt(0)).ToString().Replace('.', '-').Replace(',', ' ')}')";
                    }
                    else if (time.Count > 1)
                    {
                        cmd_str_ += $" AND ((TimeCall='{DateTime.Parse(time.ElementAt(0)).ToString().Replace('.', '-').Replace(',', ' ')}')";
                        foreach (var item in time.GetRange(1, time.Count() - 1))
                        {
                            cmd_str_ += $" OR (TimeCall='{DateTime.Parse(item).ToString().Replace('.', '-').Replace(',', ' ')}')";
                        }
                        cmd_str_ += ")";
                    }
                    switch (type_select)
                    {
                        case 0:
                            cmd_str_ += " AND (isActual='False')";
                            break;
                        case 1:
                            cmd_str_ += " AND (isActual='True')";
                            break;
                        default:
                            break;
                    }

                    if (lst_setting[9])
                    {
                        cmd_str_ += " ORDER BY NEWID()";
                    }
                }
                System.Diagnostics.Debug.WriteLine($"SQL Query -> {cmd_str_}");
                tmp_lst_PT = db.Database.SqlQuery<PT>(cmd_str_).ToList();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(">>>>> " + e.Data);
                System.Diagnostics.Debug.WriteLine(">>>>> " + e.StackTrace);
                System.Diagnostics.Debug.WriteLine(">>>>> " + e.Message);
                Response.AppendToLog(e.StackTrace);
                Response.AppendToLog(e.Message);
            }

            if (tmp_lst_PT != null && tmp_lst_PT.Count() >= 1)
            {
                int count = 1;
                try
                {
                    List<string> cmd_str_mysql = new List<string>();
                    List<string> cmd_str_mssql = new List<string>();
                    for (int i = 0; i < tmp_lst_PT.Count; i++)
                    {
                        TmpClass tmp_arg = new TmpClass();
                        tmp_arg.Number = tmp_lst_PT[i].Phone;
                        tmp_arg.Status = tmp_lst_PT[i].Status;
                        if (type_load == 1)
                        {
                            cmd_str_mssql.Add("UPDATE dbo.PTs Set isActual='1' WHERE Phone='" + tmp_lst_PT[i].Phone + "'");
                        }
                        else
                        {
                            cmd_str_mssql.Add("UPDATE dbo.PTs Set isActual='0' WHERE Phone='" + tmp_lst_PT[i].Phone + "'");
                        }
                        cmd_str_mysql.Add("INSERT INTO table" + id_table + " (Id, Number, Status) VALUES ('" + (count++) + "','" + tmp_lst_PT[i].Phone + "','0')");
                    }
                    db.Database.ExecuteSqlCommand(String.Join(";", cmd_str_mssql));
                    mysql_db.Database.ExecuteSqlCommand(String.Join(";", cmd_str_mysql));
                    mysql_db.Database.ExecuteSqlCommand("UPDATE name_table SET Name=\"" + name_table + " " + name_type + ".csv\" WHERE Id=" + id_table);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(">>>>> " + e.StackTrace);
                    System.Diagnostics.Debug.WriteLine(">>>>> " + e.Data);
                    System.Diagnostics.Debug.WriteLine(">>>>> " + e.Message);
                    Response.AppendToLog(e.StackTrace);
                    Response.AppendToLog(e.Message);
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
        }

        //Функция синхронизации номеров с первичной базой
        private void SyncNumbSThread(object _sync_lst)
        {
            List<TmpClass> sync_status_lst = (List<TmpClass>)_sync_lst;
            Dictionary<string, PT> db_PT_list_number = db.Database.SqlQuery<PT>("SELECT * FROM dbo.PTs WHERE (Phone='" + (String.Join("') OR (Phone='", sync_status_lst.Select(u => u.Number))) + "')").ToDictionary(u => u.Phone, u => u);
            DateTime dateTime = DateTime.Parse(DateTime.Now.ToLongDateString());
            Queue<string> cmd_string_lst = new Queue<string>();

            for (int i = 0; i < sync_status_lst.Count(); i++)
            {
                if (db_PT_list_number.ContainsKey(sync_status_lst[i].Number))
                {
                    if (db_PT_list_number[sync_status_lst[i].Number].Status == "завершено")
                    {
                        if (sync_status_lst[i].Status != "завершено")
                        {
                            cmd_string_lst.Enqueue("UPDATE dbo.PTs SET Status='connect' WHERE Phone='" + sync_status_lst[i].Number + "'");
                        }
                        else
                        {
                            cmd_string_lst.Enqueue("UPDATE dbo.PTs SET TimeCall='" + dateTime + "' WHERE Phone='" + sync_status_lst[i].Number + "'");
                        }
                    }
                    else if (db_PT_list_number[sync_status_lst[i].Number].Status == "connect" & (sync_status_lst[i].Status == "0" || sync_status_lst[i].Status == "занято" || sync_status_lst[i].Status == "нет ответа" || sync_status_lst[i].Status == "линия не найдена" || sync_status_lst[i].Status == "перезвонить"))
                    {
                        cmd_string_lst.Enqueue("UPDATE dbo.PTs SET TimeCall='" + dateTime + "' WHERE Phone='" + sync_status_lst[i].Number + "'");
                    }
                    else
                    {
                        cmd_string_lst.Enqueue("UPDATE dbo.PTs SET Status='" + sync_status_lst[i].Status + "', TimeCall='" + dateTime + "' WHERE Phone='" + sync_status_lst[i].Number + "'");
                    }
                }
            }
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
                    System.Diagnostics.Debug.WriteLine($"Count {lst_phone.Count}");
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

                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(@"Data Source=192.168.0.254, 1433 ;Network Library=DBMSSOCN;Initial Catalog=BD_IFsocialforms_Number;User ID=sa;Password=7oDK35jqS;",
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
                            System.Diagnostics.Debug.WriteLine(e.StackTrace);
                            System.Diagnostics.Debug.WriteLine(e.Message);
                            System.Diagnostics.Debug.WriteLine(e.Source);
                            System.Diagnostics.Debug.WriteLine(e.Source);
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
        public async Task<JsonResult> SyncBlankWithDB(string FO_kod, string OB_kod, int p_id, int gor_id, int s_id = 0, int a_id = 0, int np_id = 0, int typeNP_id = 0, int ao_id = 0, int vgmr_id = 0)
        {
            try
            {
                //List<ResultModel> lst_blank = form_db.SetResultModels.Where(u => u.ProjectID == p_id).ToList();
                List<ResultModel> lst_blank = await form_db.Database.SqlQuery<ResultModel>($"SELECT * FROM dbo.ResultModels WHERE ProjectID={p_id}").ToListAsync();
                lst_blank.AsParallel().ForAll(u =>
                {
                    u.PhoneNumber = u.PhoneNumber.Remove(0, 1).Insert(0, "7");
                });
                //List<AnswerModel> lst_answer = q_db.SetAnswers.Where(u => u.QuestionID == s_id).ToList();
                //List<AnswerModel> lst_answer = await q_db.Database.SqlQuery<AnswerModel>($"SELECT * FROM dbo.AnswerModels WHERE QuestionID={s_id}").ToListAsync();
                //List<BlankModel> lst_answer_gor = form_db.SetBlankModels.Where(u => u.QuestionID == gor_id).ToList();
                List<BlankModel> lst_answer_gor = await form_db.Database.SqlQuery<BlankModel>($"SELECT * FROM dbo.BlankModels WHERE QuestionID={gor_id}").ToListAsync();
                //List<BlankModel> lst_sex = new List<BlankModel>();
                Dictionary<int, BlankModel> lst_sex = new Dictionary<int, BlankModel>();
                if (s_id != 0)
                {
                    //lst_sex = form_db.SetBlankModels.Where(u => u.QuestionID == s_id).ToList();
                    lst_sex = await form_db.Database.SqlQuery<BlankModel>($"SELECT * FROM dbo.BlankModels WHERE QuestionID={s_id}").ToDictionaryAsync(u => u.BlankID, u => u);
                }
                //List<BlankModel> lst_age = new List<BlankModel>();
                Dictionary<int, BlankModel> lst_age = new Dictionary<int, BlankModel>();
                if (a_id != 0)
                {
                    //lst_age = form_db.SetBlankModels.Where(u => u.QuestionID == a_id).ToList();
                    lst_age = await form_db.Database.SqlQuery<BlankModel>($"SELECT * FROM dbo.BlankModels WHERE QuestionID={a_id}").ToDictionaryAsync(u => u.BlankID);
                }
                //List<BlankModel> lst_np = new List<BlankModel>();
                Dictionary<int, BlankModel> lst_np = new Dictionary<int, BlankModel>();
                if (np_id != 0)
                {
                    //lst_np = form_db.SetBlankModels.Where(u => u.QuestionID == np_id).ToList();
                    lst_np = await form_db.Database.SqlQuery<BlankModel>($"SELECT * FROM dbo.BlankModels WHERE QuestionID={np_id}").ToDictionaryAsync(u => u.BlankID, u => u);
                }
                //List<BlankModel> lst_typeNP = new List<BlankModel>();
                Dictionary<int, BlankModel> lst_typeNP = new Dictionary<int, BlankModel>();
                if (typeNP_id != 0)
                {
                    //lst_typeNP = form_db.SetBlankModels.Where(u => u.QuestionID == typeNP_id).ToList();
                    lst_typeNP = await form_db.Database.SqlQuery<BlankModel>($"SELECT * FROM dbo.BlankModels WHERE QuestionID={typeNP_id}").ToDictionaryAsync(u => u.BlankID, u => u);
                }

                Dictionary<int, BlankModel> lst_AO = new Dictionary<int, BlankModel>();
                if (ao_id != 0)
                {
                    lst_AO = await form_db.Database.SqlQuery<BlankModel>($"SELECT * FROM dbo.BlankModels WHERE QuestionID={ao_id}").ToDictionaryAsync(u => u.BlankID, u => u);
                    System.Diagnostics.Debug.WriteLine($"AO -> {lst_AO.Count()}");
                }

                Dictionary<int, BlankModel> lst_VGMR = new Dictionary<int, BlankModel>();
                if (vgmr_id != 0)
                {
                    lst_VGMR = await form_db.Database.SqlQuery<BlankModel>($"SELECT * FROM dbo.BlankModels WHERE QuestionID={vgmr_id}").ToDictionaryAsync(u => u.BlankID, u => u);
                    System.Diagnostics.Debug.WriteLine($"VGMR -> {lst_VGMR.Count()}");
                }

                List<FormNumber> lst_form_numbers = new List<FormNumber>();
                string FO = FO_kod;
                string OB = OB_kod;
                List<FormNumber> not_PTs_lst = new List<FormNumber>();
                //foreach (var item in lst_blank)
                lst_blank.ForEach(item =>
                {
                    PT tmp = db.SetPTs.FirstOrDefault(u => u.Phone == item.PhoneNumber);
                    if (lst_form_numbers.FirstOrDefault(u => u.Phone == item.PhoneNumber) != null)
                    {
                        return;
                    }
                    if (tmp != null) // Номер анкеты встречается в PTs
                    {
                        FormNumber tmp_form_number = new FormNumber();
                        tmp_form_number.FO = tmp.FO;
                        tmp_form_number.OB = tmp.OB;
                        tmp_form_number.GOR = tmp.GOR;
                        tmp_form_number.Phone = tmp.Phone;
                        if (s_id != 0)
                        {
                            //tmp_form_number.Sex = lst_answer.First(u => u.Index == lst_sex.First(x => x.BlankID == item.Id).AnswerIndex).Index == 1 ? false : true;
                            tmp_form_number.Sex = lst_sex[item.Id].AnswerIndex == 1 ? false : true;
                        }
                        if (a_id != 0)
                        {
                            //tmp_form_number.Age = DateTime.Now.Year - Int32.Parse(lst_age.First(u => u.BlankID == item.Id).Text);
                            tmp_form_number.Age = DateTime.Now.Year - Int32.Parse(lst_age[item.Id].Text);
                        }
                        tmp_form_number.Type = tmp.Type;
                        if (np_id != 0)
                        {
                            //tmp_form_number.NP = lst_np.First(u => u.BlankID == item.Id).Text;
                            tmp_form_number.NP = lst_np[item.Id].Text;
                        }
                        if (typeNP_id != 0)
                        {
                            //tmp_form_number.TypeNP = lst_typeNP.First(u => u.BlankID == item.Id).AnswerIndex == 1 ? false : true;
                            tmp_form_number.TypeNP = lst_typeNP[item.Id].AnswerIndex == 1 ? false : true;
                        }
                        if (ao_id != 0)
                        {
                            tmp_form_number.AO = "AO" + lst_AO[item.Id].AnswerIndex;
                        }
                        if (vgmr_id != 0)
                        {
                            tmp_form_number.VGMR = "VGMR" + lst_VGMR[item.Id].AnswerIndex;
                        }
                        lst_form_numbers.Add(tmp_form_number);
                    }
                    else // Номер не встречается в PTs
                    {
                        FormNumber skip_tmp_form_number = new FormNumber();
                        string GOR;
                        if ((FO == "FO1" & OB == "OB18") || (FO == "FO2" & OB == "OB10")) GOR = "GOR1";
                        else
                        {
                            GOR = "GOR" + lst_answer_gor.First(u => u.BlankID == item.Id).AnswerIndex; ;
                        }
                        skip_tmp_form_number.FO = FO;
                        skip_tmp_form_number.OB = OB;
                        skip_tmp_form_number.GOR = GOR;
                        skip_tmp_form_number.Phone = item.PhoneNumber;
                        if (s_id != 0)
                        {
                            //skip_tmp_form_number.Sex = lst_answer.First(u => u.Index == lst_sex.First(x => x.BlankID == item.Id).AnswerIndex).Index == 1 ? false : true;
                            skip_tmp_form_number.Sex = lst_sex[item.Id].AnswerIndex == 1 ? false : true;
                        }
                        if (a_id != 0)
                        {
                            //skip_tmp_form_number.Age = DateTime.Now.Year - Int32.Parse(lst_age.First(u => u.BlankID == item.Id).Text);
                            skip_tmp_form_number.Age = DateTime.Now.Year - Int32.Parse(lst_age[item.Id].Text);
                        }
                        if (np_id != 0)
                        {
                            //skip_tmp_form_number.NP = lst_np.First(u => u.BlankID == item.Id).Text;
                            skip_tmp_form_number.NP = lst_np[item.Id].Text;
                        }
                        if (typeNP_id != 0)
                        {
                            //skip_tmp_form_number.TypeNP = lst_typeNP.First(u => u.BlankID == item.Id).AnswerIndex == 1 ? false : true;
                            skip_tmp_form_number.TypeNP = lst_typeNP[item.Id].AnswerIndex == 1 ? false : true;
                        }
                        if (ao_id != 0)
                        {
                            skip_tmp_form_number.AO = "AO" + lst_AO[item.Id].AnswerIndex;
                        }
                        if (vgmr_id != 0)
                        {
                            skip_tmp_form_number.VGMR = "VGMR" + lst_VGMR[item.Id].AnswerIndex;
                        }
                        skip_tmp_form_number.Type = item.PhoneNumber.CompareTo("79000000000") == -1 ? 0 : 1;
                        lst_form_numbers.Add(skip_tmp_form_number);
                    }
                });

                /*
                lst_form_numbers.ForEach(item =>
                {
                    PT tmp = db.SetPTs.FirstOrDefault(u => u.Phone == item.Phone);
                    if (tmp != null)
                    {
                        tmp.FO = item.FO;
                        tmp.OB = item.OB;
                        tmp.GOR = item.GOR;
                        tmp.Status = "завершено";
                        tmp.TimeCall = DateTime.Parse(DateTime.Now.ToShortDateString());
                        db.SaveChanges();
                    }
                    else
                    {
                        tmp = new PT();
                        tmp.FO = item.FO;
                        tmp.OB = item.OB;
                        tmp.GOR = item.GOR;
                        tmp.Phone = item.Phone;
                        tmp.Status = "завершено";
                        tmp.Type = item.Type;
                        tmp.isActual = false;
                        tmp.TimeCall = DateTime.Parse(DateTime.Now.ToShortDateString());
                        db.SetPTs.Add(tmp);
                        db.SaveChanges();
                    }
                });
                */

                //uint count_blank = 1;
                //lst_form_numbers.ForEach(u =>
                //{
                //    System.Diagnostics.Debug.WriteLine("#" + (count_blank++) + " | Phone: " + u.Phone + " | FO: " + u.FO + " | OB: " + u.OB + " | GOR: " + u.GOR + " | Age: " + u.Age + " | Sex: " + u.Sex + " | NP: " + u.NP + " | TypeNP: " + u.TypeNP);
                //});
                int count_lst_form_numbers = lst_form_numbers.Count();
                int iter = 0;
                do
                {
                    string tmp_phone = lst_form_numbers[iter].Phone;
                    FormNumber tmp = db.SetFormNumbers.FirstOrDefault(u => u.Phone == tmp_phone);
                    if (tmp != null)
                    {
                        lst_form_numbers.Remove(lst_form_numbers[iter]);
                        count_lst_form_numbers--;
                    }
                    else
                    {
                        iter++;
                    }
                } while (iter < count_lst_form_numbers);

                //db.SetFormNumbers.AddRange(lst_form_numbers);
                //db.SaveChanges();
                lst_form_numbers.ForEach(u =>
                {
                    System.Diagnostics.Debug.WriteLine($"Item -> {u.FO}/{u.OB}/{u.GOR}/{u.AO}/{u.VGMR}/{u.Phone}/{u.Sex}/{u.Age}");
                });
                
                return Json(lst_form_numbers.Count, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.StackTrace.ToString());
                System.Diagnostics.Debug.WriteLine(e.Data.ToString());
                System.Diagnostics.Debug.WriteLine(e.Message.ToString());
                Response.AppendToLog(e.StackTrace.ToString());
                Response.AppendToLog(e.Data.ToString());
                Response.AppendToLog(e.Message.ToString());
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public void FixedTables()
        {
            List<FormNumber> lst_form_number = db.SetFormNumbers.ToList();
            Dictionary<string, string> numbers_PTs = db.SetPTs.Select(u => u.Phone).ToDictionary(u => u, u => u);
            System.Diagnostics.Debug.WriteLine($"Count FormNumbers: {lst_form_number.Count}");
            System.Diagnostics.Debug.WriteLine($"Count PTs: {numbers_PTs.Count}");

            lst_form_number.ForEach(item =>
            {
                if (numbers_PTs.ContainsKey(item.Phone))
                {
                    db.Database.ExecuteSqlCommand($"UPDATE dbo.PTs SET FO='{item.FO}', OB='{item.OB}', GOR='{item.GOR}', Status='завершено' WHERE Phone='{item.Phone}'");
                }
                else
                {
                    db.Database.ExecuteSqlCommand($"INSERT INTO dbo.PTs (FO, OB, GOR, Phone, Status, Type, isActual, TimeCall) VALUES ('{item.FO}', '{item.OB}', '{item.GOR}', '{item.Phone}', 'завершено', {item.Type}, 0, CAST('2000-01-01 00:00:00.000' AS DateTime))");
                }
            });
            System.Diagnostics.Debug.WriteLine("Proccess is done");
        }

        //Выгрузка списка названий административных округо для Москва и Санкт-Петербурга
        [HttpGet]
        public async Task<JsonResult> GetAOList(string FO_kod, string OB_kod)
        {
            return Json(await db.SetAOs.Where(u => (u.KodFO == FO_kod) && (u.KodOB == OB_kod)).ToListAsync(), JsonRequestBehavior.AllowGet);
        }

        //Выгрузка списка названий внутригородских муниципальных образований
        [HttpGet]
        public async Task<JsonResult> GetVGMRList(string FO_kod, string OB_kod, string AO_kod)
        {
            return Json(await db.SetVGMRs.Where(u => u.KodFO == FO_kod && u.KodOB == OB_kod && u.KodAO == AO_kod).ToListAsync(), JsonRequestBehavior.AllowGet);
        }
    }
}
