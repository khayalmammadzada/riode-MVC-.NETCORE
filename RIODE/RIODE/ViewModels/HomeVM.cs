using System;
using RIODE.Models;

namespace RIODE.ViewModels
{
    public class HomeVM
    {
        public List<Slider> Sliders { get; set; }
        public List<Brand> Brands { get; set; }
        public List<Category> Categories { get; set; }
        public List<Product> Products { get; set; }
        public List<Service> Services { get; set; }
        public List<AppUser> AppUsers { get; set; }
        public List<Subcategory> Subcategories { get; set; }
        public List<ProductImage> ProductImages { get; set; }

    }
}

