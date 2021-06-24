using AutoMapper;
using FileManagement.DL.Models;
using FileManagementAPI.Service.Models;

namespace FileManagementAPI.API.Infrastructure
{
    public class MappingProfile : Profile
    {
        public  MappingProfile()
        {
            // Add as many of these lines as you need to map your objects           
            CreateMap<CsDocument, CsDocumentModel>().ReverseMap();
        }
    }
}
