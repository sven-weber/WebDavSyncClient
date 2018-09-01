using System; 
using WebDav; 
using System.Linq; 
using WebDavSync.Log; 

namespace WebDavSync.Synchronization.WebDav
{

    public class WebDavLockManager 
    {
        private PrincipalLockOwner _owner; 
        private TimeSpan _lockTimeout; 
        private ILogger _logger; 
        public WebDavLockManager(ILogger logger) 
        {
            _owner = new PrincipalLockOwner(Guid.NewGuid().ToString()); 
            _lockTimeout = new TimeSpan(0,10,0); 
            _logger = logger; 
        }

        //Locks the given Resource and returns the LockToken 
        //if the locking was successfull
        //If not null will be returned 
        public string LockResource(WebDavClient client, string destUri, LockScope lockScope) 
        {   
            _logger.Debug("Locking remote element " + destUri); 
            LockResponse lockResponse = client.Lock(destUri, new LockParameters() {
                LockScope = lockScope, 
                Owner = _owner, 
            }).Result;

            if (lockResponse.IsSuccessful) 
            {
                _logger.Debug("Successfully locked"); 
                ActiveLock ActiveLock = lockResponse.ActiveLocks.Where(x => x.Owner.Value.Equals(_owner.Value)).FirstOrDefault(); 
                return ActiveLock.LockToken; 
            } else 
            {
                _logger.Error("Locking ressource failed! description: " + lockResponse.Description);
            }

            return null; 
        }

        //Unlocks the given Resource with the given lockToken 
        //And returns wheter it was successful 
        public bool UnlockResource(WebDavClient client, string destUri, string lockToken) 
        {
            _logger.Debug("Unlocking remote resource " + destUri); 
            bool success = client.Unlock(destUri, lockToken).Result.IsSuccessful; 
            if (success) 
            {
                _logger.Debug("Successfully unlocked."); 
            } else 
            {
                _logger.Debug("Unlocking failed!"); 
            }
            return success; 
        }
    }

}
