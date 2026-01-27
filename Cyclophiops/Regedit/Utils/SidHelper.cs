using System.Collections.Generic;
using System.Security.Principal;
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

        public static string GetCurrentUserSid()
        {
            try
            {
                var user = WindowsIdentity.GetCurrent();
                return user.User?.Value;
            }
            catch
            {
                return null;
            }
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

        public static RegistryFilter CreateUserSidFilter()
        {
            return RegistryFilter.CreateRegex(@"^S-1-5-21-\d+-\d+-\d+-\d+$", ignoreCase: true);
        }

        public static RegistryFilter CreateNonSystemSidFilter()
        {
            var options = new RegistryFilter.FilterOptions
            {
                Mode = RegistryFilter.FilterMode.Regex,
                Pattern = @"^S-1-5-21-\d+-\d+-\d+-\d+$",
                IgnoreCase = true,
                ExcludePatterns = new List<string>
                {
                    @".*_Classes$",
                    @".*.DEFAULT$",
                },
            };

            return new RegistryFilter(options);
        }

        public static Dictionary<string, string> GetUserSidsWithInfo()
        {
            var result = new Dictionary<string, string>();
            var allSids = GetAllUserSids();

            foreach (var sid in allSids)
            {
                try
                {
                    var securityIdentifier = new SecurityIdentifier(sid);
                    var account = securityIdentifier.Translate(typeof(NTAccount));
                    result[sid] = account.Value;
                }
                catch
                {
                    result[sid] = "Unknown";
                }
            }

            return result;
        }

        public static List<string> EnumerateUserSoftware()
        {
            var results = new List<string>();
            var userSids = GetAllUserSids();

            foreach (var sid in userSids)
            {
                try
                {
                    using (var hive = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Registry64))
                    using (var userKey = hive.OpenSubKey($"{sid}\\SOFTWARE"))
                    {
                        if (userKey != null)
                        {
                            results.Add($"{sid}\\SOFTWARE");
                        }
                    }
                }
                catch
                {
                }
            }

            return results;
        }
    }
}
