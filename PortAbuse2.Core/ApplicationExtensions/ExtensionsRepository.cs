using System;
using System.Collections.Generic;
using System.Linq;

namespace PortAbuse2.Core.ApplicationExtensions
{
    public static class ExtensionsRepository
    {
        private static readonly Dictionary<string, List<Type>> ExistedExtensions = new Dictionary<string, List<Type>>();

        static ExtensionsRepository()
        {
            var iExtension = typeof(IApplicationExtension);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => iExtension.IsAssignableFrom(p) && !p.IsInterface);
            foreach (var type in types)
            {
                var instance = Activator.CreateInstance(type) as IApplicationExtension;
                if (instance != null)
                {
                    foreach (var name in instance.AppNames)
                    {
                        AddTypeToApplication(name, type);
                    }
                }
            }
        }

        private static void AddTypeToApplication(string name, Type type)
        {
            if (ExistedExtensions.ContainsKey(name))
            {
                ExistedExtensions[name].Add(type);
            }
            else
            {
                ExistedExtensions.Add(name, new List<Type> {type});
            }
        }

        public static IEnumerable<IApplicationExtension> GetExtensionsForApp(string appName)
        {
            var result = new List<IApplicationExtension>();
            if (ExistedExtensions.ContainsKey(appName))
            {
                result.AddRange(ExistedExtensions[appName].Select(Activator.CreateInstance).OfType<IApplicationExtension>());
            }
            return result;
        }
    }
}