using System; 
using System.IO;
using System.Linq; 
using System.Collections.Generic; 
using System.Runtime.InteropServices; 
using WebDavSync.Log;  
using WebDavSync.Config; 
using WebDavSync.ReadWriteData; 


namespace WebDavSync.Synchronization.SyncIndex 
{
    public class SyncIndexManager 
    {

        #region ------------------ Properties ------------------
        private Configuration _config; 
        private string _indexFileName; 
        private JsonSerializer<SyncIndexDTO> _serilizer; 
        private ILogger _logger; 
        private SyncIndexDTO _indexBuffer; 
        #endregion
        #region ------------------ Konstruktor ------------------
        public SyncIndexManager(ILogger logger, Configuration config) 
        {
            _config = config; 
            _indexFileName = "syncindex"; 
            _logger = logger; 
            _serilizer = new JsonSerializer<SyncIndexDTO>(); 
            _indexBuffer = null; 
        }
        #endregion

        #region ------------------ public Methods ------------------

        /// <summary>
        /// Returns a deserialized DTO instance when file allready exists
        /// or creates empty File if not 
        /// </summary>
        /// <returns>Index file content as SyncIndexDTO or null if something went wrong</returns>
        public IEnumerable<SyncIndexElementDTO> GetOrCreateIndexFile() 
        {
            if (_indexBuffer == null) 
            {
                _indexBuffer = GetIndexBuffer(); 
            }
            return _indexBuffer?.elements; 
        }

        /// <summary>
        /// Adds new IndexElementDTO to the _index Buffer
        /// </summary>
        /// <param name="element"></param>
        public void AddOrUpdate(SyncIndexElementDTO element) 
        {
            SyncIndexElementDTO foundElement = FindElement(element); 
            if (foundElement == null) 
            {
                _logger.Debug("Adding new element " + element.ReleativeFilePath + " to index"); 
                _indexBuffer.elements.Add(element);
            } else
            {
                _logger.Debug("Updating element " + element.ReleativeFilePath + " in index"); 
                foundElement.LocalRevision = element.LocalRevision; 
                foundElement.RemoteRevision = element.RemoteRevision; 
            }
            UpdateFile(_indexBuffer); 
        }

        /// <summary>
        /// Removes the given Element from the List of SyncIndexElement elements 
        /// if the element could be found 
        /// </summary>
        /// <param name="element"></param>
        public void Remove(SyncIndexElementDTO element) 
        {
            SyncIndexElementDTO foundElement = FindElement(element); 
            if (foundElement!= null) 
            {
                _indexBuffer.elements.Remove(foundElement); 
                _logger.Debug("Removing " + element.ReleativeFilePath + " from index file"); 
                UpdateFile(_indexBuffer); 
            }
        }

        /// <summary>
        /// Removes all SyncIndexElementDTOs that match the given predicate
        /// </summary>
        /// <param name="predicate"></param>
        public void RemoveAll(Func<SyncIndexElementDTO, bool> match) 
        {
            SyncIndexElementDTO[] foundElements = _indexBuffer.elements.Where(match).ToArray(); 

            if (foundElements != null && foundElements.Count() > 0) 
            {
                for(int i = 0; i <foundElements.Length; i++) 
                {
                    _indexBuffer.elements.Remove(foundElements[i]); 
                    _logger.Debug("Removing " + foundElements[i].ReleativeFilePath + " from index file"); 
                }
                UpdateFile(_indexBuffer); 
            }
        }

        #endregion

        #region ------------------ private Methods ------------------
        /// <summary>
        /// returns the SyncIndex element matching given element is possible 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private SyncIndexElementDTO FindElement(SyncIndexElementDTO element) 
        {
            return _indexBuffer.elements.Where(x => x.ElementType == element.ElementType 
                                               && x.ReleativeFilePath == element.ReleativeFilePath
                                              ).FirstOrDefault(); 
        }

        /// <summary>
        /// Updates the SyncIndex file with the content of the 
        /// given DTO 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        private bool UpdateFile(SyncIndexDTO dto) 
        {
            return _serilizer.Serialize(GetIndexFilePath(), dto); 
        }

        /// <summary>
        /// Returns the SyncIndexDTO from File or created new 
        /// </summary>
        /// <returns></returns>
        private SyncIndexDTO GetIndexBuffer()
        {
            string filePath = GetIndexFilePath(); 

            if (!File.Exists(filePath)) 
            {
                //Create new and Serialize
                SyncIndexDTO dto = new SyncIndexDTO(); 
                _logger.Debug("Creating new sync index file: " + filePath); 
                if (_serilizer.Serialize(filePath, dto)) 
                {
                    //Change File Access to hidding 
                    HideFile(filePath); 
                    _logger.Debug("Creating new index file succeed"); 
                } else 
                {
                    _logger.Error("Creating file failed tough to serialization failure"); 
                }
                return dto; 
            } else 
            {
                _logger.Debug("Deserializing existing index file"); 
                SyncIndexDTO result = _serilizer.Deserialize(filePath); 
                if (result == null) _logger.Error("Deserialization of the index File failed!"); 
                return result; 
            }
        }

        /// <summary>
        /// Sets the hidding attribute for the file at the given path 
        /// </summary>
        /// <param name="path"></param>
        private void HideFile(string path) 
        {
            FileAttributes attributes = File.GetAttributes(path);
            File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.Hidden);
            _logger.Debug("Hideing sync index file at" + path); 
        }

        /// <summary>
        /// Returns the path to the index file 
        /// </summary>
        /// <returns></returns>
        private string GetIndexFilePath()
        {
            string dirPath = _config.Local.LocalSyncDir ; 

            //Append tailing backslash if is does not exist allready 
            if (!dirPath.EndsWith('/') || !dirPath.EndsWith('\\'))
            {
                dirPath += "/"; 
            }
            return dirPath + GetIndexFileName(); 
        }

        /// <summary>
        /// Returns the Name of the Index File depending on the OS Platform
        /// that is currently used 
        /// </summary>
        /// <returns></returns>
        private string GetIndexFileName() 
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
               (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)))
            {
                //Linux or OSX
                return "." + _indexFileName; 
            } else 
            {
                //Windows
                return _indexFileName;  
            }
        }
        #endregion
    }
}