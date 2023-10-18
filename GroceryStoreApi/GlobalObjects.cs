using GroceryStoreApi.Steps.ApiSteps;
using System.Collections.Generic;

namespace GroceryStoreApi
{
    public static class GlobalObjects
    {
        public static string ResponseContent { get; internal set; }
        public static string AccessTokenResponseContent { get; internal set; }
        public static string AccessToken { get; internal set; }
        public static string CartId { get; internal set; }
        public static string ItemId { get; internal set; }
        public static string OrderId{ get; internal set; }

        private static readonly Dictionary<string, string> _globalDictionary = new Dictionary<string, string>();

        public static void AddOrUpdate(string key, string value)
        {
            if (_globalDictionary.ContainsKey(key))
            {
                _globalDictionary[key] = value;
            }
            else
            {
                _globalDictionary.Add(key, value);
            }
        }

        public static string Get(string key)
        {
            if (_globalDictionary.ContainsKey(key))
            {
                return _globalDictionary[key];
            }
            return null;
        }
    }
}
