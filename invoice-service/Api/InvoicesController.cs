using InvoiceService.Models;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceService.Api
{
    [ApiController]
    [Route("[controller]")]
    public class InvoicesController : ControllerBase
    {
        // POST /invoices
        [HttpPost]
        public IActionResult CreateInvoice([FromBody] Invoice invoice)
        {
            // ...mock create invoice logic...
            return Ok();
        }
    }
}
