using System.Collections.Generic;
using System.Text.Json;
using Mapster;
using Nexum.Server.Infrastructures.Surreal;
using Nexum.Server.Models;
using Nexum.Server.Models.Penalty;
using Nexum.Server.Models.Penalty.Surreal;
using SurrealDb.Net;
using SurrealDb.Net.Models;
using Nexum.Server.API.Dto;

namespace Nexum.Server.DAC
{
    public interface IPenaltyPoliciesDAC
    {
        #region Surreal CRUD
        //Surreal Model
        Task<List<PenaltyPolicyResponseDTO>> GetAllPenaltyPoliciesAsync();
        Task<PenaltyPolicyResponseDTO?> GetPenaltyPolicyByIdAsync(int id);
        Task<PenaltyPolicyResponseDTO?> GetPenaltyPolicyByRecordIdAsync(int id);
        Task<PenaltyPolicyResponseDTO?> CreatePenaltyPolicyAsync(PenaltyPolicyRequestDTO req);
        Task DeletePenaltyPolicyByIdAsync(int id);
        Task DeletePenaltyPolicyByRecordIdAsync(int id);
        Task<PenaltyPolicyResponseDTO?> UpsertPenaltyPolicyAsync(PenaltyPolicyRequestDTO req);
        Task<PenaltyPolicyResponseDTO?> UpdatePenaltyPolicyAsync(PenaltyPolicyRequestDTO req);
        Task<List<PenaltyPolicyResponseDTO>> Query_AllAsync();
        Task<PenaltyPolicyResponseDTO?> Query_OneAsync(int id);
        #endregion

        #region Domain CRUD
        //Domain Model
        Task<PenaltyPolicyResponseDTO?> GetPenaltyPolicyByIdXAsync(int id);
        ProductContact GetPenaltyPolicies(PenaltyPoliciesRequest penaltyPoliciesRequest);
        #endregion
    }
    public class PenaltyPoliciesDAC : IPenaltyPoliciesDAC
    {
        private readonly IDbProvider<PenaltyPolicyRecord, PenaltyPolicyResponseDTO> _db;
        public PenaltyPoliciesDAC(SurrealDbProviderFactoryBase factory)
        {
            _db = factory.Create<PenaltyPolicyRecord, PenaltyPolicyResponseDTO>();
        }

        //List
        public async Task<List<PenaltyPolicyResponseDTO>> GetAllPenaltyPoliciesAsync()
        {
            var rows = await _db.List();
            return rows?.Adapt<List<PenaltyPolicyResponseDTO>>()?? new List<PenaltyPolicyResponseDTO>();
        }

        //Get by PenaltyPolicyID
        public async Task<PenaltyPolicyResponseDTO?> GetPenaltyPolicyByIdAsync(int id)
        {
            var result = await _db.Get(id.ToString());
            return result?.Adapt<PenaltyPolicyResponseDTO>();
        }

        //Get by RecordId
        public async Task<PenaltyPolicyResponseDTO?> GetPenaltyPolicyByRecordIdAsync(int id)
        {
            var rid = RecordId.From("penalty_policies", id);
            var result = await _db.Get(rid);
            return result?.Adapt<PenaltyPolicyResponseDTO>();
        }

        //Create 
        public async Task<PenaltyPolicyResponseDTO> CreatePenaltyPolicyAsync(PenaltyPolicyRequestDTO req)
        {
            var record = req.Adapt<PenaltyPolicyRecord>();
            var created = await _db.Create(record.PenaltyPolicyID.ToString(), record);
            return created.Adapt<PenaltyPolicyResponseDTO>();
        }

        //Delete by PenaltyPolicyID
        public async Task DeletePenaltyPolicyByIdAsync(int id)
        {
            await _db.Delete(id.ToString());
        }

        //Delete by RecordId
        public async Task DeletePenaltyPolicyByRecordIdAsync(int id)
        {
            var rid = RecordId.From("penalty_policies", id);
            await _db.Delete(rid);
        }

        //Upsert
        public async Task<PenaltyPolicyResponseDTO?> UpsertPenaltyPolicyAsync(PenaltyPolicyRequestDTO req)
        {
            var record = req.Adapt<PenaltyPolicyRecord>();
            var upserted = await _db.Upsert(record.PenaltyPolicyID.ToString(), record);
            return upserted.Adapt<PenaltyPolicyResponseDTO>();
        }

        //Update
        public async Task<PenaltyPolicyResponseDTO> UpdatePenaltyPolicyAsync(PenaltyPolicyRequestDTO req)
        {
            var patch = new Dictionary<string, object?>();
            patch["PenaltyPolicyID"] = req.PenaltyPolicyID;
            if (req.PolicyName != null) patch["PolicyName"] = req.PolicyName;
            if (req.PenaltyType != null) patch["PenaltyType"] = req.PenaltyType;
            if (req.FixedAmount != null) patch["FixedAmount"] = req.FixedAmount;
            if (req.PenaltyRate != null) patch["PenaltyRate"] = req.PenaltyRate;
            if (req.MaxPenalty != null) patch["MaxPenalty"] = req.MaxPenalty;
            if (req.TotalCap != null) patch["TotalCap"] = req.TotalCap;
            if (req.PenaltyFreePeriodDays != null) patch["PenaltyFreePeriodDays"] = req.PenaltyFreePeriodDays;
            if (req.MinimumPaymentRate != null) patch["MinimumPaymentRate"] = req.MinimumPaymentRate;

            var merged = await _db.Update(req.PenaltyPolicyID.ToString(), patch);
            return merged.Adapt<PenaltyPolicyResponseDTO>();
        }

        //Query
        public async Task<List<PenaltyPolicyResponseDTO>> Query_AllAsync()
        {
            var rows = await _db.Query<PenaltyPolicyResponseDTO>(
                $"SELECT * FROM penalty_policies"
                );
            return rows?.ToList() ?? new List<PenaltyPolicyResponseDTO>();
        }

        //Query One
        public async Task<PenaltyPolicyResponseDTO?> Query_OneAsync(int id)
        {
            var row = await _db.QueryOne<PenaltyPolicyResponseDTO>(
                $"SELECT * FROM penalty_policies WHERE PenaltyPolicyID = {id} LIMIT 1"
                );
            return row;
        }

        //GetX 
        public async Task<PenaltyPolicyResponseDTO?> GetPenaltyPolicyByIdXAsync(int id)
        {
            var result = await _db.GetX(id.ToString());
            return result;
        }

        public ProductContact GetPenaltyPolicies(PenaltyPoliciesRequest penaltyPoliciesRequest)
        {
            throw new NotImplementedException();
        }
    }
}
