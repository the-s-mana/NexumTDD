using Moq;
using Nexum.Server.DAC;
using Nexum.Server.Models;
using Nexum.Server.Models.Penalty;
using Nexum.Server.Services.Penalty;

namespace Nexum.Tests
{
    public record Case(
        PenaltyRequest Request,
        PenaltyResponse Expected
    );
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
                PenaltyFreePeriodDays = 5
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

        public static IEnumerable<object[]> Cases()
        {
            var now = DateTime.Now.Date;
            var req = new PenaltyRequest
            {
                UserId = 1,
                PenaltyPolicyID = 1,
                ActiveStatus = "Active",
                OutstandingBalance = 5000m,
                DueDate = now.AddDays(-6),
                PaymentAmount = 1000m  
            };
            PenaltyResponse expected = new PenaltyResponse
            {
                UserId = 1,
                OutstandingBalance = 5000m,
                MinimumPayment = 500m,
                PenaltyAmount = 600m,
            };

            yield return new object[] { new Case(req, expected) };
        }

        [Theory(DisplayName = "คำนวณค่าปรับตามสัญญา")]
        [MemberData(nameof(Cases))]
        public void CalculatePenalty_NormalCase(Case c)
        {
            //Arrange
            var sut = CreateSut(); 
            
            var policy = _policyList.Single(p => p.PenaltyPolicyID == c.Request.PenaltyPolicyID);

            switch (policy.PenaltyType)
            {
                case "Daily":
                    _daily.Setup(d => d.Calculate(It.IsAny<PenaltyContext>()))
                          .Returns((PenaltyContext ctx) =>
                              Math.Min(ctx.OverdueDays * ctx.FixedAmount, ctx.TotalCap));
                    break;
                case "Fixed":
                    _fixed.Setup(f => f.Calculate(It.IsAny<PenaltyContext>()))
                          .Returns((PenaltyContext ctx) => ctx.FixedAmount);
                    break;
                case "Percentage": 
                    _percentage.Setup(p => p.Calculate(It.IsAny<PenaltyContext>()))
                               .Returns((PenaltyContext ctx) =>
                                   Math.Min(ctx.OutstandingBalance * ctx.Percentage, ctx.MaxPenalty));
                    break;
                    default:
                    throw new NotImplementedException($"Penalty type '{policy.PenaltyType}' is not implemented.");
            }

            //Act
            var result = sut.GetPenalty(c.Request);

            //Assert
            Console.WriteLine($"[EXP] UserId={c.Expected.UserId}, " +
                  $"Outstanding={c.Expected.OutstandingBalance}, Min={c.Expected.MinimumPayment}, " +
                  $"Penalty={c.Expected.PenaltyAmount}");
            Console.WriteLine($"[ACT] UserId={result.UserId}," +
                                $"Outstanding={result.OutstandingBalance}, Min={result.MinimumPayment}, " +
                              $"Penalty={result.PenaltyAmount}");
            Console.WriteLine($"Policy={policy.PenaltyType}, " +
                          $"Policy Id={policy.PenaltyPolicyID}");
            Assert.NotNull(result);
            Assert.Equal(c.Expected.UserId, result.UserId);
            Assert.Equal(c.Expected.OutstandingBalance, result.OutstandingBalance);
            Assert.Equal(c.Expected.PenaltyAmount, result.PenaltyAmount);

            _policies.Verify(p => p.penaltyPolicies(
                It.Is<PenaltyPoliciesRequest>(x => x.PenaltyPolicyID == c.Request.PenaltyPolicyID)),
                Times.Once());

            _daily.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()),
                policy.PenaltyType == "Daily" ? Times.Once() : Times.Never());

            _percentage.Verify(p => p.Calculate(It.IsAny<PenaltyContext>()),
                policy.PenaltyType == "Percentage" ? Times.Once() : Times.Never());

            _fixed.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()),
                policy.PenaltyType == "Fixed" ? Times.Once() : Times.Never());
        }
    }
}
