using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cyclophiops.Regedit.Utils
{
    public class RegistryFilter
    {
        public class FilterOptions
        {
            public string Pattern { get; set; }

            public bool IgnoreCase { get; set; } = true;

            public RegexOptions RegexOptions { get; set; } = RegexOptions.Compiled;

            public List<string> ExcludePatterns { get; set; }

            public bool Invert { get; set; } = false;
        }

        private readonly FilterOptions _options;
        private Regex _compiledRegex;
        private List<Regex> _compiledExcludePatterns;

        public RegistryFilter(FilterOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            Initialize();
        }

        private void Initialize()
        {
            var regexOptions = _options.RegexOptions;

            if (_options.IgnoreCase)
            {
                regexOptions |= RegexOptions.IgnoreCase;
            }

            if (!string.IsNullOrEmpty(_options.Pattern))
            {
                _compiledRegex = new Regex(_options.Pattern, regexOptions);
            }

            if (_options.ExcludePatterns != null && _options.ExcludePatterns.Count > 0)
            {
                _compiledExcludePatterns = new List<Regex>();
                foreach (var pattern in _options.ExcludePatterns)
                {
                    if (!string.IsNullOrEmpty(pattern))
                    {
                        _compiledExcludePatterns.Add(new Regex(pattern, regexOptions));
                    }
                }
            }
        }

        public bool IsMatch(string input)
        {
            if (input == null)
            {
                return false;
            }

            if (_compiledExcludePatterns != null && _compiledExcludePatterns.Any(regex => regex.IsMatch(input)))
            {
                return _options.Invert;
            }

            var result = _compiledRegex?.IsMatch(input) ?? true;

            return _options.Invert ? !result : result;
        }

        public Func<string, bool> GetFilterFunc()
        {
            return IsMatch;
        }

        public static RegistryFilter CreateRegex(string pattern, bool ignoreCase = true)
        {
            return new RegistryFilter(new FilterOptions
            {
                Pattern = pattern,
                IgnoreCase = ignoreCase,
            });
        }

        public static RegistryFilter CreateWithExcludes(string pattern, List<string> excludePatterns, bool ignoreCase = true)
        {
            return new RegistryFilter(new FilterOptions
            {
                Pattern = pattern,
                ExcludePatterns = excludePatterns,
                IgnoreCase = ignoreCase,
            });
        }

        public static class Patterns
        {
            public static string ExcludeItems(params string[] items)
            {
                return $@"^(?!({string.Join("|", items)})).*";
            }

            public static string MatchStart(params string[] prefixes)
            {
                return $@"^({string.Join("|", prefixes)}).*";
            }

            public static string ExcludeSuffix(params string[] suffixes)
            {
                return $@"^(?!.*({string.Join("|", suffixes)})).*";
            }

            public static class Common
            {
                public static readonly string UserSid = @"^S-1-5-21-\d+-\d+-\d+-\d+$";

                public static readonly string ExcludeSystemKeys = @"^(?!(Classes|CloudStore|Policies|\.DEFAULT|S-1-5-18|S-1-5-19|S-1-5-20)).*";

                public static readonly string ExcludeClassesSuffix = @"^(?!.*_Classes$).*";
            }
        }
    }
}
