using System; 
using System.IO; 
using System.Threading; 
using WebDavSync.Log; 
using WebDavSync.Enums; 
using WebDavSync.ReadWriteData; 
using WebDavSync.Synchronization; 
using WebDavSync.ExtensionMethods; 

namespace WebDavSync.Synchronization.SyncElement 
{
    public class FileSyncElement : ISyncElement 
    {
        #region ------------------ Properties ------------------
        private FileInfo _info; 

        private string _relativePath; 
        private ILogger _logger; 
        private string _rootDirPath; 
        #endregion
        
        #region ------------------ Konstruktor ------------------
        /// <summary>
        /// Creates a new FileSyncElement that could be used to perform file Synchronisation 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="rootDirPath"></param>
        /// <param name="logger"></param>
        public FileSyncElement(FileInfo info, string rootDirPath, ILogger logger) 
        {
            _info = info; 
            _relativePath = info.FullName.Replace(rootDirPath, ""); 
            _logger = logger; 
            _rootDirPath = rootDirPath; 
        }
        #endregion
        
        #region ------------------ Methods ------------------

        /// <summary>
        /// Sync Elements Path relative from the root Dir 
        /// </summary>
        public string RelativePath => _relativePath;

        /// <summary>
        /// returns the SyncElement Type of the FileSync Element 
        /// </summary>
        public SyncElementType Type => SyncElementType.File; 

        /// <summary>
        /// The Absolute FilePath for the SyncElement
        /// </summary>
        public string AbsolutePath => _info.FullName; 

        /// <summary>
        /// For files the Last Write Time as tick value will be used for revision 
        /// -> Easy to restore and compare 
        /// </summary>
        /// <returns></returns>
        public string Revision => _info.LastWriteTime.Ticks.ToString(); 

        /// <summary>
        /// Uploads the File of the FileSync element ot the 
        /// Server using the provided ISync Client and returns the created or updated
        /// IRemote Property
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public IRemoteProperty Upload(CancellationToken cancleToken, ISyncClient client) 
        {
            try 
            {
                using (FileStream stream = _info.OpenRead()) 
                {
                    return client.PutFile(cancleToken, _relativePath, stream); 
                }
            } catch(Exception exc) 
            {
                _logger.Error("Error occured while uploading file " + RelativePath, exc); 
                return null; 
            }
        }

        /// <summary>
        /// Deletes the File and returns wheter this was successfull
        /// </summary>
        /// <returns></returns>
        public bool Delete() 
        {
            try 
            {
                _info.Delete(); 
                return true; 
            } catch(Exception exc) 
            {
                _logger.Error("Error occured while Deleting file" + _relativePath, exc); 
                return false; 
            }
        }

        /// <summary>
        /// Downloads the File from the ISync client and 
        /// patches the current local file with the downloaded version
        /// or creates new file 
        /// </summary>
        /// <returns>An IFileInfo property continaing information about the overwritten File, or null if download failed</returns>
        public FileInfo PatchLocalFile(CancellationToken cancleToken, ISyncClient client) 
        {
            //Get Temp File from Remote
            _logger.Debug("Patching " + _relativePath + " with version from server"); 
            FileInfo tempFile = client.DownloadRemoteFileToTemp(cancleToken, _relativePath.UrlDecode()); 
            if (tempFile != null) 
            {   
                if (cancleToken.IsCancellationRequested) return null; 
               _info = FileManager.CopyFile(tempFile.FullName, _rootDirPath + _relativePath, true, _logger);
               return _info;  
            }
            return null; 
        }
        #endregion
    }
}