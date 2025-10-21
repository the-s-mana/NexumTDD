using Nexum.Server.DAC.Providers;
using Nexum.Server.Models;
using Nexum.Server.Models.Penalty;

namespace Nexum.Server.Services.Penalty
{
    public interface IPenalty
    {
        Task<PenaltyResponse?> GetPenalty(PenaltyRequest penaltyRequest);
    }
    public class Penalty : IPenalty
    {
        public readonly IPercentagePenalty percentagePenalty;
        //public readonly IPenaltyPolicies penaltyPolicies;
        public readonly IDailyPenalty dailyPenalty;
        public readonly IFixedPenalty fixedPenalty; 
        private readonly ISurrealDbProviderFactory dbFactory;

        public Penalty(IPercentagePenalty percentagePenalty, ISurrealDbProviderFactory dbFactory , IDailyPenalty dailyPenalty, IFixedPenalty fixedPenalty)
        {
            this.percentagePenalty = percentagePenalty;
            this.dbFactory = dbFactory;
            this.dailyPenalty = dailyPenalty;
            this.fixedPenalty = fixedPenalty;
        }
        public async Task<PenaltyResponse> GetPenalty(PenaltyRequest penaltyRequest)
        {
            #region Validation
            if (penaltyRequest == null)
                throw new ArgumentNullException(nameof(penaltyRequest));

            if (penaltyRequest.OutstandingBalance <= 0)
                throw new ArgumentException("OutstandingBalance must be greater than zero.");

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

            PenaltyResponse penaltyResponse = new PenaltyResponse();

            ////Get Penalty Policies By Id (Config Penalty Policies) @Tae Edit 20/10/2025 
            //ProductContact PenaltyPolicies = await penaltyPolicies.penaltyPolicies(new PenaltyPoliciesRequest { PenaltyPolicyID = penaltyRequest.PenaltyPolicyID }); 
            
            // --- ส่วนที่ 2: แก้ไขวิธีการดึงข้อมูล --- 
            // 1. สั่งให้โรงงาน (Factory) สร้าง Provider ที่เราต้องการ
            var policyProvider = dbFactory.Create<PenaltyPolicyDocument, ProductContact>(); 
            
            // 2. ใช้ Provider ที่ได้มาเพื่อดึงและแปลงข้อมูลในขั้นตอนเดียว
            ProductContact? PenaltyPolicies = await policyProvider.GetX(penaltyRequest.PenaltyPolicyID.ToString()); 
            
            // 3. (สำคัญมาก) ตรวจสอบว่าเจอข้อมูล Policy หรือไม่
            if (PenaltyPolicies == null) { throw new Exception($"PenaltyPolicy with ID {penaltyRequest.PenaltyPolicyID} not found in the database."); }

            //คำนวนยอดชำระขั้นต่ำ
            decimal minPayment = penaltyRequest.OutstandingBalance * (PenaltyPolicies.MinimumPaymentRate / 100); //

            penaltyResponse.UserId = penaltyRequest.UserId;
            penaltyResponse.OutstandingBalance = penaltyRequest.OutstandingBalance;
            penaltyResponse.MinimumPayment = minPayment;
            penaltyResponse.PaymentAmount = penaltyRequest.PaymentAmount;

            //ตรวจสอบเลย วันครบกำหนด
            if (penaltyRequest.DueDate.Date < DateTime.Now.Date || penaltyRequest.PaymentAmount < minPayment)
            {
                int OverdueDays = (DateTime.Now.Date - penaltyRequest.DueDate.Date).Days; //คำนวณจำนวนวันที่เกินกำหนด
                // ควรคำนวนวันที่ปรับใหม่ไหม เช่น OverdueDaysNew = OverdueDays - GracePeriodDays
                if (OverdueDays > PenaltyPolicies.PenaltyFreePeriodDays)
                {
                    PenaltyContext context = new PenaltyContext
                    {
                        OutstandingBalance = penaltyRequest.OutstandingBalance,
                        OverdueDays = OverdueDays,
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
                            throw new NotSupportedException($"Penalty type '{PenaltyPolicies.PenaltyType}' is not supported.");
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
