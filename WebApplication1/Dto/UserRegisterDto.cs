﻿using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;

namespace WebApplication1.Dto
{
    public class UserRegisterDto
    {

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Full name must be between 3 and 100 characters")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [RegularExpression("^(Admin|Doctor|Patient)$", ErrorMessage = "Role must be either Admin, Doctor, or Patient")]
        public string Role { get; set; } 
    }
}
