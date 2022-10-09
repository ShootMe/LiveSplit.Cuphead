using System;
using System.Runtime.InteropServices;
namespace LiveSplit.Cuphead.Memory {
    [StructLayout(LayoutKind.Sequential)]
    internal struct ModuleInfo {
        public IntPtr BaseAddress;
        public uint ModuleSize;
        public IntPtr EntryPoint;
    }
}