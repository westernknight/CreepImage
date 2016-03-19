using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Creep
{
    public class ThreadDeal
    {
        public string address;
        public bool theading = false;
    }
    /// <summary>
    /// 单线程下载任务，多线程处理任务
    /// </summary>
    class SaveData
    {


        static string dataJsonConfig = "data.json";
        static FileInfo fi = null;
        static object lockObject = new object();

        public static string parentAddress = "http://www.zerochan.net/Graf+Zeppelin+%28Kantai+Collection%29";
        public static string lastAddress;

        public static List<string> prepareToDownloadAddressesList = new List<string>();
        public static List<string> downloadedAddressesList = new List<string>();
        public static int wholePageCount = 0;

        static string GetParentAddress(string address)
        {
            if (address.Contains("?"))
            {
                address = (address.Remove(address.IndexOf("?")));
            }
            return address;
        }
        public static void ConstructData()
        {
            fi = new FileInfo(dataJsonConfig);
            if (fi.Exists)
            {
                StreamReader sr = fi.OpenText();
                string json = sr.ReadToEnd();
                LitJson.JsonData allData = LitJson.JsonMapper.ToObject(json);
                parentAddress = (string)allData["parentAddress"];
                lastAddress = (string)allData["lastAddress"];

                
                if (parentAddress == lastAddress)
                {
                    var downloadedData = allData["downloadedAddresses"];

                    if (downloadedData.IsArray)
                    {
                        for (int i = 0; i < downloadedData.Count; i++)
                        {
                            downloadedAddressesList.Add((string)downloadedData[i]);
                        }
                    }
                    var prepareData = allData["prepareData"];
                    if (prepareData.IsArray)
                    {
                        for (int i = 0; i < prepareData.Count; i++)
                        {
                            prepareToDownloadAddressesList.Add((string)prepareData[i]);
                        }
                    }
                }
            }
            else
            {
                StreamWriter sw = new StreamWriter(fi.Open(FileMode.Create));
                LitJson.JsonData allData = new LitJson.JsonData();
                allData["parentAddress"] = parentAddress;
                allData["lastAddress"] =lastAddress= parentAddress;
                allData["downloadedAddresses"] = new LitJson.JsonData();
                allData["downloadedAddresses"].SetJsonType(LitJson.JsonType.Array);
                allData["prepareData"] = new LitJson.JsonData();
                allData["prepareData"].SetJsonType(LitJson.JsonType.Array);
                sw.Write(allData.ToJson());
                sw.Close();
            }
        }
   
   
        /// <summary>
        /// 只能一个引用,有可能重复下载完成，如果是重复下载完成，就忽略
        /// </summary>
        /// <param name="address"></param>
        public static void SaveCurrentTarget(string address, List<ThreadDeal> prepareData)
        {
            lock (lockObject)
            {
                StreamWriter sw = new StreamWriter(fi.Open(FileMode.Create));
                if (!downloadedAddressesList.Contains(address))
                {
                    downloadedAddressesList.Add(address);

                    LitJson.JsonData allData = new LitJson.JsonData();
                    allData["parentAddress"] = parentAddress;
                    allData["lastAddress"] = lastAddress;                    
                    allData["downloadedAddresses"] = new LitJson.JsonData();
                    allData["downloadedAddresses"].SetJsonType(LitJson.JsonType.Array);
                    for (int i = 0; i < downloadedAddressesList.Count; i++)
                    {
                        allData["downloadedAddresses"].Add(downloadedAddressesList[i]);
                    }
                    allData["prepareData"] = new LitJson.JsonData();
                    allData["prepareData"].SetJsonType(LitJson.JsonType.Array);
                    for (int i = 0; i < prepareData.Count; i++)
                    {
                        allData["prepareData"].Add(prepareData[i].address);
                    }
                    sw.Write(allData.ToJson());
                    sw.Close();
                }
            }

        }
    }
}
