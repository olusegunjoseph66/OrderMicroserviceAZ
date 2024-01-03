using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order.Application.DTOs.APIDataFormatters;

namespace Order.API.Controllers
{
    
    [ApiController]
    public class BaseController : ControllerBase
    {
        [NonAction]
        public CreatedResult Created(object value)
        {
            return base.Created("", value);
        }

        protected new IActionResult Response(ApiResponse response)
        {

            if (response.StatusCode == "00")
                return Ok(response);

            if (response.StatusCode == "400")
                return BadRequest(new
                {
                    success = false,
                    data = response
                });

            if (response.StatusCode == "404")
                return NotFound(new
                {
                    success = false,
                    data = response
                });

            if (response.StatusCode == "409")
                return Conflict(new
                {
                    success = false,
                    data = response
                });

            return StatusCode(StatusCodes.Status500InternalServerError, (new
            {
                success = false,
                data = response
            }));
        }
    }
}
