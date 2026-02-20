using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Project.Scripts.Interpreters
{
    public static class AuthorizedHelper
    {
        /// <summary>
        /// Attribute used on any method that has to be extracted using the <code>ExtractTypes</code> method.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, Inherited = false)]
        public class AuthorizedType : Attribute
        {
        }

        /// <summary>
        /// Attribute used on any method that has to be extracted using the <code>ExtractPublicMethods</code> method.
        /// </summary>
        /// <remarks>Non-public methods will be ignored</remarks>
        /// <remarks><code>Inherit</code> should only be used on Base methods. If not, the extraction method will throw an exception</remarks>
        [AttributeUsage(AttributeTargets.Method, Inherited = false)]
        public class AuthorizedPublicMethod : Attribute
        {
            public bool Inherit { get; }

            public AuthorizedPublicMethod(bool inherit = false)
            {
                Inherit = inherit;
            }
        }

        /// <summary>
        /// Attribute used on any method that has to be extracted using the <code>FormatSelfMethods</code> method.
        /// </summary>
        /// <remarks><code>Inherit</code> should only be used on Base methods. If not, the extraction method will throw an exception</remarks>
        [AttributeUsage(AttributeTargets.Method, Inherited = false)]
        public class AuthorizedSelfMethod : Attribute
        {
            public bool Inherit { get; }

            public AuthorizedSelfMethod(bool inherit = false)
            {
                Inherit = inherit;
            }
        }
        
        /// <summary>
        /// Extract all types with the attribute <code>AuthorizedType</code>
        /// </summary>
        /// <param name="assemblies">The assemblies from where to extract the types</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static HashSet<Type> ExtractTypes(Assembly[] assemblies)
        {
            if (assemblies is null)
                throw new ArgumentNullException(nameof(assemblies));

            
            var types = new HashSet<Type>();
            
            foreach (var type in assemblies.SelectMany(x => x.GetTypes()))
            {
                if (type.GetCustomAttribute<AuthorizedType>() is not null)
                    types.Add(type);
            }

            return types;
        }

        /// <summary>
        /// Extract all the public methods with the attribute <code>AuthorizedPublicMethod</code>.
        /// </summary>
        /// <param name="assemblies">The assemblies from where to extract the methods</param>
        /// <returns>The extracted methods by type</returns>
        /// <remarks>If the attribute <code>AuthorizedPublicMethod</code> Inherit parameter is set to true, the base method will be also added to the children types</remarks>
        public static Dictionary<Type, HashSet<MethodInfo>> ExtractPublicMethods(Assembly[] assemblies)
        {
            if(assemblies is null)
                throw new ArgumentNullException(nameof(assemblies));
            
            // Get authorized methods
            var allTypes = assemblies.SelectMany(x => x.GetTypes()).ToArray();
            var selectedTypesAndMethods = new HashSet<Tuple<Type, MethodInfo>>();
            
            // Fill the lists
            foreach (var type in allTypes)
            {
                foreach (var methodInfo in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | 
                                                           BindingFlags.Static | BindingFlags.DeclaredOnly))
                {
                    // Check attribute
                    var authorizedMethod = methodInfo.GetCustomAttribute<AuthorizedPublicMethod>();
                    
                    if (authorizedMethod is null || !methodInfo.IsPublic)
                        continue;
                    
                    if(type.IsClass && !type.IsAbstract || type.IsSealed)
                        selectedTypesAndMethods.Add(new Tuple<Type, MethodInfo>(type, methodInfo));
                    
                    if(authorizedMethod.Inherit && methodInfo.DeclaringType != type)
                        continue;
                    
                    // Search for inherited methods
                    if (!authorizedMethod.Inherit)
                        continue;
                    
                    // Inheritance only allowed for base methods
                    if (methodInfo.GetBaseDefinition().DeclaringType != type)
                        throw new ApplicationException($"The {methodInfo.Name} has the attribute {nameof(AuthorizedPublicMethod)} with {nameof(AuthorizedPublicMethod.Inherit)} set to true but is not a Base method");
                    
                    foreach (var comparisonType in allTypes)
                    {
                        // Only consider concrete types that differ from the declaring type
                        if (comparisonType == type || comparisonType.IsAbstract || comparisonType.IsInterface || comparisonType.IsGenericTypeDefinition)
                            continue;

                        // Only consider methods from the declaring type
                        if (!type.IsAssignableFrom(comparisonType))
                            continue;

                        // Inject the method to the class
                        selectedTypesAndMethods.Add(new Tuple<Type, MethodInfo>(comparisonType, methodInfo));
                    }
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
        
        /// <summary>
        /// Extract methods from the reference type and inherited methods from parents with the attribute <code>AuthorizedSelfMethod</code>.
        /// </summary>
        /// <param name="reference">Reference object from which to </param>
        /// <param name="assemblies">The assemblies from where to extract the methods</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static HashSet<MethodInfo> ExtractSelfMethods(object reference, Assembly[] assemblies = null)
        {
            if(reference is null)
                throw new ArgumentNullException(nameof(reference));
            
            var referenceType = reference.GetType();
            assemblies ??= new[] { referenceType.Assembly };
            
            var methods = new  HashSet<MethodInfo>();
            foreach (var type in assemblies.SelectMany(x => x.GetTypes()))
            {
                // Check if the type is self or if it's a parent with the inherit parameter
                var isSelf = type == referenceType;
                foreach (var methodInfo in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                                       BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                {
                    // Check the attribute
                    var authorizedMethod = methodInfo.GetCustomAttribute<AuthorizedSelfMethod>();
                    
                    if (authorizedMethod is null)
                        continue;
                    
                    // If the method comes from parent class, check if the methods is allowed on the child class
                    if (!isSelf)
                    {
                        // Check inheritance
                        if(!authorizedMethod.Inherit || !type.IsAssignableFrom(referenceType))
                            continue;
                        
                        // Inheritance only allowed for base methods
                        if (methodInfo.GetBaseDefinition().DeclaringType != type)
                            throw new ApplicationException($"The {methodInfo.Name} has the attribute {nameof(AuthorizedPublicMethod)} with {nameof(AuthorizedPublicMethod.Inherit)} set to true but is not a Base method");
                    }
                    
                    methods.Add(methodInfo);   
                }
            }

            return methods;
        }
    }
}