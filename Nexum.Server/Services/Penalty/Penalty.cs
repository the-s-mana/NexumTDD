using Nexum.Server.DAC;
using Nexum.Server.Models;
using Nexum.Server.Models.Penalty;

namespace Nexum.Server.Services.Penalty
{
    public interface IPenalty
    {
        PenaltyResponse GetPenalty(PenaltyRequest penaltyRequest);
    }
    public class Penalty : IPenalty
    {
        public readonly IPercentagePenalty percentagePenalty;
        public readonly IPenaltyPolicies penaltyPolicies;
        public readonly IDailyPenalty dailyPenalty;
        public readonly IFixedPenalty fixedPenalty;
        public Penalty(IPercentagePenalty percentagePenalty, IPenaltyPolicies penaltyPolicies, IDailyPenalty dailyPenalty, IFixedPenalty fixedPenalty)
        {
            this.percentagePenalty = percentagePenalty;
            this.penaltyPolicies = penaltyPolicies;
            this.dailyPenalty = dailyPenalty;
            this.fixedPenalty = fixedPenalty;
        }
        public PenaltyResponse GetPenalty(PenaltyRequest penaltyRequest)
        {
            PenaltyResponse penaltyResponse = new PenaltyResponse();

            #region Validation
            if (penaltyRequest == null)
                throw new ArgumentNullException(nameof(penaltyRequest));

            if (penaltyRequest.OutstandingBalance <= 0)
            {
                penaltyResponse.PenaltyAmount = 0;
                return penaltyResponse;
            }



            if (penaltyRequest.DueDate == null)
                throw new ArgumentException("DueDate must be a valid date.");

            if (string.IsNullOrEmpty(penaltyRequest.ActiveStatus) || (penaltyRequest.ActiveStatus != "Active" && penaltyRequest.ActiveStatus != "Inactive"))
                throw new ArgumentException("ActiveStatus must be either 'Active' or 'Inactive'.");

            if (penaltyRequest.UserId <= 0)
                throw new ArgumentException("UserId must be greater than zero.");

            if (penaltyRequest.ActiveStatus == "Inactive")
                throw new InvalidOperationException("Cannot calculate penalty for inactive users.");

            if (penaltyRequest.DueDate > DateTime.Now)
                throw new InvalidOperationException("DueDate cannot be in the future.");

            #endregion



            //Get Penalty Policies By Id (Config Penalty Policies)
            ProductContact PenaltyPolicies = penaltyPolicies.penaltyPolicies(new PenaltyPoliciesRequest { PenaltyPolicyID = penaltyRequest.PenaltyPolicyID });

            //คำนวนยอดชำระขั้นต่ำ
            decimal minPayment = penaltyRequest.OutstandingBalance * (PenaltyPolicies.PenaltyPolicyx.MinimumPaymentRate / 100); //

            penaltyResponse.UserId = penaltyRequest.UserId;
            penaltyResponse.OutstandingBalance = penaltyRequest.OutstandingBalance;
            penaltyResponse.MinimumPayment = minPayment;
            penaltyResponse.PaymentAmount = penaltyRequest.PaymentAmount;

            //ตรวจสอบเลย วันครบกำหนด
            if (penaltyRequest.DueDate.Date < DateTime.Now.Date || penaltyRequest.PaymentAmount < minPayment)
            {
                int OverdueDays = (DateTime.Now.Date - penaltyRequest.DueDate.Date).Days; //คำนวณจำนวนวันที่เกินกำหนด
                // ควรคำนวนวันที่ปรับใหม่ไหม เช่น OverdueDaysNew = OverdueDays - GracePeriodDays
                if (OverdueDays > PenaltyPolicies.PenaltyPolicyx.PenaltyFreePeriodDays)
                {
                    PenaltyContext context = new PenaltyContext
                    {
                        OutstandingBalance = penaltyRequest.OutstandingBalance,
                        OverdueDays = OverdueDays,
                        
                        MaxPenalty = PenaltyPolicies.PenaltyPolicyx.PenaltyMax,
                        TotalCap = PenaltyPolicies.PenaltyPolicyx.TotalCap,
                        Percentage = PenaltyPolicies.PenaltyPolicyx.PenaltyRate ?? 0m,
                        FixedAmount = PenaltyPolicies.PenaltyPolicyx.PenaltyFixed ?? 0m,
                    };
                    switch (PenaltyPolicies.PenaltyPolicyx.PenaltyType)
                    {
                        case "Percentage":
                            penaltyResponse.PenaltyAmount = percentagePenalty.Calculate(context);
                            break;
                        case "Daily":
                            penaltyResponse.PenaltyAmount = dailyPenalty.Calculate(context);
                            break;
                        case "Fixed":
                            penaltyResponse.PenaltyAmount = fixedPenalty.Calculate(context);
                            break;
                        default:
                            throw new NotSupportedException($"Penalty type '{PenaltyPolicies.PenaltyPolicyx.PenaltyType}' is not supported.");
                    }
                }
                else
                {
                    penaltyResponse.PenaltyAmount = 0;
                }
            }
            else
            {
                penaltyResponse.PenaltyAmount = 0;
                return penaltyResponse;
            }

            return penaltyResponse;
        }
    }
}
