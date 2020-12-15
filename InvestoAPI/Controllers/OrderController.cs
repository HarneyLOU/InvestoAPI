using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using InvestoAPI.Core.Entities;
using InvestoAPI.Core.Enums;
using InvestoAPI.Core.Interfaces;
using InvestoAPI.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InvestoAPI.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IWalletService _walletService;
        private readonly IMapper _mapper;

        public OrderController(IOrderService orderService, IWalletService walletService, IMapper mapper)
        {
            _orderService = orderService;
            _mapper = mapper;
            _walletService = walletService;
        }

        [HttpGet("wallet/{id}")]
        public IActionResult GetWalletOrders(int id)
        {
            var userId = HttpContext.User.Identity.Name;
            var walletModel = _walletService.GetWallet(id);
            if (walletModel != null && walletModel.OwnerId == int.Parse(userId))
            {
                var orderModel = _orderService.GetOrdersFromWallet(id);
                return Ok(_mapper.Map<IList<OrderViewModel>>(orderModel));
            }
            else return NotFound();
        }

        [HttpGet("{id}")]
        public IActionResult GetOrder(int id)
        {
            var userId = HttpContext.User.Identity.Name;
            var orderModel = _orderService.GetOrder(id);
            if (orderModel != null && orderModel.Wallet.OwnerId == int.Parse(userId)) return Ok(_mapper.Map<OrderViewModel>(orderModel));
            else return NotFound();
        }

        [HttpGet("cancel/{id}")]
        public IActionResult CancelOrder(int id)
        {
            var userId = HttpContext.User.Identity.Name;
            var orderModel = _orderService.GetOrder(id);
            if (orderModel != null && orderModel.Wallet.OwnerId == int.Parse(userId)) return Ok(_mapper.Map<OrderViewModel>(_orderService.CancelOrder(orderModel)));
            else return NotFound();
        }

        [HttpPost]
        public IActionResult AddOrder([FromBody] OrderViewModel model)
        {
            var userId = HttpContext.User.Identity.Name;
            if (userId == null) return BadRequest();

            var order = _mapper.Map<Order>(model);


            _orderService.AddOrder(order);

            return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, _mapper.Map<OrderViewModel>(order));
        }
    }
}
