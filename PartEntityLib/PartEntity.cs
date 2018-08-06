using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartEntityLib
{
   public class PartEntity
    {
        public PartEntity(String path)
        {
            FileInfo fileInfo = new FileInfo(path);
            FileName = fileInfo.Name;
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(path);
            this.Version = fileVersionInfo.FileVersion;
        }
        public String FileName { get; set; } = String.Empty;

        public String Version { get; set; } = String.Empty;
    }
}
