using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileManagement.DL.Models
{
    public class AWSS3PutObjectResponse
    {
        public CsDocumentModel Document { get; set; }
        public PutObjectResponse Response { get; set; }
    }
}
