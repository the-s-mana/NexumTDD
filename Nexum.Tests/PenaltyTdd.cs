using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Nexum.Server.Models;
using Nexum.Server.Models.Penalty;
using Nexum.Server.Services.Penalty;

namespace Nexum.Tests
{
    public class PenaltyTdd
    {
        private readonly Mock<IPenaltyPolicies> mockPenaltyPolicies;
        private readonly Mock<IPercentagePenalty> mockPercentagePenalty;
        private readonly Mock<IDailyPenalty> mockDailyPenalty;
        private readonly Mock<IFixedPenalty> mockFixedPenalty;

        public PenaltyTdd()
        {
            mockPenaltyPolicies = new Mock<IPenaltyPolicies>();
            mockPercentagePenalty = new Mock<IPercentagePenalty>();
            mockDailyPenalty = new Mock<IDailyPenalty>();
            mockFixedPenalty = new Mock<IFixedPenalty>();
        }

        #region Policy List
        private readonly List<ProductContact> _policyList = new()
        {
            new ProductContact
            {
                PenaltyPolicyID = 1,
                PolicyName = "Percentage Penalty",
                PenaltyType = "Percentage",
                PenaltyRate = 10m, //10%
                PenaltyMax = 1000.0m,
                PenaltyFreePeriodDays = 0,
                MinimumPaymentRate = 10.0m, // 10%
            },
            new ProductContact
            {
                PenaltyPolicyID = 2,
                PolicyName = "Fixed Penalty",
                PenaltyType = "Fixed",
                PenaltyFixed = 200.0m,
                MinimumPaymentRate = 10.0m, // 10%
            },
            new ProductContact
            {
                PenaltyPolicyID = 3,
                PolicyName = "Percentage Penalty",
                PenaltyType = "Percentage",
                PenaltyRate = 10m, //10%
                PenaltyMax = 300.0m,
                PenaltyFreePeriodDays = 0,
                MinimumPaymentRate = 10.0m, // 10%
            },
            new ProductContact
            {
                PenaltyPolicyID = 4,
                PolicyName = "Special Daily Penalty",
                PenaltyType = "Daily",
                PenaltyFixed = 200.0m,
                PenaltyMax = 400.0m,
                TotalCap = 1200.0m,
                PenaltyFreePeriodDays = 2,
                MinimumPaymentRate = 10.0m, // 10%
            },
        };
        #endregion

        #region Mock Data
        private void SetupMockPolicies()
        {
            mockPenaltyPolicies.Setup(m => m.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()))
            .Returns((PenaltyPoliciesRequest req) =>
            {
                return _policyList.SingleOrDefault(x => x.PenaltyPolicyID == req.PenaltyPolicyID);
            });
        }
        private void SetupMockPercentagePenalty()
        {
            mockPercentagePenalty.Setup(m => m.Calculate(It.IsAny<PenaltyContext>()))
                .Returns((PenaltyContext ctx) =>
                {
                    decimal calculatedPenalty = ctx.OutstandingBalance * ctx.Percentage / 100;
                    // คืนค่าปรับที่ไม่เกินเพดานที่กำหนด
                    return Math.Min(calculatedPenalty, ctx.MaxPenalty);
                });
        }

        #endregion

        #region Normal case

        #region Scenario 1
        //   Scenario 1: PercentMonthly ตรวจสอบ ค่าปรับ>PenaltyMax
        //   Setup :
        //   PenaltyType = "PercentMonthly" // PercentMonthly | FixMonthly
        //   PenaltyRate = 10% (0.05)
        //   MaxPenalty(จาก Issuer Config) = 300 บาท
        //   CurrentDate เลยช่วงผ่อนผันไปแล้ว(เช่น 2025-10-14)

        //   Given: OutstandingBalance = 5000
        //   When: คำนวณค่าปรับ(5000 * 10 / 100 = 500)
        //   Then: penaltyAmount ต้องเป็น 300 (เพราะ 500 > 300) ค่าปรับมากกว่า PenaltyMax
        public static IEnumerable<object[]> PercentMonthly()
        {
            yield return new object[] { 1, 3, "Active", 5000, new DateTime(2025, 10, 14), 0, 300 };
        }
        [Theory(DisplayName = "Scenario 1: PercentMonthly ตรวจสอบ ค่าปรับ > PenaltyMax")]
        [MemberData(nameof(PercentMonthly))]
        public void Scenario1_PercentMonthly_PenaltyMoreThanPenaltyMax(int userId, int penaltyPolicyID, string activeStatus, decimal outstandingBalance, DateTime dueDate, decimal paymentAmount,decimal expected)
        {
            // Arrange
            var penaltyRequest = new PenaltyRequest
            {
                UserId = userId,
                PenaltyPolicyID = penaltyPolicyID,
                ActiveStatus = activeStatus,
                OutstandingBalance = outstandingBalance,
                DueDate = dueDate,
                PaymentAmount = paymentAmount
            };
            var penaltyPoliciesRequest = new PenaltyPoliciesRequest
            {
                PenaltyPolicyID = penaltyPolicyID
            };

            SetupMockPolicies();
            SetupMockPercentagePenalty();

            var sut = new Penalty(mockPercentagePenalty.Object, mockPenaltyPolicies.Object, mockDailyPenalty.Object, mockFixedPenalty.Object);

            // Act
            var result = sut.GetPenalty(penaltyRequest);

            // Assert
            Assert.Equal(expected, result.PenaltyAmount);
            Assert.Equal(outstandingBalance * (_policyList.Single(x => x.PenaltyPolicyID == penaltyPolicyID).MinimumPaymentRate / 100), result.MinimumPayment);
            Assert.NotNull(result);
            mockPercentagePenalty.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Once);
            mockPenaltyPolicies.Verify(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()), Times.Once);

        }

        #endregion

        #region Scenario 2
        //   Scenario 2: PercentMonthly ตรวจสอบ ค่าปรับ < PenaltyMax
        //   Setup :
        //   PenaltyType = "PercentMonthly" // PercentMonthly | FixMonthly
        //   PenaltyRate = 10% (0.05)
        //   MaxPenalty(จาก Issuer Config) = 1000 บาท
        //   CurrentDate เลยช่วงผ่อนผันไปแล้ว(เช่น 2025-10-14)

        //   Given: OutstandingBalance = 7000
        //   When: คำนวณค่าปรับ(7000 * 10 / 100 = 700)
        //   Then: penaltyAmount ต้องเป็น 700 (เพราะ 700 > 1000)
        public static IEnumerable<object[]> PercentMonthly_Scenario2()
        {
            // PenaltyPolicyID = 1 (PenaltyMax = 1000), OutstandingBalance = 7000, PenaltyRate = 10%
            yield return new object[] { 1, 1, "Active", 7000, new DateTime(2025, 10, 14), 0, 700 };
        }

        [Theory(DisplayName = "Scenario 2: PercentMonthly ตรวจสอบ ค่าปรับ < PenaltyMax")]
        [MemberData(nameof(PercentMonthly_Scenario2))]
        public void Scenario2_PercentMonthly_PenaltyLessThanPenaltyMax(int userId, int penaltyPolicyID, string activeStatus, decimal outstandingBalance, DateTime dueDate, decimal paymentAmount, decimal expected)
        {
            // Arrange
            var penaltyRequest = new PenaltyRequest
            {
                UserId = userId,
                PenaltyPolicyID = penaltyPolicyID,
                ActiveStatus = activeStatus,
                OutstandingBalance = outstandingBalance,
                DueDate = dueDate,
                PaymentAmount = paymentAmount
            };
            var penaltyPoliciesRequest = new PenaltyPoliciesRequest
            {
                PenaltyPolicyID = penaltyPolicyID
            };

            SetupMockPolicies();
            SetupMockPercentagePenalty();

            var sut = new Penalty(mockPercentagePenalty.Object, mockPenaltyPolicies.Object, mockDailyPenalty.Object, mockFixedPenalty.Object);

            // Act
            var result = sut.GetPenalty(penaltyRequest);

            // Assert
            Assert.Equal(expected, result.PenaltyAmount);
            Assert.Equal(outstandingBalance * (_policyList.Single(x => x.PenaltyPolicyID == penaltyPolicyID).MinimumPaymentRate / 100), result.MinimumPayment);
            Assert.NotNull(result);
            mockPercentagePenalty.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Once);
            mockPenaltyPolicies.Verify(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()), Times.Once);
        }

        #endregion

        #region Scenario 3
        //   Scenario 3: PercentMonthly ตรวจสอบ ค่าปรับ = PenaltyMax
        //   Given: OutstandingBalance = 6000
        //   When: คำนวณค่าปรับ(6000 * 0.05 = 300)
        //   Then: penaltyAmount ต้องเป็น 300 (เพราะ 300 = 300)




        #endregion






        //   Scenario 4: FixMonthly ตรวจสอบ ค่าปรับ > PenaltyMax
        //   Given: OutstandingBalance = 6000
        //   When: PenaltyFixed = 400
        //   Then: penaltyAmount ต้องเป็น 300 (เพราะ 400 > 300)



        //   กรณีคิดค่าปรับแบบรายวัน(Daily Penalty)
        //   Setup :
        //   PenaltyType = "PercentDaily" // PercentDaily | FixDaily
        //   PenaltyRate = 0.1% (0.001) ต่อวัน
        //   PenaltyFixed = 40 ต่อวัน
        //   OutstandingBalance = 10000
        //   DueDate = 2025-10-20 
        //   GracePeriod = 3 วัน(เลยกำหนดจริงวันที่ 2025-10-24)
        //   
        //   Scenario1: PercentDaily คำนวนค่าปรับเมื่อเลยกำหนดมาแล้ว 5 วัน
        //   Given: CurrentDate = 2025-10-28 (เลยกำหนดมาแล้ว 5 วัน)
        //   When: คำนวณค่าปรับ(10000 * 0.001 * 5 วัน)
        //   Then: penaltyAmount ต้องเป็น 50
        //   
        //   When: penaltyAmount > 1000
        //   Then: penaltyAmount = 1000
        //   
        //   
        //   Scenario2: FixDaily คำนวนค่าปรับเมื่อเลยกำหนดมาแล้ว 5 วัน
        //   Given: CurrentDate = 2025-10-28 (เลยกำหนดมาแล้ว 5 วัน)
        //   When: คำนวณค่าปรับ(PenaltyFixed* 5 วัน)
        //   Then: penaltyAmount ต้องเป็น 200
        //   
        //   When: penaltyAmount > 1000
        //   Then: penaltyAmount = 1000





        #endregion

        #region Alternative case
        //Alternative cases
        //กรณีขอบเขต(Edge Cases)
        //  •	กรณี: ยอดค้างชำระ = 0
        //  •	Expected: ค่าปรับ = 0
        //  •	กรณี: อัตราค่าปรับ = 0
        //  •	Expected: ค่าปรับ = 0
        //  •	กรณี: จำนวนวันที่ผิดนัด = 0
        //  •	Expected: ค่าปรับ = 0
        //  •	กรณี: สถานะไม่ถูกต้อง(ActiveStatus ไม่ใช่ Active หรือ Inactive)
        //  •	Expected: แจ้ง validation error





        #endregion

        #region Exception case
        //- ตรวจสอบการ validate input data
        //  • UserId
        //  • PenaltyPolicyID
        //  • ActiveStatus
        //  • OutstandingBalance
        //  • DueDate
        //  • PaymentAmount
        //- ตรวจสอบ user active
        //- ตรวจสอบกรณีที่ user ถูกปรับไปแล้วในรอบนี้
        //- ตรวจสอบกรณีที่ยอดค้างชำระ = 0
        //- ตรวจสอบกรณีที่อัตราค่าปรับ = 0
        //- ตรวจสอบกรณีที่จำนวนวันที่ผิดนัด = 0
        //- ตรวจสอบกรณีที่สถานะไม่ถูกต้อง(ActiveStatus ไม่ใช่ Active หรือ Inactive)
        //- ตรวจสอบกรณีที่มีการเปลี่ยนแปลง config(อัตราค่าปรับ/Max)




        #endregion


    }
}
