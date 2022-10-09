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
		[Description("Inkwell Hell (Enter Scene)"), ToolTip("Splits when first entering Inkwell Isle DLC")]
		map_world_DLC,

		[Description("Tutorial (Level)"), ToolTip("Splits when leaving the Tutorial level")]
		level_tutorial,
		[Description("Chalice Tutorial (Level)"), ToolTip("Splits when leaving the Chalice Tutorial level")]
		level_chalice_tutorial,

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

		[Description("Glumstone The Giant (Boss)"), ToolTip("Splits when level is finished")]
		level_old_man,
		[Description("Mortimer Freeze (Boss)"), ToolTip("Splits when level is finished")]
		level_snow_cult,
		[Description("The Howling Aces (Boss)"), ToolTip("Splits when level is finished")]
		level_airplane,
		[Description("Esther Winchester (Boss)"), ToolTip("Splits when level is finished")]
		level_flying_cowboy,
		[Description("Moonshine Mob (Boss)"), ToolTip("Splits when level is finished")]
		level_rum_runners,
		[Description("Chef Saltbaker (Boss)"), ToolTip("Splits when level is finished")]
		level_saltbaker,
		[Description("Demon and Angel (Boss)"), ToolTip("Splits when level is finished")]
		level_graveyard,

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

		[Description("Chess Pawns (Boss)"), ToolTip("Splits when level is finished")]
		level_chess_pawn,
		[Description("Chess Knight (Boss)"), ToolTip("Splits when level is finished")]
		level_chess_knight,
		[Description("Chess Bishop (Boss)"), ToolTip("Splits when level is finished")]
		level_chess_bishop,
		[Description("Chess Rook (Boss)"), ToolTip("Splits when level is finished")]
		level_chess_rook,
		[Description("Chess Queen (Boss)"), ToolTip("Splits when level is finished")]
		level_chess_queen,

		[Description("Enter Level (IL)"), ToolTip("Splits when entering any level")]
		EnterLevel,
		[Description("End Level (IL)"), ToolTip("Splits when ending any level")]
		EndLevel,
	}
}