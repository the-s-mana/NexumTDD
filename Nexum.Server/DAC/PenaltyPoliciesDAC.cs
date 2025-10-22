using System.Collections.Generic;
using System.Text.Json;
using Mapster;
using Nexum.Server.Infrastructures.Surreal;
using Nexum.Server.Models;
using Nexum.Server.Models.Penalty;
using Nexum.Server.Models.Penalty.Surreal;
using SurrealDb.Net;
using SurrealDb.Net.Models;

namespace Nexum.Server.DAC
{
    public interface IPenaltyPoliciesDAC
    {
        //Surreal Model
        Task<List<PenaltyPolicyRecord>> GetAllPenaltyPoliciesAsync();
        Task<PenaltyPolicyRecord?> GetPenaltyPolicyByIdAsync(int id);
        Task<PenaltyPolicyRecord?> GetPenaltyPolicyByRecordIdAsync(int id);
        //Task<PenaltyPolicyRecord?> CreatePolicyAsync(PenaltyPolicyRecord body);
        Task<List<PenaltyPolicyRecord>> Query_AllAsync();
        Task<PenaltyPolicyRecord?> Query_OneAsync(int id);

        //Domain Model
        Task<ProductContact?> GetPenaltyPolicyByIdXAsync(int id);
        ProductContact GetPenaltyPolicies(PenaltyPoliciesRequest penaltyPoliciesRequest);
    }
    public class PenaltyPoliciesDAC : IPenaltyPoliciesDAC
    {
        private readonly IDbProvider<PenaltyPolicyRecord, ProductContact> _db;
        public PenaltyPoliciesDAC(SurrealDbProviderFactoryBase factory)
        {
            _db = factory.Create<PenaltyPolicyRecord, ProductContact>();
        }

        //List
        public async Task<List<PenaltyPolicyRecord>> GetAllPenaltyPoliciesAsync()
        {
            var rows = await _db.List();
            return rows?.ToList() ?? new List<PenaltyPolicyRecord>();
        }

        //Get by PenaltyPolicyID
        public async Task<PenaltyPolicyRecord?> GetPenaltyPolicyByIdAsync(int id)
        {
            var result = await _db.Get(id.ToString());
            return result;
        }

        //Get by RecordId
        public async Task<PenaltyPolicyRecord?> GetPenaltyPolicyByRecordIdAsync(int id)
        {
            var rid = RecordId.From("penalty_policies", id);
            return await _db.Get(rid);
        }

        ////Create 
        //public async Task<PenaltyPolicyRecord> CreatePolicyAsync(PenaltyPolicyRecord body)
        //{
        //    return await _db.Create(body);
        //}

        //Query
        public async Task<List<PenaltyPolicyRecord>> Query_AllAsync()
        {
            var rows = await _db.Query<PenaltyPolicyRecord>(
                $"SELECT * FROM penalty_policies"
                );
            return rows?.ToList() ?? new List<PenaltyPolicyRecord>();
        }

        //Query One
        public async Task<PenaltyPolicyRecord?> Query_OneAsync(int id)
        {
            var row = await _db.QueryOne<PenaltyPolicyRecord>(
                $"SELECT * FROM penalty_policies WHERE PenaltyPolicyID = {id} LIMIT 1"
                );
            return row;
        }

        //GetX 
        public async Task<ProductContact?> GetPenaltyPolicyByIdXAsync(int id)
        {
            var result = await _db.GetX(id.ToString());
            return result;
        }

        public ProductContact GetPenaltyPolicies(PenaltyPoliciesRequest penaltyPoliciesRequest)
        {
        }

        //public static List<ProductContact> GetMockPenaltyPolicies()
        //{
        //    return new List<ProductContact>
        //    {
        //        new ProductContact
        //        {
        //            PenaltyPolicyID = 1,
        //            PolicyName = "Standard Daily Penalty",
        //            PenaltyType = "Daily",
        //            FixedAmount = 100m, // 100 บาท = 100
        //            TotalCap = 1000.0m,
        //            PenaltyFreePeriodDays = 5,
        //            MinimumPaymentRate = 10.0m, // 10%
        //        },
        //        new ProductContact
        //        {
        //            PenaltyPolicyID = 2,
        //            PolicyName = "Fixed Penalty",
        //            PenaltyType = "Fixed",
        //            FixedAmount = 200.0m,
        //            MinimumPaymentRate = 10.0m, // 10%
        //        },
        //        new ProductContact
        //        {
        //            PenaltyPolicyID = 3,
        //            PolicyName = "Percentage Penalty",
        //            PenaltyType = "Percentage",
        //            PenaltyRate = 2.5m,
        //            MaxPenalty = 300.0m,
        //            PenaltyFreePeriodDays = 5,
        //            MinimumPaymentRate = 10.0m, // 10%
        //        },
        //        new ProductContact
        //        {
        //            PenaltyPolicyID = 4,
        //            PolicyName = "Special Daily Penalty",
        //            PenaltyType = "Daily",
        //            FixedAmount = 200.0m,
        //            MaxPenalty = 400.0m,
        //            TotalCap = 1200.0m,
        //            PenaltyFreePeriodDays = 2,
        //            MinimumPaymentRate = 10.0m, // 10%
        //        }
        //    };
        //}
    }
}
