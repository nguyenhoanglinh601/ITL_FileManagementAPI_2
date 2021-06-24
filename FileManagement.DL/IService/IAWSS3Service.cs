using Amazon.Runtime;
using Amazon.S3.Model;
using FileManagement.DL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FileManagement.DL.IService
{
    public interface IAWSS3Service
    {
        Task<AWSS3PutObjectResponse> PostObjectAsync(IFormFile file, int type);
        Task<AWSS3ObjectResponse> GetObjectAsync(string fileKey);
        string GeneratePreSignedURL(string fileKey, double duration);
        Task<DeleteObjectResponse> DeleteObjectAsync(string fileKey);
        Task<RestoreObjectResponse> RestoreObjectAsync(string fileKey);
        bool isExist(string fileKey);
        bool isExistCheckByName(string fileName, int docType);
        string getContentType(string fileKey);
    }
}
