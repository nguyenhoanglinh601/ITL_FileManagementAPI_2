using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
namespace FileManagementAPI.DL.DocumentFileService
{
    public static class FIleHelper
    {
        public static string ToBase64String(this Bitmap bmp, ImageFormat imageFormat)
        {
            string base64String = string.Empty;
            MemoryStream memoryStream = new MemoryStream();
            bmp.Save(memoryStream, imageFormat);
            memoryStream.Position = 0;
            byte[] byteBuffer = memoryStream.ToArray();
            memoryStream.Close();
            base64String = Convert.ToBase64String(byteBuffer);
            byteBuffer = null;
            return base64String;
        }
        public static string ToBase64ImageTag(this Bitmap bmp, ImageFormat imageFormat)
        {
            string imgTag = string.Empty;
            string base64String = string.Empty;
            base64String = bmp.ToBase64String(imageFormat);
            imgTag = "data:image" + imageFormat.ToString() + "; base64,"+ base64String;
            return imgTag;
        }
    }
   
}
