using System;
using System.Collections.Generic;
using System.Text;

namespace FileManagement.DL.DocumentFileService
{
    public class FileUpLoadInfo
    {
        public string FileName { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Icon { get; set; }
        public byte[] ContentData { get; set; }
        public byte[] IconData { get; set; }
        public string FileCheckSum { get; set; }

    }
}
