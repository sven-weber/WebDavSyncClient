using System; 
using System.Net; 
using Newtonsoft.Json; 

namespace WebDavSync.Config.DTO
{
    public class ConfigRemoteDTO 
    {
        [ConfigPathAbsolute]
        [HasTailingForwardSlash]
        [DoesNotStartWithSlash]
        [PathWithForwardSlash]
        public string RemoteServerPath { get; set; } = string.Empty; 
        
        [ConfigPathRelative]
        [HasTailingForwardSlash]
        [DoesNotStartWithSlash]
        [PathWithForwardSlash]
        public string RemoteFolderPath { get; set; } = string.Empty; 

        [ConfigNotNullOrEmpty]        
        public string UserName { get; set; } = string.Empty; 

        public string _commentSecurePassword = "Encrypted password created by the application. In order to set or change the password please run the app with -reconfigure parameter."; 
        [ConfigNotNullOrEmpty]    
        public string SecurePassword { get; set; } = string.Empty; 
    }
}