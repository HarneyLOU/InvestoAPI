using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestoAPI.Web.ViewModels
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
    }
}
