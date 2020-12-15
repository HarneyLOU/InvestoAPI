using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestoAPI.Web.ViewModels
{
    public class TeamViewModel
    {
        public int TeamId { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public int OwnerId { get; set; }

        public DateTimeOffset Created { get; set; }
    }
}
