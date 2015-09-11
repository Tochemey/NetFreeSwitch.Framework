/*
Licensed to the Apache Software Foundation (ASF) under one
or more contributor license agreements.  See the NOTICE file
distributed with this work for additional information
regarding copyright ownership.  The ASF licenses this file
to you under the Apache License, Version 2.0 (the
"License"); you may not use this file except in compliance
with the License.  You may obtain a copy of the License at

  http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing,
software distributed under the License is distributed on an
"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
KIND, either express or implied.  See the License for the
specific language governing permissions and limitations
under the License.
*/

using System;
using System.Linq;

namespace NetFreeSwitch.Framework {
    /// <summary>
    ///     Extension methods for <c>Type</c>.
    /// </summary>
    public static class TypeExtensions {
        /// <summary>
        ///     Get assembly qualified name, but without the version and public token.
        /// </summary>
        /// <param name="type">Type to get name for</param>
        /// <returns>Simple assembly qualified name. Example: <code>"MyApp.Contracts.User, MyApp.Contracts"</code></returns>
        public static string GetSimpleAssemblyQualifiedName(this Type type) {
            if (type == null) throw new ArgumentNullException("type");
            if (type.Assembly.IsDynamic)
                throw new InvalidOperationException("Can't use dynamic assemblies.");

            return type.FullName + ", " + type.Assembly.GetName().Name;
        }

        /// <summary>
        ///     Get type name as we define it in code.
        /// </summary>
        /// <param name="t">The type to get a name for.</param>
        /// <returns>String representation</returns>
        public static string GetFriendlyTypeName(this Type t) {
            if (!t.IsGenericType)
                return t.Name;
            var genericTypeName = t.GetGenericTypeDefinition().Name;
            genericTypeName = genericTypeName.Substring(0, genericTypeName.IndexOf('`'));
            var genericArgs = string.Join(",", t.GetGenericArguments().Select(GetFriendlyTypeName).ToArray());
            return string.Format("{0}<{1}>", genericTypeName, genericArgs);
        }

        /// <summary>
        ///     Check if generic types matches
        /// </summary>
        /// <param name="serviceType">Service/interface</param>
        /// <param name="concreteType">Concrete/class</param>
        /// <returns><c>true</c> if the concrete implements the service; otherwise <c>false</c></returns>
        public static bool IsAssignableFromGeneric(this Type serviceType, Type concreteType) {
            var interfaceTypes = concreteType.GetInterfaces();
            if (interfaceTypes.Any(it => it.IsGenericType && it.GetGenericTypeDefinition() == serviceType))
                return true;

            var baseType = concreteType.BaseType;
            if (baseType == null)
                return false;

            return baseType.IsGenericType && baseType.GetGenericTypeDefinition() == serviceType || IsAssignableFromGeneric(serviceType, baseType);
        }

        /// <summary>
        ///     Checks if the specified type is a type which should not be traversed when building an object hiararchy.
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns><c>true</c> if it's a simple type; otherwise <c>false</c>.</returns>
        /// <example>
        ///     <code>
        /// public string Build(object instance)
        /// {
        ///     var sb = new StringBuilder();
        ///     Build(instance, "", sb);
        ///     return sb.ToString();
        /// }
        /// 
        /// protected void Build(object instance, string prefix, StringBuilder result)
        /// {
        ///     foreach (var propInfo in instance.GetType().GetProperties())
        ///     {
        ///         if (instance.GetType().IsSimpleType())
        ///         {
        ///             var value = propInfo.GetValue(instance, null);
        ///             result.AppendLine(prefix + propInfo.Name + ": " + value);
        ///         }
        ///         else
        ///         {
        ///             var newPrefix = prefix == "" ? propInfo.Name : prefix + ".";
        ///             Build(newPrefix, 
        /// }
        /// 
        /// while (!type.IsSimpleType())
        /// {
        /// }
        /// 
        /// </code>
        /// </example>
        public static bool IsSimpleType(this Type type) {
            return type.IsPrimitive || type == typeof (decimal) || type == typeof (string) || type == typeof (DateTime) || type == typeof (Guid) || type == typeof (DateTimeOffset)
                   || type == typeof (TimeSpan);
        }
    }
}
