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

namespace NetFreeSwitch.Framework.FreeSwitch.Commands {
    /// <summary>
    /// say.
    /// The say application will use the pre-recorded sound files to read or say various things like dates, times, digits, etc. 
    /// The say application can read digits and numbers as well as dollar amounts, date/time values and IP addresses. 
    /// It can also spell out alpha-numeric text, including punctuation marks
    /// </summary>
    public class SayCommand : BaseCommand{
        public SayCommand(string text) {
            if(string.IsNullOrEmpty(text)) throw new ArgumentNullException("text");
            Text = text;
            SayType = EslSayTypes.MESSAGES;
            SayMethod = EslSayMethods.PRONOUNCED;
            Gender = EslSayGenders.FEMININE;
        }

        /// <summary>
        /// Language or Module
        /// </summary>
        public string Language { set; get; }

        /// <summary>
        /// Type <see cref="EslSayTypes"/>
        /// </summary>
        public EslSayTypes SayType { set; get; }

        /// <summary>
        /// Method <see cref="EslSayMethods"/>
        /// </summary>
        public EslSayMethods SayMethod { set; get; }

        /// <summary>
        /// Gender <see cref="EslSayGenders"/>
        /// </summary>
        public EslSayGenders Gender { set; get; }

        /// <summary>
        /// The actual text to read.
        /// </summary>
        public string Text { private set; get; }

        public override string Command { get { return "say"; } }

        public override string Argument { get { return Language + " " + SayType + " " + SayMethod.ToString().Replace("_", "/") + " " + Gender + " " + Text; } }
    }
}
