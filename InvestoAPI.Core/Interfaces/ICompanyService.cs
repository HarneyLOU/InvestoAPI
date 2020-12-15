using InvestoAPI.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace InvestoAPI.Core.Interfaces
{
    public interface ICompanyService
    {
        IEnumerable<Company> GetAll();
        Company Get(int id);
        Company Get(string symbol);
        string GetImage(string symbol);
        IEnumerable<Company> GetDowJones();
    }
}
