using System.ComponentModel;
namespace LiveSplit.Cuphead {
	public enum SplitName {
		[Description("Manual Split (Not Automatic)"), ToolTip("Specify to split manually when an automatic split does not exist yet")]
		ManualSplit,

		[Description("Start Game (Select Save)"), ToolTip("Splits when you select a new save slot")]
		StartGame,

		[Description("Shop (Enter Scene)"), ToolTip("Splits when current scene is 'Shop'")]
		Shop,
		[Description("Inkwell Isle 1 (Enter Scene)"), ToolTip("Splits when first entering Inkwell Isle I")]
		map_world_1,
		[Description("Inkwell Isle 2 (Enter Scene)"), ToolTip("Splits when first entering Inkwell Isle II")]
		map_world_2,
		[Description("Inkwell Isle 3 (Enter Scene)"), ToolTip("Splits when first entering Inkwell Isle III")]
		map_world_3,
		[Description("Inkwell Hell (Enter Scene)"), ToolTip("Splits when first entering Inkwell Hell")]
		map_world_4,

		[Description("Tutorial (Level)"), ToolTip("Splits when leaving the Tutorial level")]
		level_tutorial,

		[Description("The Root Pack (Boss)"), ToolTip("Splits when level is finished")]
		level_veggies,
		[Description("Goopy Le Grande (Boss)"), ToolTip("Splits when level is finished")]
		level_slime,
		[Description("Cagney Carnation (Boss)"), ToolTip("Splits when level is finished")]
		level_flower,
		[Description("Ribby And Croaks (Boss)"), ToolTip("Splits when level is finished")]
		level_frogs,
		[Description("Hilda Berg (Boss)"), ToolTip("Splits when level is finished")]
		level_flying_blimp,

		[Description("Baroness Von Bon Bon (Boss)"), ToolTip("Splits when level is finished")]
		level_baroness,
		[Description("Djimmi The Great (Boss)"), ToolTip("Splits when level is finished")]
		level_flying_genie,
		[Description("Beppi The Clown (Boss)"), ToolTip("Splits when level is finished")]
		level_clown,
		[Description("Wally Warbles (Boss)"), ToolTip("Splits when level is finished")]
		level_flying_bird,
		[Description("Grim Matchstick (Boss)"), ToolTip("Splits when level is finished")]
		level_dragon,

		[Description("Rumor Honeybottoms (Boss)"), ToolTip("Splits when level is finished")]
		level_bee,
		[Description("Captin Brineybeard (Boss)"), ToolTip("Splits when level is finished")]
		level_pirate,
		[Description("Werner Werman (Boss)"), ToolTip("Splits when level is finished")]
		level_mouse,
		[Description("Dr. Kahl's Robot (Boss)"), ToolTip("Splits when level is finished")]
		level_robot,
		[Description("Sally Stageplay (Boss)"), ToolTip("Splits when level is finished")]
		level_sally_stage_play,
		[Description("Cala Maria (Boss)"), ToolTip("Splits when level is finished")]
		level_flying_mermaid,
		[Description("Phantom Express (Boss)"), ToolTip("Splits when level is finished")]
		level_train,

		[Description("King Dice (Contract Cutscene)"), ToolTip("Splits when you get the cutscene trying to enter the King Dice fight without all contracts")]
		level_dice_palace_enter,
		[Description("King Dice (Boss)"), ToolTip("Splits when you beat King Dice")]
		level_dice_palace_main,
		[Description("Devil (Boss)"), ToolTip("Splits when level is finished")]
		level_devil,

		[Description("End Game (Credits)"), ToolTip("Splits when entering the credits")]
		EndGame,

		[Description("Forest Follies (Run 'n Gun)"), ToolTip("Splits when level is finished'")]
		level_platforming_1_1F,
		[Description("Treetop Trouble (Run 'n Gun)"), ToolTip("Splits when level is finished")]
		level_platforming_1_2F,
		[Description("Funfair Fever (Run 'n Gun)"), ToolTip("Splits when level is finished")]
		level_platforming_2_1F,
		[Description("Funhouse Frazzle (Run 'n Gun)"), ToolTip("Splits when level is finished")]
		level_platforming_2_2F,
		[Description("Perilous Piers (Run 'n Gun)"), ToolTip("Splits when level is finished")]
		level_platforming_3_1F,
		[Description("Rugged Ridge (Run 'n Gun)"), ToolTip("Splits when level is finished")]
		level_platforming_3_2F,

		[Description("Mausoleum I (Super)"), ToolTip("Splits when level is finished'")]
		level_mausoleum_1,
		[Description("Mausoleum II (Super)"), ToolTip("Splits when level is finished'")]
		level_mausoleum_2,
		[Description("Mausoleum III (Super)"), ToolTip("Splits when level is finished'")]
		level_mausoleum_3,

		[Description("Enter Level (IL)"), ToolTip("Splits when entering any level")]
		EnterLevel,
		[Description("End Level (IL)"), ToolTip("Splits when ending any level")]
		EndLevel,
	}
	public enum Levels {
		Test = 1,
		FlyingTest = 1429603775,
		Tutorial = 0,
		Pirate = 2,
		Bat,
		Train = 5,
		Veggies,
		Frogs,
		Bee = 1429976377,
		Mouse = 1430652919,
		Dragon = 1432722919,
		Flower = 1450266910,
		Slime = 1450863107,
		Baroness = 1451300935,
		AirshipJelly = 8,
		AirshipStork = 1459338579,
		AirshipCrab = 1459489001,
		FlyingBird = 1428495827,
		FlyingMermaid = 1446558823,
		FlyingBlimp = 1449745424,
		Robot = 1452935394,
		Clown = 1456125457,
		SallyStagePlay = 1456740288,
		DicePalaceDomino = 1458062114,
		DicePalaceCard = 1458289179,
		DicePalaceChips = 1458336090,
		DicePalaceCigar = 1458551456,
		DicePalaceTest = 1458559869,
		DicePalaceBooze = 1458719430,
		DicePalaceRoulette = 1459105708,
		DicePalacePachinko = 1459444983,
		DicePalaceRabbit = 1459928905,
		AirshipClam = 1459950766,
		FlyingGenie = 1460200177,
		DicePalaceLight = 1463124738,
		DicePalaceFlyingHorse = 1463479514,
		DicePalaceFlyingMemory = 1464322003,
		DicePalaceMain = 1465296077,
		DicePalaceEightBall = 1468483834,
		Devil = 1466688317,
		RetroArcade = 1469187579,
		Mausoleum = 1481199742,
		House = 1484633053,
		DiceGate = 1495090481,
		ShmupTutorial = 1504847973,
		Platforming_Level_1_1 = 1464969490,
		Platforming_Level_1_2,
		Platforming_Level_3_1,
		Platforming_Level_3_2,
		Platforming_Level_2_2 = 1496818712,
		Platforming_Level_2_1 = 1499704951
	}
	public enum PlayerId {
		PlayerOne,
		PlayerTwo,
		Any = 2147483646,
		None
	}
	public enum Mode {
		Any = -1,
		[Description("Simple")]
		Easy = 0,
		[Description("Regular")]
		Normal,
		[Description("Expert")]
		Hard,
		None
	}
	public enum Grade {
		Any = -1,
		[Description("D-")]
		DMinus = 0,
		D,
		[Description("D+")]
		DPlus,
		[Description("C-")]
		CMinus,
		C,
		[Description("C+")]
		CPlus,
		[Description("B-")]
		BMinus,
		B,
		[Description("B+")]
		BPlus,
		[Description("A-")]
		AMinus,
		A,
		[Description("A+")]
		APlus,
		S,
		P
	}
}