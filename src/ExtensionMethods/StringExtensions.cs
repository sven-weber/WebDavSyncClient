using System; 
using System.Net; 
using System.Text; 
using System.Security; 
using System.Security.Cryptography; 
using System.Runtime.InteropServices; 

namespace WebDavSync.ExtensionMethods 
{
    public static class StringExtensions 
    {
        public static string RemoveTailingSlashes(this string value) 
        {
            if (value.EndsWith('/') || value.EndsWith('\\')) 
            {
                value = value.Substring(0, value.Length - 1);
            }
            return value; 
        }

        public static bool PathsEqual(this string path1, string path2) 
        {
            string compareFirst = path1.Replace("\\\\", "\\").Replace("//", "/"); 
            string compareSecond = path2.Replace("\\\\", "\\").Replace("//", "/"); 
            return compareFirst.Equals(compareSecond); 
        }

        public static string UrlDecode(this string source) 
        {
            return WebUtility.UrlDecode(source); 
        }

        /// <summary>
        /// Encodes the string to http but ignores / at the encoding 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string UrlEncode(this string source) 
        {
            return WebUtility.UrlEncode(source).Replace("%2F", "/"); 
        }
    }
}