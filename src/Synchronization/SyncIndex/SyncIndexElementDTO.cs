using System; 
using System.Xml.Serialization; 
using WebDavSync.Enums; 

namespace WebDavSync.Synchronization.SyncIndex 
{
    public class SyncIndexElementDTO 
    {
        
        /// <summary>
        /// The Path to the File relative to the synchronisation 
        /// folder 
        /// </summary>
        /// <returns></returns>
        [XmlElement]
        public string ReleativeFilePath  {get;set;} = string.Empty; 

        /// <summary>
        /// The revision the File has remotely 
        /// -> Detection for change
        /// </summary>
        /// <returns></returns>
        [XmlElement]
        public string RemoteRevision {get;set;} = string.Empty; 

        /// <summary>
        /// The local revision of some kind helping to 
        /// identify wheter the file was changed or not 
        /// </summary>
        /// <returns></returns>
        [XmlElement]
        public string LocalRevision {get;set;} = string.Empty; 

        /// <summary>
        /// The Type of Element that is represented by this Element 
        /// </summary>
        /// <returns></returns>
        [XmlElement]
        public SyncElementType ElementType {get;set;}
    }
}