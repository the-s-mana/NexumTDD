namespace Nexum.Server.Services
{
    public interface IDateTimeProvider
    {
        DateTime Now { get; }
    }
    public sealed class SystemDateTimeProvider : IDateTimeProvider
    {
        public DateTime Now => DateTime.Now;
    }
}
