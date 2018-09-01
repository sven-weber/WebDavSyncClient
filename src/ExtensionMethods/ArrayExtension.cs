using System; 

namespace WebDavSync.ExtensionMethods 
{
    public static class ArrayExtension 
    {
        /// <summary>
        /// Creates a SubArray from the given Array considering the 
        /// given index and length
        /// </summary>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <param name="length"></param>
        /// <typeparam name="U"></typeparam>
        /// <returns></returns>
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }
}