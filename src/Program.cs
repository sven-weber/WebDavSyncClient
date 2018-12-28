using System;
using System.Diagnostics; 
using System.Timers; 
using System.Threading; 
using System.Reflection; 
using WebDavSync.Config; 
using WebDavSync.Log; 
using WebDavSync.Enums; 
using WebDavSync.DataProtection; 
using WebDavSync.Synchronization; 
using WebDavSync.Synchronization.WebDav; 
using Microsoft.AspNetCore.DataProtection; 
using Microsoft.Extensions.DependencyInjection;

namespace WebDavSync
{
    class Program
    {
        static LogManager _logManager; 

        /// <summary>
        /// Main Method, connects to webdav server and handles sync
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {   
            //Check the given parameters
            ParameterType type = CheckParameter(args); 

            bool runDebug = false;
            if (type == ParameterType.Dbg ||
                type == ParameterType.Unknown ||
                Debugger.IsAttached)
            {
                runDebug = true;
            }
            _logManager = new LogManager(runDebug); 

            DataProtectionManager dataProtectionManager = ResolveDataProtector(); 

            //Check Parameter 
            switch(type) 
            {
                case ParameterType.Install: 
                    Install(); 
                    break; 
                case ParameterType.Reconfigure: 
                    Reconfigure(dataProtectionManager, runDebug); 
                    break; 
                case ParameterType.Unknown:
                    LogUnknownParameterMessage();
                    break;
                default: 
                    Run(type, dataProtectionManager, runDebug); 
                    break; 
            }
        }

        /// <summary>
        /// Executes the installation of the Programm 
        /// </summary>
        static void Install() 
        {
            
        }

        /// <summary>
        /// Resolves an Instance of the DataProtectionManager using DI from aps.net
        /// because their crypto APIs are used here 
        /// </summary>
        /// <returns></returns>
        static DataProtectionManager ResolveDataProtector() 
        {
            // add data protection services
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDataProtection();
            using (ServiceProvider service = serviceCollection.BuildServiceProvider()) 
            {
                return ActivatorUtilities.CreateInstance<DataProtectionManager>(service);
            }
        }

        /// <summary>
        /// Prints a message stating that the given parameter is not known
        /// </summary>
        static void LogUnknownParameterMessage() 
        {
            ILogger logger = _logManager.GetLogger(LoggerType.Default); 
            logger.Error("The provided parameter is not known as a valid parameter!");
        }

        /// <summary>
        /// Executes a reconfiguration of the config file 
        /// </summary>
        static void Reconfigure(DataProtectionManager dataProtectionManager, bool runDebug) 
        {
            ILogger logger = _logManager.GetLogger(LoggerType.Default); 

            ConfigManager configManager = new ConfigManager(logger, dataProtectionManager); 
            if (!configManager.ConfigFileExists()) 
            {
                configManager.CreateEmptyConfig(); 
            }
            Configuration config = configManager.ReadConfig(runDebug); 

            if (config != null) 
            {
                InstallationHelper helper = new InstallationHelper(logger);
                configManager.Update(helper.Reconfigure(config)); 
            } else 
            {
                logger.Error("Could not read the configuration! Please try again."); 
                Environment.Exit(1); 
            }
        }

        /// <summary>
        /// Executes the default app 
        /// </summary>
        /// <param name="type"></param>
        static void Run(ParameterType type, DataProtectionManager protectionManager, bool runDebug) 
        {
            ILogger logger = _logManager.GetLogger(LoggerType.Default);
            
            logger.Debug("----------------------------------------"); 
            logger.Debug("-----Synchronization client started-----"); 
            logger.Debug("----------------v." + Assembly.GetEntryAssembly().GetName().Version + "---------------"); 
            logger.Debug("----------------------------------------"); 

            //Get Config 
            ConfigManager configManager = new ConfigManager(logger, protectionManager); 
            if (configManager.ConfigFileExists()) 
            {
                Configuration config = configManager.ReadConfig(runDebug); 

                //Error while Reading or validating the config 
                if (config == null|| !configManager.Validate(config)) Environment.Exit(1);
                
                //Execute Sync 
                ExecuteSync(logger, config);
            } else 
            {
                configManager.CreateEmptyConfig(); 
            }             
        }

        /// <summary>
        /// Checks the arguments if any of the known parameter is set and if so the 
        /// parameter type if returned
        /// </summary>
        /// <param name="args"></param>
        /// <returns>The found Parameter type of null if none was found</returns>
        static ParameterType CheckParameter(string[] args) 
        {
           if(args.Length > 0) 
           {
               foreach(ParameterType type in Enum.GetValues(typeof(ParameterType))) 
               {
                   string argument = args[0].ToLower(); 
                   string typeString = type.ToString().ToLower(); 
                   if (argument.Equals("-" + typeString) || 
                       argument.Equals(typeString)) 
                   {
                       return type; 
                   }
               }
               //No Parameter Type was found -> the provided Parameter is unknown
               return ParameterType.Unknown;
           }
           return ParameterType.None; 
        }

        /// <summary>
        /// Executes the synchronsation
        /// Blocking call till cancellation is requested
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        static void ExecuteSync(ILogger logger, Configuration config) 
        {
            SyncManager sync = new SyncManager(_logManager.GetLogger(LoggerType.Default)); 

            TimeSpan reoccurence = new TimeSpan(0 ,0, config.ReoccurenceTime); 

            logger.Debug("Sync will be performed every: " + reoccurence.ToString(@"dd\:hh\:mm\:ss")+ " (dd:hh:mm:ss)"); 

            //Variables for Reoccurence and stop   
            CancellationTokenSource _cancleToken = new CancellationTokenSource(); 
            ManualResetEventSlim _serverSideSync = new ManualResetEventSlim(true);  

            //Timer for reoccurence 
            System.Timers.Timer reoccurenceTimer = new System.Timers.Timer(); 
            reoccurenceTimer.Interval = config.ReoccurenceTime * 1000; 
            reoccurenceTimer.Elapsed += (sender, args) => 
            {
                _serverSideSync.Set(); 
            }; 
            reoccurenceTimer.Enabled = true; 

            //Event for cancleation 
            Console.CancelKeyPress += (sender, args) => 
            {
                logger.Debug("Canceling"); 
                _cancleToken.Cancel(); 
                args.Cancel = true; 
            };

            //Blocking operation!
            //As long as no cancellation is reqeusted, execute the Serverside checkout if reset event is set
            try 
            {
                do 
                {
                    int index = WaitHandle.WaitAny(new WaitHandle[] {_serverSideSync.WaitHandle, _cancleToken.Token.WaitHandle, }); 
                    if (index == 0) 
                    {
                        //The ServerSideSync event was set
                        sync.ExcecuteSynchronisation(_cancleToken.Token, config, new WebDavSyncClient(logger, config));
                        //Reset later -> If the execution times overlap they will be 
                        //skipped instead of increasing the hanging buffer 
                        _serverSideSync.Reset(); 
                    }
                } while(!_cancleToken.IsCancellationRequested);
            } catch(Exception exc) 
            {
                logger.Error("FATAL! An unexpected exception occured during execution:", exc); 
            } finally 
            {
                //Dispose everything 
                _cancleToken.Dispose(); 
                _serverSideSync.Dispose(); 
                reoccurenceTimer.Dispose(); 
                logger.Debug("Application terminated"); 
                logger.Debug(""); 
            }
        }
    }
}
