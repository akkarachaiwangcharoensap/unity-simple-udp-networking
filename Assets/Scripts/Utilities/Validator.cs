using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace UDP.Utilities
{
    public static class Validator
    {
        /**
         * <summary>
         * https://stackoverflow.com/questions/14977848/how-to-make-sure-that-string-is-valid-json-using-json-net
         * </summary>
         * 
         * <param name="json"></param>
         * 
         * <returns>
         * bool
         * </returns>
         */
        public static bool IsValidJson(string json)
        {
            json = json.Trim();

            if (json.StartsWith("{") && json.EndsWith("}") || json.StartsWith("[") && json.EndsWith("]"))
            {
                try
                {
                    var foo = JToken.Parse(json);
                    return true;
                }
                catch (JsonReaderException e)
                {
                    Debug.Log(e.Message);
                    return false;
                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }

}