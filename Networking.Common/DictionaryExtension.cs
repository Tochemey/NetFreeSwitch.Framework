using System.Collections.Generic;
using System.Collections.Specialized;

namespace Networking.Common {
    /// <summary>
    ///     Extension methods for dictionary
    /// </summary>
    public static class DictionaryExtension {
        /// <summary>
        /// Converts a Dictionary to NameValueCollection.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static NameValueCollection ToNameValueCollection<TKey, TValue>(this IDictionary<TKey, TValue> dict) {
            var nameValueCollection = new NameValueCollection();

            foreach (var kvp in dict) {
                string value = string.Empty;
                if (kvp.Value != null)
                    value = kvp.Value.ToString();

                nameValueCollection.Add(kvp.Key.ToString(), value);
            }

            return nameValueCollection;
        }
    }
}