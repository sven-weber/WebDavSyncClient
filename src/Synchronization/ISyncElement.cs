using System; 
using System.Threading; 
using WebDavSync.Enums; 

namespace WebDavSync.Synchronization 
{
    public interface ISyncElement  
    {
        
        /// <summary>
        /// The relative Path of the Element 
        /// -> Relative to the root sync dir 
        /// </summary>
        /// <returns></returns>
        string RelativePath { get; }

        /// <summary>
        /// The Type of element that is Synced with this instance
        /// </summary>
        /// <returns></returns>
        SyncElementType Type { get ;}

        /// <summary>
        /// The absolute local path of the Element
        /// </summary>
        /// <returns></returns>
        string AbsolutePath { get; }

        /// <summary>
        /// Returns a string holding the revision of the element 
        /// </summary>
        /// <returns></returns>
        string Revision {get;}

        /// <summary>
        /// Methods that is used to upload the specific ISyncElement to 
        /// A server using ISyncClient, the created/uploaded Remote Property will 
        /// be returned
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        IRemoteProperty Upload(CancellationToken cancleToken, ISyncClient client);

        /// <summary>
        /// Deletes the specific ISync Element 
        /// </summary>
        /// <returns></returns>
        bool Delete(); 
    }
}