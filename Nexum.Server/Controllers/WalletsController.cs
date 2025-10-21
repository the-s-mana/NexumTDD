using Microsoft.AspNetCore.Mvc;
using Nexum.Server.Data;
using Nexum.Server.Data.Models;
using Nexum.Server.Models.Book;
using Nexum.Server.Models.CreditWallet;
using Nexum.Server.Services;
using System.ComponentModel;
using static System.Reflection.Metadata.BlobBuilder;

namespace Nexum.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WalletsController : ControllerBase
{
    private readonly IWalletService _walletService;
    public WalletsController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpPost]
    [Route("CreateWallet")]
    public async Task<IActionResult> CreateWallet(CreateWalletRequestDTO wallet)
    {
        var res = await _walletService.CreateWalletAsync(wallet);

        return Ok(res);
    }

    [HttpGet("GetWalletById/{id}")]
    public async Task<ActionResult<WalletResponseDTO>> GetWalletById(string id)
    {
        var bookDto = await _walletService.GetWalletByIdAsync(id);

        if (bookDto == null)
            return NotFound();

        return Ok(bookDto);
    }
}