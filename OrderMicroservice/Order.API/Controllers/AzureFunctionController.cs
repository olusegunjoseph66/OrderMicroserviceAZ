using Microsoft.AspNetCore.Mvc;
using Order.Application.Interfaces.Services;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    public class AzureFunctionController : BaseController
    {
        private readonly IPlantService _plantService;
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IDeadLetterService _deadLetterService;
        public AzureFunctionController(IPlantService plantService,
            ICartService cartService,
            IOrderService orderService,
            IDeadLetterService deadLetterService)
        {
            _plantService = plantService;
            _cartService = cartService;
            _orderService = orderService;
            _deadLetterService = deadLetterService;
        }

        [HttpGet("azure-refresh-plants")]
        public async Task<IActionResult> AutoRefreshPlants() => Response(await _plantService.UpdatePlant().ConfigureAwait(false));

        [HttpGet("azure-abandon-carts")]
        public async Task<IActionResult> AutoAbandonCarts() => Response(await _cartService.AutoAbandonCarts(default).ConfigureAwait(false));

        [HttpGet("azure-refresh-Orders")]
        public async Task<IActionResult> AutoRefreshOrders() => Response(await _orderService.AutoRefreshOrder(default).ConfigureAwait(false));

        [HttpGet("azure-submit-Orders")]
        public async Task<IActionResult> AutoSubmitOrders() => Response(await _orderService.AutoSubmitOrder(default).ConfigureAwait(false));

        [HttpGet("azure-process-product-dead-Letter")]
        public async Task<IActionResult> ProcessDeadLetter() => Response(await _deadLetterService.ProcessAccountDeadLetterMessages(default).ConfigureAwait(false));


    }
}
