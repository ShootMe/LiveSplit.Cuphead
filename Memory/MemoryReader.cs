using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
namespace LiveSplit.Cuphead.Memory {
    internal static class MemoryReader {
        private static readonly Dictionary<int, Module64[]> ModuleCache = new Dictionary<int, Module64[]>();
        public static bool is64Bit;
        public static void Update64Bit(Process program) {
            is64Bit = program.Is64Bit();
        }
        public static T Read<T>(this Process targetProcess, IntPtr address, params int[] offsets) where T : unmanaged {
            if (targetProcess == null || address == IntPtr.Zero) { return default(T); }

            int last = OffsetAddress(targetProcess, ref address, offsets);
            if (address == IntPtr.Zero) { return default(T); }

            unsafe {
                int size = sizeof(T);
                if (typeof(T) == typeof(IntPtr)) { size = is64Bit ? 8 : 4; }
                byte[] buffer = Read(targetProcess, address + last, size);
                fixed (byte* ptr = buffer) {
                    return *(T*)ptr;
                }
            }
        }
        public static byte[] Read(this Process targetProcess, IntPtr address, int numBytes) {
            byte[] buffer = new byte[numBytes];
            if (targetProcess == null || address == IntPtr.Zero) { return buffer; }

            WinAPI.ReadProcessMemory(targetProcess.Handle, address, buffer, numBytes, out _);
            return buffer;
        }
        public static byte[] Read(this Process targetProcess, IntPtr address, int numBytes, params int[] offsets) {
            byte[] buffer = new byte[numBytes];
            if (targetProcess == null || address == IntPtr.Zero) { return buffer; }

            int last = OffsetAddress(targetProcess, ref address, offsets);
            if (address == IntPtr.Zero) { return buffer; }

            WinAPI.ReadProcessMemory(targetProcess.Handle, address + last, buffer, numBytes, out _);
            return buffer;
        }
        public static string ReadString(this Process targetProcess, IntPtr address) {
            if (targetProcess == null || address == IntPtr.Zero) { return string.Empty; }

            int length = Read<int>(targetProcess, address, is64Bit ? 0x10 : 0x8);
            if (length < 0 || length > 2048) { return string.Empty; }
            return Encoding.Unicode.GetString(Read(targetProcess, address + (is64Bit ? 0x14 : 0xc), 2 * length));
        }
        public static string ReadString(this Process targetProcess, IntPtr address, params int[] offsets) {
            if (targetProcess == null || address == IntPtr.Zero) { return string.Empty; }

            int last = OffsetAddress(targetProcess, ref address, offsets);
            if (address == IntPtr.Zero) { return string.Empty; }

            return ReadString(targetProcess, address + last);
        }
        public static string ReadAscii(this Process targetProcess, IntPtr address) {
            if (targetProcess == null || address == IntPtr.Zero) { return string.Empty; }

            StringBuilder sb = new StringBuilder();
            byte[] data = new byte[128];
            int bytesRead;
            int offset = 0;
            bool invalid = false;
            do {
                WinAPI.ReadProcessMemory(targetProcess.Handle, address + offset, data, 128, out bytesRead);
                int i = 0;
                while (i < bytesRead) {
                    byte d = data[i++];
                    if (d == 0) {
                        i--;
                        break;
                    } else if (d > 127) {
                        invalid = true;
                        break;
                    }
                }
                if (i > 0) {
                    sb.Append(Encoding.ASCII.GetString(data, 0, i));
                }
                if (i < bytesRead || invalid) {
                    break;
                }
                offset += 128;
            } while (bytesRead > 0);

            return invalid ? string.Empty : sb.ToString();
        }
        public static string ReadAscii(this Process targetProcess, IntPtr address, params int[] offsets) {
            if (targetProcess == null || address == IntPtr.Zero) { return string.Empty; }

            int last = OffsetAddress(targetProcess, ref address, offsets);
            if (address == IntPtr.Zero) { return string.Empty; }

            return ReadAscii(targetProcess, address + last);
        }
        public static void Write<T>(this Process targetProcess, IntPtr address, T value, params int[] offsets) where T : unmanaged {
            if (targetProcess == null) { return; }

            int last = OffsetAddress(targetProcess, ref address, offsets);
            if (address == IntPtr.Zero) { return; }

            byte[] buffer;
            unsafe {
                buffer = new byte[sizeof(T)];
                fixed (byte* bufferPtr = buffer) {
                    Buffer.MemoryCopy(&value, bufferPtr, sizeof(T), sizeof(T));
                }
            }
            WinAPI.WriteProcessMemory(targetProcess.Handle, address + last, buffer, buffer.Length, out _);
        }
        public static void Write(this Process targetProcess, IntPtr address, byte[] value, params int[] offsets) {
            if (targetProcess == null) { return; }

            int last = OffsetAddress(targetProcess, ref address, offsets);
            if (address == IntPtr.Zero) { return; }

            WinAPI.WriteProcessMemory(targetProcess.Handle, address + last, value, value.Length, out _);
        }
        private static int OffsetAddress(this Process targetProcess, ref IntPtr address, params int[] offsets) {
            byte[] buffer = new byte[is64Bit ? 8 : 4];
            for (int i = 0; i < offsets.Length - 1; i++) {
                WinAPI.ReadProcessMemory(targetProcess.Handle, address + offsets[i], buffer, buffer.Length, out _);
                if (is64Bit) {
                    address = (IntPtr)BitConverter.ToUInt64(buffer, 0);
                } else {
                    address = (IntPtr)BitConverter.ToUInt32(buffer, 0);
                }
                if (address == IntPtr.Zero) { break; }
            }
            return offsets.Length > 0 ? offsets[offsets.Length - 1] : 0;
        }
        public static bool Is64Bit(this Process process) {
            if (process == null) { return false; }
            WinAPI.IsWow64Process(process.Handle, out bool flag);
            return Environment.Is64BitOperatingSystem && !flag;
        }
        public static Module64 MainModule64(this Process p) {
            Module64[] modules = p.Modules64();
            return modules == null || modules.Length == 0 ? null : modules[0];
        }
        public static Module64 Module64(this Process p, string moduleName) {
            Module64[] modules = p.Modules64();
            if (modules != null) {
                for (int i = 0; i < modules.Length; i++) {
                    Module64 module = modules[i];
                    if (module.Name.Equals(moduleName, StringComparison.OrdinalIgnoreCase)) {
                        return module;
                    }
                }
            }
            return null;
        }
        public static Module64[] Modules64(this Process p) {
            lock (ModuleCache) {
                if (ModuleCache.Count > 100) { ModuleCache.Clear(); }

                IntPtr[] buffer = new IntPtr[1024];
                uint cb = (uint)(IntPtr.Size * buffer.Length);
                if (!WinAPI.EnumProcessModulesEx(p.Handle, buffer, cb, out uint totalModules, 3u)) {
                    return null;
                }
                uint moduleSize = totalModules / (uint)IntPtr.Size;
                int key = p.StartTime.GetHashCode() + p.Id + (int)moduleSize;
                if (ModuleCache.ContainsKey(key)) { return ModuleCache[key]; }

                List<Module64> list = new List<Module64>();
                StringBuilder stringBuilder = new StringBuilder(260);
                int count = 0;
                while ((long)count < (long)((ulong)moduleSize)) {
                    stringBuilder.Clear();
                    if (WinAPI.GetModuleFileNameEx(p.Handle, buffer[count], stringBuilder, (uint)stringBuilder.Capacity) == 0u) {
                        return list.ToArray();
                    }
                    string fileName = stringBuilder.ToString();
                    stringBuilder.Clear();
                    if (WinAPI.GetModuleBaseName(p.Handle, buffer[count], stringBuilder, (uint)stringBuilder.Capacity) == 0u) {
                        return list.ToArray();
                    }
                    string moduleName = stringBuilder.ToString();
                    ModuleInfo modInfo = default(ModuleInfo);
                    if (!WinAPI.GetModuleInformation(p.Handle, buffer[count], out modInfo, (uint)Marshal.SizeOf(modInfo))) {
                        return list.ToArray();
                    }
                    list.Add(new Module64 {
                        FileName = fileName,
                        BaseAddress = modInfo.BaseAddress,
                        MemorySize = (int)modInfo.ModuleSize,
                        EntryPointAddress = modInfo.EntryPoint,
                        Name = moduleName
                    });
                    count++;
                }
                ModuleCache.Add(key, list.ToArray());
                return list.ToArray();
            }
        }
    }
}