using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace SillAC
{
    /// <summary>
    /// The main config maps to character profiles based on a Regex of their name, account, and server in order of priority
    /// </summary>
    class ProfileSelector
    {
        public string FriendlyName { get; set; }
        /// <summary>
        /// Regexs that match this profile
        /// </summary>
        public string CharName { get; set; }
        public string Account { get; set; }
        public string Server { get; set; }

        /// <summary>
        /// Order in which profiles are looked at for a match.  Descending.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Path to the profile to be loaded
        /// </summary>
        public string Path { get; set; }

        public ProfileSelector() { }
    }
}
