using UnityEngine;
using TMPro;

public class GestorTemperatura : MonoBehaviour
{
    [Header("Referencias a los otros scripts")]
    public Termometro termometro;
    public FondoTemperatura fondo;

    [Header("Referencias de texto (TextMeshPro)")]
    public TMP_Text textoTemperatura; // arrastra aquí el TextMeshPro que mostrará la temperatura
    public TMP_Text textoHumedad;     // arrastra aquí el TextMeshPro que mostrará la humedad

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

        ActualizarTextos(temperatura, humedad);
    }

    void ActualizarTextos(float temperatura, float humedad)
    {
        if (textoTemperatura != null)
            textoTemperatura.text = $"{temperatura:0.0} °C";

        if (textoHumedad != null)
            textoHumedad.text = $"{humedad:0.0} %";
    }
}