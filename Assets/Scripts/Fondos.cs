using UnityEngine;
using UnityEngine.UI;

public class FondoTemperatura : MonoBehaviour
{
    [System.Serializable]
    public struct FondoPorTemperatura
    {
        public string nombre;      // solo para identificarlo en el Inspector, ej: "Nieve"
        public float temperatura;  // valor de temperatura asociado, ej: 0
        public Sprite sprite;      // la imagen de fondo para ese valor
    }

    [Header("Referencias")]
    public Image fondoA; // imagen de abajo (fija)
    public Image fondoB; // imagen de arriba (la que hace el fundido)

    [Header("Configura aquí tus 5 paisajes")]
    [Tooltip("Deben estar ordenados de menor a mayor temperatura")]
    public FondoPorTemperatura[] fondos = new FondoPorTemperatura[]
    {
        new FondoPorTemperatura { nombre = "Nieve",    temperatura = 19f },
        new FondoPorTemperatura { nombre = "Paramo",   temperatura = 19f },
        new FondoPorTemperatura { nombre = "Bosque",   temperatura = 20f },
        new FondoPorTemperatura { nombre = "Playa",    temperatura = 25f },
        new FondoPorTemperatura { nombre = "Infierno", temperatura = 30f },
    };

    // La temperatura ya no se actualiza aquí: GestorTemperatura llama
    // a ActualizarFondo() directamente cuando cambia el dato.

    public void ActualizarFondo(float temperatura)
    {
        if (fondos.Length < 2) return;

        temperatura = Mathf.Clamp(temperatura, fondos[0].temperatura, fondos[fondos.Length - 1].temperatura);

        // Buscar entre qué dos fondos está la temperatura actual
        int indiceInferior = 0;
        for (int i = 0; i < fondos.Length - 1; i++)
        {
            if (temperatura >= fondos[i].temperatura && temperatura <= fondos[i + 1].temperatura)
            {
                indiceInferior = i;
                break;
            }
        }

        FondoPorTemperatura inferior = fondos[indiceInferior];
        FondoPorTemperatura superior = fondos[indiceInferior + 1];

        // t = 0 significa "totalmente el fondo inferior", t = 1 significa "totalmente el fondo superior"
        float t = Mathf.InverseLerp(inferior.temperatura, superior.temperatura, temperatura);

        fondoA.sprite = inferior.sprite;
        fondoB.sprite = superior.sprite;

        Color colorA = fondoA.color;
        colorA.a = 1f;
        fondoA.color = colorA;

        Color colorB = fondoB.color;
        colorB.a = t;
        fondoB.color = colorB;
    }
}