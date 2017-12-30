using System;
using System.Diagnostics;
using System.Text;
namespace LiveSplit.Cuphead {
	public partial class SplitterMemory {
		private static ProgramPointer PlayerData = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "FF50C083C4108887D8000000B8????????C600000FB687D800000085C0742E8B473883780C02", 13));
		private static ProgramPointer SceneLoader = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "558BEC5783EC048B7D0883EC0C57E8????????83C410B8????????8938D9EE83EC0883EC04D91C2457", 23));
		private static ProgramPointer Level = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "FF90????????83C4108BD08B45F8B9????????89118B978C", 15));
		private static ProgramPointer PlayerManager = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "558BEC83EC18B8????????C6000083EC0C68????????E8????????83C41083EC0C8945F050", 7));
		private static ProgramPointer DebugConsole = new ProgramPointer(AutoDeref.None, new ProgramSignature(PointerVersion.V1, "558BEC83EC08E8????????85C07509E8????????85C0740E83EC0CFF7508E8????????83C410C9C3", 6));
		public Process Program { get; set; }
		public bool IsHooked { get; set; } = false;
		private DateTime lastHooked;

		public SplitterMemory() {
			lastHooked = DateTime.MinValue;
		}

		public byte[] ReadDebugCode() {
			return DebugConsole.ReadBytes(Program, 18);
		}
		public void EnableDebugConsole() {
			DebugConsole.Write(Program, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
		}
		public void DisableDebugConsole(byte[] oldDebugCode) {
			DebugConsole.Write(Program, oldDebugCode);
		}
		public bool InGame() {
			//PlayerData.inGame
			return PlayerData.Read<bool>(Program, 0x0);
		}
		public bool Loading() {
			//SceneLoader.doneAsyncLoading
			return !SceneLoader.Read<bool>(Program, 0x0, 0x3c);
		}
		public string SceneName() {
			//SceneLoader.SceneName
			return SceneLoader.Read(Program, 0x8);
		}
		public Levels CurrentLevel() {
			//Level.PreviousLevel
			return (Levels)Level.Read<int>(Program, 0x0);
		}
		public string CurrentEnemies() {
			//Level.Current.timeline
			IntPtr level = (IntPtr)Level.Read<uint>(Program, -0x20, 0x58);
			if (level == IntPtr.Zero) { return string.Empty; }

			StringBuilder sb = new StringBuilder();
			float health = Program.Read<float>(level, 0xc);
			float damage = Program.Read<float>(level, 0x10);
			sb.Append("HP: ").Append((health - damage).ToString("0.00")).Append(" / ").AppendLine(health.ToString());

			Levels current = CurrentLevel();
			if (current == Levels.Robot) {
				IntPtr body = (IntPtr)Level.Read<uint>(Program, -0x20, 0xc8);
				float hp = Program.Read<float>(body, 0x5c, 0x3c, 0x10);
				StringBuilder parts = new StringBuilder();
				if (hp > 1) { parts.Append("Antenna: ").Append(hp.ToString("0.00")).Append(' '); }

				hp = Program.Read<float>(body, 0x60, 0x3c, 0x10);
				if (hp > 1) { parts.Append("Chest: ").Append(hp.ToString("0.00")).Append(' '); }

				hp = Program.Read<float>(body, 0x64, 0x3c, 0x10);
				if (hp > 1) { parts.Append("Hatch: ").Append(hp.ToString("0.00")); }

				hp = Program.Read<float>(body, 0x60, 0x3c, 0x14);
				if (hp > 1 && parts.Length == 0) { parts.Append("Heart: ").Append(hp.ToString("0.00")); }
				if (parts.Length > 0) {
					sb.AppendLine(parts.ToString());
				}
			}

			level = (IntPtr)Level.Read<uint>(Program, -0x20, 0x58, 0x8);
			int size = Program.Read<int>(level, 0xc);
			for (int i = 0; i < size; i++) {
				float trigger = Program.Read<float>(level, 0x8, 0x10 + (i * 4), 0xc);
				if (health * trigger < health - damage) {
					sb.Append("Stage: ").Append(Program.Read((IntPtr)Program.Read<uint>(level, 0x8, 0x10 + (i * 4), 0x8))).Append(" at ").AppendLine((health * trigger).ToString("0"));
				}
			}

			return sb.ToString();
		}
		public float LevelTime() {
			//Level.Current.LevelTime
			return Level.Read<float>(Program, -0x20, 0xa4);
		}
		public Mode LevelMode() {
			//Level.Current.Mode
			if (Level.Read<uint>(Program, -0x20) != 0) {
				return (Mode)Level.Read<int>(Program, -0x20, 0x98);
			}
			return Mode.None;
		}
		public bool LevelEnding() {
			//Level.Current.Ending
			return Level.Read<bool>(Program, -0x20, 0xa8);
		}
		public bool LevelWon() {
			if (Level.Read<uint>(Program, -0x20) != 0) {
				//Level.Won
				return Level.Read<bool>(Program, -0x17);
			}
			return false;
		}
		public int Deaths() {
			IntPtr save = CurrentSave();
			//.statictics.playerOne.deaths
			return Program.Read<int>(save, 0x24, 0x8, 0x8);
		}
		public float SuperMeter() {
			IntPtr playerOne = PlayerOne();
			if (playerOne != IntPtr.Zero) {
				return Program.Read<float>(playerOne, 0x34, 0x68);
			}
			return 0;
		}
		public int Coins() {
			IntPtr save = CurrentSave();
			//.coinManager.coins
			IntPtr coins = (IntPtr)Program.Read<uint>(save, 0x14, 0x8);
			int size = Program.Read<int>(coins, 0xc);
			coins = (IntPtr)Program.Read<uint>(coins, 0x8);
			int total = 0;
			for (int i = 0; i < size; i++) {
				bool collected = Program.Read<bool>(coins, 0x10 + (i * 4), 0xc);
				if (collected) { total++; }
			}
			return total;
		}
		public bool LevelComplete(Levels levelId, Mode modeBeaten = Mode.Any, Grade gradeBeaten = Grade.Any) {
			IntPtr save = CurrentSave();
			//.levelDataManager.levelObjects
			IntPtr lvls = (IntPtr)Program.Read<uint>(save, 0x20, 0x8);
			int size = Program.Read<int>(lvls, 0xc);
			lvls = (IntPtr)Program.Read<uint>(lvls, 0x8);
			for (int i = 0; i < size; i++) {
				IntPtr item = (IntPtr)Program.Read<uint>(lvls, 0x10 + (i * 4));
				Levels level = (Levels)Program.Read<int>(item, 0x8);
				if (level == levelId) {
					bool completed = Program.Read<bool>(item, 0xc);
					Grade grade = (Grade)Program.Read<int>(item, 0x10);
					Mode difficulty = (Mode)Program.Read<int>(item, 0x14);

					return completed && grade >= gradeBeaten && difficulty >= modeBeaten;
				}
			}
			return false;
		}
		public float GameCompletion() {
			IntPtr save = CurrentSave();
			//.levelDataManager.levelObjects
			IntPtr lvls = (IntPtr)Program.Read<uint>(save, 0x20, 0x8);
			int size = Program.Read<int>(lvls, 0xc);
			lvls = (IntPtr)Program.Read<uint>(lvls, 0x8);
			float total = 0;
			for (int i = 0; i < size; i++) {
				IntPtr item = (IntPtr)Program.Read<uint>(lvls, 0x10 + (i * 4));
				Levels level = (Levels)Program.Read<int>(item, 0x8);
				bool completed = Program.Read<bool>(item, 0xc);
				Mode mode = (Mode)Program.Read<int>(item, 0x14);
				if (completed) {
					switch (level) {
						case Levels.Veggies:
						case Levels.Slime:
						case Levels.FlyingBlimp:
						case Levels.Flower:
						case Levels.Frogs:
						case Levels.Baroness:
						case Levels.Clown:
						case Levels.FlyingGenie:
						case Levels.Dragon:
						case Levels.FlyingBird:
						case Levels.Bee:
						case Levels.Pirate:
						case Levels.SallyStagePlay:
						case Levels.Mouse:
						case Levels.Robot:
						case Levels.FlyingMermaid:
						case Levels.Train:
							if (mode == Mode.Hard) {
								total += 8.5f;
							} else if (mode == Mode.Normal) {
								total += 3.5f;
							} else {
								total += 1.5f;
							}
							break;
						case Levels.Platforming_Level_1_1:
						case Levels.Platforming_Level_1_2:
						case Levels.Platforming_Level_2_1:
						case Levels.Platforming_Level_2_2:
						case Levels.Platforming_Level_3_1:
						case Levels.Platforming_Level_3_2:
							total += 1.5f;
							break;
						case Levels.DicePalaceMain:
							if (mode == Mode.Hard) {
								total += 10f;
							} else {
								total += 3f;
							}
							break;
						case Levels.Devil:
							if (mode == Mode.Hard) {
								total += 12f;
							} else {
								total += 4f;
							}
							break;
					}
				}
			}

			return total + Coins() * 0.5f + NumberOfSupers() * 1.5f;
		}
		public int NumberOfSupers() {
			IntPtr save = CurrentSave();
			//.inventories.playerOne._supers.Count
			return Program.Read<int>(save, 0xc, 0x8, 0xc, 0xc);
		}
		private IntPtr CurrentSave() {
			//PlayerData._saveFiles[PlayerData._CurrentSaveFileIndex]
			IntPtr saves = (IntPtr)PlayerData.Read<uint>(Program, 0x3);
			int saveSlot = PlayerData.Read<int>(Program, -0x5);
			return (IntPtr)Program.Read<uint>(saves, 0x10 + (saveSlot * 4));
		}
		private IntPtr PlayerOne() {
			//PlayerManager.players[PlayerId.PlayerOne]
			IntPtr players = (IntPtr)PlayerManager.Read<uint>(Program, 0x20);
			int count = Program.Read<int>(players, 0x20);
			IntPtr keys = (IntPtr)Program.Read<uint>(players, 0x10);
			players = (IntPtr)Program.Read<uint>(players, 0x14);

			for (int i = 0; i < count; i++) {
				PlayerId id = (PlayerId)Program.Read<int>(keys, 0x10 + (i * 4));
				if (id == PlayerId.PlayerOne) {
					return (IntPtr)Program.Read<uint>(players, 0x10 + (i * 4));
				}
			}

			return IntPtr.Zero;
		}
		public void SetInvincible(bool invincible) {
			IntPtr playerOne = PlayerOne();
			if (playerOne != IntPtr.Zero) {
				Program.Write<bool>(playerOne, invincible, 0x34, 0x6c);
			}
		}

		public bool HookProcess() {
			if (DateTime.Now > lastHooked.AddSeconds(1) && (Program == null || Program.HasExited)) {
				lastHooked = DateTime.Now;
				Process[] processes = Process.GetProcessesByName("Cuphead");
				Program = processes.Length == 0 ? null : processes[0];
				if (Program != null) {
					MemoryReader.Update64Bit(Program);
				}
			}

			IsHooked = Program != null;

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
	public enum AutoDeref {
		None,
		Single,
		Double
	}
	public class ProgramSignature {
		public PointerVersion Version { get; set; }
		public string Signature { get; set; }
		public int Offset { get; set; }
		public ProgramSignature(PointerVersion version, string signature, int offset) {
			Version = version;
			Signature = signature;
			Offset = offset;
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
		public AutoDeref AutoDeref { get; private set; }

		public ProgramPointer(AutoDeref autoDeref, params ProgramSignature[] signatures) {
			AutoDeref = autoDeref;
			this.signatures = signatures;
			lastID = -1;
			lastTry = DateTime.MinValue;
		}
		public ProgramPointer(AutoDeref autoDeref, params int[] offsets) {
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
			return program.Read((IntPtr)program.Read<uint>(Pointer, offsets));
		}
		public byte[] ReadBytes(Process program, int length, params int[] offsets) {
			GetPointer(program);
			return program.Read(Pointer, length, offsets);
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
			if (program == null) {
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
					if (AutoDeref != AutoDeref.None) {
						Pointer = (IntPtr)program.Read<uint>(Pointer);
						if (AutoDeref == AutoDeref.Double) {
							if (MemoryReader.is64Bit) {
								Pointer = (IntPtr)program.Read<ulong>(Pointer);
							} else {
								Pointer = (IntPtr)program.Read<uint>(Pointer);
							}
						}
					}
				}
			}
			return Pointer;
		}
		private IntPtr GetVersionedFunctionPointer(Process program) {
			if (signatures != null) {
				MemorySearcher searcher = new MemorySearcher();
				searcher.MemoryFilter = delegate (MemInfo info) {
					return (info.State & 0x1000) != 0 && (info.Protect & 0x40) != 0 && (info.Protect & 0x100) == 0;
				};
				for (int i = 0; i < signatures.Length; i++) {
					ProgramSignature signature = signatures[i];

					IntPtr ptr = searcher.FindSignature(program, signature.Signature);
					if (ptr != IntPtr.Zero) {
						Version = signature.Version;
						return ptr + signature.Offset;
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