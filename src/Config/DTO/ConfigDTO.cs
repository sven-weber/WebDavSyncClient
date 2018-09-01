using System;
using Newtonsoft.Json; 

namespace WebDavSync.Config.DTO
{

    public class ConfigDTO
    {
        public string _comment = "The time in seconds after which a server sync will be performed. Client side changes will be published directly."; 

        [BiggerThanZero]
        public int ReoccurenceTime {get; set;} = 300; 

        public ConfigLocalDTO Local {get; set;} = new ConfigLocalDTO();  
        
        public ConfigRemoteDTO Remote {get;set;} = new ConfigRemoteDTO(); 
    }
}