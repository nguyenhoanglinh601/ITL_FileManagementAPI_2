using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using FileManagementAPI.Service.Models;
using FileManagement.DL.IService;
using FileManagement.DL.Services;
using FileManagementAPI.DL.DocumentFileService;
using FileManagementAPI.Service.Contexts;
using FileManagement.DL.Infrastructure.ErrorHandler;

namespace FileManagementAPI.API.Infrastructure
{
    public static class ServiceRegister
    {
        public static void Register(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IFileUploadHandler, FileUploadHandler>();

            services.AddTransient<ICsDocumentService, CsDocumentService>();
            services.AddTransient<IContextBase<CsDocument>, Base<CsDocument>>();

            services.AddTransient<IAWSS3Service, AWSS3Service>();
        }
    }
}
