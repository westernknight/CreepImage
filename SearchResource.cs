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

        public static string NextAddress(string currentAddress)
        {
            WebClient webClient = new WebClient();
            string result = "";
            bool timeout = true;
            while (timeout)
            {
                try
                {
                    result = webClient.DownloadString(currentAddress);
                    timeout = false;
                }
                catch (Exception e)
                {
                    Console.WriteLine(currentAddress);
                    Console.WriteLine(e);
                }
            }
            
            


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
            WebClient webClient = new WebClient();
            List<string> links = new List<string>();
            List<string> smallLinks = new List<string>();
            string result = "";


            bool timeout = true;
            while (timeout)
            {
                try
                {
                    result = webClient.DownloadString(currentAddress);
                    timeout = false;
                }
                catch (Exception e)
                {
                    Console.WriteLine(currentAddress);
                    Console.WriteLine(e);
                }
            }







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
                timeout = true;
                while (timeout)
                {
                    try
                    {
                        result = webClient.DownloadString(smallLinks[i]);
                        timeout = false;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(smallLinks[i]);
                        Console.WriteLine(e);
                    }
                }
               
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
#if false
        public static int GetCurrentPage(string address)
        {
            WebClient webClient = new WebClient();
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
#endif
        public static int GetWholeWebsitePageCount(string address)
        {
            WebClient webClient = new WebClient();
            address = GetParentAddress(address);


            bool timeout = true;
            while (timeout)
            {
                try
                {
                    address = webClient.DownloadString(address);
                    timeout = false;
                }
                catch (Exception e)
                {
                    Console.WriteLine(address);
                    Console.WriteLine(e);
                }
            }
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
