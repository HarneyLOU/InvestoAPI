using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InvestoAPI.Web.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 7, ErrorMessage = "Password must have at least 7 characters")]
        public string Password { get; set; }
    }
}
