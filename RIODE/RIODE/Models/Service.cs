using System;
using System.ComponentModel.DataAnnotations;

namespace RIODE.Models
{
    public class Service
    {
        public int Id { get; set; }
        [Required, StringLength(200)]
        public string Title { get; set; }
        [Required, StringLength(200)]
        public string Subtitle { get; set; }
        [Required]
        public int Order { get; set; }
    }
}

