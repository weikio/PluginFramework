using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Weikio.PluginFramework.TypeFinding
{
    public class TypeFinder
    {
        public bool IsMatch(TypeFinderCriteria criteria, Type type, ITypeFindingContext typeFindingContext)
        {
            if (criteria.Query != null)
            {
                var isMatch = criteria.Query(typeFindingContext, type);

                if (isMatch == false)
                {
                    return false;
                }

                return true;
            }

            if (criteria.IsAbstract != null)
            {
                if (type.IsAbstract != criteria.IsAbstract.GetValueOrDefault())
                {
                    return false;
                }
            }

            if (criteria.IsInterface != null)
            {
                if (type.IsInterface != criteria.IsInterface.GetValueOrDefault())
                {
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(criteria.Name) == false)
            {
                var regEx = NameToRegex(criteria.Name);

                if (regEx.IsMatch(type.FullName) == false)
                {
                    var hasDirectNamingMatch = string.Equals(criteria.Name, type.Name, StringComparison.InvariantCultureIgnoreCase) ||
                                               string.Equals(criteria.Name, type.FullName, StringComparison.InvariantCultureIgnoreCase);

                    if (hasDirectNamingMatch == false)
                    {
                        return false;
                    }
                }
            }

            if (criteria.Inherits != null)
            {
                var inheritedType = typeFindingContext.FindType(criteria.Inherits);

                if (inheritedType.IsAssignableFrom(type) == false)
                {
                    return false;
                }
            }

            if (criteria.Implements != null)
            {
                var interfaceType = typeFindingContext.FindType(criteria.Implements);

                if (interfaceType.IsAssignableFrom(type) == false)
                {
                    return false;
                }
            }

            if (criteria.AssignableTo != null)
            {
                var assignableToType = typeFindingContext.FindType(criteria.AssignableTo);

                if (assignableToType.IsAssignableFrom(type) == false)
                {
                    return false;
                }
            }

            if (criteria.HasAttribute != null)
            {
                var attributes = type.GetCustomAttributesData();
                var attributeFound = false;

                foreach (var attributeData in attributes)
                {
                    if (string.Equals(attributeData.AttributeType.FullName, criteria.HasAttribute.FullName, StringComparison.InvariantCultureIgnoreCase) ==
                        false)
                    {
                        continue;
                    }

                    attributeFound = true;

                    break;
                }

                if (attributeFound == false)
                {
                    return false;
                }
            }

            return true;
        }

        public List<Type> Find(TypeFinderCriteria criteria, Assembly assembly, ITypeFindingContext typeFindingContext)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            var result = new List<Type>();

            var types = assembly.GetExportedTypes();

            foreach (var type in types)
            {
                var isMatch = IsMatch(criteria, type, typeFindingContext);

                if (isMatch == false)
                {
                    continue;
                }
                
                result.Add(type);
            }

            return result;
        }

        private static Regex NameToRegex(string nameFilter)
        {
            // https://stackoverflow.com/a/30300521/66988
            var regex = "^" + Regex.Escape(nameFilter).Replace("\\?", ".").Replace("\\*", ".*") + "$";

            return new Regex(regex, RegexOptions.Compiled);
        }
    }
}
