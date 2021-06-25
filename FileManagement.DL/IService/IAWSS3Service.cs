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
        AWSS3PutObjectResponse PostObject(IFormFile file, int type);
        Task<AWSS3PutObjectResponse> PostObjectAsync(IFormFile file, int type);
        AWSS3ObjectResponse GetObject(string fileKey);
        AWSS3ObjectResponse GetObject(string fileKey, bool byVersionId = false);
        Task<AWSS3ObjectResponse> GetObjectAsync(string fileKey);
        Task<AWSS3ObjectResponse> GetObjectAsync(string fileKey, bool byVersionId = false);
        string GeneratePreSignedURL(string fileKey, double duration);
        string GeneratePreSignedURL(string fileKey, double duration, bool byVersionId = false);
        DeleteObjectResponse DeleteObject(string fileKey, bool deletePermanent = false);
        DeleteObjectResponse DeleteObject(string fileKey);
        Task<DeleteObjectResponse> DeleteObjectAsync(string fileKey, bool deletePermanent = false);
        Task<DeleteObjectResponse> DeleteObjectAsync(string fileKey);
        DeleteObjectResponse RestoreObject(string fileKey);
        Task<DeleteObjectResponse> RestoreObjectAsync(string fileKey);
        bool isExist(string fileKey);
        bool isExistCheckByName(string fileName, int docType);
        string getContentType(string fileKey);
    }
}
