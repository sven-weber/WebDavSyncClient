using System; 
using System.Net; 
using WebDavSync.Config.DTO; 
using WebDavSync.DataProtection; 
using WebDavSync.ExtensionMethods; 

namespace WebDavSync.Config 
{
    public class ConfigRemote 
    {
        #region ------------------ Properties ------------------
        ConfigRemoteDTO _dto; 
        DataProtectionManager _protectionManager; 
        #endregion
        
        #region ------------------ Konstruktor ------------------
        public ConfigRemote(DataProtectionManager protectionManager)
        {
            Credentials = new NetworkCredential(); 
            _protectionManager = protectionManager; 
        }
        
        public ConfigRemote(ConfigRemoteDTO dto, DataProtectionManager protectionManager) 
        {  
            _dto = dto; 
            RemoteServerPath = dto.RemoteServerPath; 
            RemoteFolderPath = dto.RemoteFolderPath; 
            _protectionManager = protectionManager; 

            Credentials = new NetworkCredential(dto.UserName, _protectionManager.Unprotect(dto.SecurePassword)); 
        }

        #endregion
        
        #region ------------------ Methods & Public Properties ------------------
        /// <summary>
        /// Creates a new ConfigRemoteDTO instance from the current class
        /// and returns 
        /// </summary>
        /// <returns></returns>
        public ConfigRemoteDTO ToDTO() 
        {
            return new ConfigRemoteDTO() 
            {
                RemoteServerPath = this.RemoteServerPath, 
                RemoteFolderPath = this.RemoteFolderPath,
                UserName = Credentials.UserName,
                SecurePassword = _protectionManager.Protect(Credentials.Password)
            }; 
        }

        /// <summary>
        /// The Address of the Remote server 
        /// </summary>
        /// <returns></returns>
        public string RemoteServerPath { get; set; } 
        
        /// <summary>
        /// The folder that should be accessed at that remote Server
        /// </summary>
        /// <returns></returns>
        public string RemoteFolderPath { get; set; } 

        /// <summary>
        /// The Credentials used to authenticate the user 
        /// </summary>
        /// <returns></returns>
        public NetworkCredential Credentials { get; set; }
        #endregion
    }
}