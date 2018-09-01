using System; 
using System.Xml.Linq; 
using System.IO; 
using System.Net; 
using System.Linq; 
using System.Security; 
using System.Collections; 
using System.Collections.Generic;  
using System.Threading; 
using System.Threading.Tasks; 
using WebDav;
using WebDavSync.Log; 
using WebDavSync.Config; 
using WebDavSync.Synchronization; 
using WebDavSync.ExtensionMethods; 
using WebDavSync.Synchronization.SyncIndex; 

namespace WebDavSync.Synchronization.WebDav 
{
    public class WebDavSyncClient : ISyncClient
    {
        #region ------------------ Properties ------------------ 
        ILogger _logger; 

        /// <summary>
        /// Config used for the Synchronization 
        /// </summary>
        private Configuration _config; 

        /// <summary>
        /// The LockManager used for Locking remote webdav resources 
        /// </summary>
        private WebDavLockManager _lockManager;  

        /// <summary>
        /// The WebDavClient used to perform the requests in WebDav Format 
        /// </summary>
        private WebDavClient _webDavClient; 
        #endregion

        #region ------------------ Konstruktor ------------------ 
        public WebDavSyncClient(ILogger logger, Configuration config)
        {
            _logger = logger; 
            _config = config; 
            _lockManager = new WebDavLockManager(logger); 

            WebDavClientParams clientParams = new WebDavClientParams()
            {
                //Credentials = config.Remote.Credentials,
                Credentials = config.Remote.Credentials,
                BaseAddress = new Uri(_config.Remote.RemoteServerPath)
            };  
            _webDavClient = new WebDavClient(clientParams); 
        } 
        #endregion
        
        #region ------------------ Interface implementation ------------------ 
        /// <summary>
        /// Creates a Directory at the given destination Uri and 
        /// returns wheter the creation was successfull
        /// </summary>
        /// <param name="dstUri"></param>
        /// <returns></returns>
        public IRemoteProperty CreateDirectory(CancellationToken cancleToken, string dstUri) 
        {
            dstUri = _config.Remote.RemoteFolderPath.RemoveTailingSlashes() + dstUri; 
            try 
            {
                _logger.Debug("Creating remote directory " + dstUri); 
                WebDavResponse res = _webDavClient.Mkcol(dstUri, new MkColParameters() 
                {
                    CancellationToken = cancleToken, 
                }).Result; 
                if (res.IsSuccessful) 
                {
                    _logger.Debug("Creating remote directory " + dstUri + " was successfull"); 
                    return GetProperty(dstUri, _webDavClient); 
                } else 
                {
                    _logger.Error("Creating remote directory " + dstUri + " failed!"); 
                }
            } catch(Exception exc) 
            {
                _logger.Error("Unhandled exception occured while creation remote directory: " + dstUri, exc); 
            }
            return null; 
        }

        /// <summary>
        /// Remove the Directory at the given destination Uri and 
        /// returns wheter the deletion was successfull
        /// </summary>
        /// <param name="dstUri"></param>
        /// <returns></returns>
        public bool Remove(CancellationToken cancleToken, string dstUri) 
        {
            dstUri = _config.Remote.RemoteFolderPath.RemoveTailingSlashes() + dstUri; 
            try 
            {
                _logger.Debug("Deleting remote resource " + dstUri); 
                WebDavResponse res = _webDavClient.Delete(dstUri, new DeleteParameters() 
                {
                    CancellationToken = cancleToken,
                }).Result; 
                if (res.IsSuccessful) 
                {
                    _logger.Debug("Deleting remote resource " + dstUri + " was successfull"); 
                    return true;
                } else 
                {
                    _logger.Error("Deleting resource" + dstUri + " failed!"); 
                }
            } catch(Exception exc) 
            {
                _logger.Error("Unhandled exception occured while deleting remote resource: " + dstUri, exc); 
            }
            return false; 
        }

        /// <summary>
        /// Downloades the File at the given sourceUri into a Temp file in the
        /// local System.
        /// FileInfo for this Temp File will be returend 
        /// </summary>
        /// <param name="dstUri"></param>
        /// <returns></returns>
        public FileInfo DownloadRemoteFileToTemp(CancellationToken cancleToken, string sourceUri) 
        {
            sourceUri = _config.Remote.RemoteFolderPath.RemoveTailingSlashes() + sourceUri; 
            //No locks needed at this time because we will only read the File!
            try 
            {
                using (Stream webstream = _webDavClient.GetProcessedFile(sourceUri).Result.Stream) 
                {
                    //Writing the WebStream to the new generated file 
                    string tempFile = string.Format("/tmp/WebDavSync{0}.tmp", Guid.NewGuid().ToString()); 
                    _logger.Debug("Downloading remote file " + sourceUri + " to temp dst " + tempFile); 
                    bool wasCancled = false;
                    using (var filestream = new FileStream(tempFile, FileMode.CreateNew, FileAccess.ReadWrite, 
                                                           FileShare.None))
                    {
                        byte[] buffer = new byte[5120]; // read in chunks of 5KB
                        int bytesRead;
                        //As long as we can ready data, write the Data to the 
                        //new File 
                        while((bytesRead = webstream.Read(buffer, 0, buffer.Length)) > 0 && !wasCancled)
                        {
                            if (cancleToken.IsCancellationRequested) 
                            {
                                wasCancled = true; 
                            } else 
                            {
                                filestream.Write(buffer, 0, bytesRead);
                            }
                        }
                        if (!wasCancled) _logger.Debug("Download successful"); 
                        if (wasCancled) _logger.Debug("Download was cancled"); 
                    }
                    FileInfo tmpFile = new FileInfo(tempFile); 
                    //Cleanup in case cancleation is requested
                    if (wasCancled) 
                    {
                        tmpFile.Delete(); 
                        return null; 
                    }
                    
                    return tmpFile; 
                } 
            } catch (Exception exc)
            {
                _logger.Error("Error while downloading remote File as Temp file", exc); 
                return null; 
            }
        }

        /// <summary>
        /// Uploads the FileStream as File to the given resource
        /// </summary>
        /// <param name="dstUri"></param>
        /// <param name="source"></param>
        /// <returns>The new RemoteProperty of the Put File or null if this was not possible</returns>
        public IRemoteProperty PutFile(CancellationToken cancleToken, string dstUri, FileStream source) 
        {
            dstUri = _config.Remote.RemoteFolderPath.RemoveTailingSlashes() + dstUri; 
            WebDavResponse response = null; 
            try 
            {
                _logger.Debug("Uploading local file to dst " + dstUri); 
                response = _webDavClient.PutFile(dstUri, source, new PutFileParameters() 
                {
                    CancellationToken = cancleToken, 
                }).Result; 
                if (response.IsSuccessful) 
                {
                    return GetProperty(dstUri, _webDavClient); 
                } else 
                {
                    _logger.Error("Uploading file " + dstUri + " failed!"); 
                }
            } catch(Exception exc) 
            {
                _logger.Error("Error while uploading file: ", exc); 
            }
            return null; 
        }  

        /// <summary>
        /// Moves remote files and returns wheter the moving was successfull
        /// </summary>
        /// <param name="sourceUri"></param>
        /// <param name="dstUri"></param>
        /// <returns>The RemoteProperty of the new dstUri File or null if something went wrong</returns>
        public IRemoteProperty MoveFile(CancellationToken cancleToken, string sourceUri, string dstUri, bool overwrite) 
        {
            string lockToken = _lockManager.LockResource(_webDavClient, dstUri, LockScope.Exclusive); 
            string tempLockToken = _lockManager.LockResource(_webDavClient, sourceUri, LockScope.Exclusive);
            _logger.Debug("Moving remote file " + sourceUri + " to new destination " + dstUri); 
            //Cannot get Exclusive Token, retry the sync at a later time 
            if (lockToken == null|| tempLockToken == null) return null; 
            try 
            {
                WebDavResponse move = _webDavClient.Move(sourceUri, dstUri, new MoveParameters() {
                    Overwrite = overwrite, 
                    DestLockToken = lockToken, 
                    SourceLockToken = tempLockToken, 
                    CancellationToken = cancleToken
                }).Result; 

                if (move != null && move.IsSuccessful) 
                {
                    return GetProperty(dstUri, _webDavClient); 
                } else 
                {
                    _logger.Error("Error occured while moving " + sourceUri + " to " + dstUri); 
                }
            } catch (Exception exc) 
            {
                _logger.Error("Error while moving temp file to its destination address: ", exc); 
            } finally 
            {
                _lockManager.UnlockResource(_webDavClient, dstUri, lockToken); 
                _lockManager.UnlockResource(_webDavClient, sourceUri, tempLockToken); 
            }
            return null; 
        }  

        /// <summary>
        /// returns a List of availble remote Properties for the given source.
        /// This list determines what Resources can be found at the given Uri
        /// </summary>
        /// <param name="sourceUri"></param>
        /// <returns>A List of Remote properties or an empty List if nothing is found at the requested Uri</returns>
        public IEnumerable<IRemoteProperty> GetProperties(string sourceUri) 
        {
            PropfindResponse properties = FindProperties(_webDavClient, sourceUri, true).Result; 
            if (properties != null && properties.IsSuccessful) 
            {
                return properties.Resources.ToList().ConvertAll(x => new WebDavRemoteProperty(x, _config.Remote.RemoteFolderPath)); 
            }
            return null; 
        }

        //Releases all unmanaged resources used in the WebDavSyncClient 
        public void Dispose() 
        {
            _webDavClient?.Dispose(); 
        }
        #endregion

        #region ------------------ private Methods ------------------ 
        /// <summary>
        /// Try to get the Property of the given dstUri
        /// </summary>
        /// <param name="dstUri"></param>
        /// <param name="client"></param>
        /// <returns>The Found Property as WebDavRemoteProperty or null if an error occured</returns>
        private WebDavRemoteProperty GetProperty(string dstUri, WebDavClient client) 
        {
            PropfindResponse result = FindProperties(client, dstUri, false).Result; 
            if (result != null && result.IsSuccessful) 
            {
                return new WebDavRemoteProperty(result.Resources.FirstOrDefault(), 
                                                _config.Remote.RemoteFolderPath); 
            } else 
            {
                _logger.Error("Could not get Properties from destination " + dstUri); 
            }
            return null; 
        }

        /// <summary>
        /// Trys to find the Properties of the given path, 
        ///Returns null if not possible 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="url"></param>
        /// <param name="removeRootUrlElement">Sets wheter the root url element, e.g. path /folder the folder element, will be removed</param>
        /// <returns></returns>
        private async Task<PropfindResponse> FindProperties(WebDavClient client, string url, bool removeRootUrlElement) 
        {
            PropfindResponse response; 
            try 
            {
                response = await client.Propfind(url); 
            } catch(Exception exc){
                _logger.Error("Failed to find properties from server: ", exc); 
                return null; 
            }
            
            //Cut of first Element from the Resource list as it is the element at the 
            //Path itself
            List<WebDavResource> ressources = new List<WebDavResource>(response.Resources); 

            if (removeRootUrlElement && response.IsSuccessful && response.Resources.Count > 0) 
            {
                ressources.RemoveAt(0); 
            }

            return new PropfindResponse(response.StatusCode, response.Description,ressources); 
        }
        #endregion
    }
}