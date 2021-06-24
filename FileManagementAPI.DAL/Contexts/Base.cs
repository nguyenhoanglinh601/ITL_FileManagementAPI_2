using eTMS.API.Common.AppSetting;
using eTMS.API.Service;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Options;
using FileManagementAPI.Service;
using FileManagementAPI.Service.DBContextEx;

namespace FileManagementAPI.Service.Contexts
{
    public class Base<T> : ContextBase<T>
        where T : class, new()
    {
        public Base(IConnectionSettings settings) : base()
        {
            ConfigDataContext<eTMSDataContext>(settings.eTMSConnection);
        }
    }
}
