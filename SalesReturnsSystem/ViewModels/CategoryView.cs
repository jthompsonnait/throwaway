﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesReturnsSystem.ViewModels
{
    public class CategoryView
    {
        public int CategoryID { get; set; }
        public string Description { get; set; }
        public bool RemoveFromViewFlag { get; set; }
    }
}
