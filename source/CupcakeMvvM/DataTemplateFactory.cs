using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;

namespace CupcakeMvvM
{
    /// <summary>
    /// Dynamically adds DataTemplate declarations for viewmodel/view pairs.
    /// </summary>
    public static class DataTemplateFactory
    {
        /// <summary>
        /// The namespace cache.
        /// Ensures that there is only one namespace prefix per namespace.
        /// </summary>
        private static readonly Dictionary<string, string> NamespaceCache = new Dictionary<string, string>();

        /// <summary>
        /// Registers the model/view pair by creating a DataTemplate.
        /// </summary>
        public static void RegisterDataTemplate<TViewModel, TView>() where TView : FrameworkElement
        {
            RegisterDataTemplate(typeof(TViewModel), typeof(TView));
        }

        /// <summary>
        /// Registers the model/view pair by creating a DataTemplate.
        /// </summary>
        public static void RegisterDataTemplate(Type viewModelType, Type viewType)
        {

            if (viewModelType == null || viewType == null)
                return;
            if (viewModelType == viewType)
                return;

            var template = CreateTemplate(viewModelType, viewType);
            var key = template.DataTemplateKey;
            if (key == null)
                throw new InvalidOperationException("DataTemplateKey not found on DataTemplate.");
            Application.Current.Resources.Add(key, template);
        }

        private static DataTemplate CreateTemplate(Type viewModelType, Type viewType)
        {
            Debug.Assert(viewModelType?.Namespace != null, "viewModelType?.Namespace != null");
            Debug.Assert(viewType?.Namespace != null, "viewType?.Namespace != null");

            if (viewModelType?.Namespace == null)
                throw new ArgumentNullException(nameof(viewModelType));
            if (viewType?.Namespace == null)
                throw new ArgumentNullException(nameof(viewType));

            NamespaceCache[viewModelType.Namespace] = viewModelType.Namespace.ToLower();
            NamespaceCache[viewType.Namespace] = viewType.Namespace.ToLower();

            var context = new ParserContext { XamlTypeMapper = new XamlTypeMapper(new string[0]) };

            // add xmlns definitions for type namespaces using keys from dictionary.
            context.XamlTypeMapper.AddMappingProcessingInstruction(NamespaceCache[viewModelType.Namespace],
                                                                    viewModelType.Namespace,
                                                                    viewModelType.Assembly.FullName);
            context.XamlTypeMapper.AddMappingProcessingInstruction(NamespaceCache[viewType.Namespace],
                                                                    viewType.Namespace,
                                                                    viewType.Assembly.FullName);

            context.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            context.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");
            context.XmlnsDictionary.Add(NamespaceCache[viewModelType.Namespace], NamespaceCache[viewModelType.Namespace]);
            context.XmlnsDictionary.Add(NamespaceCache[viewType.Namespace], NamespaceCache[viewType.Namespace]);

            return (DataTemplate)XamlReader.Parse(
                DataTemplateXaml(viewModelType.Namespace, viewModelType.Name, viewType.Namespace, viewType.Name),
                context);
        }

        private static string DataTemplateXaml(string modelNamespace, string modelTypeName, string viewNamespace, string viewTypeName)
        {
            const string xamlTemplate = "<DataTemplate DataType=\"{{x:Type {0}:{1}}}\"><{2}:{3} /></DataTemplate>";
            return string.Format(xamlTemplate,
                NamespaceCache[modelNamespace],
                modelTypeName,
                NamespaceCache[viewNamespace],
                viewTypeName);
        }
    }
}