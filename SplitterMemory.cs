using System;
using System.Diagnostics;
namespace LiveSplit.Cuphead {
	public partial class SplitterMemory {
		private static ProgramPointer PlayerData = new ProgramPointer(true, new ProgramSignature(PointerVersion.V1, "FF50C083C4108887D8000000B8????????C600000FB687D800000085C0742E8B473883780C02|13"));
		private static ProgramPointer SceneLoader = new ProgramPointer(true, new ProgramSignature(PointerVersion.V1, "558BEC5783EC048B7D0883EC0C57E8????????83C410B8????????8938D9EE83EC0883EC04D91C2457|23"));
		private static ProgramPointer Level = new ProgramPointer(true, new ProgramSignature(PointerVersion.V1, "FF903C01000083C4108BD08B45F8B9????????89118B978C000000|15"));
		public Process Program { get; set; }
		public bool IsHooked { get; set; } = false;
		private DateTime lastHooked;

		public SplitterMemory() {
			lastHooked = DateTime.MinValue;
		}

		public bool InGame() {
			//PlayerData.inGame
			return PlayerData.Read<bool>(Program, 0x0);
		}
		public bool Loading() {
			//SceneLoader.currentlyLoading
			return SceneLoader.Read<bool>(Program, 0x10);
		}
		public string SceneName() {
			//SceneLoader.SceneName
			return SceneLoader.Read(Program, 0x8);
		}
		public int CurrentLevel() {
			//Level.Current
			return Level.Read<int>(Program, -0x20);
		}
		public float LevelTime() {
			//Level.Current.LevelTime
			return Level.Read<float>(Program, -0x20, 0xa4);
		}
		public bool LevelEnding() {
			//Level.Current.Ending
			return Level.Read<bool>(Program, -0x20, 0xa8);
		}
		public bool LevelWon() {
			//Level.Won
			return Level.Read<bool>(Program, -0x17);
		}

		public bool HookProcess() {
			if ((Program == null || Program.HasExited) && DateTime.Now > lastHooked.AddSeconds(1)) {
				lastHooked = DateTime.Now;
				Process[] processes = Process.GetProcessesByName("Cuphead");
				Program = processes.Length == 0 ? null : processes[0];
				IsHooked = true;
			}

			if (Program == null || Program.HasExited) {
				IsHooked = false;
			}

			return IsHooked;
		}
		public void Dispose() {
			if (Program != null) {
				Program.Dispose();
			}
		}
	}
	public enum PointerVersion {
		V1
	}
	public class ProgramSignature {
		public PointerVersion Version { get; set; }
		public string Signature { get; set; }
		public ProgramSignature(PointerVersion version, string signature) {
			Version = version;
			Signature = signature;
		}
		public override string ToString() {
			return Version.ToString() + " - " + Signature;
		}
	}
	public class ProgramPointer {
		private int lastID;
		private DateTime lastTry;
		private ProgramSignature[] signatures;
		private int[] offsets;
		public IntPtr Pointer { get; private set; }
		public PointerVersion Version { get; private set; }
		public bool AutoDeref { get; private set; }

		public ProgramPointer(bool autoDeref, params ProgramSignature[] signatures) {
			AutoDeref = autoDeref;
			this.signatures = signatures;
			lastID = -1;
			lastTry = DateTime.MinValue;
		}
		public ProgramPointer(bool autoDeref, params int[] offsets) {
			AutoDeref = autoDeref;
			this.offsets = offsets;
			lastID = -1;
			lastTry = DateTime.MinValue;
		}

		public T Read<T>(Process program, params int[] offsets) where T : struct {
			GetPointer(program);
			return program.Read<T>(Pointer, offsets);
		}
		public string Read(Process program, params int[] offsets) {
			GetPointer(program);
			IntPtr ptr = (IntPtr)program.Read<uint>(Pointer, offsets);
			return program.Read(ptr);
		}
		public void Write<T>(Process program, T value, params int[] offsets) where T : struct {
			GetPointer(program);
			program.Write<T>(Pointer, value, offsets);
		}
		public void Write(Process program, byte[] value, params int[] offsets) {
			GetPointer(program);
			program.Write(Pointer, value, offsets);
		}
		public IntPtr GetPointer(Process program) {
			if ((program?.HasExited).GetValueOrDefault(true)) {
				Pointer = IntPtr.Zero;
				lastID = -1;
				return Pointer;
			} else if (program.Id != lastID) {
				Pointer = IntPtr.Zero;
				lastID = program.Id;
			}

			if (Pointer == IntPtr.Zero && DateTime.Now > lastTry.AddSeconds(1)) {
				lastTry = DateTime.Now;

				Pointer = GetVersionedFunctionPointer(program);
				if (Pointer != IntPtr.Zero) {
					if (AutoDeref) {
						Pointer = (IntPtr)program.Read<uint>(Pointer);
					}
				}
			}
			return Pointer;
		}
		private IntPtr GetVersionedFunctionPointer(Process program) {
			if (signatures != null) {
				for (int i = 0; i < signatures.Length; i++) {
					ProgramSignature signature = signatures[i];

					IntPtr ptr = program.FindSignatures(signature.Signature)[0];
					if (ptr != IntPtr.Zero) {
						Version = signature.Version;
						return ptr;
					}
				}
			} else {
				IntPtr ptr = (IntPtr)program.Read<uint>(program.MainModule.BaseAddress, offsets);
				if (ptr != IntPtr.Zero) {
					return ptr;
				}
			}

			return IntPtr.Zero;
		}
	}
}