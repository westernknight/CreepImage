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
        static WebClient webClient = new WebClient();
       static void Save(string address)
        {
            string imagePath = Path.GetFileName(address);

            if (!Directory.Exists("image"))
            {
                Directory.CreateDirectory("image");
            }
            if (!Directory.Exists("image/" + imagePath))
            {
                Directory.CreateDirectory("image/" + imagePath);
            }

            webClient.DownloadFile(address, Path.GetFileName(address));
            File.Copy(Path.GetFileName(address), "image/" + imagePath + "/" + Path.GetFileName(address), true);
            File.Delete(Path.GetFileName(address));
        }
    }
}
