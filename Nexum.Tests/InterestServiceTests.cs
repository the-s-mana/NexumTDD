using System.Text;
using Moq;
using Nexum.Server.DAC;
using Nexum.Server.Models;
using Nexum.Server.Models.Interest;
using Nexum.Server.Services;
using Nexum.Server.Utils;
using SurrealDb.Net.Models;
using Xunit;

namespace Nexum.Tests
{
    public class InterestServiceTests
    {
        private readonly Mock<IAccumulatedInterestDAC> _mockAccumulatedInterestDAC;
        private readonly Mock<IInterestTransactionDAC> _mockInterestTransactionDAC;
        private readonly Mock<IDateTimeUtils> _mockDateTimeUtils;
        private readonly InterestService _interestService;
        string mockProductContactId = "e6qq5kfa19dgjvl16ecb";

        public InterestServiceTests()
        {
            Console.OutputEncoding = Encoding.UTF8; // ให้ Console.WriteLine แสดงภาษาไทยถูกต้อง
            _mockAccumulatedInterestDAC = new Mock<IAccumulatedInterestDAC>();
            _mockInterestTransactionDAC = new Mock<IInterestTransactionDAC>();
            _mockDateTimeUtils = new Mock<IDateTimeUtils>();
            // Default: ให้คืนวันที่ 2025-10-15
            _mockDateTimeUtils.Setup(x => x.GetCurrentDateTime(null)).Returns(new DateTime(2025, 10, 15));
            _interestService = new InterestService(_mockAccumulatedInterestDAC.Object, _mockInterestTransactionDAC.Object, _mockDateTimeUtils.Object);
        }


        #region Normal Cases
        // ชุดทดสอบแบบ Theory สำหรับเคสปกติ (Normal Cases)
        // อธิบายพารามิเตอร์:
        // - principal: ยอดคงเหลือ
        // - rate: อัตราดอกเบี้ย (รูปทศนิยม เช่น 0.1825 = 18.25%)
        // - interestType: ประเภทดอกเบี้ย ("PerMonth" | "PerDay")
        // - initialAccum: ดอกเบี้ยสะสมตั้งต้น
        // - expectedInterest: ดอกเบี้ยรอบนี้ที่คาดหวัง (ปัดเศษ 2 ตำแหน่ง ตามโค้ดจริง)
        // - expectedAccum: ยอดดอกเบี้ยสะสมใหม่ที่คาดหวัง
        [Theory]
        // กรณีทดสอบที่ 1: คำนวณดอกเบี้ย "รายวัน" (ไม่มีดอกเบี้ยสะสมเดิม)
        [InlineData("Normal Cases กรณีทดสอบที่ 1: คำนวณดอกเบี้ย 'รายวัน' (ไม่มีดอกเบี้ยสะสมเดิม)", 10000.0, 0.1825, "PerDay", 0.0, 5.00, 5.00)]
        // กรณีทดสอบที่ 2: คำนวณดอกเบี้ย "รายเดือน" (ไม่มีดอกเบี้ยสะสมเดิม)
        [InlineData("Normal Cases กรณีทดสอบที่ 2: คำนวณดอกเบี้ย 'รายเดือน' (ไม่มีดอกเบี้ยสะสมเดิม)", 25000.0, 0.015, "PerMonth", 0.0, 375.00, 375.00)]
        // กรณีทดสอบที่ 3: คำนวณดอกเบี้ย "รายวัน" (มีดอกเบี้ยสะสมเดิม)
        [InlineData("Normal Cases กรณีทดสอบที่ 3: คำนวณดอกเบี้ย 'รายวัน' (มีดอกเบี้ยสะสมเดิม)", 10000.0, 0.1825, "PerDay", 150.25, 5.00, 155.25)]
        // กรณีทดสอบที่ 4: คำนวณดอกเบี้ย "รายเดือน" (มีดอกเบี้ยสะสมเดิม)
        [InlineData("Normal Cases กรณีทดสอบที่ 4: คำนวณดอกเบี้ย 'รายเดือน' (มีดอกเบี้ยสะสมเดิม)", 25000.0, 0.015, "PerMonth", 450.00, 375.00, 825.00)]
        public async void CalculateInterest_NormalCases_AsTheory(string caseName, double principal, double rate, string interestType, double initialAccum, double expectedInterest, double expectedAccum)
        {
            Console.WriteLine($"Testing: {caseName}, ยอดคงเหลือ={principal}, อัตราดอกเบี้ย={rate}, ประเภทดอกเบี้ย={interestType}, ดอกเบี้ยสะสมเดิม={initialAccum}, ดอกเบี้ยรอบนี้={expectedInterest}, ดอกเบี้ยสะสมใหม่={expectedAccum}");
            // Arrange
            _mockAccumulatedInterestDAC
                .Setup(x => x.GetAccumulatedInterestByProductContactIdAsync(It.IsAny<RecordId>()))
                .Returns(Task.FromResult(new AccumulatedInterestResponseDTO { AccumInterestRemain = (decimal)initialAccum }));

            var req = new CalculateInterestRequest
            {
                PrincipalBalance = (decimal)principal,
                InterestRate = (decimal)rate,
                InterestType = interestType,
                InterestFreePeriodDays = default, // ไม่อยู่ในช่วงปลอดดอกเบี้ย
                ProductContactId = mockProductContactId,
                MaxInterestAmount = 999_999m
            };

            // Act
            var res = await _interestService.CalculateInterestAsync(req);
            Console.WriteLine($"ผลลัพธ์: ดอกเบี้ยรอบนี้={res.InterestAmount}, ดอกเบี้ยสะสมใหม่={res.AccumInterestRemain}");
            Console.WriteLine("--------------------------------");

            // Assert
            Assert.Equal((decimal)expectedInterest, res.InterestAmount);
            Assert.Equal((decimal)expectedAccum, res.AccumInterestRemain);

            _mockInterestTransactionDAC.Verify(d => d.CreateInterestTransactionAsync(
                It.Is<CreateInterestTransactionDTO>(t =>
                    t.ProductContactId == mockProductContactId &&
                    t.InterestAmount == (decimal)expectedInterest &&
                    t.AccumulatedAmount == (decimal)expectedAccum
                )), Times.Once);

            _mockAccumulatedInterestDAC.Verify(d => d.UpdateAccumulatedInterestAsync(
                     It.IsAny<string>(),
                     It.Is<Dictionary<string, object?>>(dict =>
                         dict.ContainsKey("AccumInterestRemain") &&
                         (decimal)dict["AccumInterestRemain"] == (decimal)expectedAccum
                     )
                ), Times.Once);
        }
        #endregion

        #region Alternative Cases
        // กรณีทดสอบที่ 1: ดอกเบี้ยเป็น 0 -> ระบบควรจบการทำงานก่อนบันทึกข้อมูลดอกเบี้ยสะสม
        [Fact]
        public async void CalculateInterest_ZeroPrincipal_DoNotUpdateAccumulatedInterest()
        {
            Console.WriteLine("Alternative Cases กรณีทดสอบที่ 1: ดอกเบี้ยเป็น 0 -> ระบบควรจบการทำงานก่อนบันทึกข้อมูลดอกเบี้ยสะสม (ไม่ควรเรียก UpdateAccumulatedInterest เลย)");
            // Arrange
            var initialAccum = 150.25m;
            _mockAccumulatedInterestDAC
                .Setup(x => x.GetAccumulatedInterestByProductContactIdAsync(It.IsAny<RecordId>()))
                .Returns(Task.FromResult(new AccumulatedInterestResponseDTO { AccumInterestRemain = (decimal)initialAccum }));

            var req = new CalculateInterestRequest
            {
                PrincipalBalance = 10000m,
                InterestRate = 0, // ปรับให้เป็น 0 เพื่อทดสอบกรณีที่ดอกเบี้ยเป็น 0
                InterestType = "PerDay",
                InterestFreePeriodDays = default,
                ProductContactId = mockProductContactId,
                MaxInterestAmount = 999_999m
            };

            // Act
            var res = await _interestService.CalculateInterestAsync(req);
            Console.WriteLine($"ผลลัพธ์: ดอกเบี้ยรอบนี้={res.InterestAmount}, ดอกเบี้ยสะสมใหม่={res.AccumInterestRemain}");
            Console.WriteLine("--------------------------------");

            // Assert
            Assert.Equal(0m, res.InterestAmount);
            Assert.Equal(initialAccum, res.AccumInterestRemain);

            _mockInterestTransactionDAC.Verify(d => d.CreateInterestTransactionAsync(
                It.Is<CreateInterestTransactionDTO>(t =>
                    t.InterestAmount == 0m &&
                    t.AccumulatedAmount == initialAccum
                )), Times.Once);

            // ระบบควรจบการทำงานก่อนบันทึกข้อมูลดอกเบี้ยสะสม (ไม่ควรเรียก UpdateAccumulatedInterest เลย)
            _mockAccumulatedInterestDAC.Verify(d => d.UpdateAccumulatedInterestAsync(
                     It.IsAny<string>(),
                     It.Is<Dictionary<string, object?>>(dict =>
                         dict.ContainsKey("AccumInterestRemain") &&
                         (decimal)dict["AccumInterestRemain"] == (decimal)initialAccum
                     )
                ), Times.Never);
        }

        // กรณีทดสอบที่ 2: อยู่ในช่วงปลอดดอกเบี้ย -> ต้องสร้างรายการด้วยดอกเบี้ย 0 และไม่อัปเดตยอดสะสม
        [Fact]
        public async void CalculateInterest_InFreePeriod_CreatesZeroTransaction_NoAccumUpdate()
        {
            Console.WriteLine("Alternative Cases กรณีทดสอบที่ 2: อยู่ในช่วงปลอดดอกเบี้ย -> ต้องสร้างรายการด้วยดอกเบี้ย 0 และไม่อัปเดตยอดสะสม");
            // Arrange
            var initialAccum = 50.00m;
            _mockAccumulatedInterestDAC
                .Setup(x => x.GetAccumulatedInterestByProductContactIdAsync(It.IsAny<RecordId>()))
                .Returns(Task.FromResult(new AccumulatedInterestResponseDTO { AccumInterestRemain = (decimal)initialAccum }));

            var req = new CalculateInterestRequest
            {
                PrincipalBalance = 30_000m,
                InterestRate = 0.1825m,
                InterestType = "PerDay",
                InterestFreePeriodDays = new DateTime(2025, 10, 16), // วันนี้ยังอยู่ในช่วงปลอดดอกเบี้ย
                ProductContactId = mockProductContactId,
                MaxInterestAmount = 999_999m
            };

            // Act
            var res = await _interestService.CalculateInterestAsync(req);
            Console.WriteLine($"ผลลัพธ์: ดอกเบี้ยรอบนี้={res.InterestAmount}, ดอกเบี้ยสะสมใหม่={res.AccumInterestRemain}");
            Console.WriteLine("--------------------------------");

            // Assert
            Assert.Equal(0m, res.InterestAmount);
            Assert.Equal(initialAccum, res.AccumInterestRemain);

            _mockInterestTransactionDAC.Verify(d => d.CreateInterestTransactionAsync(
                It.Is<CreateInterestTransactionDTO>(t =>
                    t.InterestAmount == 0m &&
                    t.AccumulatedAmount == initialAccum &&
                    t.Remark != null && t.Remark.Contains("ยกเว้น")
                )), Times.Once);

            _mockAccumulatedInterestDAC.Verify(d => d.UpdateAccumulatedInterestAsync(
                     It.IsAny<string>(),
                     It.Is<Dictionary<string, object?>>(dict =>
                         dict.ContainsKey("AccumInterestRemain") &&
                         (decimal)dict["AccumInterestRemain"] == (decimal)initialAccum
                     )
                ), Times.Never);
        }

        // กรณีทดสอบที่ 3: ไม่อยู่ในช่วงปลอดดอกเบี้ย -> ต้องคำนวณดอกเบี้ยรายวันตามปกติ
        [Fact]
        public async void CalculateInterest_NotInFreePeriod_CalculatesNormally()
        {
            Console.WriteLine("Alternative Cases กรณีทดสอบที่ 3: ไม่อยู่ในช่วงปลอดดอกเบี้ย -> ต้องคำนวณดอกเบี้ยรายวันตามปกติ");
            // Arrange
            var initialAccum = 50.00m;
            _mockAccumulatedInterestDAC
                .Setup(x => x.GetAccumulatedInterestByProductContactIdAsync(It.IsAny<RecordId>()))
                .Returns(Task.FromResult(new AccumulatedInterestResponseDTO { AccumInterestRemain = (decimal)initialAccum }));

            var req = new CalculateInterestRequest
            {
                PrincipalBalance = 30_000m,
                InterestRate = 0.1825m,
                InterestType = "PerDay",
                InterestFreePeriodDays = new DateTime(2025, 10, 14), // หมดช่วงปลอดดอกเบี้ยแล้ว
                ProductContactId = mockProductContactId,
                MaxInterestAmount = 999_999m
            };

            // Act
            var res = await _interestService.CalculateInterestAsync(req);
            Console.WriteLine($"ผลลัพธ์: ดอกเบี้ยรอบนี้={res.InterestAmount}, ดอกเบี้ยสะสมใหม่={res.AccumInterestRemain}");
            Console.WriteLine("--------------------------------");

            // Assert
            Assert.Equal(15.00m, res.InterestAmount); // (30,000 * 0.1825) / 365 = 15.00
            Assert.Equal(65.00m, res.AccumInterestRemain); // 50.00 + 15.00

            _mockInterestTransactionDAC.Verify(d => d.CreateInterestTransactionAsync(
                It.Is<CreateInterestTransactionDTO>(t =>
                    t.InterestAmount == 15.00m &&
                    t.AccumulatedAmount == 65.00m
                )), Times.Once);

            _mockAccumulatedInterestDAC.Verify(d => d.UpdateAccumulatedInterestAsync(
                     It.IsAny<string>(),
                     It.Is<Dictionary<string, object?>>(dict =>
                         dict.ContainsKey("AccumInterestRemain") &&
                         (decimal)dict["AccumInterestRemain"] == 65.00m
                     )
                ), Times.Once);
        }

        // กรณีทดสอบที่ 4: วันสิ้นสุดช่วงปลอดดอกเบี้ยเป็นวันนี้พอดี -> ยังถือว่าอยู่ในช่วงปลอดดอกเบี้ย (<=) ดอกเบี้ย 0 บาท
        [Fact]
        public async void CalculateInterest_FreePeriodEndsToday_StillZeroInterest()
        {
            Console.WriteLine("Alternative Cases กรณีทดสอบที่ 4: วันสิ้นสุดช่วงปลอดดอกเบี้ยเป็นวันนี้พอดี -> ยังถือว่าอยู่ในช่วงปลอดดอกเบี้ย (<=) ดอกเบี้ย 0 บาท");
            // Arrange
            var initialAccum = 10.00m;
            _mockAccumulatedInterestDAC
                .Setup(x => x.GetAccumulatedInterestByProductContactIdAsync(It.IsAny<RecordId>()))
                .Returns(Task.FromResult(new AccumulatedInterestResponseDTO { AccumInterestRemain = (decimal)initialAccum }));

            var req = new CalculateInterestRequest
            {
                PrincipalBalance = 30_000m,
                InterestRate = 0.1825m,
                InterestType = "PerDay",
                // ใช้วันที่เดียวกับวันที่ mock
                InterestFreePeriodDays = new DateTime(2025, 10, 15), // วันนี้พอดี
                ProductContactId = mockProductContactId,
                MaxInterestAmount = 999_999m
            };

            // Act
            var res = await _interestService.CalculateInterestAsync(req);
            Console.WriteLine($"ผลลัพธ์: ดอกเบี้ยรอบนี้={res.InterestAmount}, ดอกเบี้ยสะสมใหม่={res.AccumInterestRemain}");
            Console.WriteLine("--------------------------------");

            // Assert
            Assert.Equal(0m, res.InterestAmount);
            Assert.Equal(initialAccum, res.AccumInterestRemain);

            _mockInterestTransactionDAC.Verify(d => d.CreateInterestTransactionAsync(
                It.Is<CreateInterestTransactionDTO>(t =>
                    t.InterestAmount == 0m &&
                    t.AccumulatedAmount == initialAccum
                )), Times.Once);

            _mockAccumulatedInterestDAC.Verify(d => d.UpdateAccumulatedInterestAsync(
                     It.IsAny<string>(),
                     It.Is<Dictionary<string, object?>>(dict =>
                         dict.ContainsKey("AccumInterestRemain") &&
                         (decimal)dict["AccumInterestRemain"] == (decimal)initialAccum
                     )
                ), Times.Never);
        }
        #endregion

        #region Exception Cases
        // กรณีทดสอบที่ 1: Object Input เป็น Null
        [Fact]
        public async void CalculateInterest_NullRequest_ThrowsArgumentNullException()
        {
            Console.WriteLine("Exception Cases กรณีทดสอบที่ 1: Object Input เป็น Null");
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => _interestService.CalculateInterestAsync(null!));
            Console.WriteLine($"ผลลัพธ์: {ex.Message}");
            Console.WriteLine("--------------------------------");
        }

        // กรณีทดสอบที่ 2: Input ขาดหาย (Missing Value) - ทำสอบทุก key
        [Fact]
        public async void CalculateInterest_MissingRequiredField_ThrowsArgumentException()
        {
            Console.WriteLine($"Exception Cases กรณีทดสอบที่ 2: Input ขาดหาย (Missing Value)");
            var req = new CalculateInterestRequest
            {
                PrincipalBalance = 1m,
                InterestRate = 0.01m,
                InterestType = null,
                ProductContactId = mockProductContactId
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _interestService.CalculateInterestAsync(req));
            Console.WriteLine($"ผลลัพธ์: {ex.Message}");
            Console.WriteLine("--------------------------------");
        }

        // กรณีทดสอบที่ 3: ยอดคงเหลือติดลบ (Negative Value)
        [Fact]
        public async void CalculateInterest_NegativePrincipal_ThrowsArgumentException()
        {
            var req = new CalculateInterestRequest
            {
                PrincipalBalance = -1m,
                InterestRate = 0.01m,
                InterestType = "PerMonth",
                ProductContactId = mockProductContactId
            };

            await Assert.ThrowsAsync<ArgumentException>(() => _interestService.CalculateInterestAsync(req));
        }

        // กรณีทดสอบที่ 4: อัตราดอกเบี้ยจาก Config ติดลบ (Negative Value)
        [Fact]
        public async void CalculateInterest_NegativeRate_ThrowsArgumentException()
        {
            var req = new CalculateInterestRequest
            {
                PrincipalBalance = 100m,
                InterestRate = -0.01m,
                InterestType = "PerMonth",
                ProductContactId = mockProductContactId
            };

            await Assert.ThrowsAsync<ArgumentException>(() => _interestService.CalculateInterestAsync(req));
        }

        // กรณีทดสอบที่ 5: ประเภทดอกเบี้ยเป็นค่าว่าง (Empty String)
        [Fact]
        public async void CalculateInterest_EmptyInterestType_ThrowsArgumentException()
        {
            var req = new CalculateInterestRequest
            {
                PrincipalBalance = 100m,
                InterestRate = 0.01m,
                InterestType = "",
                ProductContactId = mockProductContactId
            };

            await Assert.ThrowsAsync<ArgumentException>(() => _interestService.CalculateInterestAsync(req));
        }

        // กรณีทดสอบที่ 6: ประเภทดอกเบี้ยเป็น Whitespace
        [Fact]
        public async void CalculateInterest_WhitespaceInterestType_ThrowsArgumentException()
        {
            var req = new CalculateInterestRequest
            {
                PrincipalBalance = 100m,
                InterestRate = 0.01m,
                InterestType = " ",
                ProductContactId = mockProductContactId
            };

            await Assert.ThrowsAsync<ArgumentException>(() => _interestService.CalculateInterestAsync(req));
        }

        // กรณีทดสอบที่ 7: ประเภทดอกเบี้ยไม่รองรับ (Unsupported Type)
        [Fact]
        public async void CalculateInterest_InvalidInterestType_ThrowsArgumentException()
        {
            var req = new CalculateInterestRequest
            {
                PrincipalBalance = 100m,
                InterestRate = 0.01m,
                InterestType = "PerYear",
                ProductContactId = mockProductContactId
            };

            await Assert.ThrowsAsync<ArgumentException>(() => _interestService.CalculateInterestAsync(req));
        }

        // กรณีทดสอบที่ 8: ไม่พบข้อมูล Config (เช่น AccumulatedInterest ไม่พบ)
        [Fact]
        public async void CalculateInterest_ConfigNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange: ให้ชั้น DAC โยน KeyNotFoundException เมื่อดึง AccumulatedInterest
            _mockAccumulatedInterestDAC
                .Setup(x => x.GetAccumulatedInterestByProductContactIdAsync(It.IsAny<RecordId>()))
                .Throws(new KeyNotFoundException("AccumulatedInterest not found"));

            var req = new CalculateInterestRequest
            {
                PrincipalBalance = 100m,
                InterestRate = 0.01m,
                InterestType = "PerMonth",
                ProductContactId = mockProductContactId,
                MaxInterestAmount = 999_999m
            };

            // Act + Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _interestService.CalculateInterestAsync(req));
        }

        // กรณีทดสอบที่ 9: มี Validation Errors หลายตัว
        [Fact]
        public async void CalculateInterest_MultipleValidationErrors_ThrowsArgumentException()
        {
            var req = new CalculateInterestRequest
            {
                PrincipalBalance = -100m, // ผิด: ติดลบ
                InterestRate = -0.5m,     // ผิด: ติดลบ
                InterestType = "PerYear", // ผิด: ไม่รองรับ
                ProductContactId = mockProductContactId
            };

            await Assert.ThrowsAsync<ArgumentException>(() => _interestService.CalculateInterestAsync(req));
        }

        // กรณีทดสอบที่ 10: ชั้น Data Access (DAC) เกิด Exception
        [Fact]
        public async void CalculateInterest_DacThrowsException_Propagates()
        {
            // Arrange: ให้ดึงสะสมสำเร็จ แต่ตอน CreateTransaction โยน Exception
            _mockAccumulatedInterestDAC
              .Setup(x => x.GetAccumulatedInterestByProductContactIdAsync(It.IsAny<RecordId>()))
              .Returns(Task.FromResult(new AccumulatedInterestResponseDTO { AccumInterestRemain = 0m }));

            _mockInterestTransactionDAC
                .Setup(x => x.CreateInterestTransactionAsync(It.IsAny<CreateInterestTransactionDTO>()))
                .Throws(new InvalidOperationException("DAC failure"));

            var req = new CalculateInterestRequest
            {
                PrincipalBalance = 100m,
                InterestRate = 0.01m,
                InterestType = "PerMonth",
                ProductContactId = mockProductContactId,
                MaxInterestAmount = 999_999m
            };

            // Act + Assert: ควรให้ Exception เดิมถูกส่งต่อออกมา
            await Assert.ThrowsAsync<InvalidOperationException>(() => _interestService.CalculateInterestAsync(req));
        }
        #endregion
    }
}