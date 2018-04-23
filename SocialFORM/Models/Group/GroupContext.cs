using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SocialFORM.Models.Group
{
    public class GroupContext: DbContext
    {
        public GroupContext() : base("DefaultConnection") { }

        public DbSet<GroupModel> SetGroupModels { get; set; }
    }
}