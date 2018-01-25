using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using VircaVideoCore.Dal;
using VircaVideoCore.Models;

namespace VircaVideoCore.Controllers
{
    public class UploadController : Controller
    {
        private VircaContext db = new VircaContext();
        private IHostingEnvironment _env;
        private readonly IFileProvider fileProvider;
                
        public UploadController(IHostingEnvironment env, IFileProvider fileProvider)
        {
            _env = env;
            this.fileProvider = fileProvider;
        }


        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Upload()
        {
            string order_id = HttpContext.Request?.Query["order_id"];
            if (string.IsNullOrEmpty(order_id)) order_id = "0";
            ViewBag.OrderId = "Order: " + order_id;

            IEnumerable<Virca_Orders_Video> list = IndexGetList(order_id);
            return View(list);
        }


        public IEnumerable<Virca_Orders_Video> IndexGetList(string order_id)
        {
            IEnumerable<Virca_Orders_Video> movies;
            try
            {
                using (var context = new VircaContext())
                {
                    if (order_id != "0")
                    {
                        movies = context.Virca_Orders_Video
                            .Where(item => item.No_account == order_id).ToList();
                    } else
                    {
                        movies = context.Virca_Orders_Video.ToList();
                    }
                    ViewBag.TotalFound = "Videos found: " + movies.Count();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return movies;
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            string order_num = HttpContext.Request?.Form["OrderNum"];
            if (string.IsNullOrEmpty(order_num)) order_num = "0";
            
            
            if (file == null || file.Length == 0)
                return Content("file not selected");

            try
            {
                if (file.Length > 0)
                {
                    var path = Path.Combine(
                            Directory.GetCurrentDirectory(), "UploadedFiles",
                            file.GetFilename());

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    ViewBag.Message = "File Uploaded Successfully!!";


                    using (var dbCtx = new VircaContext())
                    {
                        var data = await dbCtx.Virca_Orders_Video
                            .Where(item => item.No_account == order_num && item.Filename == file.FileName)
                            .SingleOrDefaultAsync();
                        if (data != null) //if such record exists
                        {
                            dbCtx.Virca_Orders_Video.Remove(data);
                            await dbCtx.SaveChangesAsync();
                        }

                        var newItem = new Virca_Orders_Video();
                        newItem.No_account = order_num;
                        newItem.Filename = file.FileName;
                        newItem.Date_pub = DateTime.Now;
                        newItem.Status = 0;
                        newItem.Video_id = 0;
                        dbCtx.Entry(newItem).State = EntityState.Added;
                        await dbCtx.SaveChangesAsync();
                    }
                } else
                {
                    ViewBag.Message = "File upload error.";
                }

                return RedirectToAction("Upload");
            }
            catch
            {
                ViewBag.Message = "File upload failed!!";
                return View();
            }
        }
                

        [HttpPost]
        public async Task<IActionResult> updatePost([FromBody]  ModelRequest dataRequest)
        {
            string comment = "";
            comment = dataRequest.comment;
            long idLong = dataRequest.id;
            
            try
            {
                using (var dbCtx = new VircaContext())
                {
                    var data = await dbCtx.Virca_Orders_Video.Where(item => item.Id == idLong).SingleOrDefaultAsync();
                    if (data == null)
                    {
                        return Json(new { success = false, msg = NotFound() });
                    }
                    else
                    {
                        data.Add_info = comment;
                        await dbCtx.SaveChangesAsync();
                        return Json(new { success = true, msg = "" });
                    }
                }
            }
            catch (Exception ex)
            {
                //MvcApplication.getLogger().Error(ex, "AsyncController, updatePost");
                return Json(new { success = false, msg = ex.Message });
            }
        }
        

        [HttpPost]
        public async Task<IActionResult> updateStatus([FromBody]  ModelRequest dataRequest)
        {
            byte status = dataRequest.status;
            long idLong = dataRequest.id;

            try
            {
                using (var dbCtx = new VircaContext())
                {
                    var data = await dbCtx.Virca_Orders_Video.Where(item => item.Id == idLong).SingleOrDefaultAsync();                    
                    if (data == null)
                    {
                        return Json(new { success = false, msg = "id:" + idLong + ". " + NotFound() + data.ToString() });
                    }
                    else
                    {
                        data.Status = status;
                        await dbCtx.SaveChangesAsync();
                        return Json(new { success = true, msg = "" });
                    }
                }
            }
            catch (Exception ex)
            {
                //MvcApplication.getLogger().Error(ex, "AsyncController, updateStatus");
                return Json(new { success = false, msg = ex.Message });
            }
        }

    
        private string NotFound()
        {
            return "No data found by your request.";
        }
        
               
        
        public async Task<IActionResult> Download(string filename)
        {
            string mimeType = "";
            if (filename.Contains(".h264"))
            {
                mimeType = "video/H264";
            }
            else if (filename.Contains(".mp4"))
            {
                mimeType = "video/mp4";
            }
            else if (filename.Contains(".mpg"))
            {
                mimeType = "video/mpeg";
            }
            else if (filename.Contains(".flv"))
            {
                mimeType = "video/x-flv";
            }
            else if (filename.Contains(".wmv"))
            {
                mimeType = "video/x-ms-wmv"; //video/msvideo
            }
            else if (filename.Contains(".avi"))
            {
                mimeType = "video/avi";
            }
            else if (filename.Contains(".mov"))
            {
                mimeType = "video/quicktime";
            }
            else if (filename.Contains(".mkv"))
            {
                mimeType = "video/x-matroska";
            }
            else if (filename.Contains(".asf"))
            {
                mimeType = "video/x-ms-asf";
            }
            else
            {
                mimeType = System.Net.Mime.MediaTypeNames.Application.Octet;
            }
            //https://www.mikesdotnetting.com/article/302/server-mappath-equivalent-in-asp-net-core

           
            if (filename == null)
                return Content("filename not present");

            var path = Path.Combine(
                           Directory.GetCurrentDirectory(),
                           "UploadedFiles", filename);

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, mimeType, Path.GetFileName(path));
        }


    }
}