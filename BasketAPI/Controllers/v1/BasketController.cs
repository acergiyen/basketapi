using Business.Queries.BasketQuery;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BasketAPI.Controllers.v1
{
    [ApiController]
    [Route("api/v1/")]
    public class ProductController : Controller
    {
        private readonly IMediator mediator;
        public ProductController(IMediator mediator)
        {
            this.mediator = mediator;
        }
        /// <summary>
        /// Add Product to Basket
        /// </summary>
        /// <returns></returns>
        [Route("basket"), HttpPost]
        public async Task<IActionResult> CreateBasket([FromBody] CreateBasketQuery request)
        {

            var result = await mediator.Send(request);
            return Ok(result);
        }

        /// <summary>
        /// Get Basket
        /// </summary>
        /// <param name="basketId"></param>
        /// <returns></returns>
        [Route("basket/{basketId}"), HttpGet]
        public async Task<IActionResult> GetBasket([FromRoute] int basketId)
        {
            var result = await mediator.Send(new GetBasketByIdQuery
            {
                BasketId = basketId
            });
            return Ok(result);
        }
    }
}
