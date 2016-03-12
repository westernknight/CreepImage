using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Creep
{
    class SearchResource
    {
        static WebClient webClient = new WebClient();
        public static string NextAddress(string currentAddress)
        {
            string result = webClient.DownloadString(currentAddress);


            string getSrc = result;
            int relNext = getSrc.IndexOf("rel=\"next\"");
            if (relNext!=-1)
            {
                getSrc = getSrc.Remove(relNext);
                int ahref = getSrc.LastIndexOf("href=");
                if (ahref!=-1)
                {
                    getSrc =  getSrc.Substring(ahref, getSrc.Length - ahref);

                    int firstQuotation = getSrc.IndexOf("\"");
                    int secondQuotation = getSrc.IndexOf("\"", firstQuotation+1);

                    if (firstQuotation!=-1&&secondQuotation!=-1)
                    {
                        getSrc = getSrc.Substring(firstQuotation + 1, secondQuotation - firstQuotation - 1);


                        return GetParentAddress(currentAddress) + getSrc;
                    }

                   
                }
            }
            return "";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentAddress">page address</param>
        /// <returns></returns>
        public static string[] GetAllFullImageLink(string currentAddress)
        {
            List<string> links = new List<string>();
            List<string> smallLinks = new List<string>();
            string result = webClient.DownloadString(currentAddress);

            string getSrc = result;
            while (getSrc.IndexOf("<li >") != -1)
            {
                getSrc = getSrc.Substring(getSrc.IndexOf("<li >") + ("<li >").Length);
                string pattern = @"\/[0-9]\d*";
                Match mc;
                mc = Regex.Match(getSrc, pattern);
                if (mc.Success)
                {
                    smallLinks.Add(GetParentAddress(currentAddress) + mc.Value);
                }
               
            }
            for (int i = 0; i < smallLinks.Count; i++)
            {
                result = webClient.DownloadString(smallLinks[i]);
                getSrc = result;
                getSrc = getSrc.Substring(getSrc.IndexOf("fullsizeUrl = '"));
                getSrc = getSrc.Substring(("fullsizeUrl = '").Length, getSrc.IndexOf("';") - ("fullsizeUrl = '").Length);
                links.Add(getSrc);
            }
            return links.ToArray();
        }
        static string GetParentAddress(string address)
        {
            if (address.Contains("?"))
            {
                address = (address.Remove(address.IndexOf("?")));
            }
            return address;
        }
        public static int GetCurrentPage(string address)
        {
            address = webClient.DownloadString(address);
            string pattern = @"\bpage\s\S*\sof\s\S*\b";
            Match mc;
            mc = Regex.Match(address, pattern);
            address = mc.Value;
            if (!string.IsNullOrEmpty(address))
            {
                address = address.Substring(address.IndexOf(" "), address.IndexOf("of") - address.IndexOf(" "));
                int num = 0;
                if (int.TryParse(address, out num))
                {
                    return num;
                }
            }
            
            return -1;
        }
        public static int GetWholeWebsitePageCount(string address)
        {
            address = GetParentAddress(address);
            address = webClient.DownloadString(address);
            string pattern = @"\bpage\s\S*\sof\s\S*\b";
            Match mc;
            mc = Regex.Match(address, pattern);
            address = mc.Value;
            address = address.Substring(address.LastIndexOf(" ") + 1);
            int num = 0;
            if (int.TryParse(address, out num))
            {
                return num;
            }
            return -1;
        }
    }
}
