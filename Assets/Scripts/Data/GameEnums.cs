

namespace Sumfulla.TankTankBoom
{
    public enum FailureReason
    {
        Destroyed,
        OutOfTime
    }

    public enum FiringType
    {
        STD,
        RAPID
    }

    public enum GaugeColor
    {
        STANDARD,
        READY,
        RAPID,
    }

    public enum InfoTab
    {
        SETTINGS,
        LEGAL,
        ABOUT,
        NO_ADS
    }

    public enum LayerNames
    {
        Ground,
        PlayerTarget,
        Test
    }

    public enum RunState
    {
        PAUSED,
        PLAY
    }

    public enum StrikeState
    {
        DORMANT,
        READY,
        AIRBORNE,
        DROPPING
    }

    public enum TagNames
    {
        Enemy,
        PlayerTank,
        Blimp,
        Ground
    }

    public enum TileTerrain
    {
        Random,
        HillBattle,
        JaggedRocks,
        Roughlands
    }

    public enum TemplateShape
    {
        NONE,
        ABSOLUTE,
        STRETCH,
        CENTER
    }

}