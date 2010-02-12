using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FubuMVC.Core.Util;

namespace FubuMVC.Core.Models
{
    public class PropertyBinderCache : IPropertyBinderCache
    {
        private readonly IList<IPropertyBinder> _binders = new List<IPropertyBinder>();
        private readonly Cache<PropertyInfo, IPropertyBinder> _cache = new Cache<PropertyInfo, IPropertyBinder>();

        public PropertyBinderCache(IEnumerable<IPropertyBinder> binders, IValueConverterRegistry converters)
        {
            _binders.AddRange(binders);
            _binders.Add(new ConversionPropertyBinder(converters));
            // TODO -- add the hierarchical / child binding

            _cache.OnMissing = prop => _binders.FirstOrDefault(x => x.Matches(prop));
        }

        public IPropertyBinder BinderFor(PropertyInfo property)
        {
            // TODO -- throw exception if there is no PropertyFinder for this PropertyInfo
            return _cache[property];
        }
    }
}