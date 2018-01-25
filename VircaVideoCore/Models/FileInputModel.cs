using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace VircaVideoCore.Models
{
    public class FileInputModel
    {
        public IFormFile FileToUpload { get; set; }
    }
        //[HttpPost]
        //public async Task<IActionResult> UploadFileViaModel(FileInputModel model)
            
}
