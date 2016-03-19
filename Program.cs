using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
namespace Creep
{
    class Program
    {


        static object lockObject = new object();
        public static List<ThreadDeal> prepareToDownloadAddressesList = new List<ThreadDeal>();
        static bool finish = false;
        static List<int> runningThread = new List<int>();
        static void PrepareDownLoadAddressThread()
        {
            string nowAddress = "";
            lock (lockObject)
            {
                for (int i = 0; i < SaveData.prepareToDownloadAddressesList.Count; i++)
                {
                    ThreadDeal obj = new ThreadDeal();
                    obj.address = SaveData.prepareToDownloadAddressesList[i];
                    prepareToDownloadAddressesList.Add(obj);
                }

                if (prepareToDownloadAddressesList.Count == 0)
                {
                    nowAddress = SaveData.parentAddress;
                    ThreadDeal obj = new ThreadDeal();
                    obj.address = nowAddress;
                    prepareToDownloadAddressesList.Add(obj);
                }
                else
                {
                    nowAddress = prepareToDownloadAddressesList[prepareToDownloadAddressesList.Count - 1].address;
                }

            }
            while (true)
            {

                lock (lockObject)
                {
                    if (prepareToDownloadAddressesList.Count < 30)
                    {

                        string next = SearchResource.NextAddress(nowAddress);
                        if (string.IsNullOrEmpty(next))
                        {
                            //Console.WriteLine("finish!!!");
                            finish = true;
                            break;
                        }
                        nowAddress = next;
                        ThreadDeal obj = new ThreadDeal();
                        obj.address = next;
                        prepareToDownloadAddressesList.Add(obj);

                    }
                }
                Thread.Sleep(1000);
            }
        }
        static int threadNum = 1;
        static void DownloadImageThread()
        {

            int id = threadNum++;
            runningThread.Add(id);

            string address = "";
            ThreadDeal threadingId = null;
            while (true)
            {
                
                lock (lockObject)
                {
                    threadingId = null;
                    for (int i = 0; i < prepareToDownloadAddressesList.Count; i++)
                    {
                        if (prepareToDownloadAddressesList[i].theading == false)
                        {
                            address = prepareToDownloadAddressesList[i].address;
                            prepareToDownloadAddressesList[i].theading = true;
                            threadingId = prepareToDownloadAddressesList[i];
                            break;
                        }
                    }

                }
                if (address != "")
                {

                    Console.WriteLine("(" + id + ")" + "download: " + address);


                    List<string> allFullImageLinks = new List<string>();
                    allFullImageLinks.AddRange(SearchResource.GetAllFullImageLink(address));
                    for (int i = 0; i < allFullImageLinks.Count; i++)
                    {
                        int page = SaveData.downloadedAddressesList.Count + 1;
                        Console.WriteLine("(" + id + ")" + "[" + page + "/" + SaveData.wholePageCount + "]" + allFullImageLinks[i]);
                        SaveImage.Save(allFullImageLinks[i], Path.GetFileName(SaveData.parentAddress));
                    }
                    lock (lockObject)
                    {
                        if (threadingId==null)
                        {
                            Console.WriteLine("error!!!!!!!!      threadingId = null");
                        }
                        prepareToDownloadAddressesList.Remove(threadingId);
                        SaveData.SaveCurrentTarget(address, prepareToDownloadAddressesList);
                    }
                    

                    address = "";

                }
                else if (finish)
                {
                    Console.WriteLine("thread "+id+" terminiated.");
                    runningThread.Remove(id);
                    break;
                }
                Thread.Sleep(1);
            }
        }

        static void Main(string[] args)
        {
            string[] jpgJunkFiles = Directory.GetFiles(".", "*.jpg");
            foreach (var item in jpgJunkFiles)
            {
                File.Delete(item);
            }

            SaveData.ConstructData();

            string address = SaveData.parentAddress;

            int wholePageCount = SearchResource.GetWholeWebsitePageCount(address);
            if (wholePageCount != -1)
            {
                SaveData.wholePageCount = wholePageCount;
            }




            ThreadStart starter = delegate { PrepareDownLoadAddressThread(); };
            new Thread(starter).Start();

            for (int i = 0; i < 4; i++)
            {
                ThreadStart downloadImage = delegate { DownloadImageThread(); };
                new Thread(downloadImage).Start();
            }
            bool printFinish = false;
            while (true)
            {
                if (printFinish == false && runningThread.Count == 0)
                {
                    printFinish = true;
                    Console.WriteLine("finish.");
                }
                Thread.Sleep(1);
            }

        }

    }
}
