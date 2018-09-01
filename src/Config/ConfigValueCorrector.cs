using System; 
using System.IO;
using WebDavSync.ExtensionMethods;  

namespace WebDavSync.Config
{
    /// <summary>
    /// Abstract class for ConfigValueCorrector Attributes that perform corrections
    /// of config value such as allways have a tailing backslah or something like that 
    /// </summary>
    public abstract class ConfigValueCorrector : Attribute
    {
        /// <summary>
        /// Get the current value, corrects it an returns the new value to set 
        /// </summary>
        /// <param name="currentValue"></param>
        /// <returns></returns>
        public abstract object Correct(object currentValue); 
    }


    public class HasTailingForwardSlash : ConfigValueCorrector 
    {
        public override object Correct(object currentValue) 
        {
            if (currentValue.GetType().Equals(typeof(string)))
            {
                string convertedValue = (string)currentValue; 
                if (!String.IsNullOrEmpty(convertedValue)) 
                {
                    if (!convertedValue.EndsWith('\\') || !convertedValue.EndsWith('/')) 
                    {
                        return convertedValue + "/"; 
                    }
                }
            }
            return currentValue; 
        }
    }
    public class HasNoTailingSlash : ConfigValueCorrector 
    {
        public override object Correct(object currentValue) 
        {
            if (currentValue.GetType().Equals(typeof(string)))
            {
                string convertedValue = (string)currentValue; 
                if (!String.IsNullOrEmpty(convertedValue)) 
                {
                    if (convertedValue.EndsWith('\\') || convertedValue.EndsWith('/')) 
                    {
                        //Cut of tailing slash
                        return convertedValue.RemoveTailingSlashes();  
                    }
                }
            }
            return currentValue; 
        }
    }

    public class DoesNotStartWithSlash : ConfigValueCorrector 
    {
        public override object Correct(object currentValue) 
        {
            if (currentValue.GetType().Equals(typeof(string)))
            {
                string convertedValue = (string)currentValue; 
                if (!String.IsNullOrEmpty(convertedValue)) 
                {
                    if (convertedValue.StartsWith('\\') || convertedValue.StartsWith('/')) 
                    {
                        return convertedValue.Substring(1, convertedValue.Length-1); 
                    }
                }
            }
            return currentValue; 
        }
    }
    public class PathWithForwardSlash : ConfigValueCorrector 
    {
        public override object Correct(object currentValue) 
        {
            if (currentValue.GetType().Equals(typeof(string)))
            {
                string convertedValue = (string)currentValue; 
                if (!String.IsNullOrEmpty(convertedValue)) 
                {
                    return convertedValue.Replace('\\', '/'); 
                }
            }
            return currentValue; 
        }
    }
}