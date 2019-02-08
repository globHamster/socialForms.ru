using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;

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
            try
            {
                if (System.IO.File.Exists(Path.Combine(Server.MapPath("~\\uploads"), audio_name + ".wav")))
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
            string file_path = Path.Combine(Server.MapPath("~\\uploads"), audio_name + ".wav");
            // Тип файла - content-type
            string file_type = "audio/wav";
            // Имя файла - необязательно
            string file_name = audio_name + ".wav";
            return File(file_path, file_type, file_name);
        }

        public class InputModel
        {
            public string Name { get; set; } // имя файла
            public bool? Selected { get; set; } // выбран ли файл для загрузки
        }

        //[HttpPost]
        //public FileResult AudioAllDownToZip(List<InputModel> files)
        //{
        //    ////Создание списка файлов//
        //    //string path = Server.MapPath("~/uploads/");
        //    //List<string> files = new List<string>();
        //    //DirectoryInfo dir = new DirectoryInfo(path);

        //    //files.AddRange(dir.GetFiles().Select(f => f.Name));

        //    ////Создание ZIP архива
        //    //List<string> filenames = files.Where(m => m.Selected == true).Select(f => f.Name).ToList();

        //    //string filename = Guid.NewGuid().ToString() + ".zip";

        //    //MemoryStream outputMemStream = new MemoryStream();
        //    //ZipOutputStream zipStream = new ZipOutputStream(outputMemStream);

        //    //zipStream.SetLevel(3); // уровень сжатия от 0 до 9

        //    //foreach (string file in filenames)
        //    //{
        //    //    FileInfo fi = new FileInfo(Server.MapPath("~/Files/" + file));

        //    //    string entryName = ZipEntry.CleanName(fi.Name);
        //    //    ZipEntry newEntry = new ZipEntry(entryName);
        //    //    newEntry.DateTime = fi.LastWriteTime;
        //    //    newEntry.Size = fi.Length;
        //    //    zipStream.PutNextEntry(newEntry);

        //    //    byte[] buffer = new byte[4096];
        //    //    using (FileStream streamReader = System.IO.File.OpenRead(fi.FullName))
        //    //    {
        //    //        StreamUtils.Copy(streamReader, zipStream, buffer);
        //    //    }
        //    //    zipStream.CloseEntry();
        //    //}
        //    //zipStream.IsStreamOwner = false;
        //    //zipStream.Close();

        //    //outputMemStream.Position = 0;

        //    //string file_type = "application/zip";
        //    //return File(outputMemStream, file_type, filename);
        //}
    }
}