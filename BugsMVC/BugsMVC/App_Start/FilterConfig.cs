using System.Web;
using System.Web.Mvc;

namespace BugsMVC
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());

            //Todos los métodos necesitan Autenticación, 
            //agregar [AllowAnonymous] a las acciones que no lo requieran
            filters.Add(new AuthorizeAttribute());
        }
    }
}
