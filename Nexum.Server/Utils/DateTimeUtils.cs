namespace Nexum.Server.Utils
{
    public interface IDateTimeUtils
    {
        DateTime GetCurrentDateTime(int? daysToAdd = null);
    }

    public class DateTimeUtils : IDateTimeUtils
    {
        public DateTime GetCurrentDateTime(int? daysToAdd)
        {
            return DateTime.Now.AddDays(daysToAdd ?? 0);
        }
    }
}
