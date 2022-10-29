using System;
namespace RIODE.Models
{
    public class Badge
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public List<Product>? Products { get; set; }
    }
}

