using System;
using System.Runtime.InteropServices; 

namespace WebDavSync.Installation
{
    public class InstallerFactory
    {
        /// <summary>
        /// Creates a new instance of IInstaller for the currently running OS
        /// and returns this instance 
        /// </summary>
        /// <returns></returns>
        public IInstaller CreateNewForCurrentSystem()
        {
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return new LinuxInstaller();

            return null;
        }
    }
}