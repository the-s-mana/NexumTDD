using Nexum.Server.Models;

namespace Nexum.Server.DAC
{
    public interface IAccumulatedInterestDAC
    {
        AccumulatedInterest GetAccumulatedInterest(int productContactId);
        void UpdateAccumulatedInterest(decimal accumulatedInterest);
    }

    public class AccumulatedInterestDAC : IAccumulatedInterestDAC
    {
        public AccumulatedInterest GetAccumulatedInterest(int productContactId)
        {
            AccumulatedInterest accumulatedInterest = new AccumulatedInterest
            {
                AccumulatedInterestId = 1,
                ProductContactId = productContactId,
                AccumInterestRemain = 500,
                CreateDate = DateTime.Now,
                CreateBy = "System",
                UpdateDate = DateTime.Now,
                UpdateBy = "System"
            };
            return accumulatedInterest;
        }

        public void UpdateAccumulatedInterest(decimal accumulatedInterest)
        {
            // Implementation to save the AccumulatedInterest based on the accumulatedInterest
            throw new NotImplementedException();
        }
    }
}
