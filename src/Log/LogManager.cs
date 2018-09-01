using System; 
using System.IO; 
using WebDavSync.Log.Loggers; 

namespace WebDavSync.Log
{
    public class LogManager 
    {
        #region ------------------ Properties ------------------
        private bool _useConsole; 
        #endregion 

        #region ------------------ Konstruktor ------------------
        //Creates a new LogManager that will create LogFiles for the given Messages
        //If console should be used, the Logger will output all messages in a console window
        //The Log Dir is injected -> Can be changed e.g. in case of installation logs
        public LogManager(bool useConsole) 
        { 
            _useConsole = useConsole; 
        }
        #endregion

        #region ------------------ public Methods ------------------

        /// <summary>
        /// Returns a Logger of the given Type
        /// This Logger can be used to execute specific logging actions matching the 
        /// requested type 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ILogger GetLogger(LoggerType type) 
        {
            switch(type) 
            {
                case LoggerType.Installer: 
                    return new InstallLogger(_useConsole);  
                default: 
                    return new DefaultLogger(_useConsole); 
            }
        }
        #endregion
    }
}