using System; 
using WebDavSync.Config.DTO; 
using WebDavSync.DataProtection; 

namespace WebDavSync.Config 
{
    public class ConfigLocal
    {
        #region ------------------ Properties ------------------
        private DataProtectionManager _protectionManager; 
        #endregion
        
        #region ------------------ Konstruktor ------------------
        public ConfigLocal(DataProtectionManager protectionManager) 
        {
            _protectionManager = protectionManager; 
        }
        public ConfigLocal(ConfigLocalDTO dto, DataProtectionManager protectionManager)
        {
            LocalSyncDir = dto.LocalSyncDir; 
            _protectionManager = protectionManager; 
        }
        #endregion
        
        #region ------------------ public Properties und Methods ------------------

        /// <summary>
        /// Creaes a ConfigLocalDTO from the ConfigLocal class and returns it 
        /// </summary>
        /// <returns></returns>
        public ConfigLocalDTO ToDTO() 
        {
            return new ConfigLocalDTO() 
            {
                LocalSyncDir = this.LocalSyncDir, 
            }; 
        }

        /// <summary>
        /// The Directory that should be used for synchronisation 
        /// </summary>
        /// <returns></returns>
        public string LocalSyncDir {get;set;}
        #endregion
        
    }
}