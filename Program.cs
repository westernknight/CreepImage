using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.IO;
using System.Threading;
namespace Creep
{
    class Program
    {
        static List<string> jpgList = new List<string>();
    
        static int currentIndexPage = 0;


        static string jpgDataAdressFile = "data.json";
        static string crashInfo = "crash.json";
        static FileInfo fi = null;

        static string imageJsonConfig = "config.json";
        static string imageLink = "http://www.zerochan.net/Hatsune+Miku?p=";
        static string imagePath = "Hatsune Miku";
        static int imageStartPage = 1;
        static int imagePageCount = 2;
        static bool fullResolution = true;

        static StreamWriter crashDataWriter;
        static int restoreIndex = 0;
        static bool restoreFromCrash = false;
        static void Main(string[] args)
        {
            string[] jpgJunkFiles = Directory.GetFiles(".", "*.jpg");
            foreach (var item in jpgJunkFiles)
            {
                File.Delete(item);
            }

            fi = new FileInfo(imageJsonConfig);
            if (fi.Exists)
            {
                StreamReader sr = fi.OpenText();
                try
                {
                    
                    string json = sr.ReadToEnd();
                    LitJson.JsonData data = LitJson.JsonMapper.ToObject(json);
                    imageLink = (string)data["imageLink"];
                    imagePath = (string)data["imagePath"];
                    imageStartPage = (int)data["imageStartPage"];
                    imagePageCount = (int)data["imagePageCount"];
                    fullResolution = (bool)data["fullResolution"];
                    currentIndexPage = imageStartPage - 1;//imageStartPage start from one
                    sr.Close();
                }
                catch (Exception e)
                {
                    sr.Close();
                    Console.WriteLine(e);
                    CreateJsonConfig();
                }
                
            }
            else
            {

                CreateJsonConfig();
            }
     
            fi = new FileInfo(crashInfo);
            if (fi.Exists)
            {
                StreamReader sr = new StreamReader(fi.OpenRead());
                string json = sr.ReadToEnd();
                sr.Close();
                LitJson.JsonData data = LitJson.JsonMapper.ToObject(json);
                try
                {
                    restoreIndex = (int)data["restoreIndex"];
                    restoreFromCrash = true;
                    fi = new FileInfo(jpgDataAdressFile);
                    sr = new StreamReader(fi.OpenRead());
                    jpgList = LitJson.JsonMapper.ToObject<List<string>>(sr.ReadToEnd());
                    sr.Close();
                }
                catch (Exception e)
                {

                    restoreIndex = 0;
                }
            }
            else
            {
                FileStream fs =  fi.Create();
                fs.Close();                
            }
            

 
            WebClient webClient = new WebClient();

            //若程序不需要恢复，则搜索
            if (!restoreFromCrash)
            {
                if (imagePageCount == -1)
                {
                    string result = webClient.DownloadString(imageLink + currentIndexPage);
                    imagePageCount = GetWholeWebsitePageCount(result);
                    Console.WriteLine("download " + imagePageCount + " pages.");
                }
                else
                {
                    Console.WriteLine("download " + imagePageCount + " pages.");
                }
                //搜索所有图册路径
                while (currentIndexPage < imagePageCount+(imageStartPage-1) )
                {
              
                    try
                    {
                        string result = webClient.DownloadString(imageLink + currentIndexPage + 1);
                        string getSrc = result;


                        while (getSrc.IndexOf("src=\"") != -1)
                        {
                            getSrc = getSrc.Substring(getSrc.IndexOf("src=\""));
                            string getItem = getSrc.Substring(("src=\"").Length);
                            getSrc = getItem;

                            getItem = getItem.Substring(0, getItem.IndexOf("\""));

                            if (getItem.Contains(".jpg"))
                            {
                                //to do
                                if (fullResolution)
                                {
                                    jpgList.Add(getItem.Replace("240", "full"));
                                }
                                else
                                {
                                    jpgList.Add(getItem);
                                }


                            }
                        }
                        currentIndexPage++;
                    }
                    catch (Exception e )
                    {
                        Console.WriteLine(imageLink + (currentIndexPage + 1));
                        Console.WriteLine(e);
                        Thread.Sleep(1000);
                    }
                    

                    
                }

                //记录所有图册路径到json
                {

                    fi = new FileInfo(jpgDataAdressFile);
                    StreamWriter sw = new StreamWriter(fi.Open(FileMode.Create));
                    sw.Write(LitJson.JsonMapper.ToJson(jpgList));
                    sw.Close();
                }
            }
            else
            {
                Console.WriteLine("download " + imagePageCount + " pages.");
            }
            

       
            if (!Directory.Exists("image"))
            {
                Directory.CreateDirectory("image");
            }
            if (!Directory.Exists("image/" + imagePath))
            {
                Directory.CreateDirectory("image/" + imagePath);
            }
            Console.WriteLine(jpgList.Count + " files. " + (jpgList.Count - restoreIndex)+" remain.");
            for (int i = restoreIndex; i < jpgList.Count; i++)
            {
                Console.Write(Path.GetFileName(jpgList[i]));
                webClient.DownloadFile(jpgList[i], Path.GetFileName(jpgList[i]));

                File.Copy(Path.GetFileName(jpgList[i]), "image/" + imagePath + "/" + Path.GetFileName(jpgList[i]),true);
                File.Delete(Path.GetFileName(jpgList[i]));
                Console.WriteLine("\t" + (((float)i+1) / jpgList.Count * 100) + "%");

                fi = new FileInfo(crashInfo);
                crashDataWriter = new StreamWriter(fi.OpenWrite());
                LitJson.JsonData data = new LitJson.JsonData();
                data["restoreIndex"] = i+1;
                crashDataWriter.Write(data.ToJson());
                crashDataWriter.Close();
            }




            fi = new FileInfo(crashInfo);
            if (fi.Exists)
            {
                fi.Delete();
            }
        }

        private static void CreateJsonConfig()
        {
            StreamWriter sw = new StreamWriter(fi.Open(FileMode.Create));
            LitJson.JsonData data = new LitJson.JsonData();
            data["imageLink"] = imageLink;
            data["imagePath"] = imagePath;
            data["imageStartPage"] = imageStartPage;
            data["imagePageCount"] = imagePageCount;
            data["fullResolution"] = fullResolution;
            data["note"] = "imageLink:last number of the website adress don't input,this program will increase auto to search.If imageEndPage ==-1 it means search the whole website.";
            sw.Write(data.ToJson());
            sw.Close();
        }

        static int GetWholeWebsitePageCount(string result)
        {
            string strCreep = result;

            strCreep = strCreep.Substring(strCreep.IndexOf("<p class=\"pagination\" style=\"margin-bottom: 50px; \">"));

            strCreep = strCreep.Substring(strCreep.IndexOf("page"));
            strCreep = strCreep.Substring(strCreep.IndexOf("page"), strCreep.IndexOf("<a") - strCreep.IndexOf("page"));

            string pageParse = strCreep;
            string wholePageParse = strCreep;
            pageParse = pageParse.Substring(pageParse.IndexOf(" "), pageParse.IndexOf("of") - pageParse.IndexOf(" "));
            currentIndexPage = int.Parse(pageParse);

            wholePageParse = wholePageParse.Substring(wholePageParse.IndexOf("of"));
            wholePageParse = wholePageParse.Substring(wholePageParse.IndexOf(" ") + 1);
            wholePageParse = wholePageParse.Replace(",", "");


            return int.Parse(wholePageParse);

        }
    }
}
