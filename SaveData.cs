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
   
        public static string GetTargetAddress()
        {
            string targetAddress = "http://www.zerochan.net/Fate%2Fstay+night";
            fi = new FileInfo(dataJsonConfig);
            if (fi.Exists)
            {
                StreamReader sr = fi.OpenText();
                try
                {

                    string json = sr.ReadToEnd();
                    LitJson.JsonData data = LitJson.JsonMapper.ToObject(json);
                    targetAddress = (string)data["targetAddress"];                   
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
            StreamWriter sw = new StreamWriter(fi.Open(FileMode.Create));
            LitJson.JsonData data = new LitJson.JsonData();
            data["targetAddress"] = address;
            sw.Write(data.ToJson());
            sw.Close();
        }
    }
}
