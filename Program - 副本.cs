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
        static WebClient webClient = new WebClient();

        class RestoreData
        {
            public string imageLink;
            public RestoreState restoreState = RestoreState.rs_searchPage;
            public int restoreIndex = 0;

            public List<string> searchPage = new List<string>();
            /// <summary>
            /// 通过目标页找到路径
            /// </summary>
            public List<string> jpgList = new List<string>();
            /// <summary>
            /// 每张缩略图的目标页
            /// </summary>
            public List<string> targetWebSiteList = new List<string>();
        }
        //static List<string> jpgList = new List<string>();

        //static List<string> targetWebSiteList = new List<string>();


        static RestoreData restoreData = new RestoreData();


        static int currentIndexPage = 0;


        static string restoreDataFile = "data.json";


        static FileInfo fi = null;

        static string imageJsonConfig = "config.json";
        static string imageLink = "http://www.zerochan.net/Hatsune+Miku";
        static string currentAddressParam = "http://www.zerochan.net/Hatsune+Miku";
        static string imagePath = "Hatsune Miku";

        static int imagePageCount = 2;

        static StreamWriter crashDataWriter;

        enum RestoreState
        {
            rs_searchPage,
            rs_searchImageAddress,
            rs_downloadImage,
        }

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
                    currentAddressParam = imageLink;

                    imagePath = (string)data["imagePath"];

                    imagePageCount = (int)data["imagePageCount"];

                    currentIndexPage = 0;
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

            fi = new FileInfo(restoreDataFile);
            if (fi.Exists)
            {
                StreamReader sr = new StreamReader(fi.OpenRead());
                string json = sr.ReadToEnd();
                sr.Close();
                LitJson.JsonData data = LitJson.JsonMapper.ToObject(json);
                try
                {

                    fi = new FileInfo(restoreDataFile);
                    sr = new StreamReader(fi.OpenRead());

                    restoreData = LitJson.JsonMapper.ToObject<RestoreData>(sr.ReadToEnd());

                    if (restoreData.imageLink == imageLink)
                    {
                        restoreFromCrash = true;
                    }
                    else
                    {
                        restoreData.imageLink = imageLink;
                        restoreData.restoreState = RestoreState.rs_searchPage;
                        restoreData.restoreIndex = 0;
                        restoreData.jpgList.Clear();
                        restoreData.searchPage.Clear();
                        restoreData.targetWebSiteList.Clear();
                    }
                    sr.Close();
                }
                catch (Exception e)
                {

                    Console.WriteLine(e);
                }
            }
            else
            {
                restoreData.imageLink = imageLink;
                restoreData.restoreState = RestoreState.rs_searchPage;
                restoreData.restoreIndex = 0;
                restoreData.jpgList.Clear();
                restoreData.searchPage.Clear();
                restoreData.targetWebSiteList.Clear();
            }




            //若程序不需要恢复，则搜索
            if (!restoreFromCrash)
            {


                SearchAllPagesAddress();

                restoreData.restoreState = RestoreState.rs_searchImageAddress;
                restoreData.restoreIndex = 0;
                WriteCrashFile();


                //get the image thumbs from searched pages
                SearchAllFullJpgAddress();

                Console.WriteLine("开始下载");

                restoreData.restoreState = RestoreState.rs_downloadImage;
                restoreData.restoreIndex = 0;
                WriteCrashFile();

            }
            else
            {
                switch (restoreData.restoreState)
                {
                    case RestoreState.rs_searchPage:
                        Console.WriteLine("恢复查询");
                        SearchAllPagesAddress();
                        restoreData.restoreState = RestoreState.rs_searchImageAddress;
                        restoreData.restoreIndex = 0;
                        WriteCrashFile();
                        SearchAllFullJpgAddress();

                        Console.WriteLine("开始下载");

                        restoreData.restoreState = RestoreState.rs_downloadImage;
                        restoreData.restoreIndex = 0;
                        WriteCrashFile();

                        break;
                    case RestoreState.rs_searchImageAddress:
                        Console.WriteLine("恢复搜索");
                        //get the image thumbs from searched pages
                        SearchAllFullJpgAddress();
                        Console.WriteLine("开始下载");
                        restoreData.restoreState = RestoreState.rs_downloadImage;
                        restoreData.restoreIndex = 0;
                        WriteCrashFile();
                        break;
                    case RestoreState.rs_downloadImage:
                        Console.WriteLine("恢复下载");
                        break;
                    default:
                        break;
                }


            }



            if (!Directory.Exists("image"))
            {
                Directory.CreateDirectory("image");
            }
            if (!Directory.Exists("image/" + imagePath))
            {
                Directory.CreateDirectory("image/" + imagePath);
            }
            Console.WriteLine(restoreData.jpgList.Count + " files. " + (restoreData.jpgList.Count - restoreData.restoreIndex) + " remain.");

            for (int i = restoreData.restoreIndex; i < restoreData.jpgList.Count; i++)
            {
                string address = restoreData.jpgList[i];

                Console.Write(Path.GetFileName(address));
                try
                {
                    webClient.DownloadFile(address, Path.GetFileName(address));

                    File.Copy(Path.GetFileName(address), "image/" + imagePath + "/" + Path.GetFileName(address), true);
                    File.Delete(Path.GetFileName(address));
                    Console.WriteLine("\t" + (((float)i + 1) / restoreData.jpgList.Count * 100) + "%");
                    restoreData.restoreIndex = i + 1;
                    WriteCrashFile();
                }
                catch (Exception e)
                {
                    Console.WriteLine(address);
                    Console.WriteLine(e);
                }
                Thread.Sleep(1000);
            }

        }

        private static void WriteCrashFile()
        {

            fi = new FileInfo(restoreDataFile);
            StreamWriter sw = new StreamWriter(fi.Open(FileMode.Create));
            sw.Write(LitJson.JsonMapper.ToJson(restoreData));
            sw.Close();
        }

        private static void SearchAllPagesAddress()
        {
            if (imagePageCount < 1)
            {
                string result = webClient.DownloadString(GetTheRightAddress(imageLink));
                imagePageCount = GetWholeWebsitePageCount(result);
            }
            //搜索所有图册路径
            Console.WriteLine("查询网页路径");
            currentIndexPage = restoreData.restoreIndex;
            if (restoreData.searchPage.Count>0)
            {
                currentAddressParam = restoreData.searchPage[currentIndexPage];
            }
            else
            {
                restoreData.searchPage.Add(currentAddressParam);
            }
            while (currentIndexPage < imagePageCount)
            {
                try
                {
                    string result = webClient.DownloadString(currentAddressParam);

                    //and this????

                    Console.WriteLine(currentAddressParam + " " + (currentIndexPage + 1) + "/" + imagePageCount);



                    currentIndexPage++;

                    if (currentIndexPage < imagePageCount)
                    {
                        //get the next page address
                        string getSrc = result;
                        int targetIndexPrevWordCount = 50;
                        try
                        {
                            if (getSrc.IndexOf("next") > targetIndexPrevWordCount)
                            {
                                getSrc = getSrc.Substring(getSrc.IndexOf("next") - targetIndexPrevWordCount);
                                getSrc = getSrc.Substring(getSrc.IndexOf("a href=\"") + ("a href=\"").Length, getSrc.IndexOf("\" tabindex=") - (getSrc.IndexOf("a href=\"") + ("a href=\"").Length));


                                currentAddressParam = GetTheRightAddress(imageLink) + getSrc;

                                //to do
                                restoreData.searchPage.Add(currentAddressParam);
                                restoreData.restoreIndex = currentIndexPage + 1;
                                WriteCrashFile();
                            }
                            else
                            {
                                // to do the end with error
                                throw new NullReferenceException();
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("分析下一页算法错误");
                            Console.WriteLine(e);
                            Console.ReadKey();
                            return;
                        }

                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(imageLink + (currentIndexPage + 1));
                    Console.WriteLine(e);
                    currentIndexPage++;
                    Thread.Sleep(1000);

                }
            }
            restoreData.targetWebSiteList = restoreData.searchPage;

        }



        private static void SearchAllFullJpgAddress()
        {
            int startIndex = restoreData.restoreIndex;
            for (int i = startIndex; i < restoreData.targetWebSiteList.Count; i++)
            {


                Console.WriteLine("正在搜索第" + (i + 1) + "页图片(共" + restoreData.targetWebSiteList.Count + "页)" + "    [" + restoreData.targetWebSiteList[i] + "]");
                string result = webClient.DownloadString(restoreData.targetWebSiteList[i]);
                try
                {

                    string getSrc = result;
                    string getItem = "";

                    //analyse the small image list
                    List<string> smallImageList = new List<string>();
                    while (getSrc.IndexOf("<li >") != -1)
                    {
                        getSrc = getSrc.Substring(getSrc.IndexOf("<li >"));
                        getSrc = getSrc.Substring(getSrc.IndexOf("<li >") + ("<li >").Length);
                        getItem = getSrc.Remove(getSrc.IndexOf("</li>"));
                        if (getItem.Contains("src="))
                        {
                            getItem = getSrc.Substring(getItem.IndexOf("a href=\"") + ("a href=\"").Length, getItem.IndexOf("\" tabindex=") - (getItem.IndexOf("a href=\"") + ("a href=\"").Length));
                            getItem = GetTheRightAddress(imageLink) + getItem;

                            smallImageList.Add(getItem);
                        }
                        else
                        {
                            break;
                        }
                    }
                    List<string> thisPageAllJpgSrc = new List<string>();
                    for (int j = 0; j < smallImageList.Count; j++)
                    {
                        //Console.WriteLine(smallImageList[j]);   
                        //get the original image source
                        result = webClient.DownloadString(smallImageList[j]);
                        getSrc = result;

                        try
                        {
                            getSrc = getSrc.Substring(getSrc.IndexOf("fullsizeUrl = '"));
                            getSrc = getSrc.Substring(("fullsizeUrl = '").Length, getSrc.IndexOf("';") - ("fullsizeUrl = '").Length);

                            Console.WriteLine(getSrc);
                            thisPageAllJpgSrc.Add(getSrc);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            Console.WriteLine("进入缩略图页面成功，分析图片路径算法有误");
                            Console.ReadKey();
                        }



                    }

                    restoreData.jpgList.AddRange(thisPageAllJpgSrc);
                    restoreData.restoreIndex = i + 1;
                    WriteCrashFile();

                }
                catch (Exception e)
                {
                    Console.WriteLine(result);
                    Console.WriteLine(e);
                    Console.WriteLine("分析缩略图页面算法有误");
                    Console.ReadKey();
                }

            }
        }

        private static void CreateJsonConfig()
        {
            StreamWriter sw = new StreamWriter(fi.Open(FileMode.Create));
            LitJson.JsonData data = new LitJson.JsonData();
            data["imageLink"] = imageLink;
            data["imagePath"] = imagePath;
            data["imagePageCount"] = imagePageCount;

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


            wholePageParse = wholePageParse.Substring(wholePageParse.IndexOf("of"));
            wholePageParse = wholePageParse.Substring(wholePageParse.IndexOf(" ") + 1);
            wholePageParse = wholePageParse.Replace(",", "");


            return int.Parse(wholePageParse);

        }


        /// <summary>
        /// remove the  param
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        static string GetTheRightAddress(string address)
        {
            if (address.Contains("?"))
            {
                address = (address.Remove(address.IndexOf("?")));
            }
            return address;
        }
    }
}
