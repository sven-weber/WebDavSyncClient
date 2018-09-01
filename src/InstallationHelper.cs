using System; 
using System.Security.Principal;  
using Mono.Posix; 
using System.Runtime.InteropServices; 
using WebDavSync.Log; 
using WebDavSync.Config; 
using System.Security; 

namespace WebDavSync 
{

    public class InstallationHelper
    {
        private ILogger _logger; 
        public InstallationHelper(ILogger logger) 
        {
            _logger = logger; 
        }

        //Checks if the current Users is Administrator
        private bool IsAdministrator() 
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 
                new WindowsPrincipal(WindowsIdentity.GetCurrent())
                .IsInRole(WindowsBuiltInRole.Administrator) :
                Mono.Unix.Native.Syscall.geteuid() == 0; 
        }

        public void Install() 
        {
            
            //Take a look here for service creation https://stackoverflow.com/questions/22336075/linux-process-into-a-service?utm_medium=organic&utm_source=google_rich_qa&utm_campaign=google_rich_qa
            // and here https://stackoverflow.com/questions/483781/how-should-i-log-from-a-non-root-debian-linux-daemon?utm_medium=organic&utm_source=google_rich_qa&utm_campaign=google_rich_qa
            //for service log files 




            
        }

        /// <summary>
        /// Reconfigures the Configuration of the App
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public Configuration Reconfigure(Configuration config) 
        {
            Console.Write("Do you want to configure the general settings? (y/n): "); 
            if (Console.ReadLine().ToLower().Equals("y")) 
            {
                //Configure generall Information 
                Console.WriteLine("Configuring genereall settings. Press Enter to keep current values."); 
                //Reoccurence Time 
                int newReoccurence = GetIntValue("Reouccrence time in sek (Default 300)"); 
                string LocalSyncDir = GetStringValue("Local synchronisation directory"); 
                string RemoteServerUrl = GetStringValue("Remote Server url"); 
                string RemoteServerFolder = GetStringValue("Remote Server folder");

                string correct = GetStringValue("confirm? (y/n)"); 
                if (correct.ToLower().Equals("y")) 
                {
                    if (newReoccurence != 0 ) config.ReoccurenceTime = newReoccurence; 
                    if (!String.IsNullOrEmpty(LocalSyncDir)) config.Local.LocalSyncDir = LocalSyncDir; 
                    if (!String.IsNullOrEmpty(RemoteServerUrl)) config.Remote.RemoteServerPath = RemoteServerUrl;
                    if (!String.IsNullOrEmpty(RemoteServerFolder)) config.Remote.RemoteFolderPath = RemoteServerFolder;  
                }
            }
            Console.Write("Do you want to configure the credentials? (y/n): "); 
            if (Console.ReadLine().ToLower().Equals("y")) 
            {
                Console.WriteLine("Configuring credentials. Press Enter to keep current values."); 
                //Configure credentials 
                string Username = GetStringValue("Username"); 
                SecureString password = new CredentialsManager().GetPassword(); 

                string correct = GetStringValue("confirm? (y/n)"); 
                if (correct.ToLower().Equals("y")) 
                {
                    if (!String.IsNullOrEmpty(Username)) config.Remote.Credentials.UserName = Username; 
                    if (password != null) config.Remote.Credentials.SecurePassword = password; 
                }
            }
            return config; 
        }

        /// <summary>
        /// Gets the answer for the given Text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string GetStringValue(string text) 
        {
            Console.Write(text + " :"); 
            return Console.ReadLine(); 
        }

        /// <summary>
        /// Gets a int value from the user 
        /// </summary>
        /// <param name="text"></param>
        /// <returns>The value or zero as default value</returns>
        private int GetIntValue(string text) 
        {
            bool stop = false; 
            while(!stop) 
            {
                Console.Write(text + " :"); 
                string value = Console.ReadLine(); 

                if (String.IsNullOrEmpty(value)) break; 

                if (!Int32.TryParse(value, out int _res)) 
                {
                    Console.WriteLine("The given value has a invalid format!"); 
                } else 
                {
                    stop = true; 
                    return _res; 
                }
            }
            return 0; 
        }
    }
}