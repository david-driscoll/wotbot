using System;
using System.Collections.Generic;
using System.Linq;
using NLua;

namespace wotbot
{
    public static class LuaExtensions
    {
        public static IEnumerable<object> GetValidKeys(this LuaTable table)
        {
            foreach (var key in table.Keys)
            {
                if (key is string s)
                {
                    // seed seems to be an internal state around repairing tables
                    if (s is "__default" or "dbinfo" or "seed") continue;
                }
                yield return key;

            }
        }

        public static object LuaTableToObject(this LuaTable table)
        {
            var result = new Dictionary<object, object>();
            foreach (var key in GetValidKeys(table))
            {
                var value = table[key];
                result.Add(
                    key switch
                    {
                        long lkey => lkey,
                        string skey => long.TryParse(skey, out var lkey) && lkey > 0 ? lkey : skey, // zero is not a valid array in lua (1 based everything!)
                        _ => throw new ArgumentOutOfRangeException($"Not supported type {key.GetType().FullName}")
                    },
                    value switch
                    {
                        LuaTable t => LuaTableToObject(t),
                        string or long or bool or double => value,
                        _ => throw new ArgumentOutOfRangeException($"Not supported type {value.GetType().FullName}")
                    }
                );
            }
            if (result.Keys.Count > 0 && result.Keys.All(z => z is long))
            {
                return result.Keys.OfType<long>().OrderBy(z => z).Select(z => result[z]).ToArray();
            }

            return result;
        }
    }
}
