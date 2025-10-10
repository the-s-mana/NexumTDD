using Nexum.Server.DAC;
using Nexum.Server.Models.Penalty;

namespace Nexum.Server.Services.Penalty
{
    public interface IPercentagePenalty
    {
        decimal Calculate(PenaltyContext context);
    }
    public class PercentagePenalty : IPercentagePenalty
    {
        public decimal Calculate(PenaltyContext context)
        {
            var calculatedPenalty = context.OutstandingBalance * (context.Percentage / 100);
            // คืนค่าปรับที่ไม่เกินเพดานที่กำหนด
            return Math.Min(calculatedPenalty, context.MaxPenalty);
        }
    }
}
