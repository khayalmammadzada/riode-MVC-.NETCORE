using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RIODE.Models
{
    public class Slider
    {
        public int Id { get; set; }
        [Required (ErrorMessage ="you should add a title")]
        public string Title { get; set; }
        [Required]
        public string Subtitle { get; set; }
        public string? ImageUrl { get; set; }
        [Required]
        public int Order { get; set; }
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
    }
}

