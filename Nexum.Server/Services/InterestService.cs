using Nexum.Server.DAC;
using Nexum.Server.Data.Models;
using Nexum.Server.Models;
using Nexum.Server.Models.Book;
using Nexum.Server.Models.CreditWallet;
using Nexum.Server.Models.Interest;
using Nexum.Server.Utils;
using SurrealDb.Net.Models;

namespace Nexum.Server.Services

{
    public interface IInterestService
    {
        Task<CalculateInterestResponse> CalculateInterest(CalculateInterestRequest req);

    }
    public class InterestService : IInterestService
    {
        private readonly IAccumulatedInterestDAC _accumulatedInterestDAC;
        private readonly IInterestTransactionDAC _InterestTransactionDAC;
        private readonly IDateTimeUtils _dateTimeUtils;

        public InterestService(IAccumulatedInterestDAC accumulatedInterestDAC, IInterestTransactionDAC InterestTransactionDAC, IDateTimeUtils dateTimeUtils)
        {
            _accumulatedInterestDAC = accumulatedInterestDAC;
            _InterestTransactionDAC = InterestTransactionDAC;
            _dateTimeUtils = dateTimeUtils;
        }

        private async Task<AccumulatedInterestResponseDTO> GetAccumulatedInterestByProductContactIdAsync(string id)
        {
            RecordId ProductContactId = RecordId.From(nameof(ProductContact), id);
            return await _accumulatedInterestDAC.GetAccumulatedInterestByProductContactIdAsync(ProductContactId);
        }

        public async Task<CalculateInterestResponse> CalculateInterest(CalculateInterestRequest req)
        {
            // Validate the request
            if (req == null)
            {
                throw new ArgumentNullException(nameof(req), "Request cannot be null.");
            }

            if (req.InterestType == null || req.InterestType == default(string))
            {
                throw new ArgumentException("InterestType is required.", nameof(req.InterestType));
            }

            // ตรวจสอบยอดเงินต้น
            if (req.PrincipalBalance <= 0)
            {
                throw new ArgumentException("PrincipalBalance cannot be negative or zero.", nameof(req.PrincipalBalance));
            }

            // ตรวจสอบรูปแบบดอกเบี้ย
            if (string.IsNullOrWhiteSpace(req.InterestType))
            {
                throw new ArgumentException("InterestType is required.", nameof(req.InterestType));
            }
            if (req.InterestType != "PerMonth" && req.InterestType != "PerDay")
            {
                throw new ArgumentException("InterestType is invalid.", nameof(req.InterestType));
            }

            // ตรวจสอบอัตราดอกเบี้ย
            if (req.InterestRate < 0)
            {
                throw new ArgumentException("InterestRate cannot be negative.", nameof(req.InterestRate));
            }

            // ตรวจสอบอัตราดอกเบี้ยสูงสุด
            if (req.MaxInterestAmount < 0)
            {
                throw new ArgumentException("MaxInterestAmount cannot be negative.", nameof(req.MaxInterestAmount));
            }

            // ดึงข้อมูลดอกเบี้ยสะสม
            AccumulatedInterestResponseDTO accumulatedInterest = await GetAccumulatedInterestByProductContactIdAsync(req.ProductContactId);

            if (accumulatedInterest == null)
            {
                throw new Exception($"ไม่พบข้อมูลดอกเบี้ยสะสมที่มี ProductContactId: {req.ProductContactId}");
            }

            // ตรวจสอบว่าอยู่ในระยะปลอดดอกเบี้ยหรือไม่
            if (req.InterestFreePeriodDays != default(DateTime))
            {
                // ถ้าวันปัจจุบัน <= วันสิ้นสุดระยะปลอดดอกเบี้ย
                var currentDate = _dateTimeUtils.GetCurrentDateTime();
                if (currentDate <= req.InterestFreePeriodDays)
                {
                    // สร้างรายการดอกเบี้ย หมายเหตุ ยกเว้นการคำนวณ
                    CreateInterestTransactionDTO create1 = new CreateInterestTransactionDTO
                    {
                        ProductContactId = req.ProductContactId,
                        InterestAmount = 0,
                        AccumulatedAmount = accumulatedInterest.AccumInterestRemain,
                        Remark = "ยกเว้นการคำนวณดอกเบี้ย",
                    };
                    _InterestTransactionDAC.CreateInterestTransactionAsync(create1);

                    // ส่งข้อมูลดอกเบี้ยกลับ
                    return new CalculateInterestResponse
                    {
                        InterestAmount = 0,
                        AccumInterestRemain = accumulatedInterest.AccumInterestRemain
                    };
                }
            }

            decimal interestAmount = 0;
            decimal accumulatedInterestAmount = accumulatedInterest.AccumInterestRemain;

            // คำนวณดอกเบี้ย
            if (req.InterestType == "PerMonth")
            {
                // คำนวณดอกเบี้ยต่อเดือน จากยอดเงินต้นและอัตราดอกเบี้ย ปัดเศษ 2 ตำแหน่ง
                interestAmount = Math.Round(req.PrincipalBalance * req.InterestRate, 2);
            }
            else if (req.InterestType == "PerDay")
            {
                // คำนวณดอกเบี้ยต่อวัน จากยอดเงินต้นและอัตราดอกเบี้ย ปัดเศษ 2 ตำแหน่ง
                interestAmount = Math.Round(req.PrincipalBalance * req.InterestRate / 365, 2);
            }

            // ไม่มีใน FlowChart อาจไม่ใช้
            // // อัตราดอกเบี้ยสูงสุดต่อรอบบิล
            // bool isMaxInterestAmount = false;
            // if (interestAmount > req.MaxInterestAmount)
            // {
            //     // ถ้าดอกเบี้ยรอบนี้สูงกว่าอัตราดอกเบี้ยสูงสุดต่อรอบบิล ให้ตั้งค่าเป็นอัตราดอกเบี้ยสูงสุดต่อรอบบิล
            //     interestAmount = req.MaxInterestAmount;
            //     isMaxInterestAmount = true;
            // }

            // รวมยอดดอกเบี้ยสะสม และ สร้างรายการดอกเบี้ย
            decimal newAccumulatedInterestAmount = accumulatedInterestAmount + interestAmount; // รวมยอดดอกเบี้ยสะสม

            // สร้างรายการดอกเบี้ย
            CreateInterestTransactionDTO create2 = new CreateInterestTransactionDTO
            {
                ProductContactId = req.ProductContactId,
                InterestAmount = interestAmount,
                AccumulatedAmount = newAccumulatedInterestAmount,
                Remark = "ดอกเบี้ยรอบนี้",
                // Remark = isMaxInterestAmount ? "ดอกเบี้ยรอบนี้สูงกว่าอัตราดอกเบี้ยสูงสุดต่อรอบบิล" : "ดอกเบี้ยรอบนี้",
            };
            _InterestTransactionDAC.CreateInterestTransactionAsync(create2);

            // ดอกเบี้ยรอบนี้เป็น 0
            if (interestAmount != 0)
            {
                // บันทึกข้อมูลดอกเบี้ยสะสม
                var dataToMerge = new Dictionary<string, object?>();

                dataToMerge.Add(nameof(AccumulatedInterestResponseDTO.AccumInterestRemain), newAccumulatedInterestAmount);

                _accumulatedInterestDAC.UpdateAccumulatedInterestAsync(accumulatedInterest.Id, dataToMerge);
            }

            return new CalculateInterestResponse()
            {
                InterestAmount = interestAmount,
                AccumInterestRemain = newAccumulatedInterestAmount
            };
        }
    }
}
