using System;
using System.Diagnostics;
namespace LiveSplit.Cuphead.Memory {
    public class ProgramPointer {
        private int lastID;
        private DateTime lastTry;
        private IFindPointer currentFinder;
        public IntPtr Pointer { get; private set; }
        public IFindPointer[] Finders { get; private set; }
        public string AsmName { get; private set; }
        public PointerVersion Version {
            get {
                return (currentFinder?.Version).GetValueOrDefault(PointerVersion.Steam115);
            }
        }

        public ProgramPointer(params IFindPointer[] finders) : this(string.Empty, finders) { }
        public ProgramPointer(string asmName, params IFindPointer[] finders) {
            AsmName = asmName;
            Finders = finders;
            lastID = -1;
            lastTry = DateTime.MinValue;
        }

        public T Read<T>(Process program, params int[] offsets) where T : unmanaged {
            GetPointer(program);
            return program.Read<T>(Pointer, offsets);
        }
        public string Read(Process program, params int[] offsets) {
            GetPointer(program);
            return program.ReadString(Pointer, offsets);
        }
        public byte[] ReadBytes(Process program, int length, params int[] offsets) {
            GetPointer(program);
            return program.Read(Pointer, length, offsets);
        }
        public void Write<T>(Process program, T value, params int[] offsets) where T : unmanaged {
            GetPointer(program);
            program.Write<T>(Pointer, value, offsets);
        }
        public void Write(Process program, byte[] value, params int[] offsets) {
            GetPointer(program);
            program.Write(Pointer, value, offsets);
        }
        public void ClearPointer() {
            Pointer = IntPtr.Zero;
        }
        public IntPtr GetPointer(Process program) {
            if (program == null) {
                Pointer = IntPtr.Zero;
                lastID = -1;
                return Pointer;
            } else if (program.Id != lastID) {
                Pointer = IntPtr.Zero;
                lastID = program.Id;
            } else if (Pointer != IntPtr.Zero) {
                IntPtr pointer = Pointer;
                currentFinder.VerifyPointer(program, ref pointer);
                Pointer = pointer;
            }

            if (Pointer == IntPtr.Zero && DateTime.Now > lastTry) {
                lastTry = DateTime.Now.AddSeconds(1);

                for (int i = 0; i < Finders.Length; i++) {
                    IFindPointer finder = Finders[i];
                    try {
                        Pointer = finder.FindPointer(program, AsmName);
                        if (Pointer != IntPtr.Zero || finder.FoundBaseAddress()) {
                            currentFinder = finder;
                            break;
                        }
                    } catch { }
                }
            }
            return Pointer;
        }
        public static IntPtr DerefPointer(Process program, IntPtr pointer, AutoDeref autoDeref) {
            if (pointer != IntPtr.Zero) {
                if (autoDeref != AutoDeref.None) {
                    pointer = program.Read<IntPtr>(pointer);

                    if (autoDeref == AutoDeref.Double) {
                        pointer = program.Read<IntPtr>(pointer);
                    }
                }
            }
            return pointer;
        }
        public static Tuple<IntPtr, IntPtr> GetAddressRange(Process program, string asmName) {
            Module64 module = program.Module64(asmName);
            if (module != null) {
                return new Tuple<IntPtr, IntPtr>(module.BaseAddress, module.BaseAddress + module.MemorySize);
            }
            return new Tuple<IntPtr, IntPtr>(IntPtr.Zero, IntPtr.Zero);
        }
    }
}