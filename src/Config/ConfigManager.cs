using System; 
using System.IO;
using System.Linq;  
using System.Reflection;
using System.Collections; 
using System.Collections.Generic; 
using WebDavSync; 
using WebDavSync.Log; 
using WebDavSync.Enums;
using WebDavSync.Config.DTO; 
using WebDavSync.ReadWriteData; 
using WebDavSync.DataProtection; 
using Newtonsoft.Json; 

namespace WebDavSync.Config 
{
    public class ConfigManager 
    {
        private string _configFileName; 
        private JsonSerializer<ConfigDTO> _serializer; 

        private ILogger _logger; 

        private DataProtectionManager _protectionManager; 
        public ConfigManager(ILogger log, DataProtectionManager protectionManager) 
        {
            _logger = log; 
            _configFileName = "WebDavSync.conf"; 
            _serializer= new JsonSerializer<ConfigDTO>(); 
            _protectionManager = protectionManager; 
        }

        private string GetConfigPath() 
        {
            return Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + _configFileName; 
        }

        /// <summary>
        /// Validates a specific Type and the given instance of that Type 
        /// </summary>
        /// <param name="instance"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private bool ValidateType<T>(T instance) 
        {
            bool validated = true; 
            foreach(PropertyInfo info in typeof(T).GetTypeInfo().GetProperties()) 
            {   
                ConfigAttribute attribute = (ConfigAttribute)info.GetCustomAttribute(typeof(ConfigAttribute)); 
                //If no attribute set, continue 
                if (attribute == null) continue; 
                //else validate 
                object value = instance.GetType().GetProperty(info.Name).GetValue(instance); 

                if (!attribute.Validate(value)) 
                {
                    validated = false; 
                    _logger.Error("Property " + info.Name + " has an invalid value. Value " + value + " " + attribute.ErroMessage); 
                } 
            }
            return validated; 
        }

        /// <summary>
        /// Corrects the given Type by looking for ValueCorrector attributes
        /// and executing them
        /// </summary>
        /// <param name="instance"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private void CorrectType<T>(T instance) 
        {
            foreach(PropertyInfo info in typeof(T).GetTypeInfo().GetProperties()) 
            {   
                IEnumerable<Attribute> attributes = info.GetCustomAttributes(typeof(ConfigValueCorrector)); 
                List<ConfigValueCorrector> correctors = attributes.ToList().ConvertAll(x => (ConfigValueCorrector)x); 
                foreach(ConfigValueCorrector attribute in correctors) 
                {
                    //If no attribute set, continue 
                    if (attribute == null) continue; 
                    //else validate 
                    object value = instance.GetType().GetProperty(info.Name).GetValue(instance); 
                    object newValue = attribute.Correct(value); 

                    if (!newValue.Equals(value)) 
                    {
                        instance.GetType().GetProperty(info.Name).SetValue(instance, newValue); 
                    }
                }
            } 
        }

        /// <summary>
        /// Validates the given Type and when it is valid calls correction methods 
        /// </summary>
        /// <param name="instance"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private bool CorrectAndValidateType<T>(T instance) 
        {
            bool validationRes = ValidateType<T>(instance); 
            if (validationRes) CorrectType<T>(instance); 
            return validationRes; 
        }

        /// <summary>
        /// Applies the given File Option -> e.g. creates a File if none exists
        /// etc.
        /// </summary>
        /// <param name="option"></param>
        private void ApplyReadFileOption(ReadFileOption option)
        {
            switch(option)
            {
                case ReadFileOption.CreateIfNotExists:
                    CreateConfigIfNotExists();
                    break;
            }
        }

        /// <summary>
        /// Creates an empty configuration file if no other exists at the moment 
        /// </summary>
        private void CreateConfigIfNotExists() 
        {
            if (!ConfigFileExists()) 
            {
                CreateEmptyConfig(); 
            }
        }
        
        /// <summary>
        /// Trys to read the given Configuration and applies the given ReadFileOption
        /// </summary>
        /// <param name="runDebug"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public Configuration ReadConfig(bool runDebug, ReadFileOption option) 
        {
            ApplyReadFileOption(option);            
            return ReadConfig(runDebug);
        }

        /// <summary>
        /// Trys to read the current Configuration
        /// returns null if not possible 
        /// </summary>
        /// <returns></returns>
        public Configuration ReadConfig(bool runDebug) 
        {
            ConfigDTO dto = _serializer.Deserialize(GetConfigPath()); 
            if (dto != null) 
            {
                return new Configuration(dto, _protectionManager, runDebug); 
            }
            _logger.Error("Deserializing the config file failed!"); 
            return null; 
        }

        /// <summary>
        /// Returns wheter the Config File allready exists or not 
        /// </summary>
        /// <returns></returns>
        public bool ConfigFileExists() 
        {
            try 
            {
                return File.Exists(GetConfigPath()); 
            } catch
            {
                return false; 
            }   
        }

        /// <summary>
        /// Validates wheter the given Configuration is valid by considering the 
        /// Attributes that have been set in the ConfigDTO 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>Wheter the Config could be validated or not</returns>
        public bool Validate(Configuration config) 
        {
            return Validate(config.ToDTO()); 
        }

        /// <summary>
        /// Validates wheter the given ConfigDTO is valid by considering the 
        /// Attributes that have been set in the ConfigDTO itself 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>Wheter the Config could be validated or not</returns>
        private bool Validate(ConfigDTO dto) 
        {
            bool validated = true; 
        
            _logger.Debug("Validating config."); 
            if(!CorrectAndValidateType<ConfigDTO>(dto)) validated = false;
            if(!CorrectAndValidateType<ConfigLocalDTO>(dto.Local)) validated = false;
            if(!CorrectAndValidateType<ConfigRemoteDTO>(dto.Remote)) validated = false;
            
            if(validated) 
            {
                _logger.Debug("Config validation succeed."); 
            } else 
            {
                _logger.Error("Config validation failed! Run \"reconfigure\" or edit the configuration file to change the values!"); 
            }; 
            return validated; 
        }

        /// <summary>
        /// Updates the current existing config with the values given in the DTO
        ///returns wheter the Update was successfull or not 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public bool Update(Configuration config) 
        {
            return _serializer.Serialize(GetConfigPath(), config.ToDTO(), Formatting.Indented); 
        }

        /// <summary>
        /// Creates a Empty config, returns wheter the creation was possible 
        /// </summary>
        /// <returns></returns>
        public bool CreateEmptyConfig() 
        {
            string configPath = GetConfigPath(); 
            if (!File.Exists(configPath)) 
            {
                if (_serializer.Serialize(configPath, new ConfigDTO(), Formatting.Indented))
                {
                    _logger.Error("No config File found. Empty file created at " + configPath + "."); 
                    _logger.Error("Please consider running the \"reconfigure\" command or editing the config file in order to get the service working."); 
                    return true;
                } else 
                {
                    _logger.Error("No config File found but creating empty file failed."); 
                    _logger.Error("Please try again!"); 
                    return false;
                }
            }
            return false; 
        }

    }
}