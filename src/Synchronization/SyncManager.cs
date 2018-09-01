using System; 
using System.Xml.Linq; 
using System.IO; 
using System.Net; 
using System.Linq; 
using System.Threading; 
using System.Threading.Tasks; 
using System.Collections; 
using System.Collections.Generic;  
using WebDavSync.Log; 
using WebDavSync.Enums; 
using WebDavSync.Config; 
using WebDavSync.ReadWriteData; 
using WebDavSync.Synchronization.SyncIndex; 
using WebDavSync.Synchronization.SyncElement; 
using WebDavSync.ExtensionMethods; 

namespace WebDavSync.Synchronization
{
    public class SyncManager 
    {
        #region ------------------ Properties ------------------
        /// <summary>
        /// Logger instance used to produce the sync logs
        /// </summary>
        ILogger _logger; 
        #endregion

        #region ------------------ Konstruktor ------------------
        /// <summary>
        /// Creates a new SyncManager that is used to handle the overall synchronisation tasks. 
        /// Such as which file needs to be updated etc.
        /// This SyncManager can work with several ISyncClient implementation 
        /// that handle the concrete CRUD operations 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        public SyncManager(ILogger logger) 
        {
            _logger = logger; 
        }
        #endregion

        #region ------------------ public Methods ------------------    
        /// <summary>
        /// Executes the synchronisation for the given configuration and using the 
        /// given Client 
        /// This Method can be called multiple times using different configurations or client
        /// to achive different synchronisations 
        /// </summary>
        public void ExcecuteSynchronisation(CancellationToken cancleToken, Configuration config, ISyncClient client) 
        {
            _logger.Debug("Executing synchronisation for Directory " + config.Local.LocalSyncDir + " using client of type " + client.GetType()); 
            //Check if default dir extists, else delete
            if (!Directory.Exists(config.Local.LocalSyncDir)) 
            {
                _logger.Debug("Creating sync directory at " + config.Local.LocalSyncDir); 
                Directory.CreateDirectory(config.Local.LocalSyncDir); 
            }

            //Read or create the index File 
            _logger.Debug("Reading index File"); 
            SyncIndexManager indexManager = new SyncIndexManager(_logger, config); 

            //The following three steps are performed for a synchronisation: 
            //1. Iterate all local folder and their files, check for updates on server
            //2. Remove all files that exist in index and server but not local from server -> deleted local
            //3. Add all files that are not in index to local copy 
            //Start getting Files at the root Folder 
            GetDataAndHandleSync(cancleToken, config.Local.LocalSyncDir, client, _logger, config, indexManager); 

            if (cancleToken.IsCancellationRequested) return; 

            DetectLocalDeletedElements(cancleToken, config, client, _logger, indexManager); 

            if (cancleToken.IsCancellationRequested) return; 

            FetchNewFilesFromServer(cancleToken, config, client, _logger, indexManager);
            
            _logger.Debug("Synchronisation finished");  
        }

        #endregion

        #region ------------------ private Methods ------------------

        /// <summary>
        /// Iterates all remote files and check if they have been added localy
        /// These that have not been added yet will be downloaded and added to the index 
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        /// <param name="indexManager"></param>
        private void FetchNewFilesFromServer(CancellationToken cancleToken, Configuration dto, ISyncClient client, ILogger logger, SyncIndexManager indexManager) 
        {
            FetchFolderFromServer(cancleToken, "", client, dto,logger, indexManager); 
        }

        /// <summary>
        /// Fetches the given Folder and its content from the server
        /// </summary>
        private void FetchFolderFromServer(CancellationToken cancleToken, string relativePath, ISyncClient client, Configuration conf, ILogger logger, SyncIndexManager indexManager) 
        {
            if (cancleToken.IsCancellationRequested) return; 

            //Check if folder exists locally -> else create
            string localDirectoryPath = conf.Local.LocalSyncDir + relativePath; 
            try 
            {
                if (!Directory.Exists(localDirectoryPath)) 
                {
                    _logger.Debug("Folder " + localDirectoryPath + " does not exist locally. Creating folder and adding to the index."); 
                    Directory.CreateDirectory(localDirectoryPath); 

                    indexManager.AddOrUpdate(CreateIndexElement(new DirectorySyncElement(new DirectoryInfo(localDirectoryPath), conf.Local.LocalSyncDir, logger), null)); 
                }
            } catch(Exception exc) 
            {
                _logger.Error("A error occured while creating local folder", exc); 
            }

            //Get Properties and Process them
            IEnumerable<IRemoteProperty> remoteProperties = client.GetProperties(GetRemotePath(conf, localDirectoryPath));
            
            //Something went wrong during the connection to the server 
            if (remoteProperties == null) 
            {
                _logger.Error("Something went wrong during connection to server.");
                return; 
            }

            foreach(IRemoteProperty property in remoteProperties) 
            {
                if (cancleToken.IsCancellationRequested) return; 
                if (property.ElementType.Equals(SyncElementType.Directory)) 
                {
                    FetchFolderFromServer(cancleToken, property.DecodedRelativeRemotePath, client, conf, logger, indexManager); 
                } else 
                {
                    FetchFileFromServer(cancleToken, property, client, conf, logger, indexManager);
                }
            }
        }

        /// <summary>
        /// Downloaded the given RemotePropery and adds it as a local file 
        /// </summary>
        /// <param name="remoteProperty"></param>
        /// <param name="client"></param>
        /// <param name="conf"></param>
        /// <param name="logger"></param>
        /// <param name="indexManager"></param>
        private void FetchFileFromServer(CancellationToken cancleToken, IRemoteProperty remoteProperty, ISyncClient client, Configuration conf, ILogger logger, SyncIndexManager indexManager) 
        {
            try 
            {
                string localFilePath = conf.Local.LocalSyncDir + remoteProperty.DecodedRelativeRemotePath; 
                if (!File.Exists(localFilePath)) 
                {
                    //File does not exist local, will be downloaded and added
                    _logger.Debug(remoteProperty.DecodedRelativeRemotePath + " does not exist locally. Will be downloaded and added to index"); 
                    FileInfo temp = client.DownloadRemoteFileToTemp(cancleToken, remoteProperty.DecodedRelativeRemotePath); 
                    if (temp != null) 
                    {   
                        FileInfo newFile = FileManager.CopyFile(temp.FullName, localFilePath, true, _logger);
                        if (newFile != null) 
                        {
                            FileSyncElement newFileElement = new FileSyncElement(newFile, conf.Local.LocalSyncDir, logger); 
                            indexManager.AddOrUpdate(CreateIndexElement(newFileElement, remoteProperty)); 
                        }
                    }   
                }
            } catch(Exception exc) 
            {
                logger.Error("Unexpected error while fetching " + remoteProperty.RelativeRemotePath + " from server: ", exc); 
            }
        }

        /// <summary>
        /// Iterates trough all Index Elements and checks wheter the still exist 
        /// If not they have been deleted locally and will be removed on the server
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        /// <param name="indexManager"></param>
        private void DetectLocalDeletedElements(CancellationToken cancleToken, Configuration dto, ISyncClient client, ILogger logger, SyncIndexManager indexManager) 
        {
            IEnumerable<SyncIndexElementDTO> indexElements = indexManager.GetOrCreateIndexFile(); 
            List<SyncIndexElementDTO> removeElements = new List<SyncIndexElementDTO>(); 
            foreach(SyncIndexElementDTO element in indexElements) 
            {
                if (cancleToken.IsCancellationRequested) return; 

                bool exists = true; 
                string absolutePath = dto.Local.LocalSyncDir + element.ReleativeFilePath; 
                try 
                {
                    if (element.ElementType.Equals(SyncElementType.File)) 
                    {
                        exists = File.Exists(absolutePath); 
                    } else if (element.ElementType.Equals(SyncElementType.Directory)) 
                    {
                        exists = Directory.Exists(absolutePath); 
                    }                        
                } catch(Exception exc) {_logger.Error("Error occured while checking " + element.ElementType + "existance: ", exc); }
                if (!exists) 
                {
                    _logger.Debug(element.ElementType + " " + element.ReleativeFilePath + " does not exist anymore. Will be deleted from server and index."); 
                    removeElements.Add(element); 
                }
            }

            //Get List of Directories that are contained in the removeElements list
            List<SyncIndexElementDTO> directories = removeElements.Where(x => x.ElementType == SyncElementType.Directory).ToList(); 
            foreach(SyncIndexElementDTO element in directories) 
            {
                //remove all Elements that are contained in removeElements list and 
                //are part of this Directory
                removeElements.RemoveAll(x => x.ReleativeFilePath.Length > element.ReleativeFilePath.Length && 
                                              x.ReleativeFilePath.Substring(0, element.ReleativeFilePath.Length) == element.ReleativeFilePath); 
            }

            //Remove all elements that can be removed
            foreach(SyncIndexElementDTO element in removeElements) 
            {
                if (client.Remove(cancleToken, element.ReleativeFilePath))
                {
                    //If its a Directory delete all Elements that are contained in this Directory
                    //From the index!
                    if (element.ElementType == SyncElementType.Directory) 
                    {
                        indexManager.RemoveAll(x => x.ReleativeFilePath.Length >= element.ReleativeFilePath.Length &&
                                               x.ReleativeFilePath.Substring(0, element.ReleativeFilePath.Length) == element.ReleativeFilePath); 
                    } else 
                    {
                        indexManager.Remove(element); 
                    }
                }
            }
        }

        /// <summary>
        /// Handles the synchronisation of the given ISyncElement 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="client"></param>
        /// <param name="properties"></param>
        /// <param name="indexManager"></param>
        private void HandleISyncElement(CancellationToken cancleToken, ISyncElement element, ISyncClient client, IEnumerable<IRemoteProperty> properties, 
                                        SyncIndexManager indexManager, ILogger logger, Configuration config) 
        {
            if (cancleToken.IsCancellationRequested) return; 
            //The general rules defining when to synchronize what are the same 
            //for all ISyncClients and they follow the following design: 
            //
            //1. Iterate all local folder and their files, check for updates on server:
            //  Both: 
            //      1. when it does not exist in index and server -> upload
            //      2. when it exists only in index -> delete (has been deleted remotely) 
            //      3. when exists on server but not index -> update index  
            //  Files: 
            //      1. When is exists in server and index -> check for updates 
            //2. Add all files that are not in index to local copy 
            //3. Remove all files that exist in index and server but not local from server -> deleted local

            (bool existInIndex, bool existsRemote) existance = CheckForElementExistance(element, properties, indexManager); 
            
            if (!existance.existInIndex && !existance.existsRemote) 
            {
                //Not in index and not in remote -> upload 
                logger.Debug(element.Type + " " + element.RelativePath + " does not exist in local Index and remote, " +
                              "File will be added to both"); 
                Upload(cancleToken, element, client, indexManager); 
                //Set the Method to ignore Updates -> Just handle children in case of Directory
                //Because directly checking for updates may cause exceptions because the server is 
                //slow and does not return the correct properties on request yet 
                HandleISyncElementChildrenOrUpdates(cancleToken, element, client, properties, indexManager, logger, config, true); 
            } else if (existance.existInIndex && !existance.existsRemote) 
            {
                //In index but not remote -> delete (has been deleted remotely)
                logger.Debug(element.Type + " " + element.RelativePath + " exists in index but has been removed remote. Deleting " + element.Type); 
                DeleteLocalElement(element, indexManager); 
            } else if (!existance.existInIndex && existance.existsRemote) 
            {
                //Remote but not in index -> Add to index 
                logger.Debug(element.Type + " " + element.RelativePath + " was missing in the index -> will be added."); 
                //As it cannot be said if there was changes remote, no remote revision is added 
                indexManager.AddOrUpdate(CreateIndexElement(element, null)); 
                HandleISyncElementChildrenOrUpdates(cancleToken, element, client, properties, indexManager, logger, config, false); 
            } else if (existance.existInIndex && existance.existsRemote) 
            {
                HandleISyncElementChildrenOrUpdates(cancleToken, element, client, properties, indexManager, logger, config, false); 
            }
        }
    
        /// <summary>
        /// Checks for the given elements children or remote updates, has the option to only check the children and ignore updates in case
        /// Can be usefull to give the server enough time to index new files 
        /// </summary>
        /// <param name="cancleToken"></param>
        /// <param name="element"></param>
        /// <param name="client"></param>
        /// <param name="properties"></param>
        /// <param name="indexManager"></param>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        /// <param name="ignoreUpdates"></param>
        private void HandleISyncElementChildrenOrUpdates(CancellationToken cancleToken, ISyncElement element, ISyncClient client, 
                                                         IEnumerable<IRemoteProperty> properties, SyncIndexManager indexManager, ILogger logger, 
                                                         Configuration config, bool ignoreUpdates) 
        {
            //File: check for updates 
            //Directoy: process with going one hierarchy deeper
            if (element.Type.Equals(SyncElementType.File)) 
            {
                if (!ignoreUpdates) 
                {
                    CheckForFileUpdates(cancleToken, (FileSyncElement)element, client, properties, indexManager); 
                }
            } else if (element.Type.Equals(SyncElementType.Directory)) 
            {   
                GetDataAndHandleSync(cancleToken, element.AbsolutePath, client, logger, config, indexManager); 
            } else 
            {
                throw new NotImplementedException(); 
            }
        }

        /// <summary>
        /// Checks wheter 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="client"></param>
        /// <param name="properties"></param>
        /// <param name="indexManager"></param>
        private void CheckForFileUpdates(CancellationToken cancleToken, FileSyncElement file, ISyncClient client, IEnumerable<IRemoteProperty> properties, SyncIndexManager indexManager) 
        {
            //Get Index and property instances
            SyncIndexElementDTO index = GetIndexElement(indexManager, file); 
            IRemoteProperty property = GetProperty(properties, file); 

            //Compare the change dates
            if (!index.LocalRevision.Equals(file.Revision) && index.RemoteRevision.Equals(property.RemoteRevision)) 
            {
                //File has been changed locally
                _logger.Debug("File " + file.RelativePath + " has been changed locally. Index rev: " + index.LocalRevision + " current file rev: " + file.Revision); 
                Upload(cancleToken, file, client, indexManager); 
            } else if (index.LocalRevision.Equals(file.Revision) && !index.RemoteRevision.Equals(property.RemoteRevision))
            {
                //File has been changed remotely 
                _logger.Debug("File " + file.RelativePath + " has been changed remotely. Index remote rev: " + index.RemoteRevision + " current remote rev: " + property.RemoteRevision); 
                PatchLocalFile(cancleToken, file, client, indexManager, properties); 
            } else if (!index.LocalRevision.Equals(file.Revision) && !index.RemoteRevision.Equals(property.RemoteRevision))
            {
                //Conflict! Remote and server version has been changed!
                //Priorise server version, patch Local file 
                _logger.Debug("File " + file.RelativePath + " has been changed remote and locally. Will fetch server version over local version"); 
                _logger.Debug("File " + file.RelativePath + " local index local rev: " + index.LocalRevision + " file local rev: " + file.Revision); 
                _logger.Debug("File " + file.RelativePath + " remote index rev: " + index.RemoteRevision + " file remote rev: " + property.RemoteRevision); 
                PatchLocalFile(cancleToken, file, client, indexManager, properties); 
            }

        }

        /// <summary>
        /// Gets all Files and Directories at the given Path and handles their synchronisation 
        /// </summary>
        /// <param name="absolutePath"></param>
        private void GetDataAndHandleSync(CancellationToken cancleToken, string absolutePath, ISyncClient client, 
                                          ILogger logger, Configuration config, SyncIndexManager indexManager) 
        {
            //Get the remote Properties, Files and Folder of the root folder
            IEnumerable<IRemoteProperty> remoteProperties = client.GetProperties(GetRemotePath(config, absolutePath)); 
            IEnumerable<FileSyncElement> files = GetFiles(absolutePath, config, logger); 
            IEnumerable<DirectorySyncElement> directories = GetDirectories(absolutePath, config, logger);

            //If there is no connection or something went wrong during request 
            if (remoteProperties == null)
            {
                _logger.Error("Something went wrong during connection to server.");
                return;  
            } 
            //First the Directories -> ensure they are not deleted before processing everything else
            foreach(DirectorySyncElement dir in directories) 
            {
                if (cancleToken.IsCancellationRequested) return; 
                HandleISyncElement(cancleToken, dir, client, remoteProperties, indexManager, logger, config); 
            }
            foreach(FileSyncElement file in files) 
            {
                if (cancleToken.IsCancellationRequested) return; 
                HandleISyncElement(cancleToken, file, client, remoteProperties, indexManager,logger, config); 
            }
        }

        /// <summary>
        /// Returns an absolute remote Path from the absolute local Path
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="absoluteLocalPath"></param>
        /// <returns></returns>
        private string GetRemotePath(Configuration dto, string absoluteLocalPath) 
        {
            return dto.Remote.RemoteFolderPath + absoluteLocalPath.Replace(dto.Local.LocalSyncDir, ""); 
        }

        /// <summary>
        /// Patches the local file with the current File version provided by the ISyncClient 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="client"></param>
        /// <param name="indexManager"></param>
        private void PatchLocalFile(CancellationToken cancleToken, FileSyncElement file, ISyncClient client, SyncIndexManager indexManager, 
                                    IEnumerable<IRemoteProperty> remoteProperties) 
        {
            //File has been changed remotely
            FileInfo update = file.PatchLocalFile(cancleToken, client); 
            if (update != null) 
            {
                //Updte Index Manager
                indexManager.AddOrUpdate(CreateIndexElement(file, GetProperty(remoteProperties, file))); 
            }
        }

        /// <summary>
        /// Uploads the given ISync Element using the ISyncClient
        /// If success the Element will be added or an existing element will be updated with 
        /// the new values 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="client"></param>
        /// <param name="indexManager"></param>
        private void Upload(CancellationToken cancleToken, ISyncElement element, ISyncClient client, SyncIndexManager indexManager) 
        {
            IRemoteProperty uploaded = element.Upload(cancleToken, client); 
            if (uploaded != null) 
            {
                //Upload was successfull, add as tracked element to index
                indexManager.AddOrUpdate(CreateIndexElement(element, uploaded)); 
            }
        }

        /// <summary>
        /// Deletes the given ISyncElement and removes it from index if successfull
        /// </summary>
        /// <param name="element"></param>
        /// <param name="indexManager"></param>
        private void DeleteLocalElement(ISyncElement element, SyncIndexManager indexManager) 
        {
            if (element.Delete()) 
            {
                SyncIndexElementDTO dto = GetIndexElement(indexManager, element); 
                if (element.Type == SyncElementType.Directory) 
                {
                    //If a Directory was removed, remove all sub Element that 
                    //have been contained in that directory 
                    indexManager.RemoveAll(x => x.ReleativeFilePath.Length >= dto.ReleativeFilePath.Length &&
                                           x.ReleativeFilePath.Substring(0, dto.ReleativeFilePath.Length) == dto.ReleativeFilePath); 
                } else 
                {
                    indexManager.Remove(dto); 
                }
            }
        }

        /// <summary>
        /// Creates a SyncIndex Element fromt the given local and remote Elements
        /// </summary>
        /// <param name="localElement"></param>
        /// <param name="remoteElement"></param>
        /// <returns></returns>
        private SyncIndexElementDTO CreateIndexElement(ISyncElement localElement, IRemoteProperty remoteElement) 
        {
            return new SyncIndexElementDTO() 
            {
                ReleativeFilePath = localElement.RelativePath, 
                RemoteRevision = remoteElement == null ? "" : remoteElement.RemoteRevision, 
                LocalRevision = localElement.Revision, 
                ElementType = localElement.Type
            }; 
        }

        /// <summary>
        /// Checks wheter the resource exists in the index and RemoteProperties
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private (bool existInIndex, bool existsRemote) CheckForElementExistance(ISyncElement element, IEnumerable<IRemoteProperty> remoteProperties, 
                                                                                SyncIndexManager manager) 
        {
            bool exstIndex = GetIndexElement(manager, element) != null; 
            bool exstRemote = GetProperty(remoteProperties, element) != null; 
            return (exstIndex, exstRemote); 
        }

        /// <summary>
        /// Returns the IndexElement matching the given ISyncElement 
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        private SyncIndexElementDTO GetIndexElement(SyncIndexManager manager, ISyncElement element) 
        {
            IEnumerable<SyncIndexElementDTO> elements = manager.GetOrCreateIndexFile(); 
            if (elements != null) 
            {
                return elements.Where(x => x.ElementType == element.Type && 
                                                        x.ReleativeFilePath == element.RelativePath
                                                       ).FirstOrDefault(); 
            }
            return null; 
        }


        /// <summary>
        /// Returns the Remote Property that matching the given ISyncElement 
        /// </summary>
        /// <param name="remoteProperties"></param>
        /// <param name="element"></param>
        /// <returns>Matching remote property or null if not found</returns>
        private IRemoteProperty GetProperty(IEnumerable<IRemoteProperty> remoteProperties, ISyncElement element) 
        {
            //Remote properties seem to have a slash more at the beginning 
            return remoteProperties.Where(x => x.DecodedRelativeRemotePath.PathsEqual(element.RelativePath) && 
                                          x.ElementType == element.Type).FirstOrDefault(); 
        }

        /// <summary>
        /// Returns and IEnumerable of DirectoySyncElement objects that should be found in 
        /// the Top of the given Path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private IEnumerable<DirectorySyncElement> GetDirectories(string path, Configuration conf, ILogger logger) 
        {
            List<DirectoryInfo> dirs = new DirectoryInfo(path).GetDirectories("*", SearchOption.TopDirectoryOnly).ToList(); 
            dirs.RemoveAll(x => x.Attributes.HasFlag(FileAttributes.Hidden)); 
            return dirs.ConvertAll(x => new DirectorySyncElement(x, conf.Local.LocalSyncDir, logger)); 
        }

        /// <summary>
        /// Returns an IEnumerable of FileSyncElement that are contained in the 
        /// given Directory Path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="conf"></param>
        /// <returns></returns>
        private IEnumerable<FileSyncElement> GetFiles(string path, Configuration conf, ILogger logger) 
        {
            //Get all files, remove hidden and cast
            List<FileInfo> files = new DirectoryInfo(path).GetFiles("*", SearchOption.TopDirectoryOnly).ToList(); 
            files.RemoveAll(x => x.Attributes.HasFlag(FileAttributes.Hidden)); 
            return files.ConvertAll(x => new FileSyncElement(x, conf.Local.LocalSyncDir, logger)); 
        }
        #endregion
    }
}