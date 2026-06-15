using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

public static class LabelFactory {
    private static readonly Dictionary<string, Type> typeCache;
    private static readonly Regex parseRegex = new(
        @"^\s*(?<name>\w+)\s*(?:\(\s*(?<args>[^)]*)\s*\))?\s*$",
        RegexOptions.Compiled
    );

    static LabelFactory() {
        var types = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
            try {
                foreach (var t in asm.GetTypes()) {
                    if (typeof(ObjectLabel).IsAssignableFrom(t) && !t.IsAbstract) {
                        types[t.Name] = t;
                    }
                }
            } catch (ReflectionTypeLoadException ex) {
                Debug.LogWarning($"LabelFactory: 程序集 {asm.GetName().Name} 部分类型加载失败: {ex.Message}");
                foreach (var t in ex.Types) {
                    if (t != null && typeof(ObjectLabel).IsAssignableFrom(t) && !t.IsAbstract) {
                        types[t.Name] = t;
                    }
                }
            } catch (Exception ex) {
                Debug.LogWarning($"LabelFactory: 程序集 {asm.GetName().Name} 扫描失败: {ex.Message}");
            }
        }
        typeCache = types;
    }

    /// <summary>
    /// 根据字符串描述构建标签，格式如 "HardLabel()", "LockLabel", "TestLabel(arg1, arg2)"
    /// </summary>
    public static ObjectLabel Build(string description) {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Label description cannot be null or empty.", nameof(description));

        var match = parseRegex.Match(description);
        if (!match.Success)
            throw new FormatException($"Cannot parse label description: \"{description}\". Expected format: LabelName(arg1, arg2)");

        string typeName = match.Groups["name"].Value;
        string argsString = match.Groups["args"].Value;

        Type labelType = ResolveType(typeName);
        if (labelType == null)
            throw new ArgumentException($"Unknown label type: \"{typeName}\". Ensure the class exists and inherits from ObjectLabel.");

        if (!typeof(ObjectLabel).IsAssignableFrom(labelType))
            throw new ArgumentException($"Type \"{labelType.FullName}\" does not inherit from ObjectLabel.");

        object[] args = ParseArguments(argsString);
        return (ObjectLabel)Activator.CreateInstance(labelType, args);
    }

    private static Type ResolveType(string typeName) {
        typeCache.TryGetValue(typeName, out Type result);
        return result;
    }

    private static object[] ParseArguments(string argsString) {
        if (string.IsNullOrWhiteSpace(argsString))
            return Array.Empty<object>();

        var args = new List<object>();
        int i = 0;
        while (i < argsString.Length) {
            SkipWhitespace(argsString, ref i);
            if (i >= argsString.Length) break;

            if (args.Count > 0) {
                if (argsString[i] != ',')
                    throw new FormatException($"Expected ',' between arguments at position {i}: \"{argsString}\"");
                i++; // skip comma
                SkipWhitespace(argsString, ref i);
            }

            args.Add(ParseSingleArgument(argsString, ref i));
        }

        return args.ToArray();
    }

    private static object ParseSingleArgument(string s, ref int i) {
        if (i >= s.Length)
            throw new FormatException("Unexpected end of argument list.");

        char c = s[i];

        // 字符串：双引号或单引号
        if (c == '"' || c == '\'') {
            char quote = c;
            i++; // skip opening quote
            int startId = i;
            while (i < s.Length && s[i] != quote) i++;
            if (i >= s.Length)
                throw new FormatException($"Unterminated string argument starting at {startId - 1}.");
            string value = s.Substring(startId, i - startId);
            i++; // skip closing quote
            return value;
        }

        // 布尔
        if (s.Length - i >= 4 && string.Compare(s, i, "true", 0, 4, StringComparison.OrdinalIgnoreCase) == 0) {
            i += 4;
            return true;
        }
        if (s.Length - i >= 4 && string.Compare(s, i, "null", 0, 4, StringComparison.OrdinalIgnoreCase) == 0) {
            i += 4;
            return null;
        }
        if (s.Length - i >= 5 && string.Compare(s, i, "false", 0, 5, StringComparison.OrdinalIgnoreCase) == 0) {
            i += 5;
            return false;
        }

        // 数字（整数或浮点）
        int start = i;
        if (i < s.Length && (s[i] == '+' || s[i] == '-')) i++;
        bool hasDot = false;
        while (i < s.Length) {
            char digit = s[i];
            if (char.IsDigit(digit)) { i++; continue; }
            if (digit == '.' && !hasDot) { hasDot = true; i++; continue; }
            break;
        }
        string numStr = s.Substring(start, i - start);
        if (i <= start)
            throw new FormatException($"Cannot parse argument starting at {start}: \"{s.Substring(start)}\"");

        if (hasDot)
            return float.Parse(numStr, System.Globalization.CultureInfo.InvariantCulture);
        return int.Parse(numStr, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static void SkipWhitespace(string s, ref int i) {
        while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
    }
}
