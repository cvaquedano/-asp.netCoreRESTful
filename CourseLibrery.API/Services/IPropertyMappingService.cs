using System.Collections.Generic;

namespace CourseLibrery.API.Services
{
    public interface IPropertyMappingService
    {
        Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>();
        bool ValidMappingExistisFor<TSource, TDestination>(string fields);
    }
}