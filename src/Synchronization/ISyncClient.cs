using System; 
using System.IO; 
using System.Threading; 
using System.Collections.Generic; 

namespace WebDavSync.Synchronization  
{
    public interface ISyncClient : IDisposable
    {
        /// <summary>
        /// Creates a Directory at the given destination Uri and 
        /// returns wheter the creation was successfull
        /// </summary>
        /// <param name="dstUri"></param>
        /// <returns></returns>
        IRemoteProperty CreateDirectory(CancellationToken cancleToken, string dstUri); 

        /// <summary>
        /// Remove the Directory at the given destination Uri and 
        /// returns wheter the deletion was successfull
        /// </summary>
        /// <param name="dstUri"></param>
        /// <returns></returns>
        bool Remove(CancellationToken cancleToken, string dstUri); 

        /// <summary>
        /// Downloades the File at the given sourceUri into a Temp file in the
        /// local System.
        /// FileInfo for this Temp File will be returend 
        /// </summary>
        /// <param name="dstUri"></param>
        /// <returns></returns>
        FileInfo DownloadRemoteFileToTemp(CancellationToken cancleToken, string sourceUri); 

        /// <summary>
        /// Uploads the FileStream as File to the given resource
        /// </summary>
        /// <param name="dstUri"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        IRemoteProperty PutFile(CancellationToken cancleToken, string dstUri, FileStream source); 

        /// <summary>
        /// Moves remote files and returns wheter the moving was successfull
        /// </summary>
        /// <param name="sourceUri"></param>
        /// <param name="dstUri"></param>
        /// <returns></returns>
        IRemoteProperty MoveFile(CancellationToken cancleToken, string sourceUri, string dstUri, bool overwrite); 

        /// <summary>
        /// returns a List of availble remote Properties for the given source.
        /// This list determines what Resources can be found at the given Uri
        /// </summary>
        /// <param name="sourceUri"></param>
        /// <returns>A List of Remote properties or an empty List if nothing is found at the requested Uri</returns>
        IEnumerable<IRemoteProperty> GetProperties(string sourceUri); 
    }
}