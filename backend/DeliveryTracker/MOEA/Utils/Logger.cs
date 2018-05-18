
namespace DeliveryTracker.MOEA.Utils
{
	public static class Logger
	{
		public interface ILog
		{
			void Error(
				params object[] obj);
			void Warn(
				params object[] obj);
			void Info(
				params object[] obj);
		}

		public class DumbLog : ILog
		{
			public void Error(
				params object[] obj)
			{
				
			}

			public void Warn(
				params object[] obj)
			{
			}

			public void Info(
				params object[] obj)
			{
			}
		}
		
		
		public static ILog Log { get; set; }

		static Logger()
		{
			Log = new DumbLog();
		}
	}
}
