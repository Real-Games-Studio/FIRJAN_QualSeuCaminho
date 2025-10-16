using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class paralaxNoise : MonoBehaviour
{
    [Header("Configura��es do Noise")]
    public float positionAmplitude = 0.2f;     // Intensidade do deslocamento (em unidades do mundo)
    public float rotationAmplitude = 2f;       // Intensidade da rota��o (em graus)

    public float positionFrequency = 0.2f;     // Frequ�ncia do deslocamento (velocidade do noise)
    public float rotationFrequency = 0.15f;    // Frequ�ncia da rota��o (velocidade do noise)

    public Vector2 noiseOffset;                // Offset inicial para variar entre camadas

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start()
    {
        // Guarda posi��o e rota��o iniciais
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        // Gera um offset aleat�rio para cada camada, evitando que todas balancem iguais
        noiseOffset = new Vector2(Random.Range(0f, 100f), Random.Range(0f, 100f));
    }

    void Update()
    {
        float time = Time.time;

        // --- Movimento ---
        float noiseX = (Mathf.PerlinNoise(time * positionFrequency + noiseOffset.x, 0f) - 0.5f) * 2f;
        float noiseY = (Mathf.PerlinNoise(0f, time * positionFrequency + noiseOffset.y) - 0.5f) * 2f;

        Vector3 targetPos = initialPosition + new Vector3(noiseX, noiseY, 0f) * positionAmplitude;

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 2f);

        // --- Rota��o ---
        float noiseRot = (Mathf.PerlinNoise(time * rotationFrequency + noiseOffset.x, time * rotationFrequency + noiseOffset.y) - 0.5f) * 2f;

        Quaternion targetRot = Quaternion.Euler(initialRotation.eulerAngles.x,
                                                initialRotation.eulerAngles.y,
                                                initialRotation.eulerAngles.z + noiseRot * rotationAmplitude);

        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * 2f);
    }
}
