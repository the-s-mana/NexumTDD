using Nexum.Server.Models;
using Nexum.Server.DAC;

namespace Nexum.Server.Services

{
    public interface IInterestService
    {
        CalculateInterestResponse CalculateInterest(CalculateInterestRequest req);

    }
    public class InterestService : IInterestService
    {
        private readonly IAccumulatedInterestDAC _accumulatedInterestDAC;
        private readonly IInterestTransactionDAC _InterestTransactionDAC;

        public InterestService(IAccumulatedInterestDAC accumulatedInterestDAC, IInterestTransactionDAC InterestTransactionDAC)
        {
            _accumulatedInterestDAC = accumulatedInterestDAC;
            _InterestTransactionDAC = InterestTransactionDAC;
        }
        public CalculateInterestResponse CalculateInterest(CalculateInterestRequest req)
        {
            // Validate the request
            if (req == null)
            {
                throw new ArgumentNullException(nameof(req), "Request cannot be null.");
            }

            // ตรวจสอบยอดเงินต้น
            if (req.PrincipalBalance < 0)
            {
                throw new ArgumentException("PrincipalBalance cannot be negative.", nameof(req.PrincipalBalance));
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
            AccumulatedInterest accumulatedInterest = _accumulatedInterestDAC.GetAccumulatedInterest(req.ProductContactId);

            // ตรวจสอบว่าอยู่ในระยะปลอดดอกเบี้ยหรือไม่
            if (req.InterestFreePeriodDays != default(DateTime))
            {
                // ถ้าวันปัจจุบัน <= วันสิ้นสุดระยะปลอดดอกเบี้ย
                if (DateTime.Now <= req.InterestFreePeriodDays)
                {
                    // สร้างรายการดอกเบี้ย หมายเหตุ ยกเว้นการคำนวณ
                    InterestTransaction InterestTransaction = new InterestTransaction
                    {
                        ProductContactId = req.ProductContactId,
                        InterestAmount = 0,
                        AccumulatedAmount = accumulatedInterest.AccumInterestRemain,
                        Remark = "ยกเว้นการคำนวณดอกเบี้ย",
                    };
                    _InterestTransactionDAC.CreateInterestTransaction(InterestTransaction);

                    // ส่งข้อมูลดอกเบี้ยกลับ
                    return new CalculateInterestResponse
                    {
                        InterestAmount = 0,
                        AccumInterestRemain = accumulatedInterest.AccumInterestRemain
                    };
                }
            }

            decimal interestAmount = 0;

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
            decimal accumulatedInterestAmount = accumulatedInterest.AccumInterestRemain + interestAmount; // รวมยอดดอกเบี้ยสะสม

            // สร้างรายการดอกเบี้ย
            InterestTransaction createInterestTransaction = new InterestTransaction
            {
                ProductContactId = req.ProductContactId,
                InterestAmount = interestAmount,
                AccumulatedAmount = accumulatedInterestAmount,
                Remark = "ดอกเบี้ยรอบนี้",
                // Remark = isMaxInterestAmount ? "ดอกเบี้ยรอบนี้สูงกว่าอัตราดอกเบี้ยสูงสุดต่อรอบบิล" : "ดอกเบี้ยรอบนี้",
            };
            _InterestTransactionDAC.CreateInterestTransaction(createInterestTransaction);

            // บันทึกข้อมูลดอกเบี้ยสะสม
            _accumulatedInterestDAC.UpdateAccumulatedInterest(accumulatedInterestAmount);

            return new CalculateInterestResponse()
            {
                InterestAmount = interestAmount,
                AccumInterestRemain = accumulatedInterestAmount
            };
        }
    }
}
