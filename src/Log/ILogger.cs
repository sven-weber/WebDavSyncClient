using System; 

namespace WebDavSync.Log 
{
    public interface ILogger 
    {
        string LogDir { get; }

        /// <summary>
        /// Identicates wheter the Logs should be printed in the console or not 
        /// </summary>
        /// <returns></returns>
        bool UseConsole {get;set;}
        
        /// <summary>
        /// Writes a Error Log with the given Message as content 
        /// </summary>
        /// <param name="message"></param>
        void Error(string message); 

        /// <summary>
        /// Writes a Error Log with the given Message as content and 
        /// providing the execption content in the right format 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exc"></param>
        void Error(string message, Exception exc); 

        /// <summary>
        /// Writes a Debug Log with the given Message 
        /// </summary>
        /// <param name="message"></param>
        void Debug(string message);  
    }
}