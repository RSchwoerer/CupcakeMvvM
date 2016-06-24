using CupcakeMvvM.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace CupcakeMvvM
{
    /// <summary>
    /// A source of assemblies that are used by the framework.
    /// </summary>
    public static class AssemblySource
    {
        /// <summary>
        /// Extracts the types from the specified assembly for storing in the cache.
        /// </summary>
        public static Func<Assembly, IEnumerable<Type>> ExtractTypes =
            assembly =>
                assembly.GetExportedTypes()
                    .Where(t =>
                        typeof(UIElement).IsAssignableFrom(t) ||
                        typeof(INotifyPropertyChanged).IsAssignableFrom(t));

        /// <summary>
        /// Finds a type which matches one of the elements in the sequence of names.
        /// </summary>
        public static Func<IEnumerable<string>, Type> FindTypeByNames =
            names =>
            {
                var type = names?
                    .Select(n => TypeNameCache.GetValueOrDefault(n))
                    .FirstOrDefault(t => t != null);
                return type;
            };

        internal static readonly IDictionary<string, Type> TypeNameCache = new Dictionary<string, Type>();

        /// <summary>
        /// Initializes the assembly cache using the specified assemblies.
        /// </summary>
        public static void Initialize(IEnumerable<Assembly> selectAssemblies)
        {
            TypeNameCache.Clear();
            selectAssemblies
                .SelectMany(a => ExtractTypes(a))
                .Apply(t => TypeNameCache.Add(t.FullName, t));

            TypeNameCache.Apply(t => DataTemplateFactory.RegisterDataTemplate(t.Value, ViewLocator.LocateTypeForModelType(t.Value)));
        }
    }
}