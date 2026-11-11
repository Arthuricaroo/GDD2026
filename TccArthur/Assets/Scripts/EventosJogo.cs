using System;

public static class EventosJogo
{
    // Disparado sempre que o player morre
    public static event Action PlayerMorreu;

    public static void AvisarMorteDoPlayer()
    {
        PlayerMorreu?.Invoke();
    }
}