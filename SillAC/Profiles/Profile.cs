using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;

namespace SillAC
{
    /// <summary>
    /// System wide settings and a list of rules for finding character-specific profiles
    /// </summary>
    class Profile
    {
        public static string PluginDirectory { get { return System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\"; } }
        public static string ConfigurationPath { get { return PluginDirectory + @"Config.json"; } }

        public string Trigger { get; set; }
        public int Interval {get;set;}

        public List<ProfileSelector> Profiles { get; set; }

        public Profile()
        {
            Profiles = new List<ProfileSelector>();
        }

        public static Profile GetProfile()
        {
            Profile profile = new Profile();


            var selector = new ProfileSelector() { Account = ".+", Server = ".+", CharName = ".+", Priority = 1, FriendlyName = "Default Profile", Path = "Default.json" };
            //If config doesn't exist create it
            if (!File.Exists(ConfigurationPath))
            {
                //Util.WriteToChat($"No config found.  Creating default at: {ConfigurationPath}");
                profile = new Profile()
                {
                    Profiles = new List<ProfileSelector>()
                    {
                        new ProfileSelector() { Account = ".*", Server = ".*", CharName = ".*", Priority = 1, FriendlyName = "Default Profile", Path = "Default.json" },
                        new ProfileSelector() { CharName = ".*War.*", Priority = 2, FriendlyName = "War Mage", Path = "./Profiles/War.json" },
                        new ProfileSelector() { CharName = ".*Void.*", Priority = 2, FriendlyName = "Void Mage", Path = "./Profiles/Void.json" },
                        new ProfileSelector() { CharName = ".*TH.*", Priority = 2, FriendlyName = "Two-Handed", Path = "./Profiles/TH.json" },
                        new ProfileSelector() { CharName = ".*Bow.*", Priority = 2, FriendlyName = "Missile", Path = "./Profiles/Missile.json" },
                        new ProfileSelector() { CharName = ".*Mule.*", Priority = 2, FriendlyName = "Stronk Mule", Path = "./Profiles/Mule.json" }
                    },
                    Interval = 150, 
                    Trigger = "/xp"
                };

                try
                {
                    string json = JsonConvert.SerializeObject(profile, Formatting.Indented);
                    File.WriteAllText(ConfigurationPath, json);
                }
                //Unable to save
                catch (Exception e)
                {
                    Util.LogError(e);
                }
            }
            else
            {
                //Util.WriteToChat($"Loading config from: {ConfigurationPath}");

                try
                {
                    profile = JsonConvert.DeserializeObject<Profile>(File.ReadAllText(ConfigurationPath));
                }
                //Unable to load
                catch (Exception e)
                {
                    throw e;
                    //Util.WriteToChat($"Failed to load config from: {ConfigurationPath}");
                    //Util.LogError(e);
                }
            }

            return profile;
        }

        internal CharacterProfile GetCharacterProfile()
        {
            var characterProfile = CharacterProfile.Default;

            foreach (var p in Profiles.OrderByDescending(t => t.Priority))
            {
                //TODO: Do this but more smart.  Missing/null interpreted as always a match?
                var namePattern = p.CharName ?? ".*";
                var accountPattern = p.Account ?? ".*";
                var serverPattern = p.Server ?? ".*";

                var name = Regex.Escape(Globals.Core.CharacterFilter.Name);
                var account = Regex.Escape(Globals.Core.CharacterFilter.AccountName);
                var server = Regex.Escape(Globals.Core.CharacterFilter.Server);


                //Util.WriteToChat($"Looking at policy: {name}\t{namePattern}  {account}\t{accountPattern}  {server}\t{serverPattern}");
                //Check for match
                if (!Regex.IsMatch(name, namePattern, RegexOptions.IgnoreCase))
                    continue;
                if (!Regex.IsMatch(account, accountPattern, RegexOptions.IgnoreCase))
                    continue;
                if (!Regex.IsMatch(server, serverPattern, RegexOptions.IgnoreCase))
                    continue;

                var fullPath = Path.GetFullPath(p.Path);
                if(File.Exists(fullPath))
                {
                    //Util.WriteToChat($"Matched profile {p.FriendlyName} at: {fullPath}");

                    try
                    {
                        characterProfile = JsonConvert.DeserializeObject<CharacterProfile>(File.ReadAllText(fullPath));
                        characterProfile.Path = fullPath;
                        return characterProfile;
                    }
                    catch (Exception e)
                    {
                        throw e;
                        //Util.WriteToChat($"Unable to load character profile at: {fullPath}");
                        //Util.LogError(e);
                    }
                }
                //TODO: Decide whether to create missing profile or skip
                //Profile doesn't exist
                else
                {
                    //Util.WriteToChat($"Matched profile {p.FriendlyName} but missing at: {fullPath}");

                    try
                    {
                        characterProfile = CharacterProfile.Default;
                        characterProfile.Path = fullPath;
                        var json = JsonConvert.SerializeObject(characterProfile, Formatting.Indented);
                        File.WriteAllText(fullPath, json);
                        Util.WriteToChat($"Default profile created at: {fullPath}");
                    } catch(Exception e)
                    {
                        Util.WriteToChat("Unable to write default profile.");
                        Util.LogError(e);
                    }

                    Util.WriteToChat($"Loaded character " +
                        $"profile {p.FriendlyName}");

                    return characterProfile;
                }
            }

            return characterProfile;
        }
    }
}
