using System;
using System.ComponentModel.DataAnnotations;

namespace RIODE.ViewModels
{
    public class LoginVM
    {
        [Required, StringLength(20)]
        public string? Username { get; set; }
        [Required, StringLength(30), DataType(DataType.Password)]
        public string? Password { get; set; }
        public bool RememberMe { get; set; }
    }
}

