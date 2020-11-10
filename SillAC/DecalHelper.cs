using Decal.Adapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SillAC
{
    /// <summary>
    /// Terribly named
    /// </summary>
    class DecalHelper
    {
		[DllImport("Decal.dll")]
		static extern int DispatchOnChatCommand(ref IntPtr str, [MarshalAs(UnmanagedType.U4)] int target);

		static bool Decal_DispatchOnChatCommand(string cmd)
		{
			IntPtr bstr = Marshal.StringToBSTR(cmd);

			try
			{
				bool eaten = (DispatchOnChatCommand(ref bstr, 1) & 0x1) > 0;

				return eaten;
			}
			finally
			{
				Marshal.FreeBSTR(bstr);
			}
		}

		/// <summary>
		/// This will first attempt to send the messages to all plugins. If no plugins set e.Eat to true on the message, it will then simply call InvokeChatParser.
		/// </summary>
		/// <param name="cmd"></param>
		public static void DispatchChatToBoxWithPluginIntercept(string cmd)
		{
			if (!Decal_DispatchOnChatCommand(cmd))
				CoreManager.Current.Actions.InvokeChatParser(cmd);
		}
	}
}
