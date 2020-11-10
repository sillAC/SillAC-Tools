using Decal.Adapter;
using Decal.Adapter.Wrappers;
using MyClasses.MetaViewWrappers;
using System;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Timers;

/*
 * Created by Mag-nus. 8/19/2011, VVS added by Virindi-Inquisitor.
 * 
 * No license applied, feel free to use as you wish. H4CK TH3 PL4N3T? TR45H1NG 0UR R1GHT5? Y0U D3C1D3!
 * 
 * Notice how I use try/catch on every function that is called or raised by decal (by base events or user initiated events like buttons, etc...).
 * This is very important. Don't crash out your users!
 * 
 * In 2.9.6.4+ Host and Core both have Actions objects in them. They are essentially the same thing.
 * You sould use Host.Actions though so that your code compiles against 2.9.6.0 (even though I reference 2.9.6.5 in this project)
 * 
 * If you add this plugin to decal and then also create another plugin off of this sample, you will need to change the guid in
 * Properties/AssemblyInfo.cs to have both plugins in decal at the same time.
 * 
 * If you have issues compiling, remove the Decal.Adapater and VirindiViewService references and add the ones you have locally.
 * Decal.Adapter should be in C:\Games\Decal 3.0\
 * VirindiViewService should be in C:\Games\VirindiPlugins\VirindiViewService\
*/

namespace SillAC
{
    //Attaches events from core
    [WireUpBaseEvents]

    // FriendlyName is the name that will show up in the plugins list of the decal agent (the one in windows, not in-game)
    // View is the path to the xml file that contains info on how to draw our in-game plugin. The xml contains the name and icon our plugin shows in-game.
    // The view here is SillAC.mainView.xml because our projects default namespace is SillAC, and the file name is mainView.xml.
    // The other key here is that mainView.xml must be included as an embeded resource. If its not, your plugin will not show up in-game.
    [FriendlyName("SillAC")]
    public class PluginCore : PluginBase
    {
        private SpellTabManager spellManager;
        private Timer reloadTimer;
        private FileSystemWatcher charProfileWatcher;
        private FileSystemWatcher masterProfileWatcher;
        private Regex commandParser;
        private string commandPattern = String.Join("|", Enum.GetNames(typeof(Command))).ToLower();
        private bool isFirstLogin = true;



        private Profile MasterProfile { get; set; }
        private CharacterProfile CharProfile { get; set; }


        public string CommandTrigger { get; private set; } = "/xp";

        /// <summary>
        /// This is called when the plugin is started up. This happens only once.
        /// </summary>
        protected override void Startup()
        {
            try
            {
                // This initializes our static Globals class with references to the key objects your plugin will use, Host and Core.
                // The OOP way would be to pass Host and Core to your objects, but this is easier.
                Globals.Init("SillAC", Host, Core);

                //Set directory to where the plugin is
                Directory.SetCurrentDirectory(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            }
            catch (Exception ex) { Util.LogError(ex); }
        }

        /// <summary>
        /// This is called when the plugin is shut down. This happens only once.
        /// </summary>
        protected override void Shutdown()
        {
            try
            {
                //Destroy the view.
                MVWireupHelper.WireupEnd(this);
            }
            catch (Exception ex) { Util.LogError(ex); }
        }

        [BaseEvent("LoginComplete", "CharacterFilter")]
        private void CharacterFilter_LoginComplete(object sender, EventArgs e)
        {
            try
            {
                // Subscribe to events here
                Initialize();

                //Reload timer periodically tries to load settings. Stops on success
                reloadTimer = new Timer() { AutoReset = true, Enabled = false, Interval = 1000 };
                reloadTimer.Elapsed += TryReload;


                //Globals.Core.CharacterFilter.AccountName
                //if (Globals.Core.CharacterFilter.Name.Length > 5)
                //{
                //    var suffix = Globals.Core.CharacterFilter.Name.Substring(5);

                //    var metaVars = new string[] { "charone", "chartwo", "charthree", "charfour", "charfive", "charsix", "charseven", "chareight", "charnine" };
                //    for (var i = 0; i < metaVars.Length; i++)
                //    {
                //        var charName = "Sill" + (char)('a' + i) + suffix;
                //        Command.DispatchChatToBoxWithPluginIntercept($"/vt mexec setvar[{metaVars[i]},{charName}]");
                //    }
                //}
            }
            catch (Exception ex) { Util.LogError(ex); }
        }

        [BaseEvent("Logoff", "CharacterFilter")]
        private void CharacterFilter_Logoff(object sender, Decal.Adapter.Wrappers.LogoffEventArgs e)
        {
            try
            {
                // Unsubscribe to events here, but know that this event is not gauranteed to happen. I've never seen it not fire though.

                //Stop anything currently going on
                CharProfile.Policy.Stop();
                spellManager.Stop();
                //Remove events
                masterProfileWatcher.Changed -= RequestReload;
                charProfileWatcher.Changed -= RequestReload;
                Globals.Core.CommandLineText -= Core_CommandLineText;
            }
            catch (Exception ex) { Util.LogError(ex); }
        }


        private void Initialize()
        {
            try
            {
                Util.WriteToChat("Loading profile");
                //Find profiles
                MasterProfile = Profile.GetProfile();
                CharProfile = MasterProfile.GetCharacterProfile();

                //Set up command parser
                SetupCommandParser();

                //Start experience policy
                CharProfile.Policy.Start(MasterProfile.Interval);

                //Start spell manager
                spellManager = new SpellTabManager();
                spellManager.Start(MasterProfile.Interval);

                //Watch for changes to profiles
                masterProfileWatcher = new FileSystemWatcher()
                {
                    Path = System.IO.Path.GetDirectoryName(Profile.ConfigurationPath),
                    Filter = System.IO.Path.GetFileName(Profile.ConfigurationPath),
                    EnableRaisingEvents = true,
                    NotifyFilter = NotifyFilters.LastWrite,
                };
                charProfileWatcher = new FileSystemWatcher()
                {
                    Path = System.IO.Path.GetDirectoryName(CharProfile.Path),
                    Filter = System.IO.Path.GetFileName(CharProfile.Path),
                    EnableRaisingEvents = true,
                    NotifyFilter = NotifyFilters.LastWrite,
                };

                if (isFirstLogin)
                {
                    //Run login commands
                    CharProfile.ExecuteLoginCommands();

                    //Add events if they haven't already been added
                    Globals.Core.CommandLineText += Core_CommandLineText;
                    masterProfileWatcher.Changed += RequestReload;
                    charProfileWatcher.Changed += RequestReload;
                    isFirstLogin = false;
                }
            }
            catch (Exception ex)
            {
                Util.LogError(ex);
            }

        }

        private void SetupCommandParser()
        {
            CommandTrigger = Regex.Escape(MasterProfile.Trigger);
            string commandRegex =
                $"^(?:{CommandTrigger} (?<command>{commandPattern})) (?<params>.+)$|" +  //Command with params
                $"^(?:{CommandTrigger} (?<command>{commandPattern}))$|" +  //Command no params
                $"^(?:{CommandTrigger})$";                      //Just trigger-- could use this to match anything starting with the trigger but not matching a command
                                                                //Util.WriteToChat(commandRegex);
            commandParser = new Regex(commandRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            //Util.WriteToChat($"Command regex: {commandRegex}");
        }

        private void TryReload(object sender, ElapsedEventArgs e)
        {
            try
            {
                Initialize();
                Util.WriteToChat("Reloaded settings successfully.");
                reloadTimer.Enabled = false;
            }
            catch (Exception ex)
            {
                //File most likely in use
                Util.LogError(ex);
            }
        }

        private void RequestReload(object sender, FileSystemEventArgs e)
        {
            //Util.WriteToChat($"Reloading settings after change in: {e.FullPath}");
            reloadTimer.Enabled = true;
        }


        private void Core_CommandLineText(object sender, ChatParserInterceptEventArgs e)
        {
            try
            {
                //Just the trigger dumps the list of commands
                var match = commandParser.Match(e.Text);

                //Seems like there's a weird time-sensistize need to set e.Eat
                if (match.Success)
                {
                    //Util.WriteToChat("Successful match");
                    //for (var i = 0; i < match.Groups.Count; i++)
                    //    Util.WriteToChat($"Group {i}: {match.Groups[i].Success}\t{match.Groups[i].Value}");

                    //Don't propagate if command was matched
                    e.Eat = true;

                    //Just the trigger
                    if (!match.Groups[1].Success)
                    {
                        //Util.WriteToChat("Trigger hit: " + CommandTrigger);
                        ProcessCommand(Command.Help);
                        return;
                    }

                    var command = (Command)Enum.Parse(typeof(Command), match.Groups[1].Value, true);
                    //There aren't parameters but a command
                    if (!match.Groups[2].Success)
                    {
                        //Util.WriteToChat("Command: " + match.Groups[1].Value);
                        ProcessCommand(command);
                        return;
                    }
                    //There are parameters
                    else
                    {
                        //Util.WriteToChat("Command: " + match.Groups[1].Value + "\t" + command);
                        //Util.WriteToChat("Params: " + match.Groups[2].Value);
                        ProcessCommand(command, match.Groups[2].Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.LogError(ex);
                Util.WriteToChat("Error parsing command");
            }
        }

        enum Command
        {
            //Load, 
            Plan,
            Spend,
            Level,
            Policy,
            EditPolicy,
            EditConfig,
            Help,
            LS,
            CS,
            SS
        }

        private void ProcessCommand(Command command)
        {
            switch (command)
            {
                //case Command.Load:
                //    Initialize();
                //    break;
                case Command.Policy:
                    CharProfile.Policy.PrintPolicy();
                    break;
                case Command.Plan:
                    CharProfile.Policy.PrintExperiencePlan();
                    break;
                case Command.Spend:
                case Command.Level:
                    CharProfile.Policy.SpendExperience();
                    break;
                case Command.EditConfig:
                    try { Process.Start(Profile.ConfigurationPath); }
                    catch (Exception e) { Util.LogError(e); }
                    break;
                case Command.EditPolicy:
                    try { Process.Start(CharProfile.Path); }
                    catch (Exception e) { Util.LogError(e); }
                    break;
                case Command.SS:
                    spellManager.SaveAllSpells();
                    break;
                case Command.CS:
                    spellManager.ClearAllSpells();
                    break;
                case Command.LS:
                    spellManager.LoadSpells();
                    break;
                case Command.Help:
                default:
                    Util.WriteToChat("Valid commands are: " + commandPattern);
                    break;
            }
        }

        //Could split this into a separate ParamCommand enum
        private void ProcessCommand(Command command, string parameters)
        {
            switch (command)
            {
                case Command.SS:
                    spellManager.SaveAllSpells(parameters);
                    break;
                case Command.LS:
                    spellManager.LoadSpells(parameters);
                    break;
                default:
                    Util.WriteToChat("Valid commands are: " + commandPattern);
                    break;
            }
        }
    }
}
