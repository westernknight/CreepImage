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

            webClient.DownloadFile(imageLink, Path.GetFileName(imageLink));
            File.Copy(Path.GetFileName(imageLink), "image/" + imageDirectory + "/" + Path.GetFileName(imageLink), true);
            File.Delete(Path.GetFileName(imageLink));
        }
    }
}
