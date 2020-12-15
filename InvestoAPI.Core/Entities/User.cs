using System;
using System.Collections.Generic;
using System.Text;

namespace InvestoAPI.Core.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }

        public ICollection<Wallet> Wallets { get; set; }
        public ICollection<TeamUser> TeamUsers { get; set; }
        public ICollection<Team> OwningTeams { get; set; }
    }
}
