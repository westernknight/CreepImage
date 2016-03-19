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
        static string GetParentAddress(string address)
        {
            if (address.Contains("?"))
            {
                address = (address.Remove(address.IndexOf("?")));
            }
            return address;
        }
        public static void ReadJsonData()
        {

        }
        public static string GetTargetAddress()
        {
            return targetAddress;
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
                    if (parentAddress != GetParentAddress(targetAddress))
                    {
                        currentDownloadPageIndex = 1;
                    }
                    sr.Close();

                }
                catch (Exception)
                {
                    StreamWriter sw = new StreamWriter(fi.Open(FileMode.Create));
                    LitJson.JsonData data = new LitJson.JsonData();
                    data["parentAddress"] = parentAddress;
                    data["targetAddress"] = targetAddress;
                    data["currentDownloadPageIndex"] = currentDownloadPageIndex;
                    data["wholePageCount"] = wholePageCount;
                    sw.Write(data.ToJson());
                    sw.Close();
                }

            }
            else
            {

                StreamWriter sw = new StreamWriter(fi.Open(FileMode.Create));
                LitJson.JsonData data = new LitJson.JsonData();
                data["parentAddress"] = parentAddress;
                data["targetAddress"] = targetAddress;
                data["currentDownloadPageIndex"] = currentDownloadPageIndex;
                data["wholePageCount"] = wholePageCount;
                sw.Write(data.ToJson());
                sw.Close();
            }
            return targetAddress;
        }
        /// <summary>
        /// 只能一个引用
        /// </summary>
        /// <param name="address"></param>
        public static void SaveCurrentTarget(string address)
        {
            lock (lockObject)
            {
                currentDownloadPageIndex++;
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
