using Nexum.Server.Models;

namespace Nexum.Server.DAC
{
    public interface IInterestTransactionDAC
    {
        void CreateInterestTransaction(InterestTransaction InterestTransaction);
    }

    public class InterestTransactionDAC : IInterestTransactionDAC
    {
        public void CreateInterestTransaction(InterestTransaction InterestTransaction)
        {
            // Implementation to save the InterestTransaction based on the InterestTransaction
            throw new NotImplementedException();
        }
    }
}
