using Moq;
using Nexum.Server.DAC;
using Nexum.Server.Models;
using Nexum.Server.Models.Penalty;
using Nexum.Server.Services;
using Nexum.Server.Services.Penalty;
using System.Diagnostics;
using Xunit;

namespace Nexum.Tests
{

    public class TestCase_Date20251015
    {
        //🔧 Field-level mocks
        private readonly Mock<IPenaltyPolicies> _mockPenaltyPolicies;
        private readonly Mock<IDailyPenalty> _mockDailyPenalty;
        private readonly Mock<IFixedPenalty> _mockFixedPenalty;
        private readonly Mock<IPercentagePenalty> _mockPercentagePenalty;

        //🔧 System Under Test
        private readonly Penalty _sut;

        public TestCase_Date20251015()
        {
            //✅ Initialize mocks
            _mockPenaltyPolicies = new Mock<IPenaltyPolicies>();
            _mockDailyPenalty = new Mock<IDailyPenalty>();
            _mockFixedPenalty = new Mock<IFixedPenalty>();
            _mockPercentagePenalty = new Mock<IPercentagePenalty>();

            //✅ Inject mocks into service
            _sut = new Penalty(
                _mockPercentagePenalty.Object,
                _mockPenaltyPolicies.Object,
                _mockDailyPenalty.Object,
                _mockFixedPenalty.Object
            );
        }

        #region ✅ NORMAL CASES
        [Fact]
        public void TC01_OnTimeAndPaidMin_ShouldNotApplyPenalty() //Models ค่าปรับ
        {
            //✅ On-time & Paid Min
            //Given: policy Daily(fixed= 100, grace = 5), today = 14 Oct 2025
            //And: DueDate = today, Payment = 10 % ของยอดค้าง
            //When: คำนวณค่าปรับ
            //Then: Penalty = 0, ไม่เรียกกลยุทธ์ใด ๆ
            //💬 กรณีลูกค้าชำระตรงเวลา และจ่ายถึงขั้นต่ำ ระบบจะถือว่า “ปกติ” ไม่คิดค่าปรับ

            // Arrange
            var Dailyfixed = 100m;
            var grace = 5;
            var today = new DateTime(2025, 10, 14);
            var dueDate = today;
            var outstanding = 10000m;
            var payment = outstanding * 0.10m;

            var policy = new PenaltyRequest
            {
                UserId = 1,
                OutstandingBalance = outstanding,
                DueDate = dueDate,
                ActiveStatus = "Active",
                PenaltyPolicyID = 1,
                PaymentAmount = payment
            };
            var mockPolicy = new ProductContact
            {
                PenaltyPolicyID = 1,
                PolicyName = "Standard Daily Penalty",
                PenaltyType = "Daily",
                PenaltyRate = 0m, // ไม่ได้ใช้ในกรณี Daily
                FixedAmount = Dailyfixed,
                MaxPenalty = 300.0m,
                TotalCap = 1000.0m,
                PenaltyFreePeriodDays = grace,
                MinimumPaymentRate = 10.0m // 10%
            };

            _mockPenaltyPolicies
                .Setup(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()))
                .Returns(mockPolicy);

            //Act
            var result = _sut.GetPenalty(policy);

            //Assert
            Assert.Equal(0, result.PenaltyAmount);
            _mockDailyPenalty.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Never);
            _mockFixedPenalty.Verify(e => e.Calculate(It.IsAny<PenaltyContext>()), Times.Never);
            _mockPercentagePenalty.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never);
            _mockPenaltyPolicies.Verify(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()), Times.Once);

            //mockDaily.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Never);
        }

        [Fact]
        public void TC02_OverdueWithinGrace_NotCalculatePenalty() //Models ค่าปรับ
        {
            //Overdue within Grace
            //Given: Daily(100, cap = 1000, grace = 5)
            //And: overdue = 3 วัน(< 5)
            //When: คำนวณค่าปรับ
            //Then: Penalty = 0, ไม่เรียกกลยุทธ์
            //💬 แม้จะเลยกำหนด แต่ยังอยู่ในช่วง “ผ่อนผัน” 5 วัน จึงไม่ถูกคิดค่าปรับ

            //Arrange
            var Dailyfixed = 100m;
            var Cap = 1000m;
            var grace = 5;
            var overdueDays = 3; // < grace

            var policy = new PenaltyRequest
            {
                UserId = 1,
                OutstandingBalance = 10000m,
                DueDate = DateTime.Now.AddDays(-overdueDays), // 3 วันก่อน
                ActiveStatus = "Active",
                PenaltyPolicyID = 1,
                PaymentAmount = 0m // ไม่ได้จ่าย
            };

            var mockPolicy = new ProductContact
            {
                PenaltyPolicyID = 1,
                PolicyName = "Standard Daily Penalty",
                PenaltyType = "Daily",
                PenaltyRate = 0m, // ไม่ได้ใช้ในกรณี Daily
                FixedAmount = Dailyfixed,
                MaxPenalty = 300.0m,
                TotalCap = Cap,
                PenaltyFreePeriodDays = grace,
                MinimumPaymentRate = 10.0m // 10%
            };

            _mockPenaltyPolicies
                .Setup(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()))
                .Returns(mockPolicy);

            //Act
            var result = _sut.GetPenalty(policy);

            //Assert
            Assert.Equal(0, result.PenaltyAmount);
            _mockDailyPenalty.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Never);
            _mockFixedPenalty.Verify(e => e.Calculate(It.IsAny<PenaltyContext>()), Times.Never);
            _mockPercentagePenalty.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never);
            _mockPenaltyPolicies.Verify(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()), Times.Once);
        }

        [Fact]
        public void TC03_OverdueBeyondGrace_CalculatePenaltyOneHundredEveryDays() //Models ค่าปรับ
        {
            //Overdue beyond Grace(Daily)
            //Given: Daily(100, cap = 1000, grace = 5)
            //And: overdue = 6 วัน(> 5)
            //When: คำนวณค่าปรับ
            //Then: Penalty = 600, เรียก Daily.Calculate() 1 ครั้ง ด้วย context ที่ OverdueDays = 6
            //💬 เลยช่วงผ่อนผันแล้ว คิดปรับวันละ 100 บาท รวม 6 วัน = 600 บาท

            //Arrange
            var Dailyfixed = 100m;
            var Cap = 1000m;
            var grace = 5;
            var overdueDays = 6; // > grace

            var policy = new PenaltyRequest
            {
                UserId = 1,
                OutstandingBalance = 10000m,
                DueDate = DateTime.Now.AddDays(-overdueDays), // 6 วันก่อน
                ActiveStatus = "Active",
                PenaltyPolicyID = 1,
                PaymentAmount = 0m // ไม่ได้จ่าย
            };

            var mockPolicy = new ProductContact
            {
                PenaltyPolicyID = 1,
                PolicyName = "Standard Daily Penalty",
                PenaltyType = "Daily",
                PenaltyRate = 0m, // ไม่ได้ใช้ในกรณี Daily
                FixedAmount = Dailyfixed,
                MaxPenalty = 300.0m,
                TotalCap = Cap,
                PenaltyFreePeriodDays = grace,
                MinimumPaymentRate = 10.0m // 10%
            };

            _mockPenaltyPolicies
                .Setup(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()))
                .Returns(mockPolicy);

            _mockDailyPenalty
                .Setup(d => d.Calculate(It.Is<PenaltyContext>(c => c.OverdueDays == overdueDays)))
                .Returns(600m); // 100 * 6 days

            //Act
            var result = _sut.GetPenalty(policy);

            //Assert
            Assert.Equal(600m, result.PenaltyAmount);
            _mockDailyPenalty.Verify(d => d.Calculate(It.Is<PenaltyContext>(c => c.OverdueDays == overdueDays)), Times.Once);
            _mockFixedPenalty.Verify(e => e.Calculate(It.IsAny<PenaltyContext>()), Times.Never);
            _mockPercentagePenalty.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never);
            _mockPenaltyPolicies.Verify(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()), Times.Once);
        }

        [Fact]
        public void TC04_DailyHitsCap_CalculatePenaltyCap() //Models ค่าปรับ
        {
            //Daily hits Cap
            //Given: Daily(100, cap = 1000, grace = 5), overdue = 20 วัน
            //When: คำนวณค่าปรับ
            //Then: Penalty = 1000(ชนเพดาน), เรียก Daily.Calculate() 1 ครั้ง
            //💬 ค่าปรับสะสมสูงสุดไม่เกิน 1,000 บาท ถึงจะเกิน 10–20 วันก็ตาม

            //Arrange
            var Dailyfixed = 100m;
            var Cap = 1000m;
            var grace = 5;
            var overdueDays = 20; // > grace

            var policy = new PenaltyRequest
            {
                UserId = 1,
                OutstandingBalance = 10000m,
                DueDate = DateTime.Now.AddDays(-overdueDays), //20 วันก่อน
                ActiveStatus = "Active",
                PenaltyPolicyID = 1,
                PaymentAmount = 0m //ไม่ได้จ่าย
            };

            var mockPolicy = new ProductContact
            {
                PenaltyPolicyID = 1,
                PolicyName = "Standard Daily Penalty",
                PenaltyType = "Daily",
                PenaltyRate = 0m, //ไม่ได้ใช้ในกรณี Daily
                FixedAmount = Dailyfixed,
                MaxPenalty = 300.0m,
                TotalCap = Cap,
                PenaltyFreePeriodDays = grace,
                MinimumPaymentRate = 10.0m //10%
            };

            _mockPenaltyPolicies
                .Setup(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()))
                .Returns(mockPolicy);

            _mockDailyPenalty
                .Setup(d => d.Calculate(It.Is<PenaltyContext>(c => c.OverdueDays == overdueDays)))
                .Returns(1000m); //hits cap

            //Act
            var result = _sut.GetPenalty(policy);

            //Assert
            Assert.Equal(1000m, result.PenaltyAmount);
            _mockDailyPenalty.Verify(d => d.Calculate(It.Is<PenaltyContext>(c => c.OverdueDays == overdueDays)), Times.Once);
            _mockFixedPenalty.Verify(e => e.Calculate(It.IsAny<PenaltyContext>()), Times.Never);
            _mockPercentagePenalty.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never);
            _mockPenaltyPolicies.Verify(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()), Times.Once);
        }

        [Fact]
        public void TC05_FixedPenalty_ApplyOnceWhenOverdueBeyondGrace() //Models ค่าปรับ
        {
            //Fixed penalty
            //Given: Fixed(200) และเข้าเงื่อนไขผิดนัด(overdue หรือจ่ายต่ำกว่าขั้นต่ำ)
            //When: คำนวณค่าปรับ
            //Then: Penalty = 200, เรียก Fixed.Calculate()
            //💬 ค่าปรับแบบคงที่ ไม่สนยอดค้างหรือจำนวนวัน

            //Arrange
            var FixedAmount = 200m;

            var policy = new PenaltyRequest
            {
                UserId = 1,
                OutstandingBalance = 10000m,
                DueDate = DateTime.Now.AddDays(-1), // 1 วันก่อน
                ActiveStatus = "Active",
                PenaltyPolicyID = 1,
                PaymentAmount = 0m // ไม่ได้จ่าย
            };

            var mockPolicy = new ProductContact
            {
                PenaltyPolicyID = 1,
                PolicyName = "Fixed Penalty",
                PenaltyType = "Fixed",
                PenaltyRate = 0m, // ไม่ได้ใช้ในกรณี Fixed
                FixedAmount = FixedAmount,
                MaxPenalty = 0m, // ไม่ได้ใช้ในกรณี Fixed
                TotalCap = 0m, // ไม่ได้ใช้ในกรณี Fixed
                PenaltyFreePeriodDays = 0, // ไม่ได้ใช้ในกรณี Fixed
                MinimumPaymentRate = 10.0m // 10%
            };

            _mockPenaltyPolicies
                .Setup(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()))
                .Returns(mockPolicy);

            _mockFixedPenalty
                .Setup(f => f.Calculate(It.IsAny<PenaltyContext>()))
                .Returns(FixedAmount);

            //Act
            var result = _sut.GetPenalty(policy);

            //Assert
            Assert.Equal(FixedAmount, result.PenaltyAmount);
            _mockFixedPenalty.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Once);
            _mockDailyPenalty.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Never);
            _mockPenaltyPolicies.Verify(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()), Times.Once);
            _mockPercentagePenalty.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never);
        }

        [Fact]
        public void TC06_PercentageCappedPenalty_CalculateBasedOnOutstanding() //Models ค่าปรับ
        {
            //Percentage capped
            //Given: Percentage (2.5%, max=300, grace=5), overdue=6 days
            //And: Outstanding = 20,000
            //When: คำนวณค่าปรับ
            //Then: Penalty = 300 (ชน max), เรียก Percentage.Calculate()
            //💬 2.5% ของ 20,000 = 500 แต่ระบบมีเพดานสูงสุด 300 บาท จึงคิดแค่ 300 บาท

            //Arrange
            var Percentage = 2.5m;
            var MaxPenalty = 300m;
            var grace = 5;
            var overdueDays = 6; // > grace
            var outstanding = 20000m;

            var policy = new PenaltyRequest
            {
                UserId = 1,
                OutstandingBalance = outstanding,
                DueDate = DateTime.Now.AddDays(-overdueDays), // 6 วันก่อน
                ActiveStatus = "Active",
                PenaltyPolicyID = 1,
                PaymentAmount = 0m // ไม่ได้จ่าย
            };

            var mockPolicy = new ProductContact
            {
                PenaltyPolicyID = 1,
                PolicyName = "Percentage Capped Penalty",
                PenaltyType = "Percentage",
                PenaltyRate = Percentage,
                FixedAmount = 0m, // ไม่ได้ใช้ในกรณี Percentage
                MaxPenalty = MaxPenalty,
                TotalCap = 1000.0m, // สมมติ
                PenaltyFreePeriodDays = grace,
                MinimumPaymentRate = 10.0m // 10%
            };

            _mockPenaltyPolicies
                .Setup(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()))
                .Returns(mockPolicy);

            _mockPercentagePenalty
                .Setup(p => p.Calculate(It.Is<PenaltyContext>(c => c.OutstandingBalance == outstanding && c.OverdueDays == overdueDays)))
                .Returns(MaxPenalty); // hits max

            //Act
            var result = _sut.GetPenalty(policy);

            //Assert
            Assert.Equal(MaxPenalty, result.PenaltyAmount);
            _mockPercentagePenalty.Verify(p => p.Calculate(It.Is<PenaltyContext>(c => c.OutstandingBalance == outstanding && c.OverdueDays == overdueDays)), Times.Once);
            _mockDailyPenalty.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Never);
            _mockFixedPenalty.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never);
            _mockPenaltyPolicies.Verify(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()), Times.Once);
        }

        [Fact]
        public void TC07_BoundaryMatchGrace_OtCalculatePanelty() //Models ค่าปรับ
        {
            //Boundary = Grace
            //Given: grace = 5, overdue = 5
            //when: คำนวณค่าปรับ
            //Then: ไม่คิดปรับ, ไม่เรียก strategy
            //💬 วันที่เท่ากับช่วงผ่อนผัน “พอดี” ยังถือว่าไม่โดนปรับ

            //Arrange
            var grace = 5;
            var overdueDays = 5; // = grace

            var policy = new PenaltyRequest
            {
                UserId = 1,
                OutstandingBalance = 10000m,
                DueDate = DateTime.Now.AddDays(-overdueDays), // 5 วันก่อน
                ActiveStatus = "Active",
                PenaltyPolicyID = 1,
                PaymentAmount = 0m // ไม่ได้จ่าย
            };

            var mockPolicy = new ProductContact
            {
                PenaltyPolicyID = 1,
                PolicyName = "Standard Daily Penalty",
                PenaltyType = "Daily",
                PenaltyRate = 0m, // ไม่ได้ใช้ในกรณี Daily
                FixedAmount = 100m,
                MaxPenalty = 300.0m,
                TotalCap = 1000.0m,
                PenaltyFreePeriodDays = grace,
                MinimumPaymentRate = 10.0m // 10%
            };

            _mockPenaltyPolicies
                .Setup(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()))
                .Returns(mockPolicy);

            //Act
            var result = _sut.GetPenalty(policy);

            //Assert
            Assert.Equal(0, result.PenaltyAmount);
            _mockPenaltyPolicies.Verify(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()), Times.Once);
            _mockDailyPenalty.Verify(d => d.Calculate(It.Is<PenaltyContext>(c => c.OverdueDays == overdueDays)), Times.Never);
            _mockFixedPenalty.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never);
            _mockPercentagePenalty.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never);
        }

        [Fact]
        public void TC08_PercentageBelowMaxPenalty_CalculateBasedOnOutstanding() //Models ค่าปรับ
        {
            //Percentage below Max
            //Given: Percentage (1.5%, max=300, grace=5)
            //And: Outstanding = 10,000 , overdue=10 days
            //When: คำนวณค่าปรับ
            //Then: Penalty = 150, เรียก Percentage.Calculate()
            //💬 1.5% ของ 10,000 = 150 ซึ่งไม่เกินเพดาน จึงคิดเต็มจำนวน

            //Arrange
            var Percentage = 1.5m;
            var MaxPenalty = 300m;
            var grace = 5;
            var overdueDays = 10; // > grace
            var outstanding = 10000m;
            var policy = new PenaltyRequest
            {
                UserId = 1,
                OutstandingBalance = outstanding,
                DueDate = DateTime.Now.AddDays(-overdueDays), // 10 วันก่อน
                ActiveStatus = "Active",
                PenaltyPolicyID = 1,
                PaymentAmount = 0m // ไม่ได้จ่าย
            };
            var mockPolicy = new ProductContact
            {
                PenaltyPolicyID = 1,
                PolicyName = "Percentage Capped Penalty",
                PenaltyType = "Percentage",
                PenaltyRate = Percentage,
                FixedAmount = 0m, // ไม่ได้ใช้ในกรณี Percentage
                MaxPenalty = MaxPenalty,
                TotalCap = 1000.0m, // สมมติ
                PenaltyFreePeriodDays = grace,
                MinimumPaymentRate = 10.0m // 10%
            };
            _mockPenaltyPolicies
                .Setup(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()))
                .Returns(mockPolicy);
            _mockPercentagePenalty
                .Setup(p => p.Calculate(It.Is<PenaltyContext>(c => c.OutstandingBalance == outstanding && c.OverdueDays == overdueDays)))
                .Returns(150m); //1.5% ของ 10,000

            //Act
            var result = _sut.GetPenalty(policy);

            //Assert
            Assert.Equal(150m, result.PenaltyAmount);
            _mockPenaltyPolicies.Verify(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()), Times.Once);
            _mockPercentagePenalty.Verify(p => p.Calculate(It.Is<PenaltyContext>(c => c.OutstandingBalance == outstanding && c.OverdueDays == overdueDays)), Times.Once);
            _mockDailyPenalty.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Never);
            _mockFixedPenalty.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never);
        }

        #endregion

        #region ⚠️ ALTERNATIVE CASES
        [Fact]
        public void TC12_InactiveUser_ShouldThrowException() //Models ค่าปรับ
        {
            //Inactive user
            //Given: ActiveStatus = "Inactive"
            //When: คำนวณค่าปรับ
            //Then: ขว้าง InvalidOperationException, ไม่เรียกกลยุทธ์ใด ๆ
            //💬 บัญชีไม่ได้ใช้งาน ไม่ควรถูกคำนวณค่าปรับ
            //Arrange
            var policy = new PenaltyRequest
            {
                UserId = 1,
                OutstandingBalance = 10000m,
                DueDate = DateTime.Now.AddDays(-10), // 10 วันก่อน
                ActiveStatus = "Inactive", // Inactive
                PenaltyPolicyID = 1,
                PaymentAmount = 0m // ไม่ได้จ่าย
            };
            //Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _sut.GetPenalty(policy));
            Assert.Equal("Cannot calculate penalty for inactive users.", exception.Message);
            _mockDailyPenalty.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()), Times.Never);
            _mockFixedPenalty.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never);
            _mockPercentagePenalty.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never);
            _mockPenaltyPolicies.Verify(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()), Times.Never);
        }
        #endregion

        #region TryImprement
        //[Fact]
        //public void NewCaseForImprement() 
        //{ 
        //    //Complex scenario: Overdue beyond grace + paid below min (Daily + Fixed)
        //    //Given: Daily(100, cap=1000, grace=5) + Fixed(200), overdue=7 days, Outstanding=15,000, Payment=5% ของยอดค้าง
        //    //When: คำนวณค่าปรับ
        //    //Then: Penalty = 900 (700 + 200), เรียก Daily.Calculate() และ Fixed.Calculate() อย่างละ 1 ครั้ง
        //    //💬 เลยช่วงผ่อนผันแล้ว คิดปรับวันละ 100 บาท รวม 7 วัน = 700 บาท + ค่าปรับคงที่ 200 บาท รวมเป็น 900 บาท
        //    //Arrange
        //    var Dailyfixed = 100m;
        //    var Cap = 1000m;
        //    var FixedAmount = 200m;
        //    var grace = 5;
        //    var overdueDays = 7; // > grace
        //    var outstanding = 15000m;
        //    var payment = outstanding * 0.05m; // ต่ำกว่าขั้นต่ำ
        //    var policy = new PenaltyRequest
        //    {
        //        UserId = 1,
        //        OutstandingBalance = outstanding,
        //        DueDate = DateTime.Now.AddDays(-overdueDays), // 7 วันก่อน
        //        ActiveStatus = "Active",
        //        PenaltyPolicyID = 1,
        //        PaymentAmount = payment
        //    };
        //    var mockPolicy = new ProductContact
        //    {
        //        PenaltyPolicyID = 1,
        //        PolicyName = "Daily + Fixed Penalty",
        //        PenaltyType = "Daily+Fixed",
        //        PenaltyRate = 0m, // ไม่ได้ใช้ในกรณี Daily+Fixed
        //        FixedAmount = Dailyfixed,
        //        MaxPenalty = 0m, // ไม่ได้ใช้ในกรณี Daily+Fixed
        //        TotalCap = Cap,
        //        PenaltyFreePeriodDays = grace,
        //        MinimumPaymentRate = 10.0m // 10%
        //    };
        //    _mockPenaltyPolicies
        //        .Setup(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()))
        //        .Returns(mockPolicy);
        //    _mockDailyPenalty
        //        .Setup(d => d.Calculate(It.Is<PenaltyContext>(c => c.OverdueDays == overdueDays)))
        //        .Returns(700m); // 100 * 7 days
        //    _mockFixedPenalty
        //        .Setup(f => f.Calculate(It.IsAny<PenaltyContext>()))
        //        .Returns(FixedAmount);

        //    //Act
        //    var result = _sut.GetPenalty(policy);

        //    //Assert
        //    Assert.Equal(900m, result.PenaltyAmount);
        //    _mockDailyPenalty.Verify(d => d.Calculate(It.Is<PenaltyContext>(c => c.OverdueDays == overdueDays)), Times.Once);
        //    _mockFixedPenalty.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Once);
        //    _mockPercentagePenalty.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()), Times.Never);
        //    _mockPenaltyPolicies.Verify(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()), Times.Once);
        //}
        #endregion
    }
}
