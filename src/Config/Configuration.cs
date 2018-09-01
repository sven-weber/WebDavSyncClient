using System; 
using WebDavSync.Config.DTO; 
using WebDavSync.DataProtection; 

namespace WebDavSync.Config 
{
    public class Configuration
    {
        #region ------------------ Konstruktor ------------------
        public Configuration(DataProtectionManager protectionManager) 
        {
            Local = new ConfigLocal(protectionManager); 
            Remote = new ConfigRemote(protectionManager); 
        }

        public Configuration(ConfigDTO dto, DataProtectionManager protectionManager, bool runDebug) 
        {
            RunDebug = runDebug; 
            ReoccurenceTime = dto.ReoccurenceTime; 
            Local = new ConfigLocal(dto.Local, protectionManager); 
            Remote = new ConfigRemote(dto.Remote, protectionManager); 
        }
        #endregion

        #region ------------------ Methods ------------------
        public ConfigDTO ToDTO() 
        {
            return new ConfigDTO() 
            {
                ReoccurenceTime = ReoccurenceTime, 
                Local = this.Local.ToDTO(),
                Remote = this.Remote.ToDTO()
            }; 
        }
        #endregion
        #region ------------------ public Properties ------------------
        /// <summary>
        /// Identicates wheter the App is running in Debug mode or not 
        /// </summary>
        /// <returns></returns>
        public bool RunDebug {get;}

        /// <summary>
        /// The Reoccurence Time after which the App should be executed
        /// </summary>
        /// <returns></returns>
        public int ReoccurenceTime {get; set;}

        /// <summary>
        /// The config settings regarding the local sytem
        /// </summary>
        public ConfigLocal Local {get;set; }

        /// <summary>
        /// The configurtion considering the remote Server values 
        /// </summary>
        /// <returns></returns>
        public ConfigRemote Remote {get;set;}
        #endregion
    }
}