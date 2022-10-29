using System;
namespace RIODE.Models
{
    public class ProductComment
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string Text { get; set; }
        public DateTime CreatedTime { get; set; }
        public int ProductId { get; set; }
        public string AppUserId { get; set; }
        public Product? Product { get; set; }
        public AppUser AppUser { get; set; }
    }
}

