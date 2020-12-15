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
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly IMapper _mapper;

        public WalletController(IWalletService walletService, IMapper mapper)
        {
            _walletService = walletService;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public IActionResult GetWallet(int id)
        {
            var userId = HttpContext.User.Identity.Name;
            var walletModel = _walletService.GetWallet(id);
            if (walletModel == null) return NotFound("Wallet doesn't exist");
            if(walletModel.OwnerId == int.Parse(userId)) return Ok(_mapper.Map<WalletViewModel>(walletModel));
            else return NotFound();
        }

        [HttpGet]
        public IActionResult GetWallets()
        {
            var userId = HttpContext.User.Identity.Name;
            var walletsModel = _walletService.GetWallets(int.Parse(userId)).ToList();
            var walletsView = _mapper.Map<IList<WalletViewModel>>(walletsModel);
            return Ok(walletsView);
        }

        [HttpPost]
        public IActionResult AddWallet([FromBody] WalletViewModel model)
        {
            var userId = HttpContext.User.Identity.Name;
            if (userId == null) return NotFound();

            var wallet = _mapper.Map<Wallet>(model);
            wallet.OwnerId = int.Parse(userId);
            wallet.Created = DateTime.Now;
            wallet.TeamId = model.TeamId;
            _walletService.AddWallet(wallet);
            return CreatedAtAction(nameof(GetWallet), new { id = wallet.WalletId }, _mapper.Map<WalletViewModel>(wallet));
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteWallet(int id)
        {
            var userId = HttpContext.User.Identity.Name;
            var walletModel = _walletService.GetWallet(id);
            if (walletModel.OwnerId == int.Parse(userId))
            {
                _walletService.DeleteWallet(id);
                return NoContent();
            }
            else return NotFound();
        }

        [HttpPut("{id}")]
        public IActionResult UpdateWallet(int id, WalletViewModel wallet)
        {
            if (id != wallet.WalletId)
            {
                return NotFound();
            }
            var userId = HttpContext.User.Identity.Name;
            var walletModel = _walletService.GetWallet(id);
            if (walletModel.OwnerId == int.Parse(userId))
            {
                walletModel.Name = wallet.Name;
                walletModel.Description = wallet.Description;
                if( wallet.InitMoney > walletModel.InitMoney)
                {
                    var difference = wallet.InitMoney - walletModel.InitMoney;
                    walletModel.InitMoney = wallet.InitMoney;
                    walletModel.Balance += difference;
                }

                _walletService.UpdateWallet(walletModel);
                return NoContent();
            }
            else return NotFound();
        }
    }
}
