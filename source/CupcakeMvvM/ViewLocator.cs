using CupcakeMvvM.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CupcakeMvvM
{
    internal class ViewLocator
    {
        /// <summary>
        ///   Locates the view type based on the specified model type.
        /// </summary>
        /// <returns>The view.</returns>
        /// <remarks>
        ///   Pass the model type, display location (or null) and the context instance (or null) as parameters and receive a view type.
        /// </remarks>
        public static Func<Type, Type> LocateTypeForModelType =
            modelType =>
            {
                var viewTypeName = modelType.FullName;

                if (ViewTools.InDesignMode)
                {
                    viewTypeName = ModifyModelTypeAtDesignTime(viewTypeName);
                }

                viewTypeName = viewTypeName.Substring(
                    0,
                    viewTypeName.IndexOf('`') < 0
                        ? viewTypeName.Length
                        : viewTypeName.IndexOf('`')
                    );

                var viewTypeList = TransformName(viewTypeName, null);
                var viewType = AssemblySource.FindTypeByNames(viewTypeList);

                if (viewType == null)
                {
                    Console.WriteLine($"View not found for {viewTypeName}. Searched: {string.Join(", ", viewTypeList.ToArray())}.");
                }

                return viewType;
            };

        /// <summary>
        /// Modifies the name of the type to be used at design time.
        /// </summary>
        public static Func<string, string> ModifyModelTypeAtDesignTime =
            modelTypeName =>
            {
                if (modelTypeName.StartsWith("_"))
                {
                    var index = modelTypeName.IndexOf('.');
                    modelTypeName = modelTypeName.Substring(index + 1);
                    index = modelTypeName.IndexOf('.');
                    modelTypeName = modelTypeName.Substring(index + 1);
                }

                return modelTypeName;
            };

        /// <summary>
        /// Transforms a ViewModel type name into all of its possible View type names. Optionally accepts an instance
        /// of context object
        /// </summary>
        /// <returns>Enumeration of transformed names</returns>
        /// <remarks>Arguments:
        /// typeName = The name of the ViewModel type being resolved to its companion View.
        /// context = An instance of the context or null.
        /// </remarks>
        public static Func<string, object, IEnumerable<string>> TransformName =
            (typeName, context) =>
            {
                // HACK [rs]: simple hack to locate view name.
                var nameList = new List<string> { typeName.Replace("ViewModel", "View"), typeName + "View" };
                nameList.RemoveAll(n => n == typeName); // remove anthing matching original name.
                return nameList;

                // TODO [rs]: the rest of this method hooks up the fancy rules based transforming......

                //Func<string, string> getReplaceString;
                //if (context == null)
                //{
                //    getReplaceString = r => r;
                //    return NameTransformer.Transform(typeName, getReplaceString);
                //}

                //var contextstr = ContextSeparator + context;
                //string grpsuffix = String.Empty;
                //if (useNameSuffixesInMappings)
                //{
                //    //Create RegEx for matching any of the synonyms registered
                //    var synonymregex = "(" + String.Join("|", ViewSuffixList.ToArray()) + ")";
                //    grpsuffix = RegExHelper.GetCaptureGroup("suffix", synonymregex);
                //}

                //const string grpbase = @"\${basename}";
                //var patternregex = String.Format(nameFormat, grpbase, grpsuffix) + "$";

                ////Strip out any synonym by just using contents of base capture group with context string
                //var replaceregex = "${basename}" + contextstr;

                ////Strip out the synonym
                //getReplaceString = r => Regex.Replace(r, patternregex, replaceregex);

                ////Return only the names for the context
                //return NameTransformer.Transform(typeName, getReplaceString).Where(n => n.EndsWith(contextstr));
            };

        /// <summary>
        /// Creates the view for a model.
        /// </summary>
        /// <param name="modelType">Type of the model.</param>
        public static DependencyObject CreateViewForModel(Type modelType)
        {
            var key = new DataTemplateKey(modelType);
            var r = (DataTemplate)Application.Current.FindResource(key);
            if (r == null)
                throw new InvalidOperationException($"DataTemplate not found for '{modelType.FullName}'.");
            var c = r.LoadContent();
            return c;
        }
    }
}