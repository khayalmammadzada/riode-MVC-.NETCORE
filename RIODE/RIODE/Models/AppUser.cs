using System;
using Microsoft.AspNetCore.Identity;

namespace RIODE.Models
{
    public class AppUser:IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool? IsAdmin { get; set; }
        public List<BasketItem>? BasketItems { get; set; }
    }
}

