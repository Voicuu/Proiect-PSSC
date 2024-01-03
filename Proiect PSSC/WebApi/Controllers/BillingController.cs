using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Proiect_PSSC.Domain.Workflows;
using WebApi.Dto;
using WebApi.Repository;

namespace WebApi.Controllers
{
    [Route("api/billing")]
    [ApiController]
    public class BillingController : ControllerBase
    {
        private BillingWorkflow billingWorkflow;
        private ProductRepository repository;

        public BillingController(BillingWorkflow billingWorkflow)
        {
            this.billingWorkflow = billingWorkflow;
            this.repository = new();
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
    }
}
