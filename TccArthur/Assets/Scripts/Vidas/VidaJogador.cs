using System;

public static class VidaJogador
{
    public const int VidasMaximas = 3;

    public static int Vidas { get; private set; } = VidasMaximas;

    // Avisa o HUD sempre que o numero de vidas muda
    public static event Action<int> AoMudar;

    public static void Resetar()
    {
        Vidas = VidasMaximas;
        if (AoMudar != null) AoMudar(Vidas);
    }

    // Tira 1 vida e devolve quantas sobraram
    public static int PerderVida()
    {
        if (Vidas > 0) Vidas--;
        if (AoMudar != null) AoMudar(Vidas);
        return Vidas;
    }

    // Da 1 vida. Devolve false se ja estava com as 3
    public static bool GanharVida()
    {
        if (Vidas >= VidasMaximas) return false;
        Vidas++;
        if (AoMudar != null) AoMudar(Vidas);
        return true;
    }
}