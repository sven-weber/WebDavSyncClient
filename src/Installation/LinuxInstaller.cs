using System;
using WebDavSync.Log;
using System.IO;
using WebDavSync.ReadWriteData;

namespace WebDavSync.Installation 
{
    public class LinuxInstaller : IInstaller
    {
        
        public string InstallationTarget => "/opt/WebDavSync";

        public void Install(ILogger logger)
        {
            logger.Debug("Linux detected -> Copy deployment. Target: " + InstallationTarget);
            logger.Debug("Working Directory folder:" + Directory.GetCurrentDirectory());
            try 
            {
                if (Directory.Exists(InstallationTarget)) 
                {
                    logger.Debug("Target dir exists, removing");
                    Directory.Delete(InstallationTarget);
                }

                //Copy the Directory
                FolderManager.CopyDirecory(Directory.GetCurrentDirectory(), InstallationTarget, true, logger);

                

            } catch (Exception exc)
            {
                logger.Error("Something went wrong during installation.", exc);
            }
        }

        public bool IsAdministrator() 
        {
            return Mono.Unix.Native.Syscall.geteuid() == 0;                
        }

    }
}