using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Creep
{
    class SaveImage
    {

        public static void Save(string imageLink, string imageDirectory)
        {
            WebClient webClient = new WebClient();
        
            if (!Directory.Exists("image"))
            {
                Directory.CreateDirectory("image");
            }
            if (!Directory.Exists("image/" + imageDirectory))
            {
                Directory.CreateDirectory("image/" + imageDirectory);
            }

            bool timeout = true;
            while (timeout)
            {
                try
                {
                    webClient.DownloadFile(imageLink, Path.GetFileName(imageLink));
                    timeout = false;
                    File.Copy(Path.GetFileName(imageLink), "image/" + imageDirectory + "/" + Path.GetFileName(imageLink), true);
                    File.Delete(Path.GetFileName(imageLink));
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine(imageLink);
                    Console.WriteLine(e);

                }
            }

        }
    }
}
