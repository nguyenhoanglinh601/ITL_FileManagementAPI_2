using Amazon;
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

        public AWSS3Service(ICsDocumentService doc)
        {
            _doc = doc;

            var client = new AmazonS3Client("AKIAW6FIGPT47A5EDE5D", "fOn0VY4MCuoU3fF0cMnlwg2bUR9UZqZw7fbxH5Hi", RegionEndpoint.USEast1);
            _client = client;
        }

        public AWSS3ObjectResponse GetObject(string fileKey)
        {
            var document = _doc.First(d => d.Id == new Guid(fileKey));
            if (document == null) return null;

            var request = new GetObjectRequest()
            {
                BucketName = "etms-test",
                Key = document.DocType + "/" + document.StorageFileName
            };

            GetObjectResponse response = _client.GetObjectAsync(request).Result;

            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                AWSS3ObjectResponse ObjResult = new AWSS3ObjectResponse();
                ObjResult.Result = response.ResponseStream;
                ObjResult.Extenstion = response.Headers["Content-Type"];
                ObjResult.FileName = document.FileName;

                return ObjResult;
            }
            else
            {
                return null;
            }
        }

        public AWSS3ObjectResponse GetObject(string fileKey, bool byVersionId = false)
        {
            if(byVersionId == false)
            {
                return GetObject(fileKey);
            }

            var document = _doc.First(d => d.Id == new Guid(fileKey));
            if (document == null) return null;

            var request = new GetObjectRequest()
            {
                BucketName = "etms-test",
                Key = document.DocType + "/" + document.StorageFileName,
                VersionId = document.StorageOriginVersionId
            };

            GetObjectResponse response = _client.GetObjectAsync(request).Result;

            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                AWSS3ObjectResponse ObjResult = new AWSS3ObjectResponse();
                ObjResult.Result = response.ResponseStream;
                ObjResult.Extenstion = response.Headers["Content-Type"];
                ObjResult.FileName = document.FileName;

                return ObjResult;
            }
            else
            {
                return null;
            }
        }

        public async Task<AWSS3ObjectResponse> GetObjectAsync(string fileKey)
        {
            var document = _doc.First(d => d.Id == new Guid(fileKey));
            if (document == null) return null;

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

                return ObjResult;
            }
            else
            {
                return null;
            }
        }

        public async Task<AWSS3ObjectResponse> GetObjectAsync(string fileKey, bool byVersionId = false)
        {
            if (byVersionId == false)
            {
                return await GetObjectAsync(fileKey);
            }

            var document = _doc.First(d => d.Id == new Guid(fileKey));
            if (document == null) return null;

            var request = new GetObjectRequest()
            {
                BucketName = "etms-test",
                Key = document.DocType + "/" + document.StorageFileName,
                VersionId = document.StorageOriginVersionId
            };

            GetObjectResponse response = await _client.GetObjectAsync(request);

            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                AWSS3ObjectResponse ObjResult = new AWSS3ObjectResponse();
                ObjResult.Result = response.ResponseStream;
                ObjResult.Extenstion = response.Headers["Content-Type"];
                ObjResult.FileName = document.FileName;

                return ObjResult;
            }
            else
            {
                return null;
            }
        }

        public AWSS3PutObjectResponse PostObject(IFormFile file, int type)
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
            var result = _client.PutObjectAsync(putRequest).Result;

            if (result.HttpStatusCode == HttpStatusCode.OK)
            {
                AWSS3PutObjectResponse putObjectResponse = new AWSS3PutObjectResponse();

                CsDocumentModel documentModel = new CsDocumentModel();
                documentModel.Id = fileKey;
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
                documentModel.StorageOriginVersionId = result.VersionId;

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

        public async Task<AWSS3PutObjectResponse> PostObjectAsync(IFormFile file, int type)
        {
            string stringType = ((DocumentType)type).ToString();

            Guid fileKey = Guid.NewGuid();
            DateTime timeNow = DateTime.Now;
            string millisecondsNow = ((long)(timeNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds).ToString();
            string fileExtension = file.FileName.Split(".").Last();
            string fileNameStorage = file.FileName.Split("." + fileExtension)[0] + "_" + millisecondsNow + "." + fileExtension;
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
                documentModel.Id = fileKey;
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
                documentModel.StorageOriginVersionId = result.VersionId;

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
            if (document == null) return null;

            GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
            {
                BucketName = "etms-test",
                Key = document.DocType + "/" + document.StorageFileName,
                Expires = DateTime.Now.AddMinutes(duration)
            };
            
            return _client.GetPreSignedURL(request);
        }

        public string GeneratePreSignedURL(string fileKey, double duration, bool byVersionId = false)
        {
            if(byVersionId == false)
            {
                return GeneratePreSignedURL(fileKey, duration);
            }
            var document = _doc.First(d => d.Id == new Guid(fileKey));
            if (document == null) return null;

            GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
            {
                BucketName = "etms-test",
                Key = document.DocType + "/" + document.StorageFileName,
                VersionId = document.StorageOriginVersionId,
                Expires = DateTime.Now.AddMinutes(duration)
            };

            return _client.GetPreSignedURL(request);
        }

        public DeleteObjectResponse DeleteObject(string fileKey, bool deletePermanent = false)
        {
            var document = _doc.First(d => d.Id == new Guid(fileKey));
            if (document == null) return null;

            if (deletePermanent == false)
            {
                return DeleteObject(fileKey);
            }

            DeleteObjectRequest request = new DeleteObjectRequest
            {
                BucketName = "etms-test",
                Key = document.DocType + "/" + document.StorageFileName,
                VersionId = document.StorageOriginVersionId
            };

            var result = _client.DeleteObjectAsync(request).Result;
            _doc.Delete(d => d.Id == document.Id, true);
            return result;
        }

        public DeleteObjectResponse DeleteObject(string fileKey)
        {
            var document = _doc.First(d => d.Id == new Guid(fileKey));
            if (document == null) return null;

            DeleteObjectRequest request = new DeleteObjectRequest
            {
                BucketName = "etms-test",
                Key = document.DocType + "/" + document.StorageFileName
            };
            var result = _client.DeleteObjectAsync(request).Result;

            document.Inactive = true;
            document.InactiveOn = DateTime.Now;
            document.StorageVersionId = result.VersionId;

            _doc.Update(document, d => d.Id == new Guid(fileKey), true);
            return result;
        }

        public async Task<DeleteObjectResponse> DeleteObjectAsync(string fileKey, bool deletePermanent = false)
        {
            var document = _doc.First(d => d.Id == new Guid(fileKey));
            if (document == null) return null;

            if (deletePermanent == false)
            {
                return await DeleteObjectAsync(fileKey);
            }

            DeleteObjectRequest request = new DeleteObjectRequest
            {
                BucketName = "etms-test",
                Key = document.DocType + "/" + document.StorageFileName,
                VersionId = document.StorageOriginVersionId
            };

            var result = await _client.DeleteObjectAsync(request);
            _doc.Delete(d => d.Id == document.Id, true);
            return result;
        }

        public async Task<DeleteObjectResponse> DeleteObjectAsync(string fileKey)
        {
            var document = _doc.First(d => d.Id == new Guid(fileKey));
            if (document == null) return null;

            DeleteObjectRequest request = new DeleteObjectRequest
            {
                BucketName = "etms-test",
                Key = document.DocType + "/" + document.StorageFileName
            };
            var result = await _client.DeleteObjectAsync(request);

            document.Inactive = true;
            document.InactiveOn = DateTime.Now;
            document.StorageVersionId = result.VersionId;

            _doc.Update(document, d => d.Id == new Guid(fileKey), true);
            return result;
        }

        public DeleteObjectResponse RestoreObject(string fileKey)
        {
            var document = _doc.First(d => d.Id == new Guid(fileKey));
            if (document == null) return null;

            string deleteMarkerVersionId = document.StorageVersionId;
            if (string.IsNullOrEmpty(deleteMarkerVersionId)) return null;

            DeleteObjectRequest request = new DeleteObjectRequest
            {
                BucketName = "etms-test",
                Key = document.DocType + "/" + document.StorageFileName,
                VersionId = document.StorageVersionId
            };

            var result = _client.DeleteObjectAsync(request).Result;

            document.Inactive = false;
            document.InactiveOn = null;
            document.StorageVersionId = null;
            _doc.Update(document, d => d.Id == new Guid(fileKey), true);

            return result;
        }

        public async Task<DeleteObjectResponse> RestoreObjectAsync(string fileKey)
        {
            var document = _doc.First(d => d.Id == new Guid(fileKey));
            if (document == null) return null;

            string deleteMarkerVersionId = document.StorageVersionId;
            if (string.IsNullOrEmpty(deleteMarkerVersionId)) return null;

            DeleteObjectRequest request = new DeleteObjectRequest
            {
                BucketName = "etms-test",
                Key = document.DocType + "/" + document.StorageFileName,
                VersionId = document.StorageVersionId
            };

            var result = await _client.DeleteObjectAsync(request);

            document.Inactive = false;
            document.InactiveOn = null;
            document.StorageVersionId = null;
            _doc.Update(document, d => d.Id == new Guid(fileKey), true);

            return result;
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
    }
}
