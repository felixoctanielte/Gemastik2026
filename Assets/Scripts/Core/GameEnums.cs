namespace PeduliTransit.Core
{
    public enum GameState
    {
        Boot,
        Login,
        Hub,
        Playing,
        Paused,
        Result
    }

    public enum TransportMode
    {
        Krl,
        Bus,
        AngkutanUmum
    }

    public enum EventCategory
    {
        Report,
        Initiative
    }

    public enum DecisionOutcome
    {
        Yes,
        No,
        Timeout
    }

    public enum NpcRole
    {
        Normal,
        LoudTalking,
        PrioritySeatAbuse,
        PhoneVolume,
        HarassmentHint,
        Pregnant,
        CarryingChild,
        Disability,
        Elderly
    }
}
