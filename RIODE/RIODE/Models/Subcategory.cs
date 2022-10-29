using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RIODE.Models
{
    public class Subcategory:BaseEntity
    {
        public bool? IsDeleted { get; set; }
        public string Name { get; set; }
        public string? ImageName { get; set; }
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
        public List<Product>? Products { get; set; }
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
    }
}
