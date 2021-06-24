﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CatelogueManagement.DL.Helpers.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Build Filter Dictionary<string,string> used in ExpressionExtensions.BuildPredicate to build
        /// predicates for Predicate Builder based on class's properties values. Filters are then used
        /// by PredicateParser, which converts them to appropriate types (DateTime, int, decimal, etc.)
        /// </summary>
        /// <param name="this">Object to build dictionary from</param>
        /// <param name="includeNullValues">Includes null values in dictionary</param>
        /// <returns>Dictionary with string keys and string values</returns>
        public static Dictionary<string, string> ToFilterDictionary(this object @this, bool includeNullValues)
        {
            var result = new Dictionary<string, string>();
            if (@this == null || !@this.GetType().IsClass)
                return result;

            // First, generate Dictionary<string, string> from @this by using reflection
            var props = @this.GetType().GetProperties();
            foreach (var prop in props)
            {
                var value = prop.GetValue(@this);
                if (value == null && !includeNullValues)
                    continue;

                // If value already is a dictionary add items from this dictionary
                var dictValue = value as IDictionary;
                if (dictValue != null)
                {
                    foreach (var key in dictValue.Keys)
                    {
                        var valueTemp = dictValue[key];
                        if (valueTemp == null && !includeNullValues)
                            continue;
                        result.Add(key.ToString(), valueTemp != null ? valueTemp.ToString() : null);
                    }
                    continue;
                }

                // If property ends with list, check if list of generics
                if (prop.Name.EndsWith("List", false, CultureInfo.InvariantCulture))
                {
                    var propName = prop.Name.Remove(prop.Name.Length - 4, 4);
                    var sb = new StringBuilder();
                    var list = value as IEnumerable;
                    if (list != null)
                    {
                        foreach (var item in list)
                        {
                            if (item == null)
                                continue;
                            if (sb.Length > 0)
                                sb.Append(",");
                            sb.Append(item.ToString());
                        }
                        result.Add(propName, sb.ToString());
                    }
                    continue;
                }

                var str = value != null ? value.ToString() : null;
                result.Add(prop.Name, str);
            }

            return result;
        }
        public static void SetValues4PropertiesFromChildObject(this object @this,object childObject, string[] ignores)
        {
            var childProperties = childObject.GetType().GetProperties();
            var properties = @this.GetType().GetProperties();
            foreach (var p in childProperties)
            {
                var pProperty = properties.FirstOrDefault(t => t.Name == p.Name && t.PropertyType == p.PropertyType);
                if (pProperty != null && !(ignores != null && ignores.Contains(p.Name)))
                {
                    var childValue = p.GetValue(childObject, null);
                    pProperty.SetValue(@this, childValue, null);
                }
            }
        }
        public static T CopyTo<T>(this object source, T destination)
        {
            // If any this null throw an exception
            if (source == null || destination == null)
                throw new Exception("Source or/and Destination Objects are null");
            // Getting the Types of the objects
            Type typeDest = destination.GetType();
            Type typeSrc = source.GetType();
            // Collect all the valid properties to map
            var results = from srcProp in typeSrc.GetProperties()
                          let targetProperty = typeDest.GetProperty(srcProp.Name)
                          where srcProp.CanRead
                          && targetProperty != null
                          && (targetProperty.GetSetMethod(true) != null && !targetProperty.GetSetMethod(true).IsPrivate)
                          && (targetProperty.GetSetMethod().Attributes & MethodAttributes.Static) == 0
                          && targetProperty.PropertyType.IsAssignableFrom(srcProp.PropertyType)
                          select new { sourceProperty = srcProp, targetProperty = targetProperty };
            //map the properties
            foreach (var props in results)
            {
                props.targetProperty.SetValue(destination, props.sourceProperty.GetValue(source, null), null);
            }
            return destination;
        }
    }
}