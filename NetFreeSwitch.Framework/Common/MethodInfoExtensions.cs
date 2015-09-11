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
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace NetFreeSwitch.Framework.Common {
    /// <summary>
    ///     Extension methods for <see cref="MethodInfo" />.
    /// </summary>
    public static class MethodInfoExtensions {
        /// <summary>
        ///     Delegate for <see cref="MethodInfoExtensions.ToFastDelegate" />.
        /// </summary>
        /// <param name="instance">instance to invoke the method on</param>
        /// <param name="arguments">method arguments</param>
        /// <returns>Result (if any)</returns>
        public delegate object LateBoundMethod(object instance, object[] arguments);

        /// <summary>
        ///     Create a fast delegete from the method info which also takes care of casting.
        /// </summary>
        /// <param name="method">Method to convert</param>
        /// <returns>Delegate to invoke</returns>
        public static LateBoundMethod ToFastDelegate(this MethodInfo method) {
            if (method == null) throw new ArgumentNullException("method");
            var instanceParameter = Expression.Parameter(typeof (object), "target");
            var argumentsParameter = Expression.Parameter(typeof (object[]), "arguments");

            var call = Expression.Call(Expression.Convert(instanceParameter, method.DeclaringType), method, CreateParameterExpressions(method, argumentsParameter));

            var lambda = Expression.Lambda<LateBoundMethod>(Expression.Convert(call, typeof (object)), instanceParameter, argumentsParameter);

            return lambda.Compile();
        }

        private static Expression[] CreateParameterExpressions(MethodInfo method, Expression argumentsParameter) {
            var expressions = new List<Expression>();
            var index = 0;
            foreach (var parameter in method.GetParameters()) {
                //TODO: clean up hack.
                Type argumentType;
                if (parameter.ParameterType.IsGenericParameter) argumentType = method.GetGenericArguments()[0];

                else
                    argumentType = parameter.ParameterType;

                var expression = Expression.Convert(Expression.ArrayIndex(argumentsParameter, Expression.Constant(index++)), argumentType);
                expressions.Add(expression);
            }

            return expressions.ToArray();
        }
    }
}
