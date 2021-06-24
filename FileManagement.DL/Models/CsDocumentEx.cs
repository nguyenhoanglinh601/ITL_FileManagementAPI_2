using System;
using System.Collections.Generic;
using System.Text;

namespace FileManagement.DL.Models
{
    public class csDocumentEx
    {
        public List<Guid> lstDeleted { set; get; }
        public Guid masterID { set; get; }
        public string docType { set; get; }
    }
}
