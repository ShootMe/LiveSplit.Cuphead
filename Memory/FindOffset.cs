using System;
using System.Diagnostics;
namespace LiveSplit.Cuphead.Memory {
    public class FindOffset : IFindPointer {
        private int[] Offsets;
        private IntPtr BasePtr;
        private AutoDeref AutoDeref;
        private DateTime LastVerified;
        public PointerVersion Version { get; private set; }

        public FindOffset(PointerVersion version, AutoDeref autoDeref, params int[] offsets) {
            Version = version;
            AutoDeref = autoDeref;
            Offsets = offsets;
            LastVerified = DateTime.MaxValue;
        }

        public bool FoundBaseAddress() {
            return BasePtr != IntPtr.Zero;
        }
        public void VerifyPointer(Process program, ref IntPtr pointer) {
            if (DateTime.Now > LastVerified) {
                pointer = IntPtr.Zero;
            }
        }
        public IntPtr FindPointer(Process program, string asmName) {
            if (string.IsNullOrEmpty(asmName)) {
                BasePtr = program.MainModule.BaseAddress;
            } else {
                Tuple<IntPtr, IntPtr> range = ProgramPointer.GetAddressRange(program, asmName);
                BasePtr = range.Item1;
            }

            if (Offsets.Length > 1) {
                LastVerified = DateTime.Now.AddSeconds(5);
                return ProgramPointer.DerefPointer(program, program.Read<IntPtr>(BasePtr, Offsets), AutoDeref);
            } else {
                LastVerified = DateTime.MaxValue;
                BasePtr += Offsets[0];
                return ProgramPointer.DerefPointer(program, BasePtr, AutoDeref);
            }
        }
    }
}