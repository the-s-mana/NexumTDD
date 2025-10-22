using Moq;
using Nexum.Server.DAC;
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
        private readonly IPercentagePenalty percentagePenalty = new PercentagePenalty();
        private readonly IDailyPenalty dailyPenalty = new DailyPenalty();
        private readonly IFixedPenalty fixedPenalty = new FixedPenalty();

        public PenaltyTdd()
        {
            this.mockPenaltyPolicies = new Mock<IPenaltyPolicies>();
            this.mockPercentagePenalty = new Mock<IPercentagePenalty>();
            this.mockDailyPenalty = new Mock<IDailyPenalty>();
            this.mockFixedPenalty = new Mock<IFixedPenalty>();

            #region Mock Data
            mockPenaltyPolicies.Setup(m => m.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()))
                .Returns((PenaltyPoliciesRequest req) =>
                {
                    var policy = _policyList.SingleOrDefault(x => x.PenaltyPolicyID == req.PenaltyPolicyID);
                    return policy;
                });

            // เปลี่ยนจาก mock เป็นเรียกใช้จริง
            mockPercentagePenalty.Setup(m => m.Calculate(It.IsAny<PenaltyContext>()))
                .Returns((PenaltyContext ctx) =>
                {
                    return percentagePenalty.Calculate(ctx); // เรียกใช้เมธอด Calculate จากคลาสจริง
                });

            // ตัวอย่างการตั้งค่า mock ทดสอบ
            //mockPercentagePenalty.Setup(m => m.Calculate(It.IsAny<PenaltyContext>()))
            //.Returns((PenaltyContext ctx) =>
            //{
            //    decimal calculatedPenalty = ctx.OutstandingBalance * ctx.Percentage / 100;
            //    // คืนค่าปรับที่ไม่เกินเพดานที่กำหนด
            //    return Math.Min(calculatedPenalty, ctx.MaxPenalty);
            //});
            #endregion
        }

        #region Policy List
        private readonly List<ProductContact> _policyList = new()
        {
            new ProductContact
            {
                PenaltyPolicyID = "a8gin2xnrl4wu47mwcva",
                PenaltyPolicyx = new PenaltyPolicyDTO
                {
                    PolicyName = "Percentage Penalty",
                    PenaltyType = "Percentage",
                    PenaltyRate = 10m, //10%
                    PenaltyMax = 1000.0m,
                    PenaltyFreePeriodDays = 0,
                    MinimumPaymentRate = 10.0m, // 10%
                }
            },
            new ProductContact
            {
                PenaltyPolicyID = "a8gin2xnrl4wu47mwcva",
                PenaltyPolicyx = new PenaltyPolicyDTO
                {
                    PolicyName = "Percentage Penalty",
                    PenaltyType = "Percentage",
                    PenaltyRate = 10m, //10%
                    PenaltyMax = 600.0m,
                    PenaltyFreePeriodDays = 0,
                    MinimumPaymentRate = 10.0m, // 10%
                }
            },
            new ProductContact
            {
                PenaltyPolicyID = "a8gin2xnrl4wu47mwcva",
                PenaltyPolicyx = new PenaltyPolicyDTO
                {
                    PolicyName = "Percentage Penalty",
                    PenaltyType = "Percentage",
                    PenaltyRate = 10m, //10%
                    PenaltyMax = 300.0m,
                    PenaltyFreePeriodDays = 0,
                    MinimumPaymentRate = 10.0m, // 10%
                }
            },
            new ProductContact
            {
                PenaltyPolicyID = "a8gin2xnrl4wu47mwcva",
                PenaltyPolicyx = new PenaltyPolicyDTO
                {
                    PolicyName = "Special Daily Penalty",
                    PenaltyType = "Daily",
                    PenaltyFixed = 200.0m,
                    PenaltyMax = 400.0m,
                    TotalCap = 1200.0m,
                    PenaltyFreePeriodDays = 2,
                    MinimumPaymentRate = 10.0m, // 10%
                }
            },
        };
        #endregion

        private decimal CalculateMinimumPayment(decimal outstandingBalance, string penaltyPolicyID)
        {
            var policy = _policyList.Single(x => x.PenaltyPolicyID == penaltyPolicyID);
            return outstandingBalance * (policy.PenaltyPolicyx.MinimumPaymentRate / 100);
        }

        #region Normal case
        public static IEnumerable<object[]> PercentMonthly()
        {
            //Scenario 1: PercentMonthly ตรวจสอบ ค่าปรับ > PenaltyMax
            //   Setup :
            //   PenaltyType = "PercentMonthly" // PercentMonthly | FixMonthly
            //   PenaltyRate = 10% (0.05)
            //   MaxPenalty(จาก Issuer Config) = 300 บาท
            //   CurrentDate เลยช่วงผ่อนผันไปแล้ว(เช่น 2025-10-14)

            //   Given: OutstandingBalance = 5000
            //   When: คำนวณค่าปรับ(5000 * 10 / 100 = 500)
            //   Then: penaltyAmount ต้องเป็น 300 (เพราะ 500 > 300) ค่าปรับมากกว่า PenaltyMax
            yield return new object[] { 1, 3, "Active", 5000, new DateTime(2025, 10, 14), 0, 300 };
            //Scenario 2: PercentMonthly ตรวจสอบ ค่าปรับ<PenaltyMax
            //   Setup :
            //   PenaltyType = "PercentMonthly" // PercentMonthly | FixMonthly
            //   PenaltyRate = 10% (0.05)
            //   MaxPenalty(จาก Issuer Config) = 1000 บาท
            //   CurrentDate เลยช่วงผ่อนผันไปแล้ว(เช่น 2025-10-14)

            //   Given: OutstandingBalance = 7000
            //   When: คำนวณค่าปรับ(7000 * 10 / 100 = 700)
            //   Then: penaltyAmount ต้องเป็น 700 (เพราะ 700 > 1000)
            yield return new object[] { 1, 1, "Active", 7000, new DateTime(2025, 10, 14), 0, 700 };
            //Scenario 3: PercentMonthly ตรวจสอบ ค่าปรับ = PenaltyMax
            //   Setup :
            //   PenaltyType = "PercentMonthly" // PercentMonthly | FixMonthly
            //   PenaltyRate = 10% (0.05)
            //   MaxPenalty(จาก Issuer Config) = 600 บาท
            //   CurrentDate เลยช่วงผ่อนผันไปแล้ว(เช่น 2025-10-15)

            //   Given: OutstandingBalance = 6000
            //   When: คำนวณค่าปรับ(6000 * 10 / 100 = 600)
            //   Then: penaltyAmount ต้องเป็น 600 (เพราะ 600 = 600)

            yield return new object[] { 1, 2, "Active", 6000, new DateTime(2025, 10, 15), 0, 600 };
        }
        [Theory(DisplayName = "Normal - PercentMonthly ตรวจสอบ ค่าปรับ และต้องไม่เกิน PenaltyMax")]
        [MemberData(nameof(PercentMonthly))]
        public void Scenario1_PercentMonthly_PenaltyMoreThanPenaltyMax(int userId, string penaltyPolicyID, string activeStatus, decimal outstandingBalance, DateTime dueDate, decimal paymentAmount, decimal expected)
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
            var sut = new Penalty(mockPercentagePenalty.Object, mockPenaltyPolicies.Object, mockDailyPenalty.Object, mockFixedPenalty.Object);

            // Act
            var result = sut.GetPenalty(penaltyRequest);

            // Assert
            Assert.Equal(expected, result.PenaltyAmount);
            Assert.Equal(CalculateMinimumPayment(outstandingBalance, penaltyPolicyID), result.MinimumPayment);
            Assert.NotNull(result);
            mockPercentagePenalty.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Once);
            mockPenaltyPolicies.Verify(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()), Times.Once);
        }

        #endregion

        #region Alternative case
        public static IEnumerable<object[]> AlternativeCases()
        {
            // กรณี: ยอดค้างชำระ = 0
            yield return new object[] { 1, 1, "Active", 0m, new DateTime(2025, 10, 14), 0m, 0m };

            //// กรณี: อัตราค่าปรับ = 0
            yield return new object[] { 1, 1, "Active", 5000m, new DateTime(2025, 10, 14), 0m, 0m, 0m };

            //// กรณี: จำนวนวันที่ผิดนัด = 0
            yield return new object[] { 1, 1, "Active", 5000m, DateTime.Now.Date, 0m, 0m, 0 };

            //// กรณี: สถานะไม่ถูกต้อง(ActiveStatus ไม่ใช่ Active หรือ Inactive)
            yield return new object[] { 1, 1, "Unknown", 5000m, new DateTime(2025, 10, 14), 0m, typeof(ArgumentException) };
        }

        [Theory(DisplayName = "Alternative - กรณีขอบเขต(Edge Cases)")]
        [MemberData(nameof(AlternativeCases))]
        public void AlternativeCase_PenaltyCalculation(
            int userId,
            string penaltyPolicyID,
            string activeStatus,
            decimal outstandingBalance,
            DateTime dueDate,
            decimal paymentAmount,
            object expected,
            int overdueDays = 1)
        {
            var penaltyRequest = new PenaltyRequest
            {
                UserId = userId,
                PenaltyPolicyID = penaltyPolicyID,
                ActiveStatus = activeStatus,
                OutstandingBalance = outstandingBalance,
                DueDate = dueDate,
                PaymentAmount = paymentAmount
            };
            var sut = new Penalty(mockPercentagePenalty.Object, mockPenaltyPolicies.Object, mockDailyPenalty.Object, mockFixedPenalty.Object);

            if (expected is Type exceptionType)
            {
                Assert.Throws(exceptionType, () => sut.GetPenalty(penaltyRequest));
            }
            else
            {
                var result = sut.GetPenalty(penaltyRequest);
                Assert.Equal((decimal)expected, result.PenaltyAmount);
                Assert.NotNull(result);
            }
        }

        #endregion

        #region Exception case
        //- ตรวจสอบการ validate input data
        //  • UserId
        //  • PenaltyPolicyID
        //  • ActiveStatus
        //  • OutstandingBalance
        //  • DueDate
        //  • PaymentAmount
        [Theory(DisplayName = "Exception - Validate input data")]
        [InlineData(0, "a8gin2xnrl4wu47mwcva", "Active", 5000, "2025-10-14", 0, typeof(ArgumentException), "UserId")]
        [InlineData(1, "a8gin2xnrl4wu47mwcva", "Active", 5000, "2025-10-14", 0, typeof(ArgumentException), "PenaltyPolicyID")]
        [InlineData(1, "a8gin2xnrl4wu47mwcva", "", 5000, "2025-10-14", 0, typeof(ArgumentException), "ActiveStatus")]
        [InlineData(1, "a8gin2xnrl4wu47mwcva", "Active", -100, "2025-10-14", 0, typeof(ArgumentException), "OutstandingBalance")]
        [InlineData(1, "a8gin2xnrl4wu47mwcva", "Active", 5000, "", 0, typeof(ArgumentException), "DueDate")]
        [InlineData(1, "a8gin2xnrl4wu47mwcva", "Active", 5000, "2025-10-14", -10, typeof(ArgumentException), "PaymentAmount")]
        public void ValidateInputData_ShouldThrowException(
            int userId,
            string penaltyPolicyID,
            string activeStatus,
            decimal outstandingBalance,
            string dueDateStr,
            decimal paymentAmount,
            Type expectedException,
            string paramName)
        {
            DateTime dueDate = string.IsNullOrWhiteSpace(dueDateStr) ? default : DateTime.Parse(dueDateStr);
            var penaltyRequest = new PenaltyRequest
            {
                UserId = userId,
                PenaltyPolicyID = penaltyPolicyID,
                ActiveStatus = activeStatus,
                OutstandingBalance = outstandingBalance,
                DueDate = dueDate,
                PaymentAmount = paymentAmount
            };
            var sut = new Penalty(mockPercentagePenalty.Object, mockPenaltyPolicies.Object, mockDailyPenalty.Object, mockFixedPenalty.Object);

            var ex = Assert.Throws(expectedException, () => sut.GetPenalty(penaltyRequest));
            Assert.Contains(paramName, ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        //- ตรวจสอบ user active
        [Theory(DisplayName = "Exception - ตรวจสอบ user active")]
        [InlineData(1, "a8gin2xnrl4wu47mwcva", false, "Active", 5000, "2025-10-14", 0, typeof(InvalidOperationException))]
        [InlineData(2, "a8gin2xnrl4wu47mwcva", false, "Active", 7000, "2025-10-14", 0, typeof(InvalidOperationException))]
        public void ValidateUserActive_ShouldThrowException(
            int userId,
            string penaltyPolicyID,
            bool isActive,
            string activeStatus,
            decimal outstandingBalance,
            string dueDateStr,
            decimal paymentAmount,
            Type expectedException)
        {
            DateTime dueDate = string.IsNullOrWhiteSpace(dueDateStr) ? default : DateTime.Parse(dueDateStr);
            // Mock policy to set Active property
            var policy = _policyList.Single(x => x.PenaltyPolicyID == penaltyPolicyID);
            policy.Active = isActive;
            mockPenaltyPolicies.Setup(m => m.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()))
                .Returns(policy);

            var penaltyRequest = new PenaltyRequest
            {
                UserId = userId,
                PenaltyPolicyID = penaltyPolicyID,
                ActiveStatus = activeStatus,
                OutstandingBalance = outstandingBalance,
                DueDate = dueDate,
                PaymentAmount = paymentAmount
            };
            var sut = new Penalty(mockPercentagePenalty.Object, mockPenaltyPolicies.Object, mockDailyPenalty.Object, mockFixedPenalty.Object);

            var ex = Assert.Throws(expectedException, () => sut.GetPenalty(penaltyRequest));
            Assert.Contains("active", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        //- ตรวจสอบกรณีที่ user ถูกปรับไปแล้วในรอบนี้

        //- ตรวจสอบกรณีที่ยอดค้างชำระ = 0

        //- ตรวจสอบกรณีที่อัตราค่าปรับ = 0

        //- ตรวจสอบกรณีที่จำนวนวันที่ผิดนัด = 0

        //- ตรวจสอบกรณีที่สถานะไม่ถูกต้อง(ActiveStatus ไม่ใช่ Active หรือ Inactive)

        //- ตรวจสอบกรณีที่มีการเปลี่ยนแปลง config(อัตราค่าปรับ/Max)

        #endregion


    }
}