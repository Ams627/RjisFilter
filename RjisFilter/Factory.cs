using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace RjisFilter
{
    public class Factory<I> where I : class
    {
        private static Dictionary<string, Type> typeMap;

        static Factory()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var types = assembly.GetTypes()
                        .Where(t => t.IsClass
                               && t.GetCustomAttribute<FactoryAttribute>() != null
                               && typeof(I).IsAssignableFrom(t));

            typeMap = new Dictionary<string, Type>();
            foreach (var type in types)
            {
                var att = type.GetCustomAttribute<FactoryAttribute>();
                typeMap.Add(att.Designator, type);
            }
        }

        public I Create(string name)
        {
            typeMap.TryGetValue(name, out var type);
            if (type == null)
            {
                throw new Exception($"Factory cannot create type of for type designator {name}.");
            }
            var result = Activator.CreateInstance(type);
            return result as I;
        }

        public I Create(string name, object param1)
        {
            typeMap.TryGetValue(name, out var type);
            if (type == null)
            {
                throw new Exception($"Factory cannot create type of for type designator {name}.");
            }
            var result = Activator.CreateInstance(type, param1);
            return result as I;
        }
        public I Create(string name, object param1, object param2)
        {
            typeMap.TryGetValue(name, out var type);
            if (type == null)
            {
                throw new Exception($"Factory cannot create type of for type designator {name}.");
            }
            var result = Activator.CreateInstance(type, param1, param2);
            return result as I;
        }
    }
}
