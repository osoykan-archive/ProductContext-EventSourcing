using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AggregateSource.SqlStreamStore.Tests
{
    public static class TypeExtensions
    {
        private static readonly ConcurrentDictionary<Type, string> ShortenedTypeNames = new ConcurrentDictionary<Type, string>();
        private static readonly string CoreAssemblyName = typeof(object).GetTypeInfo().Assembly.GetName().Name;

        public static string TypeQualifiedName(this Type type)
        {
            string shortened;
            if (ShortenedTypeNames.TryGetValue(type, out shortened))
            {
                return shortened;
            }
            else
            {
                var assemblyName = type.GetTypeInfo().Assembly.GetName().Name;
                shortened = assemblyName.Equals(CoreAssemblyName)
                    ? type.GetTypeInfo().FullName
                    : $"{type.GetTypeInfo().FullName}, {assemblyName}";
                ShortenedTypeNames.TryAdd(type, shortened);
                return shortened;
            }
        }
    }
}
