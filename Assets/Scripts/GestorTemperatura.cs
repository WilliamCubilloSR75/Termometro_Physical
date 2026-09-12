using UnityEngine;

public class GestorTemperatura : MonoBehaviour
{
    [Header("Referencias a los otros scripts")]
    public Termometro termometro;
    public FondoTemperatura fondo;

    [Header("Datos de entrada (aquí conectarás el sensor real)")]
    [Range(0f, 100f)]
    public float temperaturaActual = 25f;

    [Range(0f, 100f)]
    public float humedadActual = 50f;

    void Update()
    {
        // Por ahora leemos los sliders de prueba. Cuando conectes los datos
        // reales (Arduino, archivo, etc.), reemplaza estas variables
        // por donde llegan esos datos.
        ActualizarTodo(temperaturaActual, humedadActual);
    }

    public void ActualizarTodo(float temperatura, float humedad)
    {
        termometro.ActualizarTermometro(temperatura);
        termometro.ActualizarHumedad(humedad);
        fondo.ActualizarFondo(temperatura);
    }
}