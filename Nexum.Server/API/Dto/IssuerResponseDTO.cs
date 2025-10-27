namespace Nexum.Server.API.Dto
{
    public class IssuerResponseDTO
    {
        public int IssuerID { get; set; }
        public string IssuerName { get; set; } = default!;
        public string ContactEmail { get; set; } = default!;
        public string ContactPhone { get; set; } = default!;    
    }
}
