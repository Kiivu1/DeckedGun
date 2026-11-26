using UnityEngine;

// Configuración de las oleadas. Se crea con: clic derecho ▸ Create ▸ DeckedGun ▸ Wave Config.
// Entre oleadas SOLO crece el total de enemigos; el resto de valores es igual en todas.
[CreateAssetMenu(fileName = "WaveConfig", menuName = "DeckedGun/Wave Config")]
public class WaveConfig : ScriptableObject
{
    [Header("Enemigo")]
    public GameObject enemyPrefab;                  // Debe tener EnemyHealth

    [Header("Total de enemigos (lo único que crece)")]
    [Min(0)] public int firstWaveEnemies = 5;      // Total de la oleada 1
    [Min(0)] public int enemiesPerWave = 3;         // Enemigos extra en cada oleada
    [Min(0)] public int maxTotalEnemies = 0;        // Tope del total (0 = sin tope)

    [Header("Fijos en todas las oleadas")]
    [Min(1)] public int maxAlive = 20;              // Máximo de enemigos vivos a la vez
    [Min(0.05f)] public float spawnInterval = 0.5f; // Segundos entre apariciones, para que no salgan todos en el mismo frame

    [Header("Tiempos")]
    [Min(0f)] public float timeBeforeFirstWave = 2f;
    [Min(0f)] public float timeBetweenWaves = 5f;

    // Total de enemigos de la oleada waveNumber (empieza en 1)
    public int GetTotalEnemies(int waveNumber)
    {
        return ComputeTotalEnemies(firstWaveEnemies, enemiesPerWave, maxTotalEnemies, waveNumber);
    }

    // La fórmula, separada para poder probarla sin crear el asset
    public static int ComputeTotalEnemies(int firstWave, int perWave, int maxTotal, int waveNumber)
    {
        if (waveNumber < 1) waveNumber = 1;

        int total = firstWave + perWave * (waveNumber - 1);
        if (maxTotal > 0) total = Mathf.Min(total, maxTotal);
        return total;
    }
}
