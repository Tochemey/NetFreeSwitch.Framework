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
using System.IO;
using System.Security.AccessControl;

namespace NetFreeSwitch.Framework.IO {
    /// <summary>
    ///     Missing .NET features for Directory management.
    /// </summary>
    public class DirectoryUtils {
        /// <summary>
        ///     Copies files from one folder to another. Source security settings are included.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="copySubDirs">if set to <c>true</c>, sub directories are also copied.</param>
        /// <exception cref="System.ArgumentNullException">
        ///     source
        ///     or
        ///     destination
        /// </exception>
        public static void Copy(string source, string destination, bool copySubDirs) {
            if (source == null) throw new ArgumentNullException("source");
            if (destination == null) throw new ArgumentNullException("destination");
            var security = Directory.GetAccessControl(source);
            Copy(source, destination, copySubDirs, security);
        }

        /// <summary>
        ///     Copies files from one directory to another.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="copySubDirs">if set to <c>true</c>, copy all contents from the sub directories too.</param>
        /// <param name="security">The security.</param>
        /// <exception cref="System.ArgumentNullException">
        ///     source
        ///     or
        ///     destination
        ///     or
        ///     security
        /// </exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">
        ///     Source directory does not exist or could not be found:
        ///     + source
        /// </exception>
        public static void Copy(string source, string destination, bool copySubDirs, DirectorySecurity security) {
            if (source == null) throw new ArgumentNullException("source");
            if (destination == null) throw new ArgumentNullException("destination");
            if (security == null) throw new ArgumentNullException("security");
            var dir = new DirectoryInfo(source);
            var dirs = dir.GetDirectories();

            if (!dir.Exists) throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + source);

            if (!Directory.Exists(destination)) Directory.CreateDirectory(destination, security);

            var files = dir.GetFiles();
            foreach (var file in files) {
                var temppath = Path.Combine(destination, file.Name);
                file.CopyTo(temppath, false);
            }

            if (!copySubDirs)
                return;

            foreach (var subdir in dirs) {
                var temppath = Path.Combine(destination, subdir.Name);
                Copy(subdir.FullName, temppath, true);
            }
        }
    }
}
