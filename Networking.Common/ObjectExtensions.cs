﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Networking.Common {
    /// <summary>
    ///     Extension methods for object.
    /// </summary>
    public static class ObjectExtensions {
        /// <summary>
        ///     Turn anonymous object to dictionary
        /// </summary>
        /// <param name="data">Anonymous object</param>
        /// <returns>Dictionary</returns>
        public static Dictionary<string, object> ToDictionary(this object data) {
            return (from property in data.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance) where property.CanRead select property).ToDictionary(property => property.Name,
                property => property.GetValue(data, null));
        }
    }
}