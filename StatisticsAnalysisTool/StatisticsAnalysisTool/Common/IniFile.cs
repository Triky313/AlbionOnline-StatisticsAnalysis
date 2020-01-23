using System;
using System.Text;

namespace StatisticsAnalysisTool.Common
{
    public class IniFile
    {
        private readonly string _path;

        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public IniFile(string iniPath)
        {
            _path = iniPath;
        }

        public void WriteValue(string section, string key, string value) => WritePrivateProfileString(section, key, value, _path);

        public string ReadValue(string section, string key)
        {
            try
            {
                var temp = new StringBuilder(1500);
                GetPrivateProfileString(section, key, "", temp, 1500, _path);
                return temp.ToString();
            }
            catch (Exception)
            {
                return null;
            }

        }

        public bool SectionKeyExists(string section, string key)
        {
            var temp = new StringBuilder(1500);
            return (0 != GetPrivateProfileString(section, key, "", temp, 1500, _path));
        }
        
    }
}
