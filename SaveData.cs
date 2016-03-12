using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Creep
{
    class SaveData
    {


        static string dataJsonConfig = "data.json";
        static FileInfo fi = null;
        static object lockObject = new object();

        public static string parentAddress = "http://www.zerochan.net/Fate%2Fstay+night";
        public static string targetAddress = "http://www.zerochan.net/Fate%2Fstay+night";
        public static int wholePageCount = 0;
        public static int currentDownloadPageIndex = 1;

        public static string GetTargetAddress()
        {

            fi = new FileInfo(dataJsonConfig);
            if (fi.Exists)
            {
                StreamReader sr = fi.OpenText();
                try
                {

                    string json = sr.ReadToEnd();
                    LitJson.JsonData data = LitJson.JsonMapper.ToObject(json);
                    parentAddress = (string)data["parentAddress"];
                    targetAddress = (string)data["targetAddress"];
                    wholePageCount = (int)data["wholePageCount"];
                    currentDownloadPageIndex = (int)data["currentDownloadPageIndex"];
                    sr.Close();

                }
                catch (Exception)
                {
                    SaveCurrentTarget(targetAddress);
                }

            }
            else
            {

                SaveCurrentTarget(targetAddress);
            }
            return targetAddress;
        }

        public static void SaveCurrentTarget(string address)
        {
            lock (lockObject)
            {

                StreamWriter sw = new StreamWriter(fi.Open(FileMode.Create));
                LitJson.JsonData data = new LitJson.JsonData();
                data["parentAddress"] = parentAddress;
                data["targetAddress"] = address;
                data["currentDownloadPageIndex"] = currentDownloadPageIndex;
                data["wholePageCount"] = wholePageCount;
                sw.Write(data.ToJson());
                sw.Close();
            }

        }
    }
}
