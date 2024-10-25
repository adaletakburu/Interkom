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

        [HttpGet("InviteAsync")]
        public async Task<IActionResult> InviteAsync()
        {
            var call = _intercomService.InviteAsync();
            return Ok(call);
        }

        [HttpGet("AckAsync")]
        public async Task<IActionResult> AckAsync()
        {
            var call = _intercomService.AckAsync();
            return Ok(call);
        }
        [HttpGet("CancelAsync")]
        public async Task<IActionResult> CancelAsync()
        {
            var call = _intercomService.CancelAsync();
            return Ok(call);
        }
        [HttpGet("OptionsAsync")]
        public async Task<IActionResult> OptionsAsync()
        {
            var call = _intercomService.OptionsAsync();
            return Ok(call);
        }
        [HttpGet("ByeAsync")]
        public async Task<IActionResult> ByeAsync()
        {
            var call = _intercomService.ByeAsync();
            return Ok(call);
        }
    }
}
