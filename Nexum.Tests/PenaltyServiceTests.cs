//using Moq;
//using Nexum.Server.Models;
//using Nexum.Server.Models.Penalty;
//using Nexum.Server.Services.Penalty;

//namespace Nexum.Tests
//{
//    public record Case(
//        PenaltyRequest Request,
//        PenaltyResponse Expected
//    );
//    public class PenaltyServiceTests
//    {
//        private readonly Mock<IPercentagePenalty> _percentage = new();
//        private readonly Mock<IPenaltyPolicies> _policies = new();
//        private readonly Mock<IDailyPenalty> _daily = new();
//        private readonly Mock<IFixedPenalty> _fixed = new();

//        private Penalty CreateSut()
//        {
//            _policies.Setup(p => p.penaltyPolicies(It.IsAny<PenaltyPoliciesRequest>()))
//                    .Returns((PenaltyPoliciesRequest req) =>
//                        _policyList.Single(x => x.PenaltyPolicyID == req.PenaltyPolicyID));

//            return new Penalty(_percentage.Object, _policies.Object, _daily.Object, _fixed.Object);
//        }
//        #region Policy List
//        private readonly List<ProductContact> _policyList = new()
//        {
//           new ProductContact
//            {
//                PenaltyPolicyID = 1,
//                PenaltyType = "Daily",
//                FixedAmount = 100m,
//                TotalCap = 1000m,
//                PenaltyFreePeriodDays = 5
//            },
//            new ProductContact
//            {
//                PenaltyPolicyID = 2,
//                PenaltyType = "Fixed",
//                FixedAmount = 200m,
//            },
//            new ProductContact
//            {
//                PenaltyPolicyID = 3,
//                PenaltyType = "Percentage",
//                PenaltyRate = 2.5m,
//                MaxPenalty = 300m,
//                PenaltyFreePeriodDays = 5
//            },
//            new ProductContact
//            {
//                PenaltyPolicyID = 4,
//                PenaltyType = "Daily",
//                FixedAmount = 200m,
//                MaxPenalty = 400m,
//                TotalCap = 1200m,
//                PenaltyFreePeriodDays = 2
//            },
//        };
//        #endregion

//        #region NormalCases
//        public static IEnumerable<object[]> NormalCases()
//        {
//            var now = DateTime.Now.Date;

//            // 1) Daily: ค่าปรับรายวัน เกินช่วงผ่อนผัน
//            yield return new object[] {
//        new Case(
//            Request: new PenaltyRequest{
//                UserId = 1,
//                PenaltyPolicyID = 1,
//                ActiveStatus = "Active",
//                OutstandingBalance = 5000m,
//                DueDate = now.AddDays(-6),
//                PaymentAmount = 1000m
//            },
//            Expected: new PenaltyResponse{
//                UserId = 1,
//                OutstandingBalance = 5000m,
//                MinimumPayment = 500m,
//                PenaltyAmount = 600m
//            }
//        )
//    };

//            // 2) Fixed: ค่าปรับคงที่ 200
//            yield return new object[] {
//        new Case(
//            Request: new PenaltyRequest{
//                UserId = 3,
//                PenaltyPolicyID = 2,          // Fixed=200
//                ActiveStatus = "Active",
//                OutstandingBalance = 3000m,
//                DueDate = now.AddDays(-1),
//                PaymentAmount = 0m
//            },
//            Expected: new PenaltyResponse{
//                UserId = 3,
//                OutstandingBalance = 3000m,
//                MinimumPayment = 300m,
//                PenaltyAmount = 200m
//            }
//        )
//    };
//            // 3) Percentage: ไม่ชน Max (2.5% ของ 4000 = 100)
//            yield return new object[] {
//        new Case(
//            Request: new PenaltyRequest{
//                UserId = 3,
//                PenaltyPolicyID = 3,          // Rate=2.5, Max=300, Grace=5
//                ActiveStatus = "Active",
//                OutstandingBalance = 4000m,
//                DueDate = now.AddDays(-10),
//                PaymentAmount = 0m
//            },
//            Expected: new PenaltyResponse{
//                UserId = 3,
//                OutstandingBalance = 4000m,
//                MinimumPayment = 400m,
//                PenaltyAmount = 100m          // 4000 * 0.025
//            }
//        )
//    };
            
//            // 4) ไม่คิดปรับ: ไม่เกินกำหนด + จ่ายถึงขั้นต่ำ
//            yield return new object[] {
//        new Case(
//            Request: new PenaltyRequest{
//                UserId = 3,
//                PenaltyPolicyID = 2,          // Fixed=200 (แต่ไม่โดนคิดปรับเพราะไม่ Overdue/under-min)
//                ActiveStatus = "Active",
//                OutstandingBalance = 3000m,
//                DueDate = now,                 // ต้องไม่ > now
//                PaymentAmount = 300m           // = min 10%
//            },
//            Expected: new PenaltyResponse{
//                UserId = 3,
//                OutstandingBalance = 3000m,
//                MinimumPayment = 300m,
//                PenaltyAmount = 0m
//            }
//        )
//    };
//    }
//        #endregion
//        #region NormalCases Test
//        [Theory(DisplayName = "คำนวณค่าปรับตามสัญญา")]
//        [MemberData(nameof(NormalCases))]
//        public void Penalty_Calculator_Normal_Cases(Case c)
//        {
//            //Arrange
//            var sut = CreateSut();

//            var policy = _policyList.Single(p => p.PenaltyPolicyID == c.Request.PenaltyPolicyID);

//            switch (policy.PenaltyType)
//            {
//                case "Daily":
//                    _daily.Setup(d => d.Calculate(It.IsAny<PenaltyContext>()))
//                          .Returns((PenaltyContext ctx) =>
//                              Math.Min(ctx.OverdueDays * ctx.FixedAmount, ctx.TotalCap));
//                    break;
//                case "Fixed":
//                    _fixed.Setup(f => f.Calculate(It.IsAny<PenaltyContext>()))
//                          .Returns((PenaltyContext ctx) => ctx.FixedAmount);
//                    break;
//                case "Percentage":
//                    _percentage.Setup(p => p.Calculate(It.IsAny<PenaltyContext>()))
//                               .Returns((PenaltyContext ctx) =>
//                                   Math.Min(ctx.OutstandingBalance * (ctx.Percentage / 100), ctx.MaxPenalty));
//                    break;
//                default:
//                    throw new NotImplementedException($"Penalty type '{policy.PenaltyType}' is not implemented.");
//            }

//            //Act
//            var result = sut.GetPenalty(c.Request);



//            //Assert
//            Console.WriteLine($"[EXP] UserId={c.Expected.UserId}, " +
//                  $"Outstanding={c.Expected.OutstandingBalance}, Min={c.Expected.MinimumPayment}, " +
//                  $"Penalty={c.Expected.PenaltyAmount}");
//            Console.WriteLine($"[ACT] UserId={result.UserId}," +
//                                $"Outstanding={result.OutstandingBalance}, Min={result.MinimumPayment}, " +
//                              $"Penalty={result.PenaltyAmount}");
//            Console.WriteLine($"Policy={policy.PenaltyType}, " +
//                          $"Policy Id={policy.PenaltyPolicyID}");
//            Assert.NotNull(result);
//            Assert.Equal(c.Expected.UserId, result.UserId);
//            Assert.Equal(c.Expected.OutstandingBalance, result.OutstandingBalance);
//            Assert.Equal(c.Expected.PenaltyAmount, result.PenaltyAmount);

//            var now = DateTime.Now.Date;
//            var minPayment = c.Request.OutstandingBalance * 0.1m;
//            var isOverdue = c.Request.DueDate < now;
//            var underMin = c.Request.PaymentAmount < minPayment;
//            var graceDays = policy.PenaltyFreePeriodDays;   // หรือ GracePeriodDays ตามโมเดลจริง
//            var overdueDays = Math.Max(0, (now - c.Request.DueDate.Date).Days);
//            var shouldInvoke = (isOverdue || underMin) && (overdueDays > graceDays);


//            _policies.Verify(p => p.penaltyPolicies(
//                It.Is<PenaltyPoliciesRequest>(x => x.PenaltyPolicyID == c.Request.PenaltyPolicyID)),
//                Times.Once());

//            _daily.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()),
//                (policy.PenaltyType == "Daily" && shouldInvoke) ? Times.Once() : Times.Never());
//            _fixed.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()),
//                (policy.PenaltyType == "Fixed" && shouldInvoke) ? Times.Once() : Times.Never());
//            _percentage.Verify(p => p.Calculate(It.IsAny<PenaltyContext>()),
//                (policy.PenaltyType == "Percentage" && shouldInvoke) ? Times.Once() : Times.Never());
//        }
//        #endregion

//        #region AlternativeCases
//        public static IEnumerable<object[]> AlternativeCases()
//        {
//            var now = DateTime.Now.Date;

//            // 1) Daily: อยู่ในช่วง Grace (3 วัน < 5) ⇒ ไม่คิดปรับ
//            yield return new object[] {
//        new Case(
//            Request: new PenaltyRequest{
//                UserId = 1,
//                PenaltyPolicyID = 1,          // Daily: Fixed=100, TotalCap=1000, Grace=5
//                ActiveStatus = "Active",
//                OutstandingBalance = 3000m,
//                DueDate = now.AddDays(-1),
//                PaymentAmount = 0m
//            },
//            Expected: new PenaltyResponse{
//                UserId = 1,
//                OutstandingBalance = 3000m,
//                MinimumPayment = 300m,
//                PenaltyAmount = 0m
//            }
//        )
//    };

//            // 2) Daily: เกินมากจนชนเพดาน TotalCap ⇒ 20*100=2000 แต่ cap=1000
//            yield return new object[] {
//        new Case(
//            Request: new PenaltyRequest{
//                UserId = 3,
//                PenaltyPolicyID = 1,          // Daily: Fixed=100, TotalCap=1000, Grace=5
//                ActiveStatus = "Active",
//                OutstandingBalance = 10000m,
//                DueDate = now.AddDays(-20),
//                PaymentAmount = 0m
//            },
//            Expected: new PenaltyResponse{
//                UserId = 3,
//                OutstandingBalance = 10000m,
//                MinimumPayment = 1000m,
//                PenaltyAmount = 1000m         // ชน cap
//            }
//        )
//    };

//            // 3) Percentage: ชน Max (2.5% ของ 20000 = 500 > 300) และเกิน Grace
//            yield return new object[] {
//        new Case(
//            Request: new PenaltyRequest{
//                UserId = 3,
//                PenaltyPolicyID = 3,          // Rate=2.5, Max=300, Grace=5
//                ActiveStatus = "Active",
//                OutstandingBalance = 20000m,
//                DueDate = now.AddDays(-6),    // 6 > 5 ⇒ คิดปรับ
//                PaymentAmount = 0m
//            },
//            Expected: new PenaltyResponse{
//                UserId = 3,
//                OutstandingBalance = 20000m,
//                MinimumPayment = 2000m,
//                PenaltyAmount = 300m          // ชน Max
//            }
//        )
//    };
//        }
//        #endregion
//        #region AlternativeCases Test
//        [Theory(DisplayName = "คำนวณค่าปรับที่มีการชน Cap หรืออยู่ในช่วงผ่อนผัน")]
//        [MemberData(nameof(AlternativeCases))]
//        public void Penalty_Calculator_Alternative_Cases(Case c)
//        {
//            //Arrange
//            var sut = CreateSut();

//            var policy = _policyList.Single(p => p.PenaltyPolicyID == c.Request.PenaltyPolicyID);

//            switch (policy.PenaltyType)
//            {
//                case "Daily":
//                    _daily.Setup(d => d.Calculate(It.IsAny<PenaltyContext>()))
//                          .Returns((PenaltyContext ctx) =>
//                              Math.Min(ctx.OverdueDays * ctx.FixedAmount, ctx.TotalCap));
//                    break;
//                case "Fixed":
//                    _fixed.Setup(f => f.Calculate(It.IsAny<PenaltyContext>()))
//                          .Returns((PenaltyContext ctx) => ctx.FixedAmount);
//                    break;
//                case "Percentage":
//                    _percentage.Setup(p => p.Calculate(It.IsAny<PenaltyContext>()))
//                               .Returns((PenaltyContext ctx) =>
//                                   Math.Min(ctx.OutstandingBalance * (ctx.Percentage / 100), ctx.MaxPenalty));
//                    break;
//                default:
//                    throw new NotImplementedException($"Penalty type '{policy.PenaltyType}' is not implemented.");
//            }

//            //Act
//            var result = sut.GetPenalty(c.Request);

//            //Assert
//            Assert.NotNull(result);
//            Assert.Equal(c.Expected.UserId, result.UserId);
//            Assert.Equal(c.Expected.OutstandingBalance, result.OutstandingBalance);
//            Assert.Equal(c.Expected.PenaltyAmount, result.PenaltyAmount);

//            var now = DateTime.Now.Date;
//            var minPayment = c.Request.OutstandingBalance * 0.1m;
//            var isOverdue = c.Request.DueDate < now;
//            var underMin = c.Request.PaymentAmount < minPayment;
//            var graceDays = policy.PenaltyFreePeriodDays;   // หรือ GracePeriodDays ตามโมเดลจริง
//            var overdueDays = Math.Max(0, (now - c.Request.DueDate.Date).Days);
//            var shouldInvoke = (isOverdue || underMin) && (overdueDays > graceDays);


//            _policies.Verify(p => p.penaltyPolicies(
//                It.Is<PenaltyPoliciesRequest>(x => x.PenaltyPolicyID == c.Request.PenaltyPolicyID)),
//                Times.Once());

//            _daily.Verify(d => d.Calculate(It.IsAny<PenaltyContext>()),
//                (policy.PenaltyType == "Daily" && shouldInvoke) ? Times.Once() : Times.Never());
//            _fixed.Verify(f => f.Calculate(It.IsAny<PenaltyContext>()),
//                (policy.PenaltyType == "Fixed" && shouldInvoke) ? Times.Once() : Times.Never());
//            _percentage.Verify(p => p.Calculate(It.IsAny<PenaltyContext>()),
//                (policy.PenaltyType == "Percentage" && shouldInvoke) ? Times.Once() : Times.Never());
//        }
//        #endregion

//        #region ExceptionCases
//        public static IEnumerable<object[]> ExceptionCases()
//        {
//            var now = DateTime.Now.Date;

//            // 1) ข้อมูลที่ส่งให้เป็น null
//            yield return new object[] {
//        (PenaltyRequest)null!,
//        typeof(ArgumentNullException)
//    };
//            // 2) ยอดค้างชำระ <= 0
//            yield return new object[] {
//        new PenaltyRequest{
//            UserId = 1,
//            PenaltyPolicyID = 1,
//            ActiveStatus = "Active",
//            OutstandingBalance = 0m,
//            DueDate = now.AddDays(-1),
//            PaymentAmount = 0m
//        },
//        typeof(ArgumentException)
//    };

//            // 3) userId <= 0
//            yield return new object[] {
//        new PenaltyRequest {
//            UserId = 0, 
//            ActiveStatus = "Active",
//            PenaltyPolicyID = 1, 
//            OutstandingBalance = 100m,
//            DueDate = now.AddDays(-1), 
//            PaymentAmount = 0m
//        },
//        typeof(ArgumentException)
//    };
//            // 4) สถานะบัญชีเป็น null
//            yield return new object[] {
//        new PenaltyRequest {
//            UserId = 1, 
//            ActiveStatus = null!,
//            PenaltyPolicyID = 1, 
//            OutstandingBalance = 100m,
//            DueDate = now.AddDays(-1), 
//            PaymentAmount = 0m
//        },
//        typeof(ArgumentException)
//    };
//            // 5) สถานะบัญชีไม่ถูกต้อง
//            yield return new object[] {
//        new PenaltyRequest {
//            UserId = 1,
//            ActiveStatus = "Pause",
//            PenaltyPolicyID = 1, 
//            OutstandingBalance = 100m,
//            DueDate = now.AddDays(-1), 
//            PaymentAmount = 0m
//        },
//        typeof(ArgumentException)
//    };
//            // 6) สถานะบัญชีเป็น Inactive
//            yield return new object[] {
//        new PenaltyRequest {
//            UserId = 1, 
//            ActiveStatus = "Inactive",
//            PenaltyPolicyID = 1,
//            OutstandingBalance = 100m,
//            DueDate = now.AddDays(-1),
//            PaymentAmount = 0m
//        },
//        typeof(InvalidOperationException)
//    };
//            // 7) DueDate อยู่ในอนาคต
//            yield return new object[] {
//        new PenaltyRequest {
//            UserId = 1,
//            ActiveStatus = "Active",
//            PenaltyPolicyID = 1, 
//            OutstandingBalance = 100m,
//            DueDate = now.AddDays(+1), 
//            PaymentAmount = 0m
//        },
//        typeof(InvalidOperationException)
//    };
//            // 8) PolicyID ไม่ตรงกับนโยบายใดๆ
//            yield return new object[] {
//        new PenaltyRequest {
//            UserId = 1,
//            ActiveStatus = "Active",
//            PenaltyPolicyID = 999, 
//            OutstandingBalance = 100m,
//            DueDate = now.AddDays(-1), 
//            PaymentAmount = 0m
//        },
//        typeof(InvalidOperationException)
//    };
//        }
//        #endregion
//        #region ExceptionCases Test
//        [Theory(DisplayName = "กรณีข้อมูลขัดแย้งหรือไม่สมบูรณ์")]

//        [MemberData(nameof(ExceptionCases))]
//        public void Penalty_Calculator_Exception_Cases(PenaltyRequest req, Type expectedException)
//        {
//            //Arrange
//            var sut = CreateSut();

//            //Act
//            var ex = Record.Exception(() => sut.GetPenalty(req));

//            //Assert
//            Assert.NotNull(ex);
//            Assert.IsType(expectedException, ex);
//            Console.WriteLine($"[THROW] {ex.GetType().Name}: {ex.Message}");
//        }
//        #endregion
//    }
//}
