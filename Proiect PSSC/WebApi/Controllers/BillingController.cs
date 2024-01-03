using LanguageExt.ClassInstances;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Proiect_PSSC.Domain.Models.Domain_Objects;
using Proiect_PSSC.Domain.Workflows;
using WebApi.Dto;
using WebApi.Repository;

namespace WebApi.Controllers
{
    [Route("api/billing")]
    [ApiController]
    public class BillingController : ControllerBase
    {
        private ProductRepository repository;
        private OrderRepository orderRepository;

        public BillingController()
        {
            this.repository = new();
            this.orderRepository = new();
        }

        [HttpGet("/all")]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await repository.GetAllProducts();
            return Ok(products);
        }

        [HttpGet("/find")]
        public async Task<IActionResult> GetProductById([FromQuery(Name ="id")] string productId)
        {
            var product = await repository.GetProductById(productId);
            if (product == null)
            {
                return NotFound($"Product with id {productId} not found!");
            }
            return Ok(product);
        }

        [HttpGet("/bill")]
        public async Task<IActionResult> GetBillById([FromQuery(Name = "orderId")] string orderId,
                                                     [FromQuery(Name = "clientId")] string clientId)
        {
            var bill = await orderRepository.GetOrderItem(clientId, orderId);
            if (bill == null)
            {
                return NotFound($"Order with id {orderId} not found!");
            }
            return Ok(bill);
        }
    }
}
