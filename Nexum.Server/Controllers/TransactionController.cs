using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace Nexum.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly TelemetryClient _telemetryClient;

        public TransactionController(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }

        [HttpPost("deposit")]
        public IActionResult Deposit([FromBody] TxRequest request)
        {
            // --- 1. สร้าง ID ที่ไม่ซ้ำกันสำหรับ Request นี้ ---
            string trackingId = Guid.NewGuid().ToString();

            // ... (ตรรกะการฝากเงินของคุณ) ...

            _telemetryClient.TrackEvent(
                "TransactionProcessed",
                new Dictionary<string, string>
                {
                { "UserId", request.UserId },
                { "TransactionType", "Deposit" },
                { "RunningNumber", request.RunningNumber },
                { "TestRunId", request.TestRunId }
                },
                new Dictionary<string, double> { { "Amount", (double)request.Amount } }
            );

            // 3. ส่ง TrackingId กลับไปให้ Client (เพื่อยืนยัน)
            return Ok(new { Status = "Success", TrackingId = trackingId });
        }

        [HttpPost("withdraw")]
        public IActionResult Withdraw([FromBody] TxRequest request)
        {
            string trackingId = Guid.NewGuid().ToString(); // สร้าง ID ใหม่
            _telemetryClient.TrackEvent(
                "TransactionProcessed",
                new Dictionary<string, string>
                {
                { "UserId", request.UserId },
                { "TransactionType", "Withdrawal" },
                { "RunningNumber", request.RunningNumber },
                { "TestRunId", request.TestRunId }
                },
                new Dictionary<string, double> { { "Amount", (double)request.Amount } }
            );
            return Ok(new { Status = "Success", TrackingId = trackingId });
        }

        [HttpPost("transfer")]
        public IActionResult Transfer([FromBody] TxRequest request)
        {
            string trackingId = Guid.NewGuid().ToString(); // สร้าง ID ใหม่
            _telemetryClient.TrackEvent(
                "TransactionProcessed",
                new Dictionary<string, string>
                {
                { "UserId", request.UserId },
                { "TransactionType", "Transfer" },
                { "RunningNumber", request.RunningNumber },
                { "TestRunId", request.TestRunId }
                },
                new Dictionary<string, double> { { "Amount", (double)request.Amount } }
            );
            return Ok(new { Status = "Success", TrackingId = trackingId });
        }
    }

    // (DTO ของคุณ ยังใช้เหมือนเดิม)
    public class TxRequest
    {
        [DefaultValue("1")]
        public string UserId { get; set; }
        [DefaultValue(500)]
        public decimal Amount { get; set; }
        public string RunningNumber { get; set; }
        public string TestRunId { get; set; }
    }
}
