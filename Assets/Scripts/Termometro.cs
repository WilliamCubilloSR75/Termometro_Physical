using UnityEngine;
using UnityEngine.UI;

public class Termometro : MonoBehaviour
{
    [Header("Referencias - Temperatura")]
    public Image relleno; // arrastra aquí el objeto "Relleno" del termómetro

    [Header("Referencias - Humedad")]
    public Image rellenoHumedad; // arrastra aquí el objeto "Relleno" de la gotica

    [Header("Rango de temperatura")]
    public float tempMin = 21f;   // temperatura que corresponde a termómetro vacío
    public float tempMax = 30f; // temperatura que corresponde a termómetro lleno

    [Header("Rango de humedad")]
    public float humedadMin = 0f;   // humedad que corresponde a gotica vacía
    public float humedadMax = 100f; // humedad que corresponde a gotica llena

    // La temperatura y la humedad ya no se actualizan aquí: GestorTemperatura
    // llama a ActualizarTermometro() y ActualizarHumedad() directamente
    // cuando cambian los datos.

    public void ActualizarTermometro(float temperatura)
    {
        float t = Mathf.InverseLerp(tempMin, tempMax, temperatura);
        t = Mathf.Clamp01(t);

        relleno.fillAmount = t;
    }

    public void ActualizarHumedad(float humedad)
    {
        float h = Mathf.InverseLerp(humedadMin, humedadMax, humedad);
        h = Mathf.Clamp01(h);

        rellenoHumedad.fillAmount = h;
    }
}