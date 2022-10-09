using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace LiveSplit.Cuphead.Memory {
    public class MemorySearcher {
        private const int BUFFER_SIZE = 2097152;
        private readonly List<MemInfo> memoryInfo = new List<MemInfo>();
        private readonly byte[] buffer = new byte[BUFFER_SIZE];
        internal Func<MemInfo, bool> MemoryFilter = delegate (MemInfo info) {
            return (info.State & 0x1000) != 0 && (info.Protect & 0x100) == 0;
        };

        public int ReadMemory(Process process, int index, int startIndex, out int bytesRead) {
            MemInfo info = memoryInfo[index];
            int returnIndex = -1;
            int amountToRead = (int)((uint)info.RegionSize - (uint)startIndex);
            if (amountToRead > BUFFER_SIZE) {
                returnIndex = startIndex + BUFFER_SIZE;
                amountToRead = BUFFER_SIZE;
            }
            WinAPI.ReadProcessMemory(process.Handle, info.BaseAddress + startIndex, buffer, amountToRead, out bytesRead);
            return returnIndex;
        }
        public IntPtr FindSignature(Process process, string signature) {
            GetSignature(signature, out byte[] pattern, out bool[] mask);
            GetMemoryInfo(process.Handle);
            int[] offsets = GetCharacterOffsets(pattern, mask);

            for (int i = 0; i < memoryInfo.Count; i++) {
                MemInfo info = memoryInfo[i];
                int index = 0;
                do {
                    int previousIndex = index;
                    index = ReadMemory(process, i, index, out int bytesRead);

                    int result = ScanMemory(buffer, bytesRead, pattern, mask, offsets);
                    if (result != int.MinValue) {
                        return info.BaseAddress + result + previousIndex;
                    }

                    if (index > 0) { index -= pattern.Length - 1; }
                } while (index > 0);
            }

            return IntPtr.Zero;
        }
        public List<IntPtr> FindSignatures(Process process, string signature) {
            GetSignature(signature, out byte[] pattern, out bool[] mask);
            GetMemoryInfo(process.Handle);
            int[] offsets = GetCharacterOffsets(pattern, mask);

            List<IntPtr> pointers = new List<IntPtr>();
            for (int i = 0; i < memoryInfo.Count; i++) {
                MemInfo info = memoryInfo[i];
                int index = 0;
                do {
                    int previousIndex = index;
                    index = ReadMemory(process, i, index, out int bytesRead);
                    info.BaseAddress += previousIndex;
                    ScanMemory(pointers, info, buffer, bytesRead, pattern, mask, offsets);
                    info.BaseAddress -= previousIndex;

                    if (index > 0) { index -= pattern.Length - 1; }
                } while (index > 0);
            }
            return pointers;
        }
        public bool VerifySignature(Process process, IntPtr pointer, string signature) {
            GetSignature(signature, out byte[] pattern, out bool[] mask);
            int[] offsets = GetCharacterOffsets(pattern, mask);

            MemInfo memInfoStart = default(MemInfo);
            if (WinAPI.VirtualQueryEx(process.Handle, pointer, out memInfoStart, Marshal.SizeOf(memInfoStart)) == 0 ||
                WinAPI.VirtualQueryEx(process.Handle, pointer + pattern.Length, out MemInfo memInfoEnd, Marshal.SizeOf(memInfoStart)) == 0 ||
                memInfoStart.BaseAddress != memInfoEnd.BaseAddress || !MemoryFilter(memInfoStart)) {
                return false;
            }

            byte[] buff = new byte[pattern.Length];
            WinAPI.ReadProcessMemory(process.Handle, pointer, buff, buff.Length, out _);
            return ScanMemory(buff, buff.Length, pattern, mask, offsets) == 0;
        }
        public void GetMemoryInfo(IntPtr pHandle) {
            memoryInfo.Clear();
            IntPtr current = (IntPtr)65536;
            while (true) {
                MemInfo memInfo = default(MemInfo);
                int dump = WinAPI.VirtualQueryEx(pHandle, current, out memInfo, Marshal.SizeOf(memInfo));
                if (dump == 0) { break; }

                long regionSize = (long)memInfo.RegionSize;
                if (regionSize <= 0 || (int)regionSize != regionSize) {
                    if (MemoryReader.is64Bit) {
                        current = (IntPtr)((ulong)memInfo.BaseAddress + (ulong)memInfo.RegionSize);
                        continue;
                    }
                    break;
                }

                if (MemoryFilter(memInfo)) {
                    memoryInfo.Add(memInfo);
                }

                current = memInfo.BaseAddress + (int)regionSize;
            }
        }
        private int ScanMemory(byte[] data, int dataLength, byte[] search, bool[] mask, int[] offsets) {
            int current = 0;
            int end = search.Length - 1;
            while (current <= dataLength - search.Length) {
                for (int i = end; data[current + i] == search[i] || mask[i]; i--) {
                    if (i == 0) {
                        return current;
                    }
                }
                int offset = offsets[data[current + end]];
                current += offset;
            }
            return int.MinValue;
        }
        private void ScanMemory(List<IntPtr> pointers, MemInfo info, byte[] data, int dataLength, byte[] search, bool[] mask, int[] offsets) {
            int current = 0;
            int end = search.Length - 1;
            while (current <= dataLength - search.Length) {
                for (int i = end; data[current + i] == search[i] || mask[i]; i--) {
                    if (i == 0) {
                        pointers.Add(info.BaseAddress + current);
                        break;
                    }
                }
                int offset = offsets[data[current + end]];
                current += offset;
            }
        }
        private int[] GetCharacterOffsets(byte[] search, bool[] mask) {
            int[] offsets = new int[256];
            int unknown = 0;
            int end = search.Length - 1;
            for (int i = 0; i < end; i++) {
                if (!mask[i]) {
                    offsets[search[i]] = end - i;
                } else {
                    unknown = end - i;
                }
            }

            if (unknown == 0) {
                unknown = search.Length;
            }

            for (int i = 0; i < 256; i++) {
                int offset = offsets[i];
                if (unknown < offset || offset == 0) {
                    offsets[i] = unknown;
                }
            }
            return offsets;
        }
        private void GetSignature(string searchString, out byte[] pattern, out bool[] mask) {
            int length = searchString.Length >> 1;
            pattern = new byte[length];
            mask = new bool[length];

            length <<= 1;
            for (int i = 0, j = 0; i < length; i++) {
                byte temp = (byte)(((int)searchString[i] - 0x30) & 0x1F);
                pattern[j] |= temp > 0x09 ? (byte)(temp - 7) : temp;
                if (searchString[i] == '?') {
                    mask[j] = true;
                    pattern[j] = 0;
                }
                if ((i & 1) == 1) {
                    j++;
                } else {
                    pattern[j] <<= 4;
                }
            }
        }
    }
}