using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public class TinyFileDialogs
{
    #if UNITY_STANDALONE_WIN
    public const string DLL = "tinyfiledialogs.dll";
#endif
#if UNITY_STANDALONE_LINUX
        public const string DLL = "libtinyfiledialogs.so";
#endif
#if UNITY_STANDALONE_OSX
        public const string DLL = "tinyfiledialogs.dylib";
#endif
    [DllImport(DLL, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr tinyfd_selectFolderDialog(string aTitle, string aDefaultPathAndFile);
    [DllImport(DLL, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr tinyfd_openFileDialog(string aTitle, string aDefaultPathAndFile, int aNumOfFilterPatterns, string[] aFilterPatterns, string aSingleFilterDescription, int aAllowMultipleSelects);
    [DllImport(DLL, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr tinyfd_saveFileDialog(string aTitle, string aDefaultPathAndFile, int aNumOfFilterPatterns, string[] aFilterPatterns, string aSingleFilterDescription);

    public static string OpenFileDialog(string title, string defaultPath, List<string> filters, string filterDescription, bool allowMultiple)
    {
        var ptr = tinyfd_openFileDialog(title, defaultPath, filters.Count, filters.ToArray(), filterDescription, allowMultiple ? 1 : 0);

        if (ptr != IntPtr.Zero)
        {
            return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(ptr);
        }
        return null;
    }
    public static string SaveFileDialog(string title, string defaultPath, List<string> filters, string filterDescription)
    {
        var ptr = tinyfd_saveFileDialog(title, defaultPath, filters.Count, filters.ToArray(), filterDescription);

        if (ptr != IntPtr.Zero)
        {
            return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(ptr);
        }
        return null;
    }
}
