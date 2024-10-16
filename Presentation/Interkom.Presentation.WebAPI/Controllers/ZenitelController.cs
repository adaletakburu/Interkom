using Interkom.Core.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Interkom.Presentation.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZenitelController : ControllerBase
    {
        private readonly IZenitelIntercomService _intercomService;

        public async Task<IActionResult> Clients()
        {
            return Ok(_intercomService.ClientsAsync());
        }
        public async Task<IActionResult> State()
        {
            return Ok(_intercomService.StateAsync());
        }
        public async Task<IActionResult> Messages()
        {
            return Ok(_intercomService.MessagesAsync());
        }
        public async Task<IActionResult> StationState()
        {
            return Ok(_intercomService.StationStateAsync());
        }

    }
}
