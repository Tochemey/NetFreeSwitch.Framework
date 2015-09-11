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
using System.Linq.Expressions;
using System.Reflection;

namespace NetFreeSwitch.Framework.Common {
    /// <summary>
    ///     Credits http://rogeralsing.com/2008/02/28/linq-expressions-creating-objects/
    /// </summary>
    public static class ConstructorExtensions {
        /// <summary>
        ///     Creates a delegate which allocates a new object faster than  <see cref="Activator.CreateInstance()" />.
        /// </summary>
        /// <param name="ctor">The ctor.</param>
        /// <returns>The activator</returns>
        /// <remarks>
        ///     The method uses an expression tree to build
        ///     a delegate for the specified constructor
        /// </remarks>
        public static InstanceFactory CreateFactory(this ConstructorInfo ctor) {
            if (ctor == null) throw new ArgumentNullException("ctor");

            var paramsInfo = ctor.GetParameters();
            var param = Expression.Parameter(typeof (object[]), "args");

            var argsExp = new Expression[paramsInfo.Length];

            //pick each arg from the params array 
            //and create a typed expression of them
            for (var i = 0; i < paramsInfo.Length; i++) {
                Expression index = Expression.Constant(i);
                var paramType = paramsInfo[i].ParameterType;
                Expression paramAccessorExp = Expression.ArrayIndex(param, index);
                Expression paramCastExp = Expression.Convert(paramAccessorExp, paramType);
                argsExp[i] = paramCastExp;
            }

            //make a NewExpression that calls the
            //ctor with the args we just created
            var newExp = Expression.New(ctor, argsExp);

            //create a lambda with the New
            //Expression as body and our param object[] as arg
            var lambda = Expression.Lambda(typeof (InstanceFactory), newExp, param);

            //compile it
            return (InstanceFactory) lambda.Compile();
        }
    }
}
