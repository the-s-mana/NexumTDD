using Microsoft.AspNetCore.Mvc;
using Nexum.Server.Models;
using Nexum.Server.Models.CreditWallet;
using Nexum.Server.Services;

namespace Nexum.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BillingController : ControllerBase
    {

        private readonly IBillingService _billingService;

        public BillingController(IBillingService billingService)
        {
            _billingService = billingService;
        }

        [HttpPost]
        [Route("CalculateBilling")]
        public async Task<ActionResult<BillingResponse>> CalculateBilling(BillingRequest contact)
        {
            var res = await _billingService.ProcessAndCalculateBillAsync(contact);

            return Ok(res);
        }
    }
}
