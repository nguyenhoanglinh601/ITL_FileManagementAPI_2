using System;
using System.Collections.Generic;

namespace FileManagementAPI.Service.Models
{
    public partial class CsDocument
    {
        public Guid Id { get; set; }
        public string ReferenceObject { get; set; }
        public string DocType { get; set; }
        public string FileName { get; set; }
        public byte[] FileData { get; set; }
        public byte[] Icon { get; set; }
        public string FileDescription { get; set; }
        public string FileCheckSum { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public Guid BranchId { get; set; }
        public string FileExtension { get; set; }
        public string StorageFileName { get; set; }
        public string StorageVersionId { get; set; }
        public string StorageOriginVersionId { get; set; }
    }
}
