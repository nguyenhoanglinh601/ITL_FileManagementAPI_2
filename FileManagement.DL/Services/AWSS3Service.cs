using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.S3.Model;
using FileManagement.DL.IService;
using FileManagement.DL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FileManagement.DL.Services
{
    public enum DocumentType
    {
        Booking = 1,
        ChargeBehalf = 2,
        Maintenance = 3,
        Transit = 4,
        DeliveryRunSheet = 5,
        SOA = 6,
        RateCard = 7,
        OrderDetailTransportRequest = 8,
        Voucher = 9,
        FCLTransportRequest = 10,
        FCLVoucher = 11,
        DNTDropPointTransportRequest = 12,
        DNTVoucher = 13,
        Contract = 14,
        FCLTransportSurcharge = 15,
        Unknown = 9999
    }
    public class AWSS3Service : IAWSS3Service
    {
        public readonly AmazonS3Client _client;
        public readonly string profile = "default";
        public readonly ICsDocumentService _doc;

        //lúc này nếu cần xử dụng csDocument thì e tiêm interface vô xài ICsDocumentService doc
        public AWSS3Service(ICsDocumentService doc)
        {
            //thay where = GET
            //doc.Get(x => x.FileName == "abc");

            _doc = doc;
            var chain = new CredentialProfileStoreChain();
            AWSCredentials awsCredentials;
            if (chain.TryGetAWSCredentials(profile, out awsCredentials))
            {
                // Use awsCredentials to create an Amazon S3 service client
                var client = new AmazonS3Client(awsCredentials);
                _client = client;
            }
        }

        public async Task<AWSS3ObjectResponse> GetObjectAsync(string fileKey)
        {
            var document = _doc.First(d => d.Id == new Guid(fileKey));
                var request = new GetObjectRequest()
                {
                    BucketName = "etms-test",
                    Key = document.DocType + "/" + document.StorageFileName
                };

                GetObjectResponse response = await _client.GetObjectAsync(request);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    AWSS3ObjectResponse ObjResult = new AWSS3ObjectResponse();
                    ObjResult.Result = response.ResponseStream;
                    ObjResult.Extenstion = response.Headers["Content-Type"];
                    ObjResult.FileName = document.FileName;

                    //FileStreamResult file = new FileStreamResult(ObjResult.Result, ObjResult.Extenstion);
                    //file.FileDownloadName = document.FileName;
                    return ObjResult;
                }
                else
                {
                    return null;
                }
        }

        public async Task<AWSS3PutObjectResponse> PostObjectAsync(IFormFile file, int type)
        {
            string stringType = ((DocumentType)type).ToString();

            Guid fileKey = Guid.NewGuid();
            DateTime timeNow = DateTime.Now;
            string millisecondsNow = ((long)(timeNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds).ToString();
            string fileExtension = file.FileName.Split(".").Last();
            string fileNameStorage = file.FileName.Split("."+fileExtension)[0] + "_" + millisecondsNow + "." + fileExtension;

            var putRequest = new PutObjectRequest()
            {
                BucketName = "etms-test",
                Key = stringType + "/" + fileNameStorage,
                InputStream = file.OpenReadStream(),
                ContentType = file.ContentType,
 
            };
            var result = await _client.PutObjectAsync(putRequest);
            
            if (result.HttpStatusCode == HttpStatusCode.OK)
            {
                AWSS3PutObjectResponse putObjectResponse = new AWSS3PutObjectResponse();

                CsDocumentModel documentModel = new CsDocumentModel();
                documentModel.Id = new Guid(fileKey.ToString());
                documentModel.ReferenceObject = "";
                documentModel.DocType = stringType;
                documentModel.FileName = file.FileName;
                documentModel.FileExtension = fileExtension;
                documentModel.FileDescription = "";
                documentModel.FileCheckSum = "";
                documentModel.FileCheckSum = "";
                documentModel.UserCreated = "HoangLinh";
                documentModel.DatetimeCreated = timeNow;
                documentModel.Inactive = false;
                documentModel.BranchId = Guid.NewGuid();
                documentModel.StorageFileName = fileNameStorage;
                documentModel.StorageVersionId = result.VersionId;

                _doc.Add(documentModel, true);

                putObjectResponse.Document = documentModel;
                putObjectResponse.Response = result;
                 return putObjectResponse;
            }
            else
            {
                return null;
            }
        }

        public string GeneratePreSignedURL(string fileKey, double duration)
        {
            var document = _doc.First(d => d.Id == new Guid(fileKey));

            GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
            {
                BucketName = "etms-test",
                Key = document.DocType + "/" + document.StorageFileName,
                Expires = DateTime.Now.AddMinutes(duration)
            };
            
            return _client.GetPreSignedURL(request);
        }

        public async Task<DeleteObjectResponse> DeleteObjectAsync(string fileKey)
        {
            var document = _doc.First(d => d.Id == new Guid(fileKey));
            if (document.Inactive == true) return null;
  
            DeleteObjectRequest request = new DeleteObjectRequest
            {
                BucketName = "etms-test",
                Key = document.DocType + "/" + document.StorageFileName,
                VersionId = document.StorageVersionId
            };

            try
            {
                var result = await _client.DeleteObjectAsync(request);
                document.Inactive = true;
                document.InactiveOn = DateTime.Now;
                _doc.Update(document, d => d.Id == new Guid(fileKey), true);
                return result;
            }
            catch
            {
                return null;
            }
        }
        
        public async Task<RestoreObjectResponse> RestoreObjectAsync(string fileKey)
        {
            var document = _doc.First(d => d.Id == new Guid(fileKey));
            var restoreRequest = new RestoreObjectRequest
            {
                BucketName = "etms-test",
                Key = document.DocType + "/" + fileKey,
                //VersionId = "c7WBY.yV.9kcoX_dv_VGuwhb4sp64s0.",
                Days = 1,
            };
            RestoreObjectResponse restoreResponse = await _client.RestoreObjectAsync(restoreRequest);

            return restoreResponse;
        }

        public bool isExist(string fileKey)
        {
            CsDocumentModel document = _doc.First(d => d.Id == new Guid(fileKey));
            if (document != null)
            {
                return true;
            }
            return false;
        }

        public bool isExistCheckByName(string fileName, int docType)
        {
            string stringType = ((DocumentType)docType).ToString();
            CsDocumentModel document = _doc.First(d => d.FileName == fileName && d.DocType == stringType && d.Inactive != true);
            if (document != null)
            {
                return true;
            }
            return false;
        }

        public string getContentType(string fileKey)
        {
            var document = _doc.First(d => d.Id == new Guid(fileKey));
            if(document != null)
            {
                return document.FileExtension;
            }
            return null;
        }

        //public async Task<String> testDBConnection()
        //{
        //    var obj= _doc.First(d => d.Id == new Guid("9245fe4a-d402-451c-b9ed-9c1a04247482"));
        //    return obj.Id.ToString();
        //}
    }
}
