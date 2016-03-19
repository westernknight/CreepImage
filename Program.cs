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

        static List<string> downLoadAddresses = new List<string>();

        static object lockObject = new object();
        static void PrepareDownLoadAddressThread()
        {
            
           
            string nowAddress = SaveData.GetTargetAddress();
            downLoadAddresses.Add(nowAddress);
            while (true)
            {

                lock (lockObject)
                {
                    if (downLoadAddresses.Count < 30 )
                    {

                        string next = SearchResource.NextAddress(nowAddress);
                        if (string.IsNullOrEmpty(next))
                        {
                            Console.WriteLine("finish!!!");
                            break;
                        }
                        nowAddress = next;
                        downLoadAddresses.Add(next);

                    }
                }
                Thread.Sleep(1000);
            }
        }
        static int threadNum = 1;
        static void DownloadImageThread()
        {
            int id = threadNum++;
            string address = "";
            while (true)
            {
                lock (lockObject)
                {
                    if (downLoadAddresses.Count != 0)
                    {
                        address = downLoadAddresses[0];
                        downLoadAddresses.RemoveAt(0);
                    }
                }
                if (address != "")
                {

                    Console.WriteLine("(" + id + ")" + "download: " + address);

                    int page = SaveData.currentDownloadPageIndex;
                    List<string> allFullImageLinks = new List<string>();
                    allFullImageLinks.AddRange(SearchResource.GetAllFullImageLink(address));
                    for (int i = 0; i < allFullImageLinks.Count; i++)
                    {
                        Console.WriteLine("("+id+")"+"[" + page + "/" + SaveData.wholePageCount + "]" + allFullImageLinks[i]);
                        SaveImage.Save(allFullImageLinks[i], Path.GetFileName(SaveData.parentAddress));
                    }
        
                    SaveData.SaveCurrentTarget(address);
                    
                    address = "";

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

            string address = SaveData.GetTargetAddress();

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

            while (true)
            {
                Thread.Sleep(1);
            }

        }

    }
}
