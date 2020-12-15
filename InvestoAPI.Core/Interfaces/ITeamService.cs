using InvestoAPI.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace InvestoAPI.Core.Interfaces
{
    public interface ITeamService
    {
        Team GetTeam(int teamId);
        IEnumerable<Team> GetTeams(int userId);
        Team AddTeam(Team team);
        Team AddUserToTeam(int userId, string code);
        void RemoveTeam(int teamId);
    }
}
