using System; 
using System.Text;
using System.Security;   
using System.Security.Cryptography; 
using System.Runtime.InteropServices; 

namespace WebDavSync.ExtensionMethods 
{
    public static class SecureStringExtension
    {
        /// <summary>
        /// Compares the two given Secure String for equality using unmanaged memory
        /// </summary>
        /// <param name="ss1"></param>
        /// <param name="ss2"></param>
        /// <returns></returns>
        public static bool IsEqualTo(this SecureString ss1, SecureString ss2)  
        {
            IntPtr bstr1 = IntPtr.Zero;
            IntPtr bstr2 = IntPtr.Zero;
            try
            {
                bstr1 = Marshal.SecureStringToBSTR(ss1);
                bstr2 = Marshal.SecureStringToBSTR(ss2);
                int length1 = Marshal.ReadInt32(bstr1, -4);
                int length2 = Marshal.ReadInt32(bstr2, -4);
                if (length1 == length2)
                {
                    int result = 0; 
                    for (int x = 0; x < length1; ++x)
                    {
                        result |= Marshal.ReadByte(bstr1, x) ^ Marshal.ReadByte(bstr2, x);
                    }
                    return result == 0; 
                } 
                return false;
            }
            finally
            {
                if (bstr2 != IntPtr.Zero) Marshal.ZeroFreeBSTR(bstr2);
                if (bstr1 != IntPtr.Zero) Marshal.ZeroFreeBSTR(bstr1);
            }       
        }
    }
}