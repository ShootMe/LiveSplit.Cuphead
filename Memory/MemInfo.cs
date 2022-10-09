using System;
using System.Runtime.InteropServices;
namespace LiveSplit.Cuphead.Memory {
    [StructLayout(LayoutKind.Sequential)]
    internal struct MemInfo {
        public IntPtr BaseAddress;
        public IntPtr AllocationBase;
        public uint AllocationProtect;
        public IntPtr RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
        public override string ToString() {
            return $"{BaseAddress} {Protect:X} {State:X} {Type:X} {RegionSize:X}";
        }
    }
}