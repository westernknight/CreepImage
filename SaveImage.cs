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
       static void Save(string imageLink)
        {
            string imagePath = Path.GetFileName(imageLink);

            if (!Directory.Exists("image"))
            {
                Directory.CreateDirectory("image");
            }
            if (!Directory.Exists("image/" + imagePath))
            {
                Directory.CreateDirectory("image/" + imagePath);
            }

            webClient.DownloadFile(imageLink, Path.GetFileName(imageLink));
            File.Copy(Path.GetFileName(imageLink), "image/" + imagePath + "/" + Path.GetFileName(imageLink), true);
            File.Delete(Path.GetFileName(imageLink));
        }
    }
}
