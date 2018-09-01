using System; 
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace WebDavSync.DataProtection 
{
    public class DataProtectionManager 
    {
        #region ------------------ Properties ------------------
        IDataProtector _protector;
        #endregion
        
        #region ------------------ Konstruktor ------------------
        /// <summary>
        /// Creates a new DataProtectionManager taht can be used to protect 
        /// data like settings
        /// </summary>
        /// <param name="provider"></param>
        public DataProtectionManager(IDataProtectionProvider provider) 
        {
            _protector = provider.CreateProtector("WebDavSyncClient"); 
        }
        #endregion
        
        #region ------------------ Methods ------------------

        /// <summary>
        /// Protects the given Data and returns the result
        /// Data can later be unprotected using the Unprotect method 
        /// </summary>
        /// <param name="input"></param>
        /// <returns>The protected result of the input</returns>
        public string Protect(string input)
        {
            if (String.IsNullOrEmpty(input)) 
            {
                return input; 
            }
            return _protector.Protect(input); 
        } 

        /// <summary>
        /// Unprotectes the given data that has before been protected with this class
        /// </summary>
        /// <param name="protectedData"></param>
        /// <returns></returns>
        public string Unprotect(string protectedData) 
        {
            if (String.IsNullOrEmpty(protectedData)) 
            {   
                return protectedData; 
            }
            try 
            {
                return _protector.Unprotect(protectedData); 
            } catch
            {
                //Cannot be protected! e.g. Someone changed the string per hand
                return String.Empty; 
            }
        } 

        #endregion
        
    }

}