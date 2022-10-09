using System.ComponentModel;
namespace LiveSplit.Cuphead {
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