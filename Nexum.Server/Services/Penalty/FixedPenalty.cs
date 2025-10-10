using Nexum.Server.Models.Penalty;

namespace Nexum.Server.Services.Penalty
{
    public interface IFixedPenalty
    {
        decimal Calculate(PenaltyContext context);
    }
    public class FixedPenalty : IFixedPenalty
    {
        public decimal Calculate(PenaltyContext context)
        {
            // คืนค่าปรับตามยอดคงที่ที่กำหนด
            return context.FixedAmount;
        }
    }
}
