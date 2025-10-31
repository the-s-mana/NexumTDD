using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;

namespace Nexum.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertController : ControllerBase
    {
        private readonly ILogger<AlertController> _logger;

        public AlertController(ILogger<AlertController> logger)
        {
            _logger = logger;
        }

        // Endpoint นี้ต้องตรงกับที่เราจะตั้งค่าใน Action Group
        // เช่น: https://your-app.azurewebsites.net/api/alert/fraud
        [HttpPost("fraud")]
        public async Task<IActionResult> ReceiveFraudAlert()
        {
            // 1. อ่าน JSON ดิบๆ ที่ Azure ส่งมา
            string requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            _logger.LogWarning("--- !!! FRAUD ALERT RECEIVED !!! ---");
            // _logger.LogInformation(requestBody); // (ใช้สำหรับ Debug ถ้าอยากเห็น JSON ทั้งก้อน)

            try
            {
                // 2. Parse JSON
                var payload = JsonNode.Parse(requestBody);

                // 3. ดึงข้อมูลที่จำเป็น (ข้อมูล KQL Query จะอยู่ในนี้)
                var context = payload["data"]["AlertContext"];
                var alertName = context["Name"].GetValue<string>();
                var alertTime = context["FiredTime"].GetValue<DateTime>();

                // 4. ดึงตารางผลลัพธ์ KQL (ผู้ใช้ที่ทำผิดกฎ)
                var searchResults = context["SearchResults"];
                var tables = searchResults["tables"][0];
                var columns = tables["columns"]; // (เก็บชื่อคอลัมน์)
                var rows = tables["rows"].AsArray(); // (เก็บข้อมูล)

                _logger.LogCritical(
                    "Alert Rule '{RuleName}' triggered at {AlertTime}. Found {Count} suspicious users.",
                    alertName,
                    alertTime,
                    rows.Count
                );

                // 5. วน Loop ผู้ใช้ที่ทำผิดกฎ
                foreach (var row in rows)
                {
                    // KQL Query ของเรามี 7 คอลัมน์:
                    // [0] UserId, [1] DepositCount, [2] WithdrawCount, [3] TransferCount,
                    // [4] DepositTotal, [5] WithdrawTotal, [6] TransferTotal

                    string userId = row[0].GetValue<string>();
                    int depositCount = row[1].GetValue<int>();
                    int withdrawCount = row[2].GetValue<int>();
                    int transferCount = row[3].GetValue<int>();
                    double depositTotal = row[4].GetValue<double>();
                    double withdrawTotal = row[5].GetValue<double>();
                    double transferTotal = row[6].GetValue<double>();

                    // 6. Log รายละเอียดของแต่ละคน
                    _logger.LogWarning(
                        "SUSPICIOUS USER DETECTED: UserId={UserId}, DepositCount={DCount}, WithdrawCount={WCount}, TransferCount={TCount}, DepositTotal={DTotal}, WithdrawTotal={WTotal}, TransferTotal={TTotal}",
                        userId, depositCount, withdrawCount, transferCount, depositTotal, withdrawTotal, transferTotal
                    );

                    // --- (TODO: ใส่ Logic ของคุณตรงนี้) ---
                    // เช่น สั่ง Flag user ใน Database, ส่ง Email ฯลฯ
                    // ------------------------------------
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse Azure Alert Webhook payload.");
                _logger.LogError("Raw Payload: {Payload}", requestBody);
            }

            // 7. ตอบกลับ Azure ว่ารับทราบแล้ว
            return Ok("Acknowledged");
        }
    }
}
