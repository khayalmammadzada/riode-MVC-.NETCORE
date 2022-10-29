using System;
using RIODE.Models;
using System.Numerics;

namespace RIODE.ViewModels
{
    public class ShopIndexVM
    {
        public IEnumerable<Product>? Products { get; set; }
        public IEnumerable<Category>? Categories { get; set; }
        public IEnumerable<Subcategory>? Subcategories { get; set; }
        public IEnumerable<Brand>? Brands { get; set; }
        public IEnumerable<Color>? Colors { get; set; }
        public IEnumerable<ProductColor>? ProductColors { get; set; }

    
    }
}

