using System; 
using System.IO; 
using System.Collections.Generic; 

namespace WebDavSync.Log 
{
    public abstract class LoggerBase 
    {

        /// <summary>
        /// Returns a of FileInfo object for the given LogLeven that represents 
        /// the LogFile this level does affect 
        /// </summary>
        /// <param name="logLevel"></param>
        /// <returns></returns>
        protected abstract FileInfo GetLogFile(LogLevel logLevel);

        /// <summary>
        /// Returns the Configuration of LogLevel that are effected by a Log Level
        /// e.g.it could come handy to allways Log into Debug Log but seperate Error 
        /// logs in extra file on top
        /// </summary>
        /// <returns></returns>
        protected abstract List<LogLevel> GetLogLevelConfiguration(LogLevel logLevel); 

        /// <summary>
        /// Writes the given message to the File GetLogFile provides.
        /// The written Log depens on the LogType and when useConsole is true
        /// the output will also be writte to the console 
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="message"></param>
        /// <param name="useConsole"></param>
        protected void DoWriteLog(LogLevel logLevel, string message, bool useConsole) 
        {
            string formatedMessage = string.Format("{0};{1}; {2}", DateTime.Now.ToString("H:mm:ss:fff"), logLevel.ToString(), message); 
            //Basic but simple way 
            //using important for unmanged resources to be cleared with dispose!
            //Write to each LogFile that should be logged in in the given LogLevel
            foreach(LogLevel logFile in GetLogLevelConfiguration(logLevel)) 
            {
                using (StreamWriter writer = GetLogFile(logFile).AppendText()) 
                {
                    writer.WriteLine(formatedMessage);
                    writer.Flush();  
                }; 
            }
            
            if (useConsole) Console.WriteLine(formatedMessage); 
        } 
    }
}