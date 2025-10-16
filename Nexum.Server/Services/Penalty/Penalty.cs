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
        public readonly IDateTimeProvider _clock;
        public Penalty(IPercentagePenalty percentagePenalty, IPenaltyPolicies penaltyPolicies, IDailyPenalty dailyPenalty, IFixedPenalty fixedPenalty, IDateTimeProvider? clock = null)
        {
            this.percentagePenalty = percentagePenalty;
            this.penaltyPolicies = penaltyPolicies;
            this.dailyPenalty = dailyPenalty;
            this.fixedPenalty = fixedPenalty;
            this._clock = clock ?? new SystemDateTimeProvider();
        }
        public PenaltyResponse GetPenalty(PenaltyRequest penaltyRequest)
        {
            #region Validation
            if (penaltyRequest == null)
                throw new ArgumentNullException(nameof(penaltyRequest));

            if (penaltyRequest.OutstandingBalance <= 0)
                throw new ArgumentException("OutstandingBalance must be greater than zero.");

            if (penaltyRequest.DueDate == default(DateTime))
                throw new ArgumentException("DueDate must be a valid date.");

            if (string.IsNullOrEmpty(penaltyRequest.ActiveStatus) || (penaltyRequest.ActiveStatus != "Active" && penaltyRequest.ActiveStatus != "Inactive"))
                throw new ArgumentException("ActiveStatus must be either 'Active' or 'Inactive'.");

            if (penaltyRequest.UserId <= 0)
                throw new ArgumentException("UserId must be greater than zero.");

            if (penaltyRequest.ActiveStatus == "Inactive")
                throw new InvalidOperationException("Cannot calculate penalty for inactive users.");

            var now = _clock.Now;
            Console.WriteLine($"Current DateTime: {now}");
            if (penaltyRequest.DueDate > now)
                throw new InvalidOperationException("DueDate cannot be in the future.");

            #endregion

            PenaltyResponse penaltyResponse = new PenaltyResponse();

            //Get Penalty Policies By Id (Config Penalty Policies)
            ProductContact PenaltyPolicies = penaltyPolicies.penaltyPolicies(new PenaltyPoliciesRequest { PenaltyPolicyID = penaltyRequest.PenaltyPolicyID });

            //คำนวนยอดชำระขั้นต่ำ
            decimal minPayment = penaltyRequest.OutstandingBalance * (PenaltyPolicies.MinimumPaymentRate / 100); //

            penaltyResponse.UserId = penaltyRequest.UserId;
            penaltyResponse.OutstandingBalance = penaltyRequest.OutstandingBalance;
            penaltyResponse.MinimumPayment = minPayment;
            penaltyResponse.PaymentAmount = penaltyRequest.PaymentAmount;

            //ตรวจสอบเลย วันครบกำหนด
            var overdue = now - penaltyRequest.DueDate;
            bool isOverdue = overdue > TimeSpan.Zero;
            bool underMin = penaltyRequest.PaymentAmount < minPayment;
            
            if(!(isOverdue || underMin))
            {
                penaltyResponse.PenaltyAmount = 0;
                return penaltyResponse;
            }
            
            var grace = TimeSpan.FromDays(PenaltyPolicies.PenaltyFreePeriodDays);
            if(overdue <= grace)
            {
                penaltyResponse.PenaltyAmount = 0;
                return penaltyResponse;
            }
            
            var amountBase = penaltyRequest.OutstandingBalance - penaltyRequest.PaymentAmount;
            if (amountBase < 0) amountBase = 0;
            
            int chargeDays = (int)Math.Ceiling((overdue - grace).TotalDays);
            if(chargeDays < 0) chargeDays = 0;
            PenaltyContext context = new PenaltyContext
            {
                OutstandingBalance = penaltyRequest.OutstandingBalance - penaltyRequest.PaymentAmount,
                OverdueDays = chargeDays,
                MaxPenalty = PenaltyPolicies.MaxPenalty,
                TotalCap = PenaltyPolicies.TotalCap,
                Percentage = PenaltyPolicies.PenaltyRate,
                FixedAmount = PenaltyPolicies.FixedAmount,
            };
            switch (PenaltyPolicies.PenaltyType)
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
                    throw new NotSupportedException($"Penalty type '{PenaltyPolicies.PenaltyPolicyID}' is not supported.");
            }
            
            return penaltyResponse;
            //if (penaltyRequest.DueDate.Date < today || penaltyRequest.PaymentAmount < minPayment)
            //{
            //    int OverdueDays = Math.Max(0, (today - penaltyRequest.DueDate.Date).Days); //คำนวณจำนวนวันที่เกินกำหนด
            //    // ควรคำนวนวันที่ปรับใหม่ไหม เช่น OverdueDaysNew = OverdueDays - GracePeriodDays
            //    if (OverdueDays > PenaltyPolicies.PenaltyFreePeriodDays)
            //    {
            //        PenaltyContext context = new PenaltyContext
            //        {
            //            OutstandingBalance = penaltyRequest.OutstandingBalance - penaltyRequest.PaymentAmount,
            //            OverdueDays = OverdueDays,
            //            MaxPenalty = PenaltyPolicies.MaxPenalty,
            //            TotalCap = PenaltyPolicies.TotalCap,
            //            Percentage = PenaltyPolicies.PenaltyRate,
            //            FixedAmount = PenaltyPolicies.FixedAmount,
            //        };
            //        switch (PenaltyPolicies.PenaltyType)
            //        {
            //            case "Percentage":
            //                penaltyResponse.PenaltyAmount = percentagePenalty.Calculate(context);
            //                break;
            //            case "Daily":
            //                penaltyResponse.PenaltyAmount = dailyPenalty.Calculate(context);
            //                break;
            //            case "Fixed":
            //                penaltyResponse.PenaltyAmount = fixedPenalty.Calculate(context);
            //                break;
            //            default:
            //                throw new NotSupportedException($"Penalty type '{PenaltyPolicies.PenaltyPolicyID}' is not supported.");
            //        }
            //    }
            //    else
            //    {
            //        penaltyResponse.PenaltyAmount = 0;
            //    }
            //}
            //else
            //{
            //    penaltyResponse.PenaltyAmount = 0;
            //    return penaltyResponse;
            //}

            //return penaltyResponse;
        }
    }
}
