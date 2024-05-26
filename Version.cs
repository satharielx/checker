using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace checker
{
    static class Version
    {
        public static Tuple<string, bool> GetFileVersion(string filePath)
        {
            try {
                var info = FileVersionInfo.GetVersionInfo(filePath);
                string version = $"{info.FileMajorPart}.{info.FileMinorPart}.{info.FileBuildPart}.{info.FilePrivatePart}";
                return Tuple.Create(version, true);
            }
            catch(Exception e)
            {
                return Tuple.Create(e.Message, false);
            }
        }
    }
}
