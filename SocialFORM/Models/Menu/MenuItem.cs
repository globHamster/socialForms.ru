using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialFORM.Models.Menu
{
    public class MenuItem
    {
        public int Id { get; set; }
        public string Header { get; set; }  // заголовок меню
        public string Url { get; set; } // адрес ссылки
        public int? Order { get; set; }  // порядок следования пункта в подменю
        public int? ParentId { get; set; }  // ссылка на id родительского меню
        public int? RoleId { get; set; }  // ссылка на роль
        public MenuItem Parent { get; set; }    // родительское меню

        public Role Role { get; set; }      // связь с ролью
        public ICollection<MenuItem> Children { get; set; }   // дочерние пункты меню
        public MenuItem()
        {
            Children = new List<MenuItem>();
        }
    }
}