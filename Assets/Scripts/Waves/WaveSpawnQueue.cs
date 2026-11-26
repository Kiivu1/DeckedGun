using System;

// Lleva la cuenta de UNA oleada: cuántos enemigos quedan en cola, cuántos están vivos
// y si se puede sacar otro. No sabe nada de Unity; WaveManager le avisa de cada spawn y cada muerte.
// Siempre se cumple: Remaining + Alive + Killed == Total
public class WaveSpawnQueue
{
    public int Total { get; }
    public int MaxAlive { get; }
    public int Remaining { get; private set; } // Aún no han aparecido
    public int Alive { get; private set; }     // Están ahora en la escena
    public int Killed { get; private set; }

    // Se puede sacar otro si queda alguno en cola y hay hueco
    public bool CanSpawn => Remaining > 0 && Alive < MaxAlive;

    // La oleada acaba cuando ya han salido todos y no queda ninguno vivo
    public bool IsComplete => Remaining == 0 && Alive == 0;

    public WaveSpawnQueue(int totalEnemies, int maxAlive)
    {
        // Protección contra valores absurdos del Inspector.
        // Con maxAlive 0 nunca saldría nadie y la oleada se quedaría colgada
        Total = Math.Max(0, totalEnemies);
        MaxAlive = Math.Max(1, maxAlive);
        Remaining = Total;
    }

    // Ha aparecido un enemigo. Devuelve false (sin cambiar nada) si no había hueco o cola
    public bool RegisterSpawn()
    {
        if (!CanSpawn) return false;

        Remaining--;
        Alive++;
        return true;
    }

    // Ha muerto un enemigo. Si no hay vivos se ignora, para que Alive nunca sea negativo
    public void RegisterDeath()
    {
        if (Alive == 0) return;

        Alive--;
        Killed++;
    }
}
