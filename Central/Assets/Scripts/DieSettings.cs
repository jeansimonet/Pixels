using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public class DieSettings
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
    string name;

    public static byte[] ToByteArray(DieSettings settings)
    {
        int size = Marshal.SizeOf<DieSettings>();
        System.IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(settings, ptr, false);
        byte[] ret = new byte[size];
        Marshal.Copy(ptr, ret, 0, size);
        Marshal.FreeHGlobal(ptr);
        return ret;
    }

    public static DieSettings FromByteArray(byte[] data)
    {
        System.IntPtr ptr = Marshal.AllocHGlobal(data.Length);
        Marshal.Copy(data, 0, ptr, data.Length);
        var ret = Marshal.PtrToStructure<DieSettings>(ptr);
        Marshal.FreeHGlobal(ptr);
        return ret;
    }

}
