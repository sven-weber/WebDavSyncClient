using System; 

namespace WebDavSync.Config.DTO
{
    public class ConfigLocalDTO 
    {
        [ConfigLocalPathCanExist]
        [HasNoTailingSlash]
        [PathWithForwardSlash]
        public string LocalSyncDir {get;set;} = string.Empty; 
    }

}