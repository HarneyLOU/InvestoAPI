using InvestoAPI.Core.Entities;
using InvestoAPI.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InvestoAPI.Infrastructure.Services
{
    public class TeamService : ITeamService
    {
        private readonly ILogger _logger;
        private readonly ApplicationContext _context;
        private readonly IWalletService _walletService;

        public TeamService(
            ILogger<TeamService> logger,
            ApplicationContext context,
            IWalletService walletService)
        {
            _logger = logger;
            _context = context;
            _walletService = walletService;
        }

        public Team GetTeam(int teamId)
        {
            return _context.Teams.Where(t => t.TeamId == teamId).FirstOrDefault();
        }

        public IEnumerable<Team> GetTeams(int userId)
        {
            return _context.Teams.Include(m => m.Members).Where(t => t.Members.FirstOrDefault(u => u.UserId == userId) != null);
        }

        public Team AddTeam(Team team)
        {
            team.Created = DateTime.Now;
            team.Code = Guid.NewGuid();
            _context.Teams.Add(team);
            _context.SaveChanges();

            var members = _context.Teams.Include(m => m.Members).Where(t => t.TeamId == team.TeamId).SingleOrDefault().Members;
            var tu = new TeamUser() { UserId = team.OwnerId, TeamId = team.TeamId };
            members.Add(tu);
            _context.SaveChanges();

            return team; 
        }

        public Team AddUserToTeam(int userId, string code)
        {
            var team = _context.Teams.Include(m => m.Members).Where(t => t.Code.ToString() == code).SingleOrDefault();
            if (team == null) return null;
            var teamWallet = _walletService.GetTeamWallet(team.TeamId);
            if(teamWallet != null)
            {
                var members = team.Members;
                var tu = new TeamUser() { UserId = userId, TeamId = team.TeamId };
                if (members.FirstOrDefault(t => t.TeamId == tu.TeamId && t.UserId == tu.UserId) == null)
                {
                    members.Add(tu);
                    var newWallet = new Wallet()
                    {
                        Name = teamWallet.Name,
                        Description = teamWallet.Description,
                        OwnerId = userId,
                        TeamId = teamWallet.TeamId,
                        InitMoney = teamWallet.InitMoney,
                        Created = teamWallet.Created
                    };
                    _walletService.AddWallet(newWallet);
                    _context.SaveChanges();
                    return team;
                }
            }
            return null;
        }

        public void RemoveTeam(int teamId)
        {
            _context.Teams.Remove(GetTeam(teamId));
            _context.SaveChanges();
        }
    }
}
