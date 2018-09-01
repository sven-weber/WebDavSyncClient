using System; 
using System.IO;
using System.Collections.Generic; 

namespace WebDavSync.Log.Loggers 
{
    public class DefaultLogger : LoggerBase, ILogger
    {
        #region ------------------ Properties ------------------
        private Dictionary<LogLevel, FileInfo> _fileNames; 
        private bool _useConsole; 
        private string _logDir; 
        private Dictionary<LogLevel, List<LogLevel>> _logLevelConfig; 
        #endregion

        #region ------------------ Konstruktor ------------------
        /// <summary>
        /// Creates a new Instance of the Default Logger that can be used to 
        /// perform Logging 
        /// </summary>
        /// <param name="useConsole"></param>
        public DefaultLogger(bool useConsole)  
        {
            _fileNames = new Dictionary<LogLevel, FileInfo>(); 
            _useConsole = useConsole;
            //The LogDir can be used here directly -> The creation of the 
            //Log Dir will be coverd the the installation process of the Programm
            //We can throw a Exception if it does not exist 
            _logDir = "/var/log/WebDavSync/"; 

            if (!Directory.Exists(_logDir)) throw new DirectoryNotFoundException("The speficied default log Directory " +_logDir + 
                                                                                 "could not be found. Maybe something went wrong during installation!"); 

            //Set the Log configuration for the LogLevel
            _logLevelConfig = new Dictionary<LogLevel, List<LogLevel>>() 
            {
                {
                    LogLevel.ERROR, new List<LogLevel>() 
                    {
                        LogLevel.DEBUG, 
                        LogLevel.ERROR
                    }
                },
                {
                    LogLevel.DEBUG, new List<LogLevel>() 
                    {
                        LogLevel.DEBUG
                    }
                }
            }; 
        }
        #endregion

        #region ------------------ public Methods ------------------

        public string LogDir => _logDir; 

        /// <summary>
        /// Identicates wheter the Logs should be printed in the console or not 
        /// </summary>
        /// <returns></returns>
        public bool UseConsole 
        {
            get 
            {
                return _useConsole;    
            }  
            set 
            {
                _useConsole = value; 
            }
        }

        /// <summary>
        /// Writes a Error Log with the given Message as content 
        /// </summary>
        /// <param name="message"></param>
        public void Error(string message) => DoWriteLog(LogLevel.ERROR, message, _useConsole); 

        /// <summary>
        /// Writes a Error Log with the given Message as content and 
        /// providing the execption content in the right format 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exc"></param>
        public void Error(string message, Exception exc) 
        {
            DoWriteLog(LogLevel.ERROR, string.Format("{0}. Exception: {1}, InnerException{2}, StackTrace: {3}", 
                       message, exc.Message, exc.InnerException, exc.StackTrace), _useConsole); 
        }

        /// <summary>
        /// Writes a Debug Log with the given Message 
        /// </summary>
        /// <param name="message"></param>
        public void Debug(string message) => DoWriteLog(LogLevel.DEBUG, message, _useConsole); 
        #endregion

        #region ------------------ private Methods ------------------
        /// <summary>
        /// Returns the Name of the LogFile for the given LogType
        /// considering the current Date 
        /// </summary>
        /// <param name="logType"></param>
        /// <returns></returns>
        protected override FileInfo GetLogFile(LogLevel logLevel) 
        {
            string fileName = _logDir + DateTime.Now.ToString("yyyyddMM") + " - "+ logLevel.ToString() + ".log"; 
            if (!_fileNames.ContainsKey(logLevel)) 
            {
                _fileNames.Add(logLevel, new FileInfo(fileName)); 
            } else if (!_fileNames[logLevel].Equals(fileName)) 
            {
                _fileNames[logLevel] = new FileInfo(fileName); 
            } 
            return _fileNames[logLevel]; 
        }

        /// <summary>
        /// Returns the LogLevel configuration for a given LogLevel
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        protected override List<LogLevel> GetLogLevelConfiguration(LogLevel level) => _logLevelConfig[level]; 
        #endregion
    }
}