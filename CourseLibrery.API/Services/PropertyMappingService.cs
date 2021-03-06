﻿using CourseLibrary.API.Entities;
using CourseLibrery.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrery.API.Services
{
    public class PropertyMappingService : IPropertyMappingService
    {
        private Dictionary<string, PropertyMappingValue> _authorPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "Id", new PropertyMappingValue(new List<string>{ "Id" }) },
                { "MainCategory", new PropertyMappingValue(new List<string>{"MainCategory" }) },
                { "Age", new PropertyMappingValue(new List<string>{"DateOfBirth" },true) },
                { "Name", new PropertyMappingValue(new List<string>{"FirstName","LastName" }) },

            };

        private IList<IPropertyMapping> _propertyMappings = new List<IPropertyMapping>();

        public PropertyMappingService()
        {
            _propertyMappings.Add(new PropertyMapping<AuthorDto, Author>(_authorPropertyMapping));
        }

        public bool ValidMappingExistisFor<TSource, TDestination>(string fields)
        {
            var propertyMapping = GetPropertyMapping<TSource, TDestination>();
            
            if (string.IsNullOrWhiteSpace(fields))
            {
                return true;
            }

            var fieldsByAfterSplit = fields.Split(',');

            foreach (var field in fieldsByAfterSplit)
            {
                var trimmedFields = field.Trim();
           

                var indexOfFirstSpace = trimmedFields.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ? trimmedFields : trimmedFields.Remove(indexOfFirstSpace);

                if (!propertyMapping.ContainsKey(propertyName))
                {
                    return false;
                }
              
            }
            return true;

        }

        public Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
        {
            var matchingMapping = _propertyMappings.OfType<PropertyMapping<TSource, TDestination>>();

            if (matchingMapping.Count() == 1)
            {
                return matchingMapping.First()._mappingDictionary;
            }

            throw new Exception($"Cannot find exaxt property mapping instance for < {typeof(TSource)},{typeof(TDestination)}");
        }
    }
}
