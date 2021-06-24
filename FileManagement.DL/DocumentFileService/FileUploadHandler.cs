
using eTMS.API.Common.Helper;
using FileManagement.DL.DocumentFileService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Threading.Tasks;


namespace FileManagementAPI.DL.DocumentFileService
{
    public interface IFileUploadHandler
    {
        Task<FileUpLoadInfo> HandleUpload(IFormFile file, string path);
    }

    public class FileUploadHandler : IFileUploadHandler
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public FileUploadHandler(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<FileUpLoadInfo> HandleUpload(IFormFile file, string filePath)
        {
            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string contentType = file.ContentType;
            string fileExt = Path.GetExtension(file.FileName).ToLower();
            byte[] iconData;
            string iconBase64String = "";
            string icon = "";
            if (fileExt == ".png" || fileExt == ".jpeg" || fileExt == ".jpg")
            {
                IImageFormat form;
                var outputStream = new MemoryStream();
                using (var image = Image.Load(file.OpenReadStream(), out form))
                {
                    image.Mutate(x => x
                         .Resize(18, 18)
                         .Grayscale());
                    image.Save(outputStream, form);
                    iconData = outputStream.ToArray();
                    //  iconBase64String = Convert.ToBase64String(iconData);
                    // icon = "data:image/png;base64," + iconBase64String;
                }
            }
            else
            {
                iconData = new byte[0];
            }
            switch (contentType)
            {
                case "text/plain":
                    icon = "email-full-icon.png";
                    break;
                case "application/octet-stream":
                    icon = "email-full-icon.png";
                    break;
                case "application/pdf":
                    icon = "pdf-full-icon.png";
                    break;
                case "application/vnd.ms-excel":
                    icon = "excel-full-icon.png";
                    break;
                case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                    icon = "excel-full-icon.png";
                    break;
                case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                    icon = "word-full-icon.png";
                    break;
                case "application/msword":
                    icon = "word-full-icon.png";
                    break;
                default:
                    Console.WriteLine("Default case");
                    break;
            }
            return new FileUpLoadInfo() { FileName = file.FileName, Name = file.Name, FileCheckSum = file.CreateMD5(), ContentData = file.ToBytes(), IconData = iconData, Icon = icon };
        }
    }
}
