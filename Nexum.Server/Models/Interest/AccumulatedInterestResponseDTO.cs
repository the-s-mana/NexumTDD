namespace Nexum.Server.Models.Interest;
public class AccumulatedInterestResponseDTO : BaseEntityDTO
{
    public string Id { get; set; }
    public string ProductContactId { get; set; }
    
    public decimal AccumInterestRemain { get; set; }
}
