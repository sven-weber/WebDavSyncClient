using System; 
using System.IO; 
using System.Threading; 
using WebDavSync.Log; 
using WebDavSync.Enums; 
using WebDavSync.Synchronization; 
using WebDavSync.ExtensionMethods; 

namespace WebDavSync.Synchronization.SyncElement 
{
    public class DirectorySyncElement : ISyncElement 
    {
        #region ------------------ Properties ------------------
        private DirectoryInfo _info; 

        private string _relativePath; 
        private ILogger _logger; 
        #endregion
        
        #region ------------------ Konstruktor ------------------

        public DirectorySyncElement(DirectoryInfo info, string rootDirPath, ILogger logger) 
        {
            _info = info; 
            _relativePath = info.FullName.Replace(rootDirPath, ""); 
            _logger = logger; 
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
        public SyncElementType Type => SyncElementType.Directory; 

        /// <summary>
        /// The Absolute FilePath for the SyncElement
        /// </summary>
        public string AbsolutePath => _info.FullName; 

        /// <summary>
        /// As it is easy to compare and restore last write 
        /// time in Ticks will be used as revision 
        /// </summary>
        public string Revision => _info.LastWriteTime.Ticks.ToString(); 
        
        /// <summary>
        /// Creates a Remote Directory at the relative Path of the dir and returns
        /// the Createes IRemoteProperty
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public IRemoteProperty Upload(CancellationToken cancleToken, ISyncClient client) 
        {
            return client.CreateDirectory(cancleToken, _relativePath); 
        }

        /// <summary>
        /// Deletes the local Directory 
        /// </summary>
        /// <returns></returns>
        public bool Delete()
        {
            try 
            {
                _info.Delete(true); 
                return true;
            } catch(Exception exc) 
            {
                _logger.Error("Error occured while deleting directory " + RelativePath, exc); 
                return false; 
            }
        }
        #endregion
    }
}