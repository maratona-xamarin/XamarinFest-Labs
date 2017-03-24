using System;

namespace Core.Helpers
{
	public class LogHelper
	{
		private static LogHelper _instance;
		public static LogHelper Instance => _instance ?? (_instance = new LogHelper());

		private LogHelper() { }

		private void ShowLog(string text)
		{
			System.Diagnostics.Debug.WriteLine(text);
		}

		public void AddLog(string text)
		{
			ShowLog(text);
		}

		public void AddLog(Exception e)
		{
			ShowLog(e.Message);
		}
	}
}
