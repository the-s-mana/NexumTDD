using Moq;
using Nexum.Server.DAC;
using Nexum.Server.Models;
using Nexum.Server.Services;
using Xunit;

namespace Nexum.Tests
{
    public class InterestServiceTests
    {
        //private readonly Mock<INexumConfigDAC> _mockNexumConfigDAC;

        private readonly Mock<IAccumulatedInterestDAC> _mockAccumulatedInterestDAC;
        private readonly Mock<IInterestTransactionDAC> _mockInterestTransactionDAC;
        private readonly InterestService _interestService;

        public InterestServiceTests()
        {
            //_mockNexumConfigDAC = new Mock<INexumConfigDAC>();
            _mockAccumulatedInterestDAC = new Mock<IAccumulatedInterestDAC>();
            _mockInterestTransactionDAC = new Mock<IInterestTransactionDAC>();

            _interestService = new InterestService(_mockAccumulatedInterestDAC.Object, _mockInterestTransactionDAC.Object);
        }

        //1. Normal Cases (8 Cases)
        #region Normal Cases
        public static IEnumerable<object[]> PerMonthTestData()
        {
            yield return new object[] { 10000m, 0.15m, 500m, 1500m, 2000m };
            yield return new object[] { 5000m, 0.12m, 200m, 600m, 800m };
            yield return new object[] { 20000m, 0.18m, 1000m, 3600m, 4600m };
            yield return new object[] { 1000000m, 0.12m, 50000m, 120000m, 170000m };
        }

        [Theory(DisplayName = "Normal - คำนวณรายเดือน (PerMonth)")]
        [MemberData(nameof(PerMonthTestData))]
        public void CalculateInterest_PerMonth_ShouldCalculateCorrectly(decimal p, decimal r, decimal acc, decimal expectedInterest, decimal expectedAcc)
        {
            //Arrange
            var request = new CalculateInterestRequest { PrincipalBalance = p, InterestRate = r, InterestType = "PerMonth", ProductContactId = 1, MaxInterestAmount = decimal.MaxValue };
            _mockAccumulatedInterestDAC.Setup(d => d.GetAccumulatedInterest(1)).Returns(new AccumulatedInterest { AccumInterestRemain = acc });
            //Act
            var result = _interestService.CalculateInterest(request);
            //Assert & Verify
            Assert.Equal(expectedInterest, result.InterestAmount);
            Assert.Equal(expectedAcc, result.AccumInterestRemain);
            _mockInterestTransactionDAC.Verify(d => d.CreateInterestTransaction(It.IsAny<InterestTransaction>()), Times.Once);
            _mockAccumulatedInterestDAC.Verify(d => d.UpdateAccumulatedInterest(expectedAcc), Times.Once);
        }

        public static IEnumerable<object[]> PerDayTestData()
        {
            yield return new object[] { 10000m, 0.15m, 300m, 4.11m, 304.11m };
            yield return new object[] { 5000m, 0.12m, 100m, 1.64m, 101.64m };
            yield return new object[] { 20000m, 0.18m, 500m, 9.86m, 509.86m };
            yield return new object[] { 100000m, 0.20m, 1000m, 54.79m, 1054.79m };
        }

        [Theory(DisplayName = "Normal - คำนวณรายวัน (PerDay)")]
        [MemberData(nameof(PerDayTestData))]
        public void CalculateInterest_PerDay_ShouldCalculateCorrectly(decimal p, decimal r, decimal acc, decimal expectedInterest, decimal expectedAcc)
        {
            //Arrange
            var request = new CalculateInterestRequest { PrincipalBalance = p, InterestRate = r, InterestType = "PerDay", ProductContactId = 1, MaxInterestAmount = decimal.MaxValue };
            _mockAccumulatedInterestDAC.Setup(d => d.GetAccumulatedInterest(1)).Returns(new AccumulatedInterest { AccumInterestRemain = acc });
            //Act
            var result = _interestService.CalculateInterest(request);
            //Assert & Verify
            Assert.Equal(expectedInterest, result.InterestAmount);
            Assert.Equal(expectedAcc, result.AccumInterestRemain);
            _mockInterestTransactionDAC.Verify(d => d.CreateInterestTransaction(It.IsAny<InterestTransaction>()), Times.Once);
            _mockAccumulatedInterestDAC.Verify(d => d.UpdateAccumulatedInterest(expectedAcc), Times.Once);
        }
        #endregion

        //2. Alternative Cases (8 Cases)
        #region Alternative Cases
        public static IEnumerable<object[]> ZeroValuesTestData()
        {
            yield return new object[] { 0m, 0.15m, 100m, 0m, 100m };
            yield return new object[] { 10000m, 0m, 200m, 0m, 200m };
            yield return new object[] { 0m, 0m, 50m, 0m, 50m };
        }

        [Theory(DisplayName = "Alternative - กรณีค่าเป็นศูนย์")]
        [MemberData(nameof(ZeroValuesTestData))]
        public void CalculateInterest_ZeroValues_ShouldReturnZeroInterest(decimal p, decimal r, decimal acc, decimal expectedInterest, decimal expectedAcc)
        {
            //Arrange
            var request = new CalculateInterestRequest { PrincipalBalance = p, InterestRate = r, InterestType = "PerMonth", ProductContactId = 1, MaxInterestAmount = decimal.MaxValue };
            _mockAccumulatedInterestDAC.Setup(d => d.GetAccumulatedInterest(1)).Returns(new AccumulatedInterest { AccumInterestRemain = acc });
            //Act
            var result = _interestService.CalculateInterest(request);
            //Assert
            Assert.Equal(expectedInterest, result.InterestAmount);
            Assert.Equal(expectedAcc, result.AccumInterestRemain);
        }

        public static IEnumerable<object[]> InterestFreePeriodTestData()
        {
            yield return new object[] { DateTime.Now.AddDays(15), 500m, 0m, 500m, false };
            yield return new object[] { DateTime.Now.AddDays(-5), 300m, 1500m, 1800m, true };
            yield return new object[] { DateTime.Now, 200m, 1500m, 1700m, true };
        }

        [Theory(DisplayName = "Alternative - กรณีช่วงปลอดดอกเบี้ย")]
        [MemberData(nameof(InterestFreePeriodTestData))]
        public void CalculateInterest_InterestFreePeriod_ShouldBehaveCorrectly(DateTime freePeriod, decimal acc, decimal expectedInterest, decimal expectedAcc, bool shouldUpdate)
        {
            //Arrange
            var request = new CalculateInterestRequest { PrincipalBalance = 10000m, InterestRate = 0.15m, InterestType = "PerMonth", ProductContactId = 1, InterestFreePeriodDays = freePeriod, MaxInterestAmount = decimal.MaxValue };
            _mockAccumulatedInterestDAC.Setup(d => d.GetAccumulatedInterest(1)).Returns(new AccumulatedInterest { AccumInterestRemain = acc });
            //Act
            var result = _interestService.CalculateInterest(request);
            //Assert
            Assert.Equal(expectedInterest, result.InterestAmount);
            Assert.Equal(expectedAcc, result.AccumInterestRemain);
            //Verify
            _mockInterestTransactionDAC.Verify(d => d.CreateInterestTransaction(It.IsAny<InterestTransaction>()), Times.Once);
            _mockAccumulatedInterestDAC.Verify(d => d.UpdateAccumulatedInterest(It.IsAny<decimal>()), shouldUpdate ? Times.Once() : Times.Never());
        }

        public static IEnumerable<object[]> MaxInterestAmountTestData()
        {
            yield return new object[] { "PerMonth", 1000000m, 0.20m, 50000m, 150000m, 200000m, 150000m };
            yield return new object[] { "PerDay", 1000000m, 0.365m, 2000m, 500m, 2500m, 500m };
        }

        [Theory(DisplayName = "Alternative - กรณีดอกเบี้ยเกินเพดาน")]
        [MemberData(nameof(MaxInterestAmountTestData))]
        public void CalculateInterest_ExceedsMaxAmount_ShouldCapInterest(string type, decimal p, decimal r, decimal acc, decimal expectedInterest, decimal expectedAcc, decimal max)
        {
            //Arrange
            var request = new CalculateInterestRequest { PrincipalBalance = p, InterestRate = r, InterestType = type, ProductContactId = 1, MaxInterestAmount = max };
            _mockAccumulatedInterestDAC.Setup(d => d.GetAccumulatedInterest(1)).Returns(new AccumulatedInterest { AccumInterestRemain = acc });
            //Act
            var result = _interestService.CalculateInterest(request);
            //Assert
            Assert.Equal(expectedInterest, result.InterestAmount);
        }
        #endregion

        //3. Exception Cases (10 Cases)
        #region Exception Cases
        public static IEnumerable<object[]> ValidationErrorTestData()
        {
            yield return new object[] { new CalculateInterestRequest { PrincipalBalance = -1m, InterestRate = 0.15m, InterestType = "PerMonth" }, "PrincipalBalance cannot be negative" };
            yield return new object[] { new CalculateInterestRequest { PrincipalBalance = 10000m, InterestRate = -0.1m, InterestType = "PerMonth" }, "InterestRate cannot be negative" };
            yield return new object[] { new CalculateInterestRequest { PrincipalBalance = 10000m, InterestRate = 0.15m, InterestType = null! }, "InterestType is required" };
            yield return new object[] { new CalculateInterestRequest { PrincipalBalance = 10000m, InterestRate = 0.15m, InterestType = "" }, "InterestType is required" };
            yield return new object[] { new CalculateInterestRequest { PrincipalBalance = 10000m, InterestRate = 0.15m, InterestType = "   " }, "InterestType is required" };
            yield return new object[] { new CalculateInterestRequest { PrincipalBalance = 10000m, InterestRate = 0.15m, InterestType = "Invalid" }, "InterestType is invalid" };
            yield return new object[] { new CalculateInterestRequest { PrincipalBalance = 10000m, InterestRate = 0.15m, InterestType = "PerMonth", MaxInterestAmount = -1m }, "MaxInterestAmount cannot be negative" };
        }

        [Theory(DisplayName = "Exception - Input Validation")]
        [MemberData(nameof(ValidationErrorTestData))]
        public void CalculateInterest_InvalidData_ShouldThrowArgumentException(CalculateInterestRequest request, string expectedMessage)
        {
            //Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => _interestService.CalculateInterest(request));
            Assert.Contains(expectedMessage, ex.Message);
            _mockAccumulatedInterestDAC.Verify(d => d.GetAccumulatedInterest(It.IsAny<int>()), Times.Never);
        }

        [Fact(DisplayName = "Exception - Null Request")]
        public void CalculateInterest_NullRequest_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _interestService.CalculateInterest(null!));
        }

        [Fact(DisplayName = "Exception - DAC Failure")]
        public void CalculateInterest_DacFailure_ShouldPropagateException()
        {
            //Arrange
            var request = new CalculateInterestRequest { PrincipalBalance = 10000m, InterestRate = 0.15m, InterestType = "PerMonth", ProductContactId = 1, MaxInterestAmount = decimal.MaxValue };
            _mockAccumulatedInterestDAC.Setup(d => d.GetAccumulatedInterest(1)).Throws(new InvalidOperationException());
            //Act & Assert
            Assert.Throws<InvalidOperationException>(() => _interestService.CalculateInterest(request));
        }

        [Fact(DisplayName = "Exception - Multiple Validation Errors")]
        public void CalculateInterest_MultipleErrors_ShouldThrowFirst()
        {
            //Arrange
            var request = new CalculateInterestRequest { PrincipalBalance = -1m, InterestRate = -0.1m };
            //Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => _interestService.CalculateInterest(request));
            Assert.Contains("PrincipalBalance", ex.Message); // Should fail on the first error
        }
        #endregion
    }
}
