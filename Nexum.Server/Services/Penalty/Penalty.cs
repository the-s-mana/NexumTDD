using System;
using Nexum.Server.DAC;
using Nexum.Server.Models.Penalty;

namespace Nexum.Server.Services.Penalty
{
    public interface IPenalty
    {
        Task<PenaltyResponse> GetPenaltyAsync(PenaltyRequest penaltyRequest);
        // ถ้าจำเป็นต้องเก็บ sync wrapper ไว้จริง ๆ ใส่ [Obsolete] จะดีกว่า
        // PenaltyResponse GetPenalty(PenaltyRequest penaltyRequest) => GetPenaltyAsync(penaltyRequest).GetAwaiter().GetResult();
    }

    public class Penalty : IPenalty
    {
        private readonly IPercentagePenalty percentagePenalty;
        private readonly IDailyPenalty dailyPenalty;
        private readonly IFixedPenalty fixedPenalty;
        private readonly IPenaltyPoliciesDAC penaltyPoliciesDAC;
        private readonly IDateTimeProvider clock;

        public Penalty(
            IPercentagePenalty percentagePenalty,
            IDailyPenalty dailyPenalty,
            IFixedPenalty fixedPenalty,
            IPenaltyPoliciesDAC penaltyPoliciesDAC,
            IDateTimeProvider? clock = null)
        {
            this.percentagePenalty = percentagePenalty;
            this.dailyPenalty = dailyPenalty;
            this.fixedPenalty = fixedPenalty;
            this.penaltyPoliciesDAC = penaltyPoliciesDAC;
            this.clock = clock ?? new SystemDateTimeProvider();
        }

        public async Task<PenaltyResponse> GetPenaltyAsync(PenaltyRequest req)
        {
            // ----- Validation -----
            if (req is null) throw new ArgumentNullException(nameof(req));
            if (req.OutstandingBalance <= 0) throw new ArgumentOutOfRangeException(nameof(req.OutstandingBalance), "OutstandingBalance must be greater than zero.");
            if (req.DueDate == default(DateTime)) throw new ArgumentOutOfRangeException(nameof(req.DueDate), "DueDate must be a valid date.");
            if (req.UserId <= 0) throw new ArgumentOutOfRangeException(nameof(req.UserId), "UserId must be greater than zero.");
            if (req.ActiveStatus == null) throw new ArgumentException("ActiveStatus must be either 'Active' or 'Inactive'.");
            if (!string.Equals(req.ActiveStatus, "Active", StringComparison.OrdinalIgnoreCase) && !string.Equals(req.ActiveStatus, "Inactive", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("ActiveStatus must be either 'Active' or 'Inactive'.", nameof(req.ActiveStatus));
            if (!string.Equals(req.ActiveStatus, "Active", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Cannot calculate penalty for inactive users.", nameof(req.ActiveStatus));

            var now = clock.Now;
            if (req.DueDate > now) throw new InvalidOperationException("DueDate cannot be in the future.");

            // ----- Load policy (DTO) -----
            var policyDto = await penaltyPoliciesDAC.GetPenaltyPolicyByIdXAsync(req.PenaltyPolicyID);
            if (policyDto is null)
                throw new KeyNotFoundException($"Penalty policy {req.PenaltyPolicyID} not found.");

            // ----- Compute minimum payment -----
            var minPayment = Math.Round(
                req.OutstandingBalance * (Convert.ToDecimal(policyDto.MinimumPaymentRate) / 100m),
                2, MidpointRounding.AwayFromZero);

            var resp = new PenaltyResponse
            {
                UserId = req.UserId,
                OutstandingBalance = req.OutstandingBalance,
                MinimumPayment = minPayment,
                PaymentAmount = req.PaymentAmount
            };

            // ----- Early exits (no penalty) -----
            var overdueDays = (now.Date - req.DueDate.Date).Days;         // integer days overdue
            var underMin = req.PaymentAmount < minPayment;

            if (overdueDays <= 0 && !underMin)
            {
                resp.PenaltyAmount = 0;
                return resp;
            }

            var inGrace = overdueDays <= policyDto.PenaltyFreePeriodDays;
            var chargeDays = inGrace ? 0 : overdueDays;
            if (chargeDays == 0 && !underMin)
            {
                resp.PenaltyAmount = 0;
                return resp;
            }

            // ----- Base amount to charge -----
            var amountBase = Math.Max(0m, req.OutstandingBalance - req.PaymentAmount);

            var context = new PenaltyContext
            {
                OutstandingBalance = amountBase,
                OverdueDays = chargeDays,
                MaxPenalty = Convert.ToDecimal(policyDto.MaxPenalty),
                TotalCap = Convert.ToDecimal(policyDto.TotalCap),
                Percentage = Convert.ToDecimal(policyDto.PenaltyRate),
                FixedAmount = Convert.ToDecimal(policyDto.FixedAmount),
            };

            // ----- Strategy -----
            decimal penalty = policyDto.PenaltyType switch
            {
                "Percentage" => percentagePenalty.Calculate(context),
                "Daily" => dailyPenalty.Calculate(context),
                "Fixed" => fixedPenalty.Calculate(context),
                _ => throw new NotSupportedException($"Penalty type '{policyDto.PenaltyType}' is not supported.")
            };

            resp.PenaltyAmount = penalty;
            return resp;
        }
    }
}
