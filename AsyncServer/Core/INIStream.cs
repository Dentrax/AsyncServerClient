#region License
// ====================================================
// AsyncServerClient Copyright(C) 2015-2019 Furkan Türkal
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;

namespace ServerConsole
{
   public class INIStream
    {
        public string strPath;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        public INIStream(string strINIPath)
        {
            strPath = strINIPath;
        }

        public void Write(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, this.strPath);
        }

        public void Write(string Key, string Value)
        {
            WritePrivateProfileString(Assembly.GetExecutingAssembly().GetName().Name, Key, Value, this.strPath);
        }

        public string Read(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp, 255, this.strPath);
            return temp.ToString();
        }

        public string Read(string Key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Assembly.GetExecutingAssembly().GetName().Name, Key, "", temp, 255, this.strPath);
            return temp.ToString();
        }
    }
}
