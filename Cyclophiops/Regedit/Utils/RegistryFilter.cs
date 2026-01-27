using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cyclophiops.Regedit.Utils
{
    public class RegistryFilter
    {
        public enum FilterMode
        {
            None,
            Exact,
            Contains,
            StartsWith,
            EndsWith,

            /// <summary>
            /// 正则表达式匹配
            /// </summary>
            Regex,

            /// <summary>
            /// 通配符匹配（*和?）
            /// </summary>
            Wildcard,

            /// <summary>
            /// 自定义过滤函数
            /// </summary>
            Custom,
        }

        /// <summary>
        /// 过滤选项
        /// </summary>
        public class FilterOptions
        {
            public FilterMode Mode { get; set; } = FilterMode.None;

            /// <summary>
            /// Gets or sets 过滤模式参数（根据不同模式使用）
            /// </summary>
            public string Pattern { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether 是否忽略大小写（默认忽略）
            /// </summary>
            public bool IgnoreCase { get; set; } = true;

            /// <summary>
            /// Gets or sets 正则表达式选项
            /// </summary>
            public RegexOptions RegexOptions { get; set; } = RegexOptions.None;

            /// <summary>
            /// Gets or sets 自定义过滤函数
            /// </summary>
            public Func<string, bool> CustomFilter { get; set; }

            /// <summary>
            /// Gets or sets 多个模式（用于OR逻辑）
            /// </summary>
            public List<string> Patterns { get; set; }

            /// <summary>
            /// Gets or sets 排除模式（黑名单）
            /// </summary>
            public List<string> ExcludePatterns { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether 是否反转过滤结果
            /// </summary>
            public bool Invert { get; set; } = false;
        }

        private readonly FilterOptions _options;
        private Regex _compiledRegex;
        private List<Regex> _compiledPatterns;
        private List<Regex> _compiledExcludePatterns;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistryFilter"/> class.
        /// 创建注册表过滤器
        /// </summary>
        /// <param name="options">过滤选项</param>
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

            // 编译主正则表达式
            if (_options.Mode == FilterMode.Regex && !string.IsNullOrEmpty(_options.Pattern))
            {
                _compiledRegex = new Regex(_options.Pattern, regexOptions);
            }
            else if (_options.Mode == FilterMode.Wildcard && !string.IsNullOrEmpty(_options.Pattern))
            {
                _compiledRegex = new Regex(WildcardToRegex(_options.Pattern), regexOptions);
            }

            // 编译多个模式
            if (_options.Patterns != null && _options.Patterns.Count > 0)
            {
                _compiledPatterns = new List<Regex>();
                foreach (var pattern in _options.Patterns)
                {
                    if (!string.IsNullOrEmpty(pattern))
                    {
                        var regex = _options.Mode == FilterMode.Wildcard
                            ? new Regex(WildcardToRegex(pattern), regexOptions)
                            : new Regex(pattern, regexOptions);
                        _compiledPatterns.Add(regex);
                    }
                }
            }

            // 编译排除模式
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

        /// <summary>
        /// 将通配符转换为正则表达式
        /// </summary>
        /// <param name="pattern">通配符模式</param>
        /// <returns>正则表达式</returns>
        private string WildcardToRegex(string pattern)
        {
            return "^" + Regex.Escape(pattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".") + "$";
        }

        /// <summary>
        /// 执行过滤
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <returns>是否通过过滤</returns>
        public bool IsMatch(string input)
        {
            if (input == null)
            {
                return false;
            }

            // 检查排除列表（黑名单）
            if (_compiledExcludePatterns != null && _compiledExcludePatterns.Any(regex => regex.IsMatch(input)))
            {
                return _options.Invert;
            }

            bool result;

            switch (_options.Mode)
            {
                case FilterMode.None:
                    result = true;
                    break;

                case FilterMode.Exact:
                    result = string.Equals(input, _options.Pattern,
                        _options.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
                    break;

                case FilterMode.Contains:
                    result = input.IndexOf(_options.Pattern,
                        _options.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) >= 0;
                    break;

                case FilterMode.StartsWith:
                    result = input.StartsWith(_options.Pattern,
                        _options.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
                    break;

                case FilterMode.EndsWith:
                    result = input.EndsWith(_options.Pattern,
                        _options.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
                    break;

                case FilterMode.Regex:
                case FilterMode.Wildcard:
                    result = _compiledRegex != null && _compiledRegex.IsMatch(input);
                    break;

                case FilterMode.Custom:
                    result = _options.CustomFilter?.Invoke(input) ?? true;
                    break;

                default:
                    result = true;
                    break;
            }

            // 检查多个模式（OR逻辑）
            if (!result && _compiledPatterns != null && _compiledPatterns.Count > 0)
            {
                result = _compiledPatterns.Any(regex => regex.IsMatch(input));
            }

            // 反转结果
            return _options.Invert ? !result : result;
        }

        /// <summary>
        /// 获取过滤器的 Func 委托
        /// </summary>
        /// <returns>过滤函数</returns>
        public Func<string, bool> GetFilterFunc()
        {
            return IsMatch;
        }

        /// <summary>
        /// 创建无过滤器（全部通过）
        /// </summary>
        public static RegistryFilter CreateNone()
        {
            return new RegistryFilter(new FilterOptions { Mode = FilterMode.None });
        }

        /// <summary>
        /// 创建精确匹配过滤器
        /// </summary>
        public static RegistryFilter CreateExact(string pattern, bool ignoreCase = true)
        {
            return new RegistryFilter(new FilterOptions
            {
                Mode = FilterMode.Exact,
                Pattern = pattern,
                IgnoreCase = ignoreCase,
            });
        }

        /// <summary>
        /// 创建包含匹配过滤器
        /// </summary>
        /// <returns></returns>
        public static RegistryFilter CreateContains(string pattern, bool ignoreCase = true)
        {
            return new RegistryFilter(new FilterOptions
            {
                Mode = FilterMode.Contains,
                Pattern = pattern,
                IgnoreCase = ignoreCase,
            });
        }

        /// <summary>
        /// 创建开头匹配过滤器
        /// </summary>
        public static RegistryFilter CreateStartsWith(string pattern, bool ignoreCase = true)
        {
            return new RegistryFilter(new FilterOptions
            {
                Mode = FilterMode.StartsWith,
                Pattern = pattern,
                IgnoreCase = ignoreCase,
            });
        }

        /// <summary>
        /// 创建结尾匹配过滤器
        /// </summary>
        public static RegistryFilter CreateEndsWith(string pattern, bool ignoreCase = true)
        {
            return new RegistryFilter(new FilterOptions
            {
                Mode = FilterMode.EndsWith,
                Pattern = pattern,
                IgnoreCase = ignoreCase,
            });
        }

        /// <summary>
        /// 创建正则表达式过滤器
        /// </summary>
        public static RegistryFilter CreateRegex(string pattern, bool ignoreCase = true)
        {
            return new RegistryFilter(new FilterOptions
            {
                Mode = FilterMode.Regex,
                Pattern = pattern,
                IgnoreCase = ignoreCase,
            });
        }

        /// <summary>
        /// 创建通配符过滤器（支持 * 和 ?）
        /// </summary>
        public static RegistryFilter CreateWildcard(string pattern, bool ignoreCase = true)
        {
            return new RegistryFilter(new FilterOptions
            {
                Mode = FilterMode.Wildcard,
                Pattern = pattern,
                IgnoreCase = ignoreCase,
            });
        }

        /// <summary>
        /// 创建自定义过滤器
        /// </summary>
        public static RegistryFilter CreateCustom(Func<string, bool> customFilter)
        {
            return new RegistryFilter(new FilterOptions
            {
                Mode = FilterMode.Custom,
                CustomFilter = customFilter,
            });
        }

        /// <summary>
        /// 创建多模式过滤器（OR逻辑）
        /// </summary>
        public static RegistryFilter CreateMultiPattern(List<string> patterns, bool ignoreCase = true)
        {
            return new RegistryFilter(new FilterOptions
            {
                Mode = FilterMode.Regex,
                Patterns = patterns,
                IgnoreCase = ignoreCase,
            });
        }

        /// <summary>
        /// 创建带排除列表的过滤器
        /// </summary>
        public static RegistryFilter CreateWithExcludes(string pattern, List<string> excludePatterns, bool ignoreCase = true)
        {
            return new RegistryFilter(new FilterOptions
            {
                Mode = FilterMode.Regex,
                Pattern = pattern,
                ExcludePatterns = excludePatterns,
                IgnoreCase = ignoreCase,
            });
        }
    }
}
