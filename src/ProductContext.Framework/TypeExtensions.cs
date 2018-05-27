using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace ProductContext.Framework
{
    public static class TypeExtensions
    {
        private static readonly ConcurrentDictionary<Type, string> s_shortenedTypeNames = new ConcurrentDictionary<Type, string>();
        private static readonly string s_coreAssemblyName = typeof(object).GetTypeInfo().Assembly.GetName().Name;

        public static string TypeQualifiedName(this Type type)
        {
            if (s_shortenedTypeNames.TryGetValue(type, out string shortened))
            {
                return shortened;
            }

            string assemblyName = type.GetTypeInfo().Assembly.GetName().Name;
            shortened = assemblyName.Equals(s_coreAssemblyName)
                ? type.GetTypeInfo().FullName
                : $"{type.GetTypeInfo().FullName}, {assemblyName}";
            s_shortenedTypeNames.TryAdd(type, shortened);
            return shortened;
        }
    }
}
