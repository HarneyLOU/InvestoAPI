using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InvestoAPI.Web.ViewModels
{
    public class AuthenticateTokenViewModel
    {
        [Required]
        public string Token { get; set; }
    }
}
