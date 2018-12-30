using System;
using WebDavSync.Log;

namespace WebDavSync.Installation 
{

    public interface IInstaller
    {
        void Install(ILogger log);

        bool IsAdministrator();

        string InstallationTarget {get;}
    }

}