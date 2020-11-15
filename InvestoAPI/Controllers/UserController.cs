using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestoAPI.Core.Interfaces;
using InvestoAPI.Core.Entities;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using InvestoAPI.Web.ViewModels;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using InvestoAPI.Core.Helpers.Exceptions;
using Microsoft.Extensions.Configuration;

namespace InvestoAPI.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IConfiguration Configuration;

            public UserController(
                IUserService userService,
                IMapper mapper,
                IConfiguration config)
            {
                _userService = userService;
                _mapper = mapper;
                Configuration = config;
            }

            [AllowAnonymous]
            [HttpPost("authenticate")]
            public IActionResult Authenticate([FromBody] AuthenticateViewModel model)
            {
                var user = _userService.Authenticate(model.Username, model.Password);

                if (user == null)
                    return BadRequest(new { message = "Username or password is incorrect" });

                return Ok(new
                {
                    Id = user.Id,
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Token = GenerateJwtToken(user)
                });
            }
        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterViewModel model)
        {
            var user = _mapper.Map<User>(model);

            try
            {
                _userService.Create(user, model.Password);
                return Ok();
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = Role.Admin)]
        [HttpGet]
        public IActionResult GetAll()
        {
            var userId = User.FindFirst(ClaimTypes.Name)?.Value;
            Console.WriteLine(userId);
            var users = _userService.GetAll();
            var model = _mapper.Map<IList<UserViewModel>>(users);
            return Ok(model);
        }

        [Authorize(Roles = Role.Admin)]
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var user = _userService.GetById(id);
            var model = _mapper.Map<UserViewModel>(user);
            return Ok(model);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UpdateViewModel model)
        {
            var user = _mapper.Map<User>(model);
            user.Id = id;

            try
            {
                _userService.Update(user, model.Password);
                return Ok();
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _userService.Delete(id);
            return Ok();
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Configuration["AppSettings:Secret"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return tokenString;
        }
    }
}
