namespace Nexum.Server.Utils
{
    public interface IDateTimeUtils
    {
        DateTime GetCurrentDateTime();
    }

    public class DateTimeUtils : IDateTimeUtils
    {
        public DateTime GetCurrentDateTime()
        {
            return DateTime.Now;
        }
    }
}
