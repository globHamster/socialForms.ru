using SocialFORM.Models.DB;
using SocialFORM.Models.Number;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SocialFORM.Models
{
    public class NumberAppContext : DbContext
    {
        public NumberAppContext() : base("name=NumberConnection") { }
        public DbSet<FO> SetFO { get; set; }
        public DbSet<OB> SetOB { get; set; }
        public DbSet<GOR> SetGOR { get; set; }
        public DbSet<Diap> SetDiap { get; set; }
        public DbSet<FormNumber> SetFormNumbers { get; set; }
        public DbSet<PT> SetPTs { get; set; }
        public DbSet<AO> SetAOs { get; set; }
        public DbSet<VGMR> SetVGMRs { get; set; }
    }
}