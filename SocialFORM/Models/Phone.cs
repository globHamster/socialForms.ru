using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SocialFORM.Models
{
    public class Phone
    {
        public int Id { get; set; }
        public string Number { get; set; }
    }

    public class TestTable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TestTable(int Id, string Name)
        {
            this.Id = Id;
            this.Name = Name;
        }
    }

    [DbConfigurationType(typeof(MySql.Data.EntityFramework.MySqlEFConfiguration))]
    public class PhoneContext : DbContext
    {
        public PhoneContext() : base("conn")
        { }

        public DbSet<Phone> Phones { get; set; }
        public DbSet<TestTable> Tables { get; set; }
    }
}