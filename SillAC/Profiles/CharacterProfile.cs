using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Timers;

namespace SillAC
{
    /// <summary>
    /// Per-character settings go here
    /// </summary>
    public class CharacterProfile
    {
        private const int DEFAULT_LOGIN_COMMAND_DELAY = 5000;
        private Timer loginCommandTimer;

        [JsonIgnore]
        public string Path { get; set; }

        public string LoginCommands { get; set; }

        public ExperiencePolicy Policy { get; set; }

        public CharacterProfile()
        {
            loginCommandTimer = new Timer { Interval = DEFAULT_LOGIN_COMMAND_DELAY, AutoReset = false, };
            loginCommandTimer.Elapsed += DelayedLoginCOmmands;
        }

        public void ExecuteLoginCommands()
        {
            if (LoginCommands == null)
            {
                //Util.WriteToChat("No login commands");
                return;
            }

            var path = System.IO.Path.GetFullPath(LoginCommands);
            if (File.Exists(LoginCommands))
            {
                loginCommandTimer.Enabled = true;
            }
        }

        private void DelayedLoginCOmmands(object sender, ElapsedEventArgs e)
        {
            var path = System.IO.Path.GetFullPath(LoginCommands);
            try
            {
                if (File.Exists(LoginCommands))
                {
                    //Util.WriteToChat($"Running batch commands at: {path}");
                    DecalHelper.DispatchChatToBoxWithPluginIntercept($"/loadfile {path}");
                }
            }
            catch (Exception ex)
            {
                Util.WriteToChat($"Unable to run login commands at: {path}");
                Util.LogError(ex);
            }
        }
        public static CharacterProfile Default
        {
            get
            {
                return new CharacterProfile()
                {
                    Policy = ExperiencePolicy.Default
                };
            }
        }
    }
}
