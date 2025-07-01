using EmailService.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmailService.Api
{
    [ApiController]
    [Route("[controller]")]
    public class EmailsController : ControllerBase
    {
        // POST /emails
        [HttpPost]
        public IActionResult SendEmail([FromBody] EmailNotification notification)
        {
            // ...mock send...
            return Ok();
        }
    }
}
