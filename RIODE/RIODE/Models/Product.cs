using System;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace RIODE.Models
{
    public class Product:BaseEntity
    {

        [Required]
        public string? Name { get; set; }
        public int? ReviewSum { get; set; } = 0;
        public int? ReviewCount { get; set; } = 0;
        [Required]
        public decimal? CostPrice { get; set; }
        [Required]
        public decimal? SellPrice { get; set; }
        public string? SubTitle { get; set; }
        [Required]
        public int? StockCount { get; set; }
        public int? BadgeId { get; set; }
        public Badge? Badge { get; set; }
        public int BrandId { get; set; }
        public Brand? Brand { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public int SubcategoryId { get; set; }
        public Subcategory? Subcategory { get; set; }




        public ICollection<ProductImage>? ProductImages { get; set; }
        public ICollection<ProductComment>? ProductComments { get; set; }
        public ICollection<ProductColor>? ProductColors { get; set; }




        [NotMapped]
        public List<int>? ImagesId { get; set; }


        [NotMapped]
        public List<int>? ColorIds { get; set; }


        [NotMapped]
        public IFormFile? MainImage { get; set; }
        [NotMapped]
        public IFormFile? HoverImage { get; set; }
        [NotMapped]
        public List<IFormFile>? Images { get; set; }

    }
}

