using System;

namespace Weikio.PluginFramework.TypeFinding
{
    public class TypeFinderCriteriaBuilder
    {
        private Type _inherits;
        private Type _implements;
        private Type _assignable;
        private bool? _isAbstract = false;
        private bool? _isInterface = false;
        private string _name;
        private Type _attribute;

        public TypeFinderCriteria Build()
        {
            var res = new TypeFinderCriteria
            {
                IsInterface = _isInterface,
                Implements = _implements,
                Inherits = _inherits,
                AssignableTo = _assignable,
                Name = _name,
                IsAbstract = _isAbstract,
                HasAttribute = _attribute
            };

            return res;
        }

        public static implicit operator TypeFinderCriteria(TypeFinderCriteriaBuilder criteriaBuilder)
        {
            return criteriaBuilder.Build();
        }

        public static TypeFinderCriteriaBuilder Create()
        {
            return new TypeFinderCriteriaBuilder();
        }

        public TypeFinderCriteriaBuilder HasName(string name)
        {
            _name = name;

            return this;
        }

        public TypeFinderCriteriaBuilder Implements<T>()
        {
            return Implements(typeof(T));
        }

        public TypeFinderCriteriaBuilder Implements(Type t)
        {
            _implements = t;

            return this;
        }

        public TypeFinderCriteriaBuilder Inherits<T>()
        {
            return Inherits(typeof(T));
        }

        public TypeFinderCriteriaBuilder Inherits(Type t)
        {
            _inherits = t;

            return this;
        }

        public TypeFinderCriteriaBuilder IsAbstract(bool? isAbstract)
        {
            _isAbstract = isAbstract;

            return this;
        }

        public TypeFinderCriteriaBuilder IsInterface(bool? isInterface)
        {
            _isInterface = isInterface;

            return this;
        }
        
        public TypeFinderCriteriaBuilder AssignableTo(Type assignableTo)
        {
            _assignable = assignableTo;

            return this;
        }

        public TypeFinderCriteriaBuilder HasAttribute(Type attribute)
        {
            _attribute = attribute;

            return this;
        }
    }
}
