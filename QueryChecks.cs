using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StormBot
{
    class QueryChecks
    {
        public static bool containsQuotes(string query)
        {
            if (query.Contains("'") || query.Contains("\""))
            {
                return true;
            }
            return false;
        }
    }
}