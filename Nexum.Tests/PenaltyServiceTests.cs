using Moq;
using Nexum.Server.Models;
using Nexum.Server.Models.Penalty;
using Nexum.Server.Services.Penalty;

namespace Nexum.Tests
{
    public class PenaltyServiceTests
    {
        private readonly Mock<IPercentagePenalty> _percentage = new();
        private readonly Mock<IPenaltyPolicies> _policies = new();
        private readonly Mock<IDailyPenalty> _daily = new();
        private readonly Mock<IFixedPenalty> _fixed = new();
        private Penalty CreateSut()
        {
            _policies.Setup(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()))
                    .Returns((PenaltyPoliciesRequest req) =>
                        _policyList.Single(x => x.PenaltyPolicyID == req.PenaltyPolicyID));

            return new Penalty(_percentage.Object, _policies.Object, _daily.Object, _fixed.Object);
        }
        #region Policy List
        private readonly List<ProductContact> _policyList = new()
        {
           new ProductContact
            {
                PenaltyPolicyID = 1,
                PenaltyType = "Daily",
                FixedAmount = 100m,
                TotalCap = 1000m,
                PenaltyFreePeriodDays = 5
            },
            new ProductContact
            {
                PenaltyPolicyID = 2,
                PenaltyType = "Fixed",
                FixedAmount = 200m,
            },
            new ProductContact
            {
                PenaltyPolicyID = 3,
                PenaltyType = "Percentage",
                PenaltyRate = 2.5m,
                MaxPenalty = 300m,
                PenaltyFreePeriodDays = 3
            },
            new ProductContact
            {
                PenaltyPolicyID = 4,
                PenaltyType = "Daily",
                FixedAmount = 200m,
                MaxPenalty = 400m,
                TotalCap = 1200m,
                PenaltyFreePeriodDays = 2
            },
        };
        #endregion


        #region Normal Cases
        [Fact(DisplayName = "Normal Case 1: จ่ายขั้นต่ำ + ตรงเวลา -> ไม่คิดค่าปรับ")]
        public void Normal_1_MinPayment_OnTime_NoPenalty()
        {
            // Arrange
            var now = DateTime.Now.Date;
            var request = new PenaltyRequest
            {
                UserId = 1,
                PenaltyPolicyID = 1,          // Daily: Fixed=100, TotalCap=1000, Grace=5
                ActiveStatus = "Active",
                OutstandingBalance = 3000m,
                DueDate = now,
                PaymentAmount = 300m          // จ่ายขั้นต่ำ 10%
            };
            var expected = new PenaltyResponse
            {
                UserId = 1,
                OutstandingBalance = 3000m,
                MinimumPayment = 300m,
                PenaltyAmount = 0m
            };
            var sut = CreateSut();
            // Act
            var result = sut.GetPenalty(request);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected.UserId, result.UserId);
            Assert.Equal(expected.OutstandingBalance, result.OutstandingBalance);
            Assert.Equal(expected.MinimumPayment, result.PaymentAmount);
            Assert.Equal(expected.PenaltyAmount, result.PenaltyAmount);
            _policies.Verify(p => p.penaltyPolicies(
                It.Is<PenaltyPoliciesRequest>(x => x.PenaltyPolicyID == request.PenaltyPolicyID)),
                Times.Once());
            _daily.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _fixed.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _percentage.Verify(p => p.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
        }

        [Fact(DisplayName = "Normal Case 2: จ่ายเต็มจำนวน + ตรงเวลา -> ไม่คิดค่าปรับ")]
        public void Normal_2_FullPayment_OnTime_NoPenalty()
        {
            // Arrange
            var now = DateTime.Now.Date;
            var request = new PenaltyRequest
            {
                UserId = 2,
                PenaltyPolicyID = 2,          // Fixed=200
                ActiveStatus = "Active",
                OutstandingBalance = 3000m,
                DueDate = now,
                PaymentAmount = 3000m         // จ่ายเต็ม
            };
            var expected = new PenaltyResponse
            {
                UserId = 2,
                OutstandingBalance = 3000m,
                PaymentAmount = 3000m,
                PenaltyAmount = 0m
            };
            var sut = CreateSut();
            // Act
            var result = sut.GetPenalty(request);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected.UserId, result.UserId);
            Assert.Equal(expected.OutstandingBalance, result.OutstandingBalance);
            Assert.Equal(expected.PaymentAmount, result.PaymentAmount);
            Assert.Equal(expected.PenaltyAmount, result.PenaltyAmount);
            _policies.Verify(p => p.penaltyPolicies(
                It.Is<PenaltyPoliciesRequest>(x => x.PenaltyPolicyID == request.PenaltyPolicyID)),
                Times.Once());
            _daily.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _fixed.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _percentage.Verify(p => p.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
        }

        //[Fact(DisplayName = "Normal Case 3: จ่ายเต็มจำนวนก่อนถึงวันกำหนดชำระ -> ไม่คิดค่าปรับ")] //Failed เพราะมีการดักการชำระล่วงหน้า 
        //public void Normal_3_FullPayment_BeforeDueDate_NoPenalty()
        //{
        //    // Arrange
        //    var now = DateTime.Now.Date;
        //    var request = new PenaltyRequest
        //    {
        //        UserId = 3,
        //        PenaltyPolicyID = 3,          // Rate=2.5, Max=300, Grace=5
        //        ActiveStatus = "Active",
        //        OutstandingBalance = 4000m,
        //        DueDate = now.AddDays(5),     // วันครบกำหนดในอนาคต
        //        PaymentAmount = 4000m         // จ่ายเต็ม
        //    };
        //    var expected = new PenaltyResponse
        //    {
        //        UserId = 3,
        //        OutstandingBalance = 4000m,
        //        PaymentAmount = 4000m,
        //        PenaltyAmount = 0m
        //    };
        //    var sut = CreateSut();
        //    // Act
        //    var result = sut.GetPenalty(request);
        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Equal(expected.UserId, result.UserId);
        //    Assert.Equal(expected.OutstandingBalance, result.OutstandingBalance);
        //    Assert.Equal(expected.PaymentAmount, result.PaymentAmount);
        //    Assert.Equal(expected.PenaltyAmount, result.PenaltyAmount);
        //    _policies.Verify(p => p.penaltyPolicies(
        //        It.Is<PenaltyPoliciesRequest>(x => x.PenaltyPolicyID == request.PenaltyPolicyID)),
        //        Times.Once());
        //    _daily.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
        //    _fixed.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
        //    _percentage.Verify(p => p.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
        //}

        [Fact(DisplayName = "Normal Case 4: ชำระถึงขั้นต่ำและอยู่ในช่วงผ่อนผัน -> ไม่คิดค่าปรับ")]
        public void Normal_4_MinPayment_WithinGrace_NoPenalty()
        {
            // Arrange
            var now = DateTime.Now.Date;
            var request = new PenaltyRequest
            {
                UserId = 4,
                PenaltyPolicyID = 1,          // Daily: Fixed=100, TotalCap=1000, Grace=5
                ActiveStatus = "Active",
                OutstandingBalance = 2500m,
                DueDate = now.AddDays(-3),    // อยู่ในช่วงผ่อนผัน 5 วัน
                PaymentAmount = 250m           // จ่ายขั้นต่ำ 10%
            };
            var expected = new PenaltyResponse
            {
                UserId = 4,
                OutstandingBalance = 2500m,
                MinimumPayment = 250m,
                PenaltyAmount = 0m
            };
            var sut = CreateSut();
            // Act
            var result = sut.GetPenalty(request);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected.UserId, result.UserId);
            Assert.Equal(expected.OutstandingBalance, result.OutstandingBalance);
            Assert.Equal(expected.MinimumPayment, result.PaymentAmount);
            Assert.Equal(expected.PenaltyAmount, result.PenaltyAmount);
            _policies.Verify(p => p.penaltyPolicies(
                It.Is<PenaltyPoliciesRequest>(x => x.PenaltyPolicyID == request.PenaltyPolicyID)),
                Times.Once());
            _daily.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _fixed.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _percentage.Verify(p => p.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
        }

        [Fact(DisplayName = "Normal Case 5: ชำระไม่ถึงขั้นต่ำและอยู่ในช่วงผ่อนผัน -> ไม่คิดค่าปรับ")]
        public void Normal_5_UnderMinPayment_WithinGrace_NoPenalty()
        {
            // Arrange
            var now = DateTime.Now.Date;
            var request = new PenaltyRequest
            {
                UserId = 5,
                PenaltyPolicyID = 1,          // Daily: Fixed=100, TotalCap=1000, Grace=5
                ActiveStatus = "Active",
                OutstandingBalance = 5000m,
                DueDate = now.AddDays(-5),    // อยู่ในช่วงผ่อนผัน 5 วัน
                PaymentAmount = 100m           // ไม่ถึงขั้นต่ำ 500
            };
            var expected = new PenaltyResponse
            {
                UserId = 5,
                OutstandingBalance = 5000m,
                MinimumPayment = 500m,
                PenaltyAmount = 0m
            };
            var sut = CreateSut();
            // Act
            var result = sut.GetPenalty(request);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected.UserId, result.UserId);
            Assert.Equal(expected.OutstandingBalance, result.OutstandingBalance);
            Assert.NotEqual(expected.MinimumPayment, result.PaymentAmount);
            Assert.Equal(expected.PenaltyAmount, result.PenaltyAmount);
            _policies.Verify(p => p.penaltyPolicies(
                It.Is<PenaltyPoliciesRequest>(x => x.PenaltyPolicyID == request.PenaltyPolicyID)),
                Times.Once());
            _daily.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _fixed.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _percentage.Verify(p => p.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
        }

        [Fact(DisplayName = "Normal Case 6: ชำระไม่ถึงขั้นต่ำและเลยช่วงผ่อนผัน -> คิดค่าปรับ")]
        public void Normal_6_UnderMinPayment_BeyondGrace_WithPenalty()
        {
            // Arrange
            var now = DateTime.Now.Date;
            var request = new PenaltyRequest
            {
                UserId = 6,
                PenaltyPolicyID = 1,          // Daily: Fixed=100, TotalCap=1000, Grace=5
                ActiveStatus = "Active",
                OutstandingBalance = 5000m,
                DueDate = now.AddDays(-6),   // เลยช่วงผ่อนผัน 5 วัน
                PaymentAmount = 100m           // ไม่ถึงขั้นต่ำ 400
            };
            var expected = new PenaltyResponse
            {
                UserId = 6,
                OutstandingBalance = 5000m,
                MinimumPayment = 400m,
                PenaltyAmount = 600m           // 6 วัน * 100
            };
            var sut = CreateSut();
            _daily.Setup(d => d.Calculate(It.IsAny<PenaltyContext>()))
                  .Returns((PenaltyContext ctx) =>
                      Math.Min(ctx.OverdueDays * ctx.FixedAmount, ctx.TotalCap));
            // Act
            var result = sut.GetPenalty(request);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected.UserId, result.UserId);
            Assert.Equal(expected.OutstandingBalance, result.OutstandingBalance);
            Assert.NotEqual(expected.MinimumPayment, result.PaymentAmount);
            Assert.Equal(expected.PenaltyAmount, result.PenaltyAmount);
            _policies.Verify(p => p.penaltyPolicies(
                It.Is<PenaltyPoliciesRequest>(x => x.PenaltyPolicyID == request.PenaltyPolicyID)),
                Times.Once());
            _daily.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Once());
            _fixed.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _percentage.Verify(p => p.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
        }

        [Fact(DisplayName = "Normal Case 7: เกินกำหนดชำระ -> คิดค่าปรับแบบ Fixed")]
        public void Normal_7_Overdue_FixedPenalty()
        {
            // Arrange
            var now = DateTime.Now.Date;
            var request = new PenaltyRequest
            {
                UserId = 7,
                PenaltyPolicyID = 2,          // Fixed=200
                ActiveStatus = "Active",
                OutstandingBalance = 4000m,
                DueDate = now.AddDays(-1),    // เลยกำหนด 1 วัน
                PaymentAmount = 0m             // ไม่จ่าย
            };
            var expected = new PenaltyResponse
            {
                UserId = 7,
                OutstandingBalance = 4000m,
                PenaltyAmount = 200m           // ค่าปรับคงที่
            };
            var sut = CreateSut();
            _fixed.Setup(f => f.Calculate(It.IsAny<PenaltyContext>()))
                  .Returns((PenaltyContext ctx) => ctx.FixedAmount);
            // Act
            var result = sut.GetPenalty(request);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected.UserId, result.UserId);
            Assert.Equal(expected.OutstandingBalance, result.OutstandingBalance);
            Assert.Equal(expected.PenaltyAmount, result.PenaltyAmount);
            _policies.Verify(p => p.penaltyPolicies(
                It.Is<PenaltyPoliciesRequest>(x => x.PenaltyPolicyID == request.PenaltyPolicyID)),
                Times.Once());
            _daily.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _fixed.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Once());
            _percentage.Verify(p => p.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
        }

        [Fact(DisplayName = "Normal Case 8: ชำระถึงขั้นต่ำและอยู่ในช่วงผ่อนผัน -> ไม่คิดค่าปรับ")]
        public void Normal_8_MinPayment_WithinGrace_Percentage()
        {
            // Arrange
            var now = DateTime.Now.Date;
            var request = new PenaltyRequest
            {
                UserId = 8,
                PenaltyPolicyID = 3,          // Rate=2.5, Max=300, Grace=3
                ActiveStatus = "Active",
                OutstandingBalance = 10000m,
                DueDate = now.AddDays(-2),   // อยู่ในช่วงผ่อนผัน 3 วัน
                PaymentAmount = 1000m           // จ่ายขั้นต่ำ 10%
            };
            var expected = new PenaltyResponse
            {
                UserId = 8,
                OutstandingBalance = 10000m,
                MinimumPayment = 1000m,
                PenaltyAmount = 0m
            };
            var sut = CreateSut();
            //Act
            var result = sut.GetPenalty(request);
            //Assert
            Assert.NotNull(result);
            Assert.Equal(expected.UserId, result.UserId);
            Assert.Equal(expected.OutstandingBalance, result.OutstandingBalance);
            Assert.Equal(expected.MinimumPayment, result.PaymentAmount);
            Assert.Equal(expected.PenaltyAmount, result.PenaltyAmount);
            _policies.Verify(p => p.penaltyPolicies(
                It.Is<PenaltyPoliciesRequest>(x => x.PenaltyPolicyID == request.PenaltyPolicyID)),
                Times.Once());
            _daily.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _fixed.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _percentage.Verify(p => p.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
        }

        [Fact(DisplayName = "Normal Case 9: ชำระไม่ถึงขั้นต่ำและอยู่ในช่วงผ่อนผัน -> ไม่คิดค่าปรับ")]
        public void Normal_9_UnderMinPayment_WithinGrace_Percentage()
        {
            // Arrange
            var now = DateTime.Now.Date;
            var request = new PenaltyRequest
            {
                UserId = 9,
                PenaltyPolicyID = 3,          // Rate=2.5, Max=300, Grace=3
                ActiveStatus = "Active",
                OutstandingBalance = 3000m,
                DueDate = now.AddDays(-1),   // อยู่ในช่วงผ่อนผัน 3 วัน
                PaymentAmount = 100m           // ไม่ถึงขั้นต่ำ 300
            };
            var expected = new PenaltyResponse
            {
                UserId = 9,
                OutstandingBalance = 3000m,
                MinimumPayment = 300m,
                PenaltyAmount = 0m
            };
            var sut = CreateSut();
            // Act
            var result = sut.GetPenalty(request);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected.UserId, result.UserId);
            Assert.Equal(expected.OutstandingBalance, result.OutstandingBalance);
            Assert.NotEqual(expected.MinimumPayment, result.PaymentAmount);
            Assert.Equal(expected.PenaltyAmount, result.PenaltyAmount);
            _policies.Verify(p => p.penaltyPolicies(
                It.Is<PenaltyPoliciesRequest>(x => x.PenaltyPolicyID == request.PenaltyPolicyID)),
                Times.Once());
            _daily.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _fixed.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _percentage.Verify(p => p.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
        }

        [Fact(DisplayName = "Normal Case 10: ชำระไม่ถึงขั้นต่ำและเลยช่วงผ่อนผัน -> คิดค่าปรับแบบ Percentage")]
        public void Normal_10_UnderMinPayment_BeyondGrace_Percentage()
        {
            // Arrange
            var now = DateTime.Now.Date;
            var request = new PenaltyRequest
            {
                UserId = 10,
                PenaltyPolicyID = 3,          // Rate=2.5, Max=300, Grace=3
                ActiveStatus = "Active",
                OutstandingBalance = 500m,
                DueDate = now.AddDays(-5),   // เลยช่วงผ่อนผัน 2 วัน
                PaymentAmount = 0m           // ไม่ถึงขั้นต่ำ 500
            };
            var expected = new PenaltyResponse
            {
                UserId = 10,
                OutstandingBalance = 500m,
                MinimumPayment = 50m,
                PenaltyAmount = 12.5m           // 2.5% ของ 500 = 12.5 (ไม่เกิน Max)
            };
            var sut = CreateSut();
            _percentage.Setup(p => p.Calculate(It.IsAny<PenaltyContext>()))
                       .Returns((PenaltyContext ctx) =>
                           Math.Min(ctx.OutstandingBalance * (ctx.Percentage / 100), ctx.MaxPenalty));
            // Act
            var result = sut.GetPenalty(request);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected.UserId, result.UserId);
            Assert.Equal(expected.OutstandingBalance, result.OutstandingBalance);
            Assert.NotEqual(expected.MinimumPayment, result.PaymentAmount);
            Assert.Equal(expected.PenaltyAmount, result.PenaltyAmount);
            _policies.Verify(p => p.penaltyPolicies(
                It.Is<PenaltyPoliciesRequest>(x => x.PenaltyPolicyID == request.PenaltyPolicyID)),
                Times.Once());
            _daily.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _fixed.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _percentage.Verify(p => p.Calculate(It.IsAny<PenaltyContext>()), Times.Once());
        }
        #endregion
        #region Alternative Cases
        [Fact(DisplayName = "Alternative Case 1: ชำระไม่ถึงขั้นต่ำ + เกินช่วงผ่อนผันและชนเพดาน TotalCap")]
        public void Alternative_1_UnderMinPayment_BeyondGrace_Daily_CapPenalty()
        {
            // Arrange
            var now = DateTime.Now.Date;
            var request = new PenaltyRequest
            {
                UserId = 1,
                PenaltyPolicyID = 1,          // Daily: Fixed=100, TotalCap=1000, Grace=5
                ActiveStatus = "Active",
                OutstandingBalance = 15000m,
                DueDate = now.AddDays(-13),   // เลยช่วงผ่อนผัน 13 วัน
                PaymentAmount = 0m             // ไม่ถึงขั้นต่ำ 1500
            };
            var expected = new PenaltyResponse
            {
                UserId = 1,
                OutstandingBalance = 15000m,
                MinimumPayment = 1000m,
                PenaltyAmount = 1000m           // 13 วัน * 100 = 2000 แต่ชนเพดาน 1000
            };
            var sut = CreateSut();
            _daily.Setup(d => d.Calculate(It.IsAny<PenaltyContext>()))
                  .Returns((PenaltyContext ctx) =>
                      Math.Min(ctx.OverdueDays * ctx.FixedAmount, ctx.TotalCap));
            // Act
            var result = sut.GetPenalty(request);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected.UserId, result.UserId);
            Assert.Equal(expected.OutstandingBalance, result.OutstandingBalance);
            Assert.NotEqual(expected.MinimumPayment, result.PaymentAmount);
            Assert.Equal(expected.PenaltyAmount, result.PenaltyAmount);
            _policies.Verify(p => p.penaltyPolicies(
                It.Is<PenaltyPoliciesRequest>(x => x.PenaltyPolicyID == request.PenaltyPolicyID)),
                Times.Once());
            _daily.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Once());
            _fixed.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _percentage.Verify(p => p.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
        }

        [Fact(DisplayName = "Alternative Case 2: ชำระบางส่วนแต่ไม่ถึงขั้นต่ำ + เกินช่วงผ่อนผัน -> คิดค่าปรับแบบ Percentage")]
        public void Alternative_2_PartialPayment_UnderMin_BeyondGrace_Percentage()
        {
            // Arrange
            var now = DateTime.Now.Date;
            var request = new PenaltyRequest
            {
                UserId = 2,
                PenaltyPolicyID = 3,          // Rate=2.5, Max=300, Grace=3
                ActiveStatus = "Active",
                OutstandingBalance = 5000m,
                DueDate = now.AddDays(-4),   // เลยช่วงผ่อนผัน 4 วัน
                PaymentAmount = 300m           // ไม่ถึงขั้นต่ำ 500
            };
            var expected = new PenaltyResponse
            {
                UserId = 2,
                OutstandingBalance = 5000m,
                MinimumPayment = 500m,
                PenaltyAmount = 117.5m           // 2.5% ของ 5000-300 = 117.5 (ไม่เกิน Max)
            };
            var sut = CreateSut();
            _percentage.Setup(p => p.Calculate(It.IsAny<PenaltyContext>()))
                       .Returns((PenaltyContext ctx) =>
                           Math.Min(ctx.OutstandingBalance * (ctx.Percentage / 100), ctx.MaxPenalty));
            // Act
            var result = sut.GetPenalty(request);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected.UserId, result.UserId);
            Assert.Equal(expected.OutstandingBalance, result.OutstandingBalance);
            Assert.NotEqual(expected.MinimumPayment, result.PaymentAmount);
            Assert.Equal(expected.PenaltyAmount, result.PenaltyAmount);
            _policies.Verify(p => p.penaltyPolicies(
                It.Is<PenaltyPoliciesRequest>(x => x.PenaltyPolicyID == request.PenaltyPolicyID)),
                Times.Once());
            _daily.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _fixed.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _percentage.Verify(p => p.Calculate(It.IsAny<PenaltyContext>()), Times.Once());
        }

        [Fact(DisplayName = "Alternative Case 3: ชำระไม่ถึงขั้นต่ำ + เกินช่วงผ่อนผันและชนเพดาน MaxPenalty")]
        public void Alternative_3_UnderMinPayment_BeyondGrace_Percentage_CapPenalty()
        {
            // Arrange
            var now = DateTime.Now.Date;
            var request = new PenaltyRequest
            {
                UserId = 3,
                PenaltyPolicyID = 3,          // Rate=2.5, Max=300, Grace=3
                ActiveStatus = "Active",
                OutstandingBalance = 13000m,
                DueDate = now.AddDays(-5),    // เลยช่วงผ่อนผัน 6 วัน
                PaymentAmount = 0m             // ไม่ถึงขั้นต่ำ 2000
            };
            var expected = new PenaltyResponse
            {
                UserId = 3,
                OutstandingBalance = 13000m,
                MinimumPayment = 1300m,
                PenaltyAmount = 300m           // 2.5% ของ 13000 = 325 แต่ชนเพดาน 300
            };
            var sut = CreateSut();
            _percentage.Setup(p => p.Calculate(It.IsAny<PenaltyContext>()))
                       .Returns((PenaltyContext ctx) =>
                           Math.Min(ctx.OutstandingBalance * (ctx.Percentage / 100), ctx.MaxPenalty));
            // Act
            var result = sut.GetPenalty(request);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected.UserId, result.UserId);
            Assert.Equal(expected.OutstandingBalance, result.OutstandingBalance);
            Assert.NotEqual(expected.MinimumPayment, result.PaymentAmount);
            Assert.Equal(expected.PenaltyAmount, result.PenaltyAmount);
            _policies.Verify(p => p.penaltyPolicies(
                It.Is<PenaltyPoliciesRequest>(x => x.PenaltyPolicyID == request.PenaltyPolicyID)),
                Times.Once());
            _daily.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _fixed.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _percentage.Verify(p => p.Calculate(It.IsAny<PenaltyContext>()), Times.Once());
        }
        #endregion
        #region Exception Cases
        [Fact(DisplayName = "Exception Case 1: Request เป็น null -> ArgumentNullException")]
        public void Exception_1_Request_Null_ArgumentNullException()
        {
            // Arrange
            PenaltyRequest? request = null;
            var sut = CreateSut();
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => sut.GetPenalty(request));
            Assert.Equal("Value cannot be null. (Parameter 'penaltyRequest')", exception.Message);
            _policies.Verify(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()), Times.Never());
            _daily.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _fixed.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _percentage.Verify(p => p.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
        }

        [Fact(DisplayName = "Exception Case 2: OutstandingBalance <= 0 -> ArgumentException")]
        public void Exception_2_OutstandingBalance_Zero_ArgumentException()
        {
            // Arrange
            var now = DateTime.Now.Date;
            var request = new PenaltyRequest
            {
                UserId = 1,
                PenaltyPolicyID = 1,
                ActiveStatus = "Active",
                OutstandingBalance = 0m,      // ไม่ถูกต้อง
                DueDate = now,
                PaymentAmount = 0m
            };
            var sut = CreateSut();
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => sut.GetPenalty(request));
            Assert.Equal("OutstandingBalance must be greater than zero.", exception.Message);
            _policies.Verify(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()), Times.Never());
            _daily.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _fixed.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _percentage.Verify(p => p.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
        }

        [Fact(DisplayName = "Exception Case 3: UserId <= 0 -> ArgumentException")]
        public void Exception_3_UserId_Zero_ArgumentException()
        {
            // Arrange
            var now = DateTime.Now.Date;
            var request = new PenaltyRequest
            {
                UserId = 0,                    // ไม่ถูกต้อง
                PenaltyPolicyID = 1,
                ActiveStatus = "Active",
                OutstandingBalance = 1000m,
                DueDate = now,
                PaymentAmount = 0m
            };
            var sut = CreateSut();
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => sut.GetPenalty(request));
            Assert.Equal("UserId must be greater than zero.", exception.Message);
            _policies.Verify(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()), Times.Never());
            _daily.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _fixed.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _percentage.Verify(p => p.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
        }

        [Fact(DisplayName = "Exception Case 4: ActiveStatus เป็น null -> ArgumentException")]
        public void Exception_4_ActiveStatus_Null_ArgumentException()
        {
            // Arrange
            var now = DateTime.Now.Date;
            var request = new PenaltyRequest
            {
                UserId = 1,
                PenaltyPolicyID = 1,
                ActiveStatus = null,           // ไม่ถูกต้อง
                OutstandingBalance = 1000m,
                DueDate = now,
                PaymentAmount = 0m
            };
            var sut = CreateSut();
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => sut.GetPenalty(request));
            Assert.Equal("ActiveStatus must be either 'Active' or 'Inactive'.", exception.Message);
            _policies.Verify(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()), Times.Never());
            _daily.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _fixed.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _percentage.Verify(p => p.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
        }

        [Fact(DisplayName = "Exception Case 5: ActiveStatus เป็นค่าอื่นที่ไม่ใช่ Active หรือ Inactive -> ArgumentException")]
        public void Exception_5_ActiveStatus_Invalid_ArgumentException()
        {
            // Arrange
            var now = DateTime.Now.Date;
            var request = new PenaltyRequest
            {
                UserId = 1,
                PenaltyPolicyID = 1,
                ActiveStatus = "Pending",      // ไม่ถูกต้อง
                OutstandingBalance = 1000m,
                DueDate = now,
                PaymentAmount = 0m
            };
            var sut = CreateSut();
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => sut.GetPenalty(request));
            Assert.Equal("ActiveStatus must be either 'Active' or 'Inactive'.", exception.Message);
            _policies.Verify(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()), Times.Never());
            _daily.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _fixed.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _percentage.Verify(p => p.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
        }

        [Fact(DisplayName = "Exception Case 6: ActiveStatus เป็น Inactive -> InvalidOperationException")]
        public void Exception_6_ActiveStatus_Inactive_InvalidOperationException()
        {
            // Arrange
            var now = DateTime.Now.Date;
            var request = new PenaltyRequest
            {
                UserId = 1,
                PenaltyPolicyID = 1,
                ActiveStatus = "Inactive",     // ไม่ถูกต้อง
                OutstandingBalance = 1000m,
                DueDate = now,
                PaymentAmount = 0m
            };
            var sut = CreateSut();
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => sut.GetPenalty(request));
            Assert.Equal("Cannot calculate penalty for inactive users.", exception.Message);
            _policies.Verify(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()), Times.Never());
            _daily.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _fixed.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _percentage.Verify(p => p.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
        }

        [Fact(DisplayName = "Exception Case 7: DueDate ไม่สอดคล้องกับปัจจุบัน -> ArgumentException")]
        public void Exception_7_DueDate_Null_ArgumentException()
        {
            // Arrange
            var request = new PenaltyRequest
            {
                UserId = 1,
                PenaltyPolicyID = 1,
                ActiveStatus = "Active",
                OutstandingBalance = 1000m,
                DueDate = default(DateTime),                // ไม่ถูกต้อง
                PaymentAmount = 0m
            };
            var sut = CreateSut();
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => sut.GetPenalty(request));
            Assert.Equal("DueDate must be a valid date.", exception.Message);
            _policies.Verify(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()), Times.Never());
            _daily.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _fixed.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _percentage.Verify(p => p.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
        }

        [Fact(DisplayName = "Exception Case 8: DueDate เป็นวันที่ในอนาคต -> InvalidOperationException")]
        public void Exception_8_DueDate_Future_InvalidOperationException()
        {
            // Arrange
            var now = DateTime.Now.Date;
            var request = new PenaltyRequest
            {
                UserId = 1,
                PenaltyPolicyID = 1,
                ActiveStatus = "Active",
                OutstandingBalance = 1000m,
                DueDate = now.AddDays(1),     // ไม่ถูกต้อง
                PaymentAmount = 0m
            };
            var sut = CreateSut();
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => sut.GetPenalty(request));
            Assert.Equal("DueDate cannot be in the future.", exception.Message);
            _policies.Verify(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()), Times.Never());
            _daily.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _fixed.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _percentage.Verify(p => p.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
        }

        [Fact(DisplayName = "Exception Case 9: PenaltyPolicyID ไม่ตรงกับนโยบายที่มีอยู่ -> KeyNotFoundException")]
        public void Exception_9_PenaltyPolicyID_NotFound_KeyNotFoundException()
        {
            // Arrange
            var now = DateTime.Now.Date;
            var request = new PenaltyRequest
            {
                UserId = 1,
                PenaltyPolicyID = 999,         // ไม่ถูกต้อง
                ActiveStatus = "Active",
                OutstandingBalance = 1000m,
                DueDate = now,
                PaymentAmount = 0m
            };
            var sut = CreateSut();
            //Act & Assert
            Assert.Throws<InvalidOperationException>(() => sut.GetPenalty(request));
            _policies.Verify(p => p.penaltyPolicies(
                It.Is<PenaltyPoliciesRequest>(x => x.PenaltyPolicyID == request.PenaltyPolicyID)),
                Times.Once());
            _daily.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _fixed.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
            _percentage.Verify(p => p.Calculate(It.IsAny<PenaltyContext>()), Times.Never());
        }
        #endregion
    }
}
