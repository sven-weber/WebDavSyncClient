using System; 
using WebDavSync.Enums; 

namespace WebDavSync.Synchronization 
{
    public interface IRemoteProperty
    { 
        SyncElementType ElementType { get; }
        
        /// <summary>
        /// Relative Remote Path that was http decoded and has the right chars for e.g. spacing etc.
        /// </summary>
        /// <value></value>
        string DecodedRelativeRemotePath {get;}

        string RelativeRemotePath { get; }

        string RemoteRevision { get; }
    }
}   