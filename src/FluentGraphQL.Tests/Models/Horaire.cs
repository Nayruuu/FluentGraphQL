namespace FluentGraphQL.Tests;

public class Horaire
{
    public TimeSpan HeureDebut { get; set; }

    public TimeSpan? HeureFin { get; set; }

    public bool? Actif { get; set; }
}
