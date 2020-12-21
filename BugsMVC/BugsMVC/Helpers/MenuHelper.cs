using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugsMVC.Helpers
{
    public static class MenuHelper
    {
        public static string GetActiveMenu(HttpRequestBase request, string urlFirstLevel)
        {
            return GetActiveMenu(request, 1, urlFirstLevel);
        }

        public static string GetActiveMenu(HttpRequestBase request, int level, string urlFirstLevel, string urlSecondLevel = "")
        {
            string result = String.Empty;
            string url = request.RawUrl.ToString();
            if (url.IndexOf("?") > -1)
            {
                url = url.Substring(0, url.IndexOf("?"));
            }

            string[] urlDecompossed = url.Split('/');
            string[] urlFirstLevelItems = urlFirstLevel.Split(',');

            switch (level)
            {
                case 1:
                    {
                        if (urlDecompossed.Contains(urlFirstLevel))
                        {
                            result = "active";
                        }
                        break;
                    }
                case 2:
                    {
                        if (urlFirstLevelItems.Contains(urlDecompossed[1].ToLower()) && urlDecompossed.Count() == 2 && String.IsNullOrEmpty(urlSecondLevel))
                        {
                            result = "active";
                        }

                        if (urlFirstLevelItems.Contains(urlDecompossed[1].ToLower()) && urlDecompossed.Count() > 2 && urlDecompossed[2].ToLower() == urlSecondLevel)
                        {
                            result = "active";
                        }

                        break;
                    }
            }

            return result;
        }        
    }
}