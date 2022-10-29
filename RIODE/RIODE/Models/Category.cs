using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RIODE.Models
{
    public class Category
    {
        public int Id { get; set; }
        [StringLength(40, ErrorMessage = "Name cannot be more than 40 characters"), Required]
        public string Name { get; set; }
        public string? ImageName { get; set; }
        public List<Subcategory>? Subcategories{ get; set; }
        public List<Product>? Products { get; set; }
        [NotMapped]
        public IFormFile? ImageFile { get; set; }

    }
}

