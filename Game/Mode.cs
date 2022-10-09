using System.ComponentModel;
namespace LiveSplit.Cuphead {
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
}