using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Cyclophiops.Regedit.Utils
{
    public class SidHelper
    {
        private static readonly Regex _sidPattern = new Regex(
            @"^S-1-5-21-\d+-\d+-\d+-\d+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool IsUserSid(string sid)
        {
            if (string.IsNullOrEmpty(sid))
            {
                return false;
            }

            return _sidPattern.IsMatch(sid);
        }

        public static List<string> GetAllUserSids()
        {
            var sids = new List<string>();

            try
            {
                using (var usersKey = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Registry64))
                {
                    var subKeyNames = usersKey.GetSubKeyNames();
                    foreach (var name in subKeyNames)
                    {
                        if (IsUserSid(name))
                        {
                            sids.Add(name);
                        }
                    }
                }
            }
            catch
            {
            }

            return sids;
        }
    }
}
