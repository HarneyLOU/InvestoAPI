using AutoMapper;
using InvestoAPI.Core.Entities;
using InvestoAPI.Core.Interfaces;
using InvestoAPI.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace InvestoAPI.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TeamController : ControllerBase
    {

        private readonly ITeamService _teamService;
        private readonly IMapper _mapper;

        public TeamController(ITeamService teamService, IMapper mapper)
        {
            _teamService = teamService;
            _mapper = mapper;
        }

        // GET: api/<TeamController>
        [HttpGet]
        public IActionResult Get()
        {
            var userId = HttpContext.User.Identity.Name;
            return Ok(_mapper.Map<IList<TeamViewModel>>(_teamService.GetTeams(int.Parse(userId))));
        }

        // POST api/<TeamController>
        [HttpPost]
        public IActionResult Post([FromBody] TeamViewModel model)
        {
            var userId = HttpContext.User.Identity.Name;
            var team = _mapper.Map<Team>(model);
            team.OwnerId = int.Parse(userId);
            _teamService.AddTeam(team);
            return Ok(_mapper.Map<TeamViewModel>(team));
        }

        // PUT api/<TeamController>/5
        [HttpGet("{code}")]
        public IActionResult Join(string code)
        {
            var userId = HttpContext.User.Identity.Name;
            if (userId == null) return Unauthorized();
            return Ok(_mapper.Map<TeamViewModel>(_teamService.AddUserToTeam(int.Parse(userId), code)));

        }

        // DELETE api/<TeamController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var userId = HttpContext.User.Identity.Name;
            var team = _teamService.GetTeam(id);
            if (team == null || team.OwnerId != int.Parse(userId)) return NotFound("Wallet doesn't exist");
            _teamService.RemoveTeam(id);
            return NoContent();
        }
    }
}
