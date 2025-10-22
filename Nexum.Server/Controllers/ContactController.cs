using Microsoft.AspNetCore.Mvc;
using Nexum.Server.Models.CreditWallet;
using Nexum.Server.Services;

namespace Nexum.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactController : ControllerBase
{
    private readonly IContactService _contactService;
    public ContactController(IContactService contactService)
    {
        _contactService = contactService;
    }

    [HttpPost]
    [Route("CreateContact")]
    public async Task<IActionResult> CreateContact(CreateContactRequestDTO contact)
    {
        var res = await _contactService.CreateContactAsync(contact);

        return Ok(res);
    }

    [HttpGet("GetContactByWalletId/{id}")]
    public async Task<ActionResult<ContactResponseDTO>> GetContactByWalletId(string id)
    {
        var bookDto = await _contactService.GetContactByWalletIdAsync(id);

        if (bookDto == null)
            return NotFound();

        return Ok(bookDto);
    }
}