using System;
using System.Diagnostics;
namespace LiveSplit.Cuphead.Memory {
    internal class Module64 {
        public IntPtr BaseAddress { get; set; }
        public IntPtr EntryPointAddress { get; set; }
        public string FileName { get; set; }
        public int MemorySize { get; set; }
        public string Name { get; set; }
        public FileVersionInfo FileVersionInfo => FileVersionInfo.GetVersionInfo(FileName);
        public override string ToString() {
            return Name ?? base.ToString();
        }
    }
}