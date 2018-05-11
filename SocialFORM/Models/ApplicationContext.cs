using SocialFORM.Models.Form;
using SocialFORM.Models.Menu;
using SocialFORM.Models.Session;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SocialFORM.Models
{
    public class ApplicationContext: DbContext
    {
        public ApplicationContext() : base("DefaultConnection"){ }
        public DbSet<MenuItem> SetMenuItems { get; set; }
        public DbSet<DataUser> SetDataUsers { get; set; }
        public DbSet<Privilege> SetPriveleges { get; set; }
        public DbSet<User> SetUser { get; set; }
        public DbSet<Role> SetRoles { get; set; }
        public DbSet<ResultModel> SetResultModels { get; set; }
        public DbSet<SessionModel> SetSession { get; set; }
        public DbSet<BlankModel> SetBlankModels { get; set; }
        public DbSet<SchoolDay> SetSchoolDay { get; set; }

    }
}