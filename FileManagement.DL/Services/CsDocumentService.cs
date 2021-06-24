using Amazon.S3.Model;
using AutoMapper;
using eTMS.API.Common.Globals;
using eTMS.IdentityServer.DL.Infrastructure.UserManager;
using FileManagement.DL.DocumentFileService;
using FileManagement.DL.IService;
using FileManagement.DL.Models;
using FileManagementAPI.Common;
using FileManagementAPI.DL.DocumentFileService;
using FileManagementAPI.Service.DBContextEx;
using FileManagementAPI.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Common.Items;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using ITL.NetCore.Connection.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FileManagement.DL.Services
{
    //mình sẽ tổ chức lớp RepositoryBase này, thì lúc này trong đây có sẵn context luôn + các hàm CRUD mặc định
    public class CsDocumentService : RepositoryBase<CsDocument, CsDocumentModel>, ICsDocumentService
    {
        private readonly string[] ACCEPTED_FILE_TYPES = new[] { ".jpg", ".jpeg", ".png", ".doc", ".docx", ".xlsx", ".xls", ".pdf" };
        private readonly IFileUploadHandler _fileHandler;
        private readonly ICurrentUser _currentUser;
        private IStringLocalizer _localizer;
        //private IAWSS3Service _AWSS3Service;

        //private IContextBase<OpsDtbtransportRequestOrderItemRoute> _opsDTBTransportRequestItemRouteContext;

        public CsDocumentService(
            IContextBase<CsDocument> repository,
            IMapper mapper,
            IFileUploadHandler fileHandler,
            ICurrentUser currentUser,
            IStringLocalizer<LanguageSub> localizer
            ) : base(repository, mapper)
        {
            _fileHandler = fileHandler;
            _currentUser = currentUser;
            _localizer = localizer;

            //VD: 
            /*var db = ((eTMSDataContext)DC);*/ //<- thì đây là datacontext e cần xài

            //_opsDTBTransportRequestItemRouteContext = new ContextBase<OpsDtbtransportRequestOrderItemRoute>(DataContext.DC);

            AddAutoValue(new DataFieldAutoValue(nameof(CsDocument.Id), dr =>
            {
                CsDocument item = dr as CsDocument;
                return item.Id == Guid.Empty ? Guid.NewGuid() : item.Id;
            }, dr => DataMode == DataModeType.AddNew));
            AddAutoValue(new DataFieldAutoValue(nameof(CsDocument.BranchId), _currentUser.WorkPlaceID, dr => DataMode == DataModeType.AddNew));
            AddAutoValue(new DataFieldAutoValue(nameof(CsDocument.UserCreated), _currentUser.UserID, dr => DataMode == DataModeType.AddNew));
            AddAutoValue(new DataFieldAutoValue(nameof(CsDocument.DatetimeCreated), DateTime.Now, dr => DataMode == DataModeType.AddNew));

            AddAutoValue(new DataFieldAutoValue(nameof(CsDocument.UserModified), _currentUser.UserID, dr => DataMode == DataModeType.Edit));
            AddAutoValue(new DataFieldAutoValue(nameof(CsDocument.DatetimeModified), DateTime.Now, dr => DataMode == DataModeType.Edit));

            AddAutoValue(new DataFieldAutoValue(nameof(CsDocument.InactiveOn), dr =>
            {
                CsDocument item = (CsDocument)dr;
                if (item.Inactive ?? false)
                    if (item.InactiveOn == null)
                        return DateTime.Now;
                    else return item.InactiveOn;
                else return null;
            }, dr => DataMode == DataModeType.AddNew || DataMode == DataModeType.Edit));
        }

        public List<CsDocumentModel> LoadDocument(string referenceObject, DocumentType docType)
        {

            return Get(t => t.DocType.Equals(Enum.GetName(typeof(DocumentType), docType))
                  && t.ReferenceObject.Equals(referenceObject)).ToList(); ;

        }
        public IQueryable<CsDocumentModel> LoadDocument(string referenceObject)
        {
            return Get(t => t.ReferenceObject.Equals(referenceObject));
        }

        public List<CsDocumentModel> LoadDocument(string referenceObject, eTMS.API.Common.Globals.DocumentType docType)
        {
            throw new NotImplementedException();
        }


        //public IQueryable<CsDocumentModel> GetDTBTransportDocument(Guid gTransportID)
        //{
        //    List<string> lstType = new List<string> {
        //        Enum.GetName(typeof(DocumentType), DocumentType.DNTDropPointTransportRequest),
        //        Enum.GetName(typeof(DocumentType), DocumentType.DNTVoucher),
        //        Enum.GetName(typeof(DocumentType), DocumentType.Voucher)
        //    };

        //    List<string> lstTransportItemID = _opsDTBTransportRequestItemRouteContext
        //                                .Get(troir => troir.TransportRequestId == gTransportID)
        //                                .Select(troir => troir.Id.ToString())
        //                                .ToList();

        //    var q = (from doc in Get()
        //             where
        //                 lstTransportItemID.Contains(doc.ReferenceObject)
        //                 && lstType.Contains(doc.DocType)
        //             select doc).ToList()
        //            .Union(
        //                LoadDocument(gTransportID.ToString())
        //                    .Where(doc => doc.DocType == lstType[0]).ToList()
        //            )
        //            .GroupBy(doc => doc.FileCheckSum)
        //            .Select(sl => sl.FirstOrDefault())
        //            .AsQueryable();

        //    return q;
        //}

        //public async Task<HttpStatusCode> UploadFileAsync(IFormFile file, int type)
        //{
        //    try
        //    {
        //        var putObjectResponse = await _AWSS3Service.postObjectAsync(file, type);
        //        CsDocumentModel csDocument = new CsDocumentModel();
        //        csDocument.Id = new Guid(putObjectResponse.request.Key);
        //        csDocument.DocType = putObjectResponse.request.Headers["Content-Type"];
        //        csDocument.BranchId = Guid.NewGuid();
        //        await AddAsync(csDocument);
        //        return putObjectResponse.response.HttpStatusCode;
        //    }
        //    catch (Exception e)
        //    {
        //        return HttpStatusCode.InternalServerError;
        //    }

        //}

        //public async Task<AWSS3ObjectResponse> DownloadFileAsync(string fileKey, int type)
        //{
        //    try
        //    {
        //        AWSS3ObjectResponse objectResponse = await _AWSS3Service.getObjectAsync(fileKey, type);
        //        return objectResponse;
        //    }
        //    catch (Exception e)
        //    {
        //        return null;
        //    }
        //}
    }
}
