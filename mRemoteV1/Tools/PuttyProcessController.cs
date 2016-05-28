

namespace mRemoteNG.Tools
{
	public class PuttyProcessController : ProcessController
	{
		public bool Start(CommandLineArguments arguments = null)
		{
			string filename = "";
			if (Settings.Default.UseCustomPuttyPath)
			{
				filename = Settings.Default.CustomPuttyPath;
			}
			else
			{
				filename = App.Info.GeneralAppInfo.PuttyPath;
			}
			return Start(filename, arguments);
		}
	}
}
