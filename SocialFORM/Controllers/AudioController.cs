using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using System.IO.Compression;
using SocialFORM.Models;
using SocialFORM.Models.Project;
using System.Web.UI;
using System.Threading;

namespace SocialFORM.Controllers
{
    public class AudioController : Controller
    {

        // GET: Audio
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public int postToServerRecording()
        {
            try
            {
                var file = Request.Files[0];
                var fileName = Path.GetFileName(file.FileName);
                var fileType = file.ContentType.Split('/');
                var path = Path.Combine(Server.MapPath("~\\uploads"), fileName + '.' + fileType[1]);
                file.SaveAs(path);
                System.Diagnostics.Debug.WriteLine(file.FileName);
                System.Diagnostics.Debug.WriteLine(file.ContentType);
                System.Diagnostics.Debug.WriteLine(file.InputStream);
                return 200;
            }
            catch (Exception e)
            {
                Response.AppendToLog(e.StackTrace);
                return 403;
            }
        }

        [HttpGet]
        public int AudioFind(string audio_name)
        {
            System.Diagnostics.Debug.WriteLine(audio_name);
            try
            {
                System.Diagnostics.Debug.WriteLine(Path.Combine(Server.MapPath("~\\uploads\\"), audio_name + ".mp3").ToString());
                if (System.IO.File.Exists(Path.Combine(Server.MapPath("~\\uploads\\"), audio_name + ".mp3")))
                {
                    return 200;
                }
                else
                {
                    throw new Exception("Файл не найден");
                }
            }
            catch (Exception e)
            {
                return 404;
            }
        }

        public FileResult AudioDownloads(string audio_name)
        {
            // Путь к файлу
            string file_path = Path.Combine(Server.MapPath("~\\uploads"), audio_name + ".mp3");
            // Тип файла - content-type
            string file_type = "audio/mp3";
            // Имя файла - необязательно
            string file_name = audio_name + ".mp3";
            var file = File(file_path, file_type, file_name);
            Response.AddHeader("accept-ranges:", " bytes");
            return file;
        }

        public class InputModel
        {
            public string Name { get; set; } // имя файла
            public bool? Selected { get; set; } // выбран ли файл для загрузки
        }

        [HttpPost]
        public ActionResult AudioAllDownToZip(string fileGuid, string mimeType, string filename)
        {
            try
            {
                if (Session[fileGuid] != null)
                {
                    List<string> data = Session[fileGuid] as List<string>;
                    Session.Remove(fileGuid);  // Cleanup session data
                    int idi = 0;

                    string time = DateTime.Now.ToString("HH.mm.ss");
                    ////
                    //  Объем файлов
                    long lenghtFile = 0;
                    ////
                    //  Количество Файлов ZIP
                    int countFile = 0;
                    ////
                    //  Пути файлов ZIP
                    string pathFiles = "";
                    ////
                    //  Имена файло ZIP
                    string nameFiles = "";
                    ////
                    //  Создаем файл ZIP
                    ZipArchive zip = System.IO.Compression.ZipFile.Open(Server.MapPath("~/zipfiles/bundle_" + time + ".zip"), ZipArchiveMode.Create);
                    ////
                    //  Запускаем цикл по списку аудио-файлов
                    data.ForEach(file =>
                    {
                        ////
                        //  Добовляем к объему размер файла
                        lenghtFile += new FileInfo(Server.MapPath("~/uploads/" + file)).Length;
                        ////
                        //  Проверяем объем меньше ли требуемого объема
                        if (lenghtFile < 4294967295)
                        ////
                        //  Меньше
                        {
                            ////
                            //  Записываем текущий аудио файл в ZIP
                            zip.CreateEntryFromFile(Server.MapPath("~/uploads/" + file), file);
                            System.Diagnostics.Debug.WriteLine("ok === >>>>" + idi);
                            idi++;
                        }
                        else
                        ////
                        //  Привысил
                        {
                            ////
                            //  Закрываем предыдущий фаил ZIP
                            zip.Dispose();
                            ////
                            //  Записываем путь Файла
                            pathFiles += "~/zipfiles/bundle_" + time + ".zip#";
                            ////
                            //  Создаем новый TIME для нового файла ZIP
                            time = DateTime.Now.ToString("HH.mm.ss");
                            ////
                            //  Создаем новый ZIP
                            zip = System.IO.Compression.ZipFile.Open(Server.MapPath("~/zipfiles/bundle_" + time + ".zip"), ZipArchiveMode.Create);
                            ////
                            //  Записываем текущий аудио файл d ZIP
                            zip.CreateEntryFromFile(Server.MapPath("~/uploads/" + file), file);
                            ////
                            //  Обнуляем объем
                            lenghtFile = new FileInfo(Server.MapPath("~/uploads/" + file)).Length;
                            ////
                            //  Записываем имена файла
                            nameFiles += filename + "_Часть_" + countFile + ".zip#";
                            ////
                            //  Увеличивам количество Файлов ZIP
                            countFile++;

                            System.Diagnostics.Debug.WriteLine("ok === >>>>" + idi);
                            idi++;
                        }
                    });

                    zip.Dispose();

                    if (lenghtFile < 4294967295)
                    {
                        ////
                        //  Записываем путь Файла
                        pathFiles += "~/zipfiles/bundle_" + time + ".zip#";
                        ////
                        //  Записываем имена файла
                        nameFiles += filename + "_Часть_" + countFile + ".zip#";
                        ////
                        //  Увеличивам количество Файлов ZIP
                        countFile++;
                    }

                    System.Diagnostics.Debug.WriteLine("Complate");
                    System.Diagnostics.Debug.WriteLine("File === >>>>" + fileGuid + ".zip");

                    //return File(Server.MapPath("~/zipfiles/bundle_" + time + ".zip"), mimeType, filename);
                    return new JsonResult()
                    {
                        Data = new
                        {
                            CountFile = countFile,
                            FileGuid = pathFiles,
                            MimeType = mimeType,
                            FileName = nameFiles
                        }
                    };

                }
                else
                {
                    Exception e = new Exception();
                    Response.AppendToLog(e.StackTrace);
                    Response.AppendToLog(e.Message);
                    // Log the error if you want
                    return new EmptyResult();
                }
            }
            catch (Exception e)
            {
                Response.AppendToLog(e.StackTrace);
                Response.AppendToLog(e.Message);
                return new EmptyResult();
            }
        }

        [HttpGet]
        public FileResult Download(string fileGuid, string mimeType, string filename)
        {
            return File(Server.MapPath(fileGuid), mimeType, filename);
        }

        [HttpPost]
        public ActionResult RunReport(int id_project, int id_user, string date)
        {
            using (var stream = new MemoryStream())
            {
                ApplicationContext db = new ApplicationContext();
                ProjectContext db2 = new ProjectContext();
                object locked = new object();
                const int BufferSize = 8192;
                byte[] buffer = new byte[BufferSize];

                int count = db.SetResultModels.Where(u => u.ProjectID == id_project).Count();
                string name_project = db2.SetProjectModels.Where(u => u.Id == id_project).First().NameProject;
                Queue<InputModel> inputModelsQueue = new Queue<InputModel>();

                List<Models.Form.ResultModel> bd_tmp = db.SetResultModels.Where(u => u.ProjectID == id_project).ToList();

                if (id_user != 0)
                {
                    bd_tmp = bd_tmp.Where(u => u.UserID == id_user).ToList();
                }
                if (date != "null")
                {
                    DateTime StartDate = DateTime.Parse(date);
                    DateTime EndDate = DateTime.Parse(date).AddDays(1);
                    bd_tmp = bd_tmp.Where(u => u.Data > StartDate && u.Data < EndDate).ToList();
                }
                int idi = 0;

                ////Создание списка файлов//
                bd_tmp.AsParallel().ForAll(u =>
                {
                    lock (locked)
                    {
                        if (System.IO.File.Exists(Path.Combine(Server.MapPath("~\\uploads"), u.BlankID.ToString() + "_" + name_project + ".mp3")))
                        {
                            inputModelsQueue.Enqueue(new InputModel { Name = u.BlankID + "_" + name_project + ".mp3", Selected = true });
                            System.Diagnostics.Debug.WriteLine("i === >>>>" + u.BlankID + "  state === >>>>" + true);
                            idi++;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("i === >>>>" + u.BlankID + "  state === >>>>" + false);
                        }
                    }
                });

                //Создание ZIP архива
                List<string> filenames = inputModelsQueue.Where(m => m.Selected == true).Select(f => f.Name).ToList();

                Session[name_project] = filenames;
                return new JsonResult()
                {
                    Data = new
                    {
                        FileGuid = name_project,
                        MimeType = "application/zip",
                        FileName = name_project
                    }
                };
            }
        }

        [HttpPost]
        public void DeleteAudioRemove(string audio_name)
        {
            ApplicationContext db = new ApplicationContext();
            ProjectContext db2 = new ProjectContext();

            if (System.IO.File.Exists(Path.Combine(Server.MapPath("~\\uploads\\"), audio_name + ".mp3")))
            {
                System.IO.File.Delete(Path.Combine(Server.MapPath("~\\uploads\\"), audio_name + ".mp3"));
                string[] tmp = audio_name.Split('_');

                //System.IO.File.Move("oldfilename", "newfilename");

            }

        }
    }
}