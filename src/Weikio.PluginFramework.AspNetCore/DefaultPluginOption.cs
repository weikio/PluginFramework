using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weikio.PluginFramework.AspNetCore
{
    public class DefaultPluginOption
    {
        public Func<IServiceProvider, IEnumerable<Type>, Type> DefaultType { get; set; }
            = (serviceProvider, implementingTypes) => implementingTypes.FirstOrDefault();
    }
}
