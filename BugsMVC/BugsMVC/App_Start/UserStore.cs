using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace BugsMVC.App_Start
{    
    public class UserStore<TUser, TRole> : UserStore<TUser, TRole, string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>, IUserStore<TUser>, IUserStore<TUser, string>, IDisposable 
        where TUser : global::Microsoft.AspNet.Identity.EntityFramework.IdentityUser
        where TRole : global::Microsoft.AspNet.Identity.EntityFramework.IdentityRole
    {
        public UserStore(DbContext context): base(context)
        {
        }
    }
}