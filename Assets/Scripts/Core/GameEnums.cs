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
        Timeout,
        WrongReport,
        Negur,
        Cancel
    }

    public enum NpcRole
    {
        Normal,
        LoudTalking,
        PrioritySeatAbuse,
        PhoneVolume,
        HarassmentHint,
        Fighting,
        Pregnant,
        CarryingChild,
        Disability,
        Elderly,
        Security,
        TicketOfficer,
        DriverAssistant
    }

    public enum ResponderKind
    {
        Security,
        TicketOfficer,
        DriverAssistant
    }
}
