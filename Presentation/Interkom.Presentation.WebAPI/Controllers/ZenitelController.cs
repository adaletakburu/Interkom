using Interkom.Core.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Interkom.Presentation.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZenitelController : ControllerBase
    {
        private readonly IZenitelIntercomService _intercomService;

        public ZenitelController(IZenitelIntercomService intercomService)
        {
            _intercomService = intercomService;
        }

        [HttpGet("GetFullStationList")]
        public async Task<IActionResult> GetFullStationList()
        {
            var call = _intercomService.GetFullStationList();
            return Ok(call);
        }
    }
}
