using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileManagement.DL.Models
{
    public class AWSS3ObjectResponse
    {
        public Stream Result { get; set; }
        public string Extenstion { get; set; }
        public string FileName { get; set; }
    }
}
