using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BugsMVC.DAL;

namespace BugsMVC.Helpers
{
    public static class SelectListHelpers
    {
        public static BugsContext db = new BugsContext();

        public static List<SelectListItem> CargarZonas(Guid locacionID)
        {
            int i = 1;

            List<SelectListItem> items =  new List<SelectListItem>();

            foreach (var z in db.Locaciones.Where(x => x.LocacionID == locacionID))
            {
                if (z.NombreZona1 != "")
                    items.Add(new SelectListItem { Value = i++.ToString(), Text = z.NombreZona1 });
                if (z.NombreZona2 != "")
                    items.Add(new SelectListItem { Value = i++.ToString(), Text = z.NombreZona2 });
                if (z.NombreZona3 != "")
                    items.Add(new SelectListItem { Value = i++.ToString(), Text = z.NombreZona3 });
                if (z.NombreZona4 != "")
                    items.Add(new SelectListItem { Value = i++.ToString(), Text = z.NombreZona4 });
                if (z.NombreZona5 != "")
                    items.Add(new SelectListItem { Value = i++.ToString(), Text = z.NombreZona5 });
            }
         
            return items.ToList();
        }
    }

   
}