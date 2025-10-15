using Moq;
using Nexum.Server.DAC;
using Nexum.Server.Models;
using Nexum.Server.Services;
using Xunit;

namespace Nexum.Tests
{
    public class InterestServiceTests
    {
        private readonly Mock<IAccumulatedInterestDAC> _mockAccumulatedInterestDAC;
        private readonly Mock<IInterestTransactionDAC> _mockInterestTransactionDAC;
        private readonly InterestService _interestService;

        public InterestServiceTests()
        {
            _mockAccumulatedInterestDAC = new Mock<IAccumulatedInterestDAC>();
            _mockInterestTransactionDAC = new Mock<IInterestTransactionDAC>();
            _interestService = new InterestService(_mockAccumulatedInterestDAC.Object, _mockInterestTransactionDAC.Object);
        }


        #region Normal Cases
        // กรณีทดสอบที่ 1: คำนวณดอกเบี้ย "รายวัน" (ไม่มีดอกเบี้ยสะสมเดิม)
        [Fact]
        public void CalculateInterest_PerDay_NoAccumulated_Returns5BathAndAccum5()
        {
            // Arrange
            var initialAccum = 0m;
            _mockAccumulatedInterestDAC
                .Setup(x => x.GetAccumulatedInterest(It.IsAny<int>()))
                .Returns(new AccumulatedInterest { AccumInterestRemain = initialAccum });

            var req = new CalculateInterestRequest
            {
                PrincipalBalance = 10_000m,
                InterestRate = 0.1825m,
                InterestType = "PerDay",
                InterestFreePeriodDays = default, // ไม่อยู่ในช่วงปลอดดอกเบี้ย
                ProductContactId = 1,
                MaxInterestAmount = 999_999m
            };

            // Act
            var res = _interestService.CalculateInterest(req);

            // Assert
            Assert.Equal(5.00m, res.InterestAmount); // (10,000 * 0.1825) / 365 = 5.00
            Assert.Equal(5.00m, res.AccumInterestRemain); // 0 + 5.00

            _mockInterestTransactionDAC.Verify(d => d.CreateInterestTransaction(
                It.Is<InterestTransaction>(t =>
                    t.ProductContactId == 1 &&
                    t.InterestAmount == 5.00m &&
                    t.AccumulatedAmount == 5.00m
                )), Times.Once);

            _mockAccumulatedInterestDAC.Verify(d => d.UpdateAccumulatedInterest(5.00m), Times.Once);
        }

        // กรณีทดสอบที่ 2: คำนวณดอกเบี้ย "รายเดือน" (ไม่มีดอกเบี้ยสะสมเดิม)
        [Fact]
        public void CalculateInterest_PerMonth_NoAccumulated_Returns375BathAndAccum375()
        {
            // Arrange
            var initialAccum = 0m;
            _mockAccumulatedInterestDAC
                .Setup(x => x.GetAccumulatedInterest(It.IsAny<int>()))
                .Returns(new AccumulatedInterest { AccumInterestRemain = initialAccum });

            var req = new CalculateInterestRequest
            {
                PrincipalBalance = 25_000m,
                InterestRate = 0.015m,
                InterestType = "PerMonth",
                InterestFreePeriodDays = default,
                ProductContactId = 1,
                MaxInterestAmount = 999_999m
            };

            // Act
            var res = _interestService.CalculateInterest(req);

            // Assert
            Assert.Equal(375.00m, res.InterestAmount); // 25,000 * 0.015 = 375.00
            Assert.Equal(375.00m, res.AccumInterestRemain); // 0 + 375.00

            _mockInterestTransactionDAC.Verify(d => d.CreateInterestTransaction(
                It.Is<InterestTransaction>(t =>
                    t.ProductContactId == 1 &&
                    t.InterestAmount == 375.00m &&
                    t.AccumulatedAmount == 375.00m
                )), Times.Once);

            _mockAccumulatedInterestDAC.Verify(d => d.UpdateAccumulatedInterest(375.00m), Times.Once);
        }

        // กรณีทดสอบที่ 3: คำนวณดอกเบี้ย "รายวัน" (มีดอกเบี้ยสะสมเดิม)
        [Fact]
        public void CalculateInterest_PerDay_WithAccumulated_AccumulateCorrectly()
        {
            // Arrange
            var initialAccum = 150.25m;
            _mockAccumulatedInterestDAC
                .Setup(x => x.GetAccumulatedInterest(It.IsAny<int>()))
                .Returns(new AccumulatedInterest { AccumInterestRemain = initialAccum });

            var req = new CalculateInterestRequest
            {
                PrincipalBalance = 10_000m,
                InterestRate = 0.1825m,
                InterestType = "PerDay",
                InterestFreePeriodDays = default,
                ProductContactId = 1,
                MaxInterestAmount = 999_999m
            };

            // Act
            var res = _interestService.CalculateInterest(req);

            // Assert
            Assert.Equal(5.00m, res.InterestAmount);
            Assert.Equal(155.25m, res.AccumInterestRemain); // 150.25 + 5.00

            _mockInterestTransactionDAC.Verify(d => d.CreateInterestTransaction(
                It.Is<InterestTransaction>(t =>
                    t.InterestAmount == 5.00m &&
                    t.AccumulatedAmount == 155.25m
                )), Times.Once);

            _mockAccumulatedInterestDAC.Verify(d => d.UpdateAccumulatedInterest(155.25m), Times.Once);
        }

        // กรณีทดสอบที่ 4: คำนวณดอกเบี้ย "รายเดือน" (มีดอกเบี้ยสะสมเดิม)
        [Fact]
        public void CalculateInterest_PerMonth_WithAccumulated_AccumulateCorrectly()
        {
            // Arrange
            var initialAccum = 450.00m;
            _mockAccumulatedInterestDAC
                .Setup(x => x.GetAccumulatedInterest(It.IsAny<int>()))
                .Returns(new AccumulatedInterest { AccumInterestRemain = initialAccum });

            var req = new CalculateInterestRequest
            {
                PrincipalBalance = 25_000m,
                InterestRate = 0.015m,
                InterestType = "PerMonth",
                InterestFreePeriodDays = default,
                ProductContactId = 1,
                MaxInterestAmount = 999_999m
            };

            // Act
            var res = _interestService.CalculateInterest(req);

            // Assert
            Assert.Equal(375.00m, res.InterestAmount);
            Assert.Equal(825.00m, res.AccumInterestRemain); // 450.00 + 375.00

            _mockInterestTransactionDAC.Verify(d => d.CreateInterestTransaction(
                It.Is<InterestTransaction>(t =>
                    t.InterestAmount == 375.00m &&
                    t.AccumulatedAmount == 825.00m
                )), Times.Once);

            _mockAccumulatedInterestDAC.Verify(d => d.UpdateAccumulatedInterest(825.00m), Times.Once);
        }
        #endregion

        #region Alternative Cases
        // กรณีทดสอบที่ 1: ยอดคงเหลือเป็นศูนย์ -> ดอกเบี้ยรอบนี้เป็น 0 และยอดสะสมคงเดิม
        [Fact]
        public void CalculateInterest_ZeroPrincipal_KeepAccumulatedUnchanged()
        {
            // Arrange
            var initialAccum = 150.25m;
            _mockAccumulatedInterestDAC
                .Setup(x => x.GetAccumulatedInterest(It.IsAny<int>()))
                .Returns(new AccumulatedInterest { AccumInterestRemain = initialAccum });

            var req = new CalculateInterestRequest
            {
                PrincipalBalance = 0m,
                InterestRate = 0.1825m,
                InterestType = "PerDay",
                InterestFreePeriodDays = default,
                ProductContactId = 1,
                MaxInterestAmount = 999_999m
            };

            // Act
            var res = _interestService.CalculateInterest(req);

            // Assert
            Assert.Equal(0m, res.InterestAmount);
            Assert.Equal(initialAccum, res.AccumInterestRemain);

            _mockInterestTransactionDAC.Verify(d => d.CreateInterestTransaction(
                It.Is<InterestTransaction>(t =>
                    t.InterestAmount == 0m &&
                    t.AccumulatedAmount == initialAccum
                )), Times.Once);

            _mockAccumulatedInterestDAC.Verify(d => d.UpdateAccumulatedInterest(initialAccum), Times.Once);
        }

        // กรณีทดสอบที่ 2: อยู่ในช่วงปลอดดอกเบี้ย -> ต้องสร้างรายการด้วยดอกเบี้ย 0 และไม่อัปเดตยอดสะสม
        [Fact]
        public void CalculateInterest_InFreePeriod_CreatesZeroTransaction_NoAccumUpdate()
        {
            // Arrange
            var initialAccum = 50.00m;
            _mockAccumulatedInterestDAC
                .Setup(x => x.GetAccumulatedInterest(It.IsAny<int>()))
                .Returns(new AccumulatedInterest { AccumInterestRemain = initialAccum });

            var req = new CalculateInterestRequest
            {
                PrincipalBalance = 30_000m,
                InterestRate = 0.1825m,
                InterestType = "PerDay",
                InterestFreePeriodDays = DateTime.Now.AddDays(1), // วันนี้ยังอยู่ในช่วงปลอดดอกเบี้ย
                ProductContactId = 1,
                MaxInterestAmount = 999_999m
            };

            // Act
            var res = _interestService.CalculateInterest(req);

            // Assert
            Assert.Equal(0m, res.InterestAmount);
            Assert.Equal(initialAccum, res.AccumInterestRemain);

            _mockInterestTransactionDAC.Verify(d => d.CreateInterestTransaction(
                It.Is<InterestTransaction>(t =>
                    t.InterestAmount == 0m &&
                    t.AccumulatedAmount == initialAccum &&
                    t.Remark != null && t.Remark.Contains("ยกเว้น")
                )), Times.Once);

            _mockAccumulatedInterestDAC.Verify(d => d.UpdateAccumulatedInterest(It.IsAny<decimal>()), Times.Never);
        }

        // กรณีทดสอบที่ 3: ไม่อยู่ในช่วงปลอดดอกเบี้ย -> ต้องคำนวณดอกเบี้ยรายวันตามปกติ
        [Fact]
        public void CalculateInterest_NotInFreePeriod_CalculatesNormally()
        {
            // Arrange
            var initialAccum = 50.00m;
            _mockAccumulatedInterestDAC
                .Setup(x => x.GetAccumulatedInterest(It.IsAny<int>()))
                .Returns(new AccumulatedInterest { AccumInterestRemain = initialAccum });

            var req = new CalculateInterestRequest
            {
                PrincipalBalance = 30_000m,
                InterestRate = 0.1825m,
                InterestType = "PerDay",
                InterestFreePeriodDays = DateTime.Now.AddDays(-1), // หมดช่วงปลอดดอกเบี้ยแล้ว
                ProductContactId = 1,
                MaxInterestAmount = 999_999m
            };

            // Act
            var res = _interestService.CalculateInterest(req);

            // Assert
            Assert.Equal(15.00m, res.InterestAmount); // (30,000 * 0.1825) / 365 = 15.00
            Assert.Equal(65.00m, res.AccumInterestRemain); // 50.00 + 15.00

            _mockInterestTransactionDAC.Verify(d => d.CreateInterestTransaction(
                It.Is<InterestTransaction>(t =>
                    t.InterestAmount == 15.00m &&
                    t.AccumulatedAmount == 65.00m
                )), Times.Once);

            _mockAccumulatedInterestDAC.Verify(d => d.UpdateAccumulatedInterest(65.00m), Times.Once);
        }

        // กรณีทดสอบที่ 4: วันสิ้นสุดช่วงปลอดดอกเบี้ยเป็นวันนี้พอดี -> ยังถือว่าอยู่ในช่วงปลอดดอกเบี้ย (<=) ดอกเบี้ย 0 บาท
        [Fact]
        public void CalculateInterest_FreePeriodEndsToday_StillZeroInterest()
        {
            // Arrange
            var initialAccum = 10.00m;
            _mockAccumulatedInterestDAC
                .Setup(x => x.GetAccumulatedInterest(It.IsAny<int>()))
                .Returns(new AccumulatedInterest { AccumInterestRemain = initialAccum });

            var req = new CalculateInterestRequest
            {
                PrincipalBalance = 30_000m,
                InterestRate = 0.1825m,
                InterestType = "PerDay",
                // หมายเหตุ: ตั้งค่าเป็นอนาคตเล็กน้อยเพื่อหลีกเลี่ยงปัญหา timing ระหว่าง Arrange/Act
                InterestFreePeriodDays = DateTime.Now.AddSeconds(2), // ใกล้เคียงวันนี้พอดี
                ProductContactId = 1,
                MaxInterestAmount = 999_999m
            };

            // Act
            var res = _interestService.CalculateInterest(req);

            // Assert
            Assert.Equal(0m, res.InterestAmount);
            Assert.Equal(initialAccum, res.AccumInterestRemain);

            _mockInterestTransactionDAC.Verify(d => d.CreateInterestTransaction(
                It.Is<InterestTransaction>(t =>
                    t.InterestAmount == 0m &&
                    t.AccumulatedAmount == initialAccum
                )), Times.Once);

            _mockAccumulatedInterestDAC.Verify(d => d.UpdateAccumulatedInterest(It.IsAny<decimal>()), Times.Never);
        }
        #endregion

        #region Exception Cases
        // กรณีทดสอบที่ 1: Object Input เป็น Null
        [Fact]
        public void CalculateInterest_NullRequest_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _interestService.CalculateInterest(null!));
        }

        // กรณีทดสอบที่ 2: Input ขาดหาย (Missing Value) - InterestType ไม่ได้ระบุ (null)
        [Fact]
        public void CalculateInterest_MissingInterestType_ThrowsArgumentException()
        {
            var req = new CalculateInterestRequest
            {
                PrincipalBalance = 100m,
                InterestRate = 0.01m,
                InterestType = null,
                ProductContactId = 1
            };

            Assert.Throws<ArgumentException>(() => _interestService.CalculateInterest(req));
        }

        // กรณีทดสอบที่ 3: ยอดคงเหลือติดลบ (Negative Value)
        [Fact]
        public void CalculateInterest_NegativePrincipal_ThrowsArgumentException()
        {
            var req = new CalculateInterestRequest
            {
                PrincipalBalance = -1m,
                InterestRate = 0.01m,
                InterestType = "PerMonth",
                ProductContactId = 1
            };

            Assert.Throws<ArgumentException>(() => _interestService.CalculateInterest(req));
        }

        // กรณีทดสอบที่ 4: อัตราดอกเบี้ยจาก Config ติดลบ (Negative Value)
        [Fact]
        public void CalculateInterest_NegativeRate_ThrowsArgumentException()
        {
            var req = new CalculateInterestRequest
            {
                PrincipalBalance = 100m,
                InterestRate = -0.01m,
                InterestType = "PerMonth",
                ProductContactId = 1
            };

            Assert.Throws<ArgumentException>(() => _interestService.CalculateInterest(req));
        }

        // กรณีทดสอบที่ 5: ประเภทดอกเบี้ยเป็นค่าว่าง (Empty String)
        [Fact]
        public void CalculateInterest_EmptyInterestType_ThrowsArgumentException()
        {
            var req = new CalculateInterestRequest
            {
                PrincipalBalance = 100m,
                InterestRate = 0.01m,
                InterestType = "",
                ProductContactId = 1
            };

            Assert.Throws<ArgumentException>(() => _interestService.CalculateInterest(req));
        }

        // กรณีทดสอบที่ 6: ประเภทดอกเบี้ยเป็น Whitespace
        [Fact]
        public void CalculateInterest_WhitespaceInterestType_ThrowsArgumentException()
        {
            var req = new CalculateInterestRequest
            {
                PrincipalBalance = 100m,
                InterestRate = 0.01m,
                InterestType = " ",
                ProductContactId = 1
            };

            Assert.Throws<ArgumentException>(() => _interestService.CalculateInterest(req));
        }

        // กรณีทดสอบที่ 7: ประเภทดอกเบี้ยไม่รองรับ (Unsupported Type)
        [Fact]
        public void CalculateInterest_InvalidInterestType_ThrowsArgumentException()
        {
            var req = new CalculateInterestRequest
            {
                PrincipalBalance = 100m,
                InterestRate = 0.01m,
                InterestType = "PerYear",
                ProductContactId = 1
            };

            Assert.Throws<ArgumentException>(() => _interestService.CalculateInterest(req));
        }

        // กรณีทดสอบที่ 8: ไม่พบข้อมูล Config (เช่น AccumulatedInterest ไม่พบ)
        [Fact]
        public void CalculateInterest_ConfigNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange: ให้ชั้น DAC โยน KeyNotFoundException เมื่อดึง AccumulatedInterest
            _mockAccumulatedInterestDAC
                .Setup(x => x.GetAccumulatedInterest(It.IsAny<int>()))
                .Throws(new KeyNotFoundException("AccumulatedInterest not found"));

            var req = new CalculateInterestRequest
            {
                PrincipalBalance = 100m,
                InterestRate = 0.01m,
                InterestType = "PerMonth",
                ProductContactId = 1,
                MaxInterestAmount = 999_999m
            };

            // Act + Assert
            Assert.Throws<KeyNotFoundException>(() => _interestService.CalculateInterest(req));
        }

        // กรณีทดสอบที่ 9: มี Validation Errors หลายตัว
        [Fact]
        public void CalculateInterest_MultipleValidationErrors_ThrowsArgumentException()
        {
            var req = new CalculateInterestRequest
            {
                PrincipalBalance = -100m, // ผิด: ติดลบ
                InterestRate = -0.5m,     // ผิด: ติดลบ
                InterestType = "PerYear", // ผิด: ไม่รองรับ
                ProductContactId = 1
            };

            Assert.Throws<ArgumentException>(() => _interestService.CalculateInterest(req));
        }

        // กรณีทดสอบที่ 10: ชั้น Data Access (DAC) เกิด Exception
        [Fact]
        public void CalculateInterest_DacThrowsException_Propagates()
        {
            // Arrange: ให้ดึงสะสมสำเร็จ แต่ตอน CreateTransaction โยน Exception
            _mockAccumulatedInterestDAC
                .Setup(x => x.GetAccumulatedInterest(It.IsAny<int>()))
                .Returns(new AccumulatedInterest { AccumInterestRemain = 0m });

            _mockInterestTransactionDAC
                .Setup(x => x.CreateInterestTransaction(It.IsAny<InterestTransaction>()))
                .Throws(new InvalidOperationException("DAC failure"));

            var req = new CalculateInterestRequest
            {
                PrincipalBalance = 100m,
                InterestRate = 0.01m,
                InterestType = "PerMonth",
                ProductContactId = 1,
                MaxInterestAmount = 999_999m
            };

            // Act + Assert: ควรให้ Exception เดิมถูกส่งต่อออกมา
            Assert.Throws<InvalidOperationException>(() => _interestService.CalculateInterest(req));
        }
        #endregion
    }
}
