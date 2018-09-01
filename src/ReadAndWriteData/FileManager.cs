using System; 
using System.IO; 
using WebDavSync.Log; 

namespace WebDavSync.ReadWriteData 
{
    public static class FileManager 
    {  
        /// <summary>
        /// Executes a File Copy from the given src Path to the dst Path
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="dstPath"></param>
        /// <param name="overwrite"></param>
        /// <param name="logger"></param>
        /// <returns>A FileInfo object for the dst Path file or null or something went wrong </returns>
        public static FileInfo CopyFile(string srcPath, string dstPath, bool overwrite, ILogger logger) 
        {
            //Copy the TempFile in order to overwrite the local file
            logger.Debug("Moving file " + srcPath + " to dst " + dstPath); 
            try 
            { 
                //Need to use File.Copy here because FileInfo.MoveTo does not provide
                //a options to overwrite existing files 
                File.Copy(srcPath, dstPath, overwrite); 
                //Update the local FileInfo object 
                FileInfo info = new FileInfo(dstPath); 
                logger.Debug("Moving successfully."); 
                return info; 
            } catch(Exception exc)
            {
                logger.Error("Moving file failed: ", exc); 
            }
            return null; 
        }
        
    }
}