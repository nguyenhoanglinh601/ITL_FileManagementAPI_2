using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using eTMS.API.Common.AppSetting;
using eTMS.IdentityServer.DL.Infrastructure.ErrorHandler;
using FileManagement.DL.IService;
using FileManagement.DL.Models;
using FileManagementAPI.Common;
using ITL.NetCore.Common.Items;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace FileManagementAPI.Controllers
{
    //[MiddlewareFilter(typeof(LocalizationMiddleware))]
    [ApiVersion("1.0")]
    //[Route("api/v{version:apiVersion}/{lang}/[controller]")]
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class FileManagementController : ControllerBase
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        //private string Root = "D:\\File\\";
        private readonly IStringLocalizer _stringLocalizer;
        private readonly IAWSS3Service _AWSS3Service;
        public FileManagementController(IMapper mapper,
            IHostingEnvironment hostingEnvironment,
            IStringLocalizer<LanguageSub> stringLocalizer,IServiceOptions options,
            IAWSS3Service AWSS3Service)
        {
            _hostingEnvironment = hostingEnvironment;
            _stringLocalizer = stringLocalizer;
            //Root = options.FileManagementOptions.URL_SAVE_FILE;
            _AWSS3Service = AWSS3Service;
        }
        /// <summary>
        /// Add IFormFileCollection
        /// </summary>
        /// <returns></returns>
        [HttpPost, DisableRequestSizeLimit]
        public async Task<IActionResult> Add(string docType,IFormFile file)
        {
            try
            {
                //var file = Request.Form.Files[0];
                if (file != null && file.Length > 0)
                {
                    //string fullPath = string.Format("{0}{1}\\{2}", Root, DocType, FileName);
                    //if (!Directory.Exists(string.Format("{0}{1}", Root, DocType)))
                    //{
                    //    Directory.CreateDirectory(string.Format("{0}{1}", Root, DocType));
                    //}
                    //using (var stream = new FileStream(fullPath, FileMode.Create))
                    //{
                    //    file.CopyTo(stream);
                    //}
                    var result = _AWSS3Service.PostObject(file, Int32.Parse(docType));
                    if(result != null) {
                        return Ok(new ResponseResult(HttpStatusCode.OK, _stringLocalizer[LanguageSub.MSG_INSERT_SUCCESS]));
                    }       
                }
                return Ok(new ResponseResult(HttpStatusCode.BadRequest, _stringLocalizer[LanguageSub.MSG_OBJECT_EMPTY]));
            }
            catch (Exception ex)
            {
                return Ok(new ResponseResult(HttpStatusCode.BadRequest, _stringLocalizer[LanguageSub.MSG_INSERT_FAIL_SYSTEM_ERROR]));
            }
        }
        /// <summary>
        /// Delete File
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public IActionResult Delete(string fileKey, bool deletePermanent)
        {
            try
            {
                //string fullPath = string.Format("{0}{1}", Root,fileName);
                //if (System.IO.File.Exists(fullPath))
                //{
                //    System.IO.File.Delete(fullPath);
                //    return Ok(new ResponseResult(HttpStatusCode.OK, _stringLocalizer[LanguageSub.MSG_DELETE_SUCCESS]));
                //}
                if (_AWSS3Service.isExist(fileKey))
                {
                    var result = _AWSS3Service.DeleteObject(fileKey, deletePermanent);
                    if(result != null)
                    {
                        return Ok(new ResponseResult(HttpStatusCode.OK, _stringLocalizer[LanguageSub.MSG_DELETE_SUCCESS]));
                    }
                    return Ok(new ResponseResult(HttpStatusCode.BadRequest, _stringLocalizer[LanguageSub.MSG_DELETE_FAIL_SYSTEM_ERROR]));
                }
                return Ok(new ResponseResult(HttpStatusCode.BadRequest, _stringLocalizer[LanguageSub.MSG_OBJECT_NOT_EXISTS]));
            }
            catch (Exception ex)
            {
                return Ok(new ResponseResult(HttpStatusCode.BadRequest, _stringLocalizer[LanguageSub.MSG_DELETE_FAIL_SYSTEM_ERROR]));
            }
        }

        [HttpGet]
        public async Task<IActionResult> LoadDataFileAsync(string fileKey, bool byVersionId)
        {
            try
            {
                //string fullPath = string.Format("{0}{1}", Root, fileName);
                //string test = "\\\\192.168.7.88\\inetpub\\eTMS-FileUpload\\FCLTransportRequest";
                //if (System.IO.File.Exists(fullPath))
                if(_AWSS3Service.isExist(fileKey))
                {
                    //var memory = new MemoryStream();
                    //using (var stream = new FileStream(fullPath, FileMode.Open))
                    //{
                    //    await stream.CopyToAsync(memory);
                    //}
                    //memory.Position = 0;
                    //return Ok(memory.ToArray());

                    var ObjResult = _AWSS3Service.GetObject(fileKey, byVersionId);
                    if (ObjResult != null) return File(ObjResult.Result, ObjResult.Extenstion, ObjResult.FileName);
                    return Ok(new ResponseResult(HttpStatusCode.BadRequest, _stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND]));
                }
            }
            catch (Exception ex)
            {
                return Ok(new ResponseResult(HttpStatusCode.BadRequest, _stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND]));
            }
            return Ok(new ResponseResult(HttpStatusCode.BadRequest, _stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND]));
        }
        [HttpGet("CheckFileName/{DocType}/{FileName}")]
        public IActionResult CheckFileName(int DocType,string FileName)
        {
            //int count = 1;
            //string fullPath = string.Format("{0}{1}\\{2}", Root, DocType, FileName);
            //string fileNameOnly = Path.GetFileNameWithoutExtension(fullPath);
            //string extension = Path.GetExtension(fullPath);
            //string path = Path.GetDirectoryName(fullPath);
            //string newFullPath = fullPath;
            //while (System.IO.File.Exists(newFullPath))
            //{
            //    string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
            //    newFullPath = Path.Combine(path, tempFileName + extension);
            //}
            //return Ok(new ResponseResult(HttpStatusCode.OK,newFullPath.Substring(string.Format("{0},{1}\\",Root,DocType).Length-1)));

            var result = _AWSS3Service.isExistCheckByName(FileName, DocType);
            if (result == true) return Ok();
            return Ok(new ResponseResult(HttpStatusCode.NotFound, _stringLocalizer[LanguageSub.MSG_OBJECT_NOT_EXISTS]));
        }
        [HttpGet("GetFullPathFile")]
        public IActionResult GetFullPathFile(string fileKey, double duration, bool byVersionId)
        {
            //return Ok(new ResponseResult(HttpStatusCode.OK, string.Format("{0}{1}\\{2}", Root, DocType, FileName)));
            var result = _AWSS3Service.GeneratePreSignedURL(fileKey, duration, byVersionId);
            if (result != null)
            {
                return Ok(result);
            }
            return Ok(new ResponseResult(HttpStatusCode.NotFound, _stringLocalizer[LanguageSub.MSG_OBJECT_NOT_EXISTS]));
        }
        private string GetContentType(string fileKey)
        {
            //var provider = new FileExtensionContentTypeProvider();
            //string contentType;
            //if (!provider.TryGetContentType(path, out contentType))
            //{
            //    contentType = "application/octet-stream";
            //}
            //return contentType;

            return _AWSS3Service.getContentType(fileKey);
        }

        [HttpGet(nameof(UserGuide))]
        //[Authorize]
        public IActionResult UserGuide()
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(),
                                    "wwwroot", "UserGuide", "eTMS USER GUIDE DOCUMENT_v02.docx");

            return PhysicalFile(file, "application/msword");
        }

        [Route("restoreObject")]
        [HttpPost]
        public async Task<IActionResult> restoreObject(string fileKey)
        {
            var result = _AWSS3Service.RestoreObject(fileKey);
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return Ok(new ResponseResult(HttpStatusCode.NotFound, _stringLocalizer[LanguageSub.MSG_OBJECT_NOT_EXISTS]));
            }
        }

        //[Route("deleteTest")]
        //[HttpPost]
        //public async Task<IActionResult> deteleTest(string fileKey, bool deletePermanent)
        //{
        //    return Ok(_AWSS3Service.DeleteObjectAsync(fileKey, deletePermanent));
        //}

        //[Route("restoreTest")]
        //[HttpPost]
        //public async Task<IActionResult> restoreTest(string fileKey)
        //{
        //    var result = _AWSS3Service.RestoreObjectAsync(fileKey);
        //    return Ok(result);
        //}
    }


}
