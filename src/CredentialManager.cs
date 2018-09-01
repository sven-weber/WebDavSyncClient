using System; 
using System.Security; 
using WebDavSync.ExtensionMethods; 

namespace WebDavSync 
{
    public class CredentialsManager
    {

        #region ------------------ Properties ------------------
        #endregion
        
        #region ------------------ Konstruktor ------------------
        #endregion
        
        #region ------------------ public Methods ------------------
        /// <summary>
        /// Trys to get the password from the user, confirms the password 
        /// with asking the user for retyping it
        /// </summary>
        /// <returns>The password as SecureString or null if the string could not be gotten</returns>
        public SecureString GetPassword()
        {
            bool retry = true; 
            while(retry) 
            {
                Console.Write("Please type your password: "); 
                SecureString firstPass = GetPasswordFromConsole(); 
                Console.WriteLine(""); 
                Console.Write("Please retype your password: "); 
                SecureString secondPass = GetPasswordFromConsole(); 
                Console.WriteLine(""); 
                if (firstPass.IsEqualTo(secondPass)) 
                {
                    retry = false; 
                    return firstPass; 
                } else 
                {
                    Console.Write("The given password are not equal! Try again? (y/n):"); 
                    if (!Console.ReadLine().ToLower().Equals("y")) 
                    {
                        retry = false; 
                    }
                }
            } 
            return null; 
        }
        #endregion
        

        #region ------------------ private Methods ------------------
        /// <summary>
        /// Gets the Password as Secure string which will then be returned 
        /// </summary>
        /// <returns></returns>
        private SecureString GetPasswordFromConsole()
        {
            var pwd = new SecureString();
            while (true)
            {
                ConsoleKeyInfo i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (i.Key == ConsoleKey.Backspace)
                {
                    if (pwd.Length > 0)
                    {
                        pwd.RemoveAt(pwd.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else
                {
                    pwd.AppendChar(i.KeyChar);
                    Console.Write("*");
                }
            }
            return pwd;
        }
        #endregion
    }
}