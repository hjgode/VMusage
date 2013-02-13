using System;

using System.Collections.Generic;
using System.Text;

using System.IO;

namespace Logging
{
    static class utils
    {
        public static string appPath
        {
            get {
                string m_appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                if (!m_appPath.EndsWith("\\"))
                    m_appPath += @"\"; 
                return m_appPath;
            }
        }
    }
    class fileLogger
    {
        string m_appPath;
        long _maxLength = 1000000;
        string _sFileName = "\\filelogger.txt";
        public string _logFile
        {
            get {
                return _sFileName;
            }
            set { 
                _sFileName = value; 
            }
        }
        string newFile
        {
            get
            {
                string sFile = m_appPath + System.Reflection.Assembly.GetExecutingAssembly().GetName() + ".log";
                if (System.IO.File.Exists(sFile))
                {
                    //test if filesize>1MB and if create a backup and start a new file
                    FileInfo fi = new System.IO.FileInfo(sFile);
                    if (fi.Length > _maxLength)
                    {
                        System.IO.File.Copy(sFile, sFile + ".bak", true);
                        System.IO.File.Delete(sFile);
                    }
                }
                _sFileName = sFile;
                return sFile;
            }
        }
        void initAppPath()
        {
            m_appPath = utils.appPath;
        }
        public fileLogger()
        {
            initAppPath();
            _sFileName = newFile;
        }
        public fileLogger(string sFileName)
        {
            initAppPath();
            _sFileName = sFileName;
        }
        public void addLog(string s)
        {
            DateTime dt = DateTime.Now;
            string sDateTime = dt.Year.ToString("0000") + dt.Month.ToString("00") + dt.Day.ToString("00") + " " + dt.Hour.ToString("00") + ":" + dt.Minute.ToString("00");
            using (StreamWriter sw = File.AppendText(_logFile))
            {
                sw.WriteLine(sDateTime + " " + s);
                sw.Flush();
            }
        }
    }
}
