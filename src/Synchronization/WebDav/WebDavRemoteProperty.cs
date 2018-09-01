using System; 
using WebDav; 
using WebDavSync.Enums; 
using WebDavSync.Synchronization; 
using WebDavSync.ExtensionMethods; 

namespace WebDavSync.Synchronization.WebDav 
{
    public class WebDavRemoteProperty : IRemoteProperty
    {
        #region ------------------ Properties ------------------

        private WebDavResource _resource; 
        private SyncElementType _type; 
        private string _relativePath; 
        private string _decodedRelativePath; 
        #endregion

        #region ------------------ Konstruktor ------------------
        public WebDavRemoteProperty(WebDavResource resource, string baseDirectory) 
        {
            _resource = resource; 
            //Find the resource Type 
            _type = _resource.IsCollection == true ? SyncElementType.Directory : SyncElementType.File; 
            _relativePath = _resource.Uri.Replace(baseDirectory, ""); 
            //Cut of Tailing slash, does not exist in convention in order 
            //to be able to compare paths
            _relativePath = _relativePath.RemoveTailingSlashes(); 
            _decodedRelativePath = _relativePath.UrlDecode(); 
        }
        #endregion
        #region ------------------ public Properties ------------------
        public SyncElementType ElementType => _type; 

        public string DecodedRelativeRemotePath => _decodedRelativePath; 

        public string RelativeRemotePath => _relativePath; 

        public string RemoteRevision => _resource.ETag; 
        #endregion
    }
}