﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialFORM.Models.Question
{
    public class Block
    {
        public int Id { get; set; }
        public int ProjectID { get; set; }
        public int fromQuestion { get; set; }
        public int toQuestion { get; set; }
        public int typeBlock { get; set; }
    }
}