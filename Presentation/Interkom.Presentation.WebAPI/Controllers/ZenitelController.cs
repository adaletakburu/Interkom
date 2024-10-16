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

        [HttpGet("GetAllClients")]
        public async Task<IActionResult> GetAllClients()
        {
            var clients = _intercomService.ClientsAsync();
            return Ok(clients);
        }

        [HttpGet("GetAllStates")]
        public async Task<IActionResult> GetAllStates()
        {
            var state = _intercomService.StateAsync();
            return Ok(state);
        }

        [HttpGet("GetAllMessages")]
        public async Task<IActionResult> GetAllMessages()
        {
            var messages = _intercomService.MessagesAsync();
            return Ok(messages);
        }

        [HttpGet("GetStationState")]
        public async Task<IActionResult> GetStationState()
        {
            var stationState = _intercomService.StationStateAsync();
            return Ok(stationState);
        }
    }
}
