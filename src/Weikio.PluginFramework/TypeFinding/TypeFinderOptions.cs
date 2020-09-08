using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Weikio.PluginFramework.TypeFinding
{
    public class TypeFinderOptions
    {
        /// <summary>
        /// Gets or sets the <see cref="TypeFinderCriteria"/>
        /// </summary>
        public List<TypeFinderCriteria> TypeFinderCriterias { get; set; } = new List<TypeFinderCriteria>(Defaults.GetDefaultTypeFinderCriterias());

        public static class Defaults
        {
            public static List<TypeFinderCriteria> TypeFinderCriterias { get; set; } = new List<TypeFinderCriteria>();

            public static ReadOnlyCollection<TypeFinderCriteria> GetDefaultTypeFinderCriterias()
            {
                return TypeFinderCriterias.AsReadOnly();
            }
        }
    }
}
