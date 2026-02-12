using System;
using System.Collections.Generic;
using System.Reflection;
using Project.Scripts.Interpreters.Libraries;

namespace Project.Scripts.Interpreters
{
    public static class AuthorizedHelper
    {
        /// <summary>
        /// Attribute to use on allowed types that do not contain AuthorizedMethod attributes
        /// </summary>
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, Inherited = false)]
        public class AuthorizedType : Attribute
        {
        }

        /// <summary>
        /// Attribute to use on allowed methods. No need to add the AuthorizedType attribute to the class.
        /// </summary>
        [AttributeUsage(AttributeTargets.Method, Inherited = false)]
        public class AuthorizedMethod : Attribute
        {
            public bool IncludeInherited { get; }

            public AuthorizedMethod(bool includeInherited = false)
            {
                IncludeInherited = includeInherited;
            }
        }

        public static Dictionary<Type, HashSet<MethodInfo>> ExtractTypesAndMethods()
        {
            // Get authorized methods
            var allTypes = new HashSet<Type>();
            var attributeMethodInfos = new HashSet<MethodInfo>();
            var selectedTypesAndMethods = new HashSet<Tuple<Type, MethodInfo>>();
            
            // Fill the lists
            foreach (var type in typeof(World).Assembly.GetTypes())
            {
                allTypes.Add(type);

                if (type.GetCustomAttribute<AuthorizedType>() is not null) 
                    selectedTypesAndMethods.Add(new Tuple<Type, MethodInfo>(type, null));

                foreach (var methodInfo in type.GetMethods())
                {
                    var authorizedMethod = methodInfo.GetCustomAttribute<AuthorizedMethod>();
                    if (authorizedMethod is null)
                        continue;
                    
                    if(authorizedMethod.IncludeInherited && methodInfo.DeclaringType != type)
                        continue;
                    
                    attributeMethodInfos.Add(methodInfo);
                }
            }

            // Fill the dictionary with authorized types
            foreach (var attributeMethodInfo in attributeMethodInfos)
            {
                // Only consider methods with non-null declaring types
                if (attributeMethodInfo.DeclaringType is null)
                    continue;

                // Add the target method if it is not abstract or interface
                selectedTypesAndMethods.Add(new Tuple<Type, MethodInfo>(attributeMethodInfo.DeclaringType, attributeMethodInfo));

                // Search for inherited methods
                if (!attributeMethodInfo.GetCustomAttribute<AuthorizedMethod>().IncludeInherited)
                    continue;
                
                foreach (var type in allTypes)
                {
                    // Only consider concrete types that differ from the declaring type
                    if (type is null || type == attributeMethodInfo.DeclaringType ||
                        type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition)
                        continue;

                    // Only consider methods from the declaring type
                    if (!attributeMethodInfo.DeclaringType.IsAssignableFrom(type))
                        continue;

                    // Add the method to the class
                    selectedTypesAndMethods.Add(new Tuple<Type, MethodInfo>(type, attributeMethodInfo));
                }
            }

            // Fill the dictionary
            var dictionary = new Dictionary<Type, HashSet<MethodInfo>>();
            foreach (var (type, methodInfo) in selectedTypesAndMethods)
            {
                if (type is null)
                    continue;

                if (!dictionary.ContainsKey(type))
                    dictionary.Add(type, new HashSet<MethodInfo>());
                
                dictionary[type].Add(methodInfo);
            }

            return dictionary;
        }
    }
}