using System; 
using System.Xml.Serialization; 
using System.Collections.Generic; 

namespace WebDavSync.Synchronization.SyncIndex
{
    public class SyncIndexDTO 
    {
        /// <summary>
        /// All Elements that are currently configured for syncing 
        /// </summary>
        /// <returns></returns>
        [XmlArray]
        public List<SyncIndexElementDTO> elements { get; set; } = new List<SyncIndexElementDTO>(); 
    }
}