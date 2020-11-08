using InvestoAPI.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace InvestoAPI.Core.Interfaces
{
    public interface ICompanyService
    {
        IEnumerable<Company> GetAll();
        IEnumerable<Company> GetDowJones();
    }
}
