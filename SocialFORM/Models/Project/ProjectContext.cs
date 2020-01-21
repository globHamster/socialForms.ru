using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SocialFORM.Models.Project
{
    public class ProjectContext: DbContext
    {
        public ProjectContext() : base("DefaultConnection") { }
        public DbSet<ProjectModel> SetProjectModels { get; set; }
        public DbSet<CustomerProject> SetCustomerProjects { get; set; }
    }
}