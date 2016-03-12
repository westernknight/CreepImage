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
            downLoadAddresses.Add(SaveData.GetTargetAddress());

            while (true)
            {

                lock (lockObject)
                {
                    if (downLoadAddresses.Count < 30 && downLoadAddresses.Count!=0)
                    {

                        string next = SearchResource.NextAddress(downLoadAddresses[downLoadAddresses.Count - 1]);
                        if (string.IsNullOrEmpty(next))
                        {
                            break;
                        }
                        downLoadAddresses.Add(next);

                    }
                }
                Thread.Sleep(1000);
            }
        }

        static void DownloadImageThread()
        {
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

                    Console.WriteLine("download: " + address);

                    int page = SaveData.currentDownloadPageIndex++;
                    List<string> allFullImageLinks = new List<string>();
                    allFullImageLinks.AddRange(SearchResource.GetAllFullImageLink(address));
                    for (int i = 0; i < allFullImageLinks.Count; i++)
                    {
                        Console.WriteLine("[" + page + "/" + SaveData.wholePageCount + "]" + allFullImageLinks[i]);
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
            string address = SaveData.GetTargetAddress();

            int wholePageCount = SearchResource.GetWholeWebsitePageCount(address);
            if (wholePageCount != -1)
            {
                SaveData.wholePageCount = wholePageCount;
                SaveData.SaveCurrentTarget(address);
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
