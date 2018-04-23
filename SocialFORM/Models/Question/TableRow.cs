using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialFORM.Models.Question
{
    public class TableRow
    {
        public int Id { get; set; }
        public int TableID { get; set; }
        public string TableRowText { get; set; }
        public int? IndexRow { get; set; }
        public TableRow(int id_q, string text)
        {
            TableID = id_q;
            TableRowText = text;
        }
        public TableRow()
        {

        }
    }
}