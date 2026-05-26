namespace NuaSpa.Application.Messaging;

/// <summary>Tipovi poruka koje API objavljuje, a Worker obrađuje.</summary>
public static class NuaSpaMessageTypes
{
    public const string RezervacijaPotvrda = "rezervacija.potvrda";
    public const string RezervacijaOtkazana = "rezervacija.otkazana";
    public const string TherapistInvite = "therapist.invite";
    public const string UslugaKreirana = "usluga.kreirana";
    public const string SendEmail = "email.send";
}
