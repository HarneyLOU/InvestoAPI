using System;
using System.Collections.Generic;
using System.Text;

namespace InvestoAPI.Core.Entities
{
    public class TeamUser
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public int TeamId { get; set; }
        public Team Team { get; set; }
    }
}
