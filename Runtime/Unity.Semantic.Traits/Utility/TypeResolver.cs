using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Unity.Semantic.Traits.Utility
{
    /// <summary>
    /// Type resolver with a configurable list of lookup assemblies
    /// </summary>
    public static class TypeResolver
    {
        const string k_ValidateTypePattern = @"^[0-9]+|\s";

        internal const string TraitsDisplayName = "Semantic Traits";
        internal const string TraitsQualifier = "Generated.Semantic.Traits";
        internal const string EnumsQualifier = TraitsQualifier + ".Enums";

        static HashSet<string> s_Resolvers = new HashSet<string>()
        {
            $"{TraitsQualifier}.{{0}},{TraitsQualifier}",
            $"{EnumsQualifier}.{{0}},{TraitsQualifier}",
            $"{{0}},{TraitsQualifier}",
            "{0},Assembly-CSharp",
            "{0},UnityEngine",
            "{0}"
        };

        internal const string ComponentDataSuffix = "Data";

        static Dictionary<string, Type> s_TypeCache = new Dictionary<string, Type>();

        /// <summary>
        /// Add a resolver to the type lookup system
        /// </summary>
        /// <param name="formatString">A format string that includes '{0}' for the typeName to be integrated. Examples:
        /// "{0},Assembly-CSharp", "CustomNamespace.{0},CustomAssembly"
        /// </param>
        public static void AddResolver(string formatString)
        {
            if (!formatString.Contains("{0}"))
                Debug.LogError("Format string must contain '{0}' for typeName to be integrated");
            else
                s_Resolvers.Add(formatString);
        }

        internal static bool TryGetType(string typeName, out Type type)
        {
            if (s_TypeCache.TryGetValue(typeName, out type))
                return true;

            foreach (var resolver in s_Resolvers)
            {
                type = Type.GetType(string.Format(resolver, typeName));
                if (type != null)
                    break;
            }

            if (type != null)
                s_TypeCache.Add(typeName, type);

            return type != null;
        }

        internal static string ToTypeNameCase(string name)
        {
            return Regex.Replace(name, k_ValidateTypePattern, string.Empty);
        }

        internal static string GetUnmangledName(Type type)
        {
            if (type.GetGenericArguments().Length == 0)
                return type.ToString();

            var genericArguments = type.GetGenericArguments();
            var typeDefinition = type.Name;
            var unmangledName = typeDefinition.Substring(0, typeDefinition.IndexOf("`"));
            return unmangledName + "<" + String.Join(",", genericArguments.Select(GetUnmangledName)) + ">";
        }
    }
}
