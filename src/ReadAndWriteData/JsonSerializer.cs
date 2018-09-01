using System; 
using System.IO; 
using System.Linq; 
using System.Text; 
using System.Collections.Generic; 
using Newtonsoft.Json; 
using WebDavSync.ExtensionMethods; 

namespace WebDavSync.ReadWriteData
{
    public class JsonSerializer<T> where T : class 
    {
        //Deserializes the File at the Path when File Exists
        //If not or an error occures null will be returned 
        public T Deserialize(string path) 
        {
            if (!File.Exists(path)) return null; 

            try 
            {
                using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) 
                {
                    StringBuilder builder = new StringBuilder(); 
                    byte[] buffer = new byte[5120]; // read in chunks of 5KB
                    int bytesRead;
                    //As long as we can ready data, read it 
                    while((bytesRead = file.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        //Decode and append to string builder
                        builder.Append(Encoding.UTF8.GetString(buffer.SubArray(0, bytesRead)));  
                    }
                    return (T)JsonConvert.DeserializeObject<T>(builder.ToString()); 
                }
            } catch
            {
                return null; 
            }
        }

        /// <summary>
        /// Serializes the given object to the given path using a default non prettyfied format
        /// </summary>
        /// <param name="path"></param>
        /// <param name="element"></param>
        /// <param name="format"></param>
        /// <returns>Wheter the serialization was successfull or not</returns>
        public bool Serialize(string path, T element) 
        {
            return Serialize(path, element, Formatting.None); 
        }

        /// <summary>
        /// Serializes the given object to the given path considering the given Format
        /// </summary>
        /// <param name="path"></param>
        /// <param name="element"></param>
        /// <param name="format"></param>
        /// <returns>Wheter the serialization was successfull or not</returns>
        public bool Serialize(string path, T element, Formatting format) 
        {
            try 
            {
                using (FileStream file = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)) 
                {
                    string json = JsonConvert.SerializeObject(element, format); 
                    Byte[] bytes = Encoding.UTF8.GetBytes(json); 
                    file.Write(bytes, 0, bytes.Length); 

                    // Truncate to the new length
                    file.SetLength(bytes.Length);

                    file.Flush();                     
                    
                    return true; 
                }
            } catch
            {
                return false; 
            }
        }
    }
}