using UnityEngine;
using UnityEngine.UI;

public class FondoTemperatura : MonoBehaviour
{
    [System.Serializable]
    public struct FondoPorTemperatura
    {
        public string nombre;          // solo para identificarlo en el Inspector, ej: "Nieve"
        public float temperatura;      // valor de temperatura asociado, ej: 0
        public Sprite sprite;          // la imagen de fondo para ese valor
        // Número de estado para el Animator (0: Nieve/Frio, 1: Paramo/Fresco, 2: Bosque/Templado, 3: Playa/Calor, 4: Infierno)
        public int idAnimacion;        
        public string nombreAnimacion; // Nombre del estado o Trigger (opcional para Fallback/CrossFade)
        public Material material;      // Material opcional a aplicar al objeto/personaje en este estado
    }

    [Header("Referencias")]
    public Image fondoA; // imagen de abajo (fija)
    public Image fondoB; // imagen de arriba (la que hace el fundido)
    public Animator animatorPersonaje; // Componente Animator de tu modelo 3D
    public Renderer rendererObjetivo;  // Componente Renderer (MeshRenderer/SkinnedMeshRenderer) para cambiar material

    [Header("Configuración de Parámetros de Animator")]
    [Tooltip("Nombre del parámetro Int creado en la pestaña Parameters del Animator (ej: 'Estado')")]
    public string nombreParametroInt = "Estado";

    [Header("Configura aquí tus 5 paisajes")]
    [Tooltip("Deben estar ordenados de menor a mayor temperatura")]
    public FondoPorTemperatura[] fondos = new FondoPorTemperatura[]
    {
        new FondoPorTemperatura { nombre = "Nieve",    temperatura = 20f, idAnimacion = 0, nombreAnimacion = "Frio" },
        new FondoPorTemperatura { nombre = "Paramo",   temperatura = 21f, idAnimacion = 1, nombreAnimacion = "Paramo" },
        new FondoPorTemperatura { nombre = "Bosque",   temperatura = 23f, idAnimacion = 2, nombreAnimacion = "Templado" },
        new FondoPorTemperatura { nombre = "Playa",    temperatura = 25f, idAnimacion = 3, nombreAnimacion = "Calor" },
        new FondoPorTemperatura { nombre = "Infierno", temperatura = 27f, idAnimacion = 4, nombreAnimacion = "Infierno" },
    };

    [Header("Configuración de Transición")]
    [Tooltip("Si está activado, los fondos se funden gradualmente. Si está desactivado, la imagen de fondo cambia exactamente en el mismo instante que la animación del personaje.")]
    public bool fundidoGradual = true;

    private int idAnimacionActual = -1;

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

        // Elegir el paisaje dominante (t >= 0.5 activa el paisaje superior)
        int indiceDominante = (t >= 0.5f) ? (indiceInferior + 1) : indiceInferior;
        FondoPorTemperatura seleccionado = fondos[indiceDominante];

        // 1. Cambiar animación si cambió el estado
        if (seleccionado.idAnimacion != idAnimacionActual)
        {
            idAnimacionActual = seleccionado.idAnimacion;
            CambiarAnimacion(seleccionado);
        }

        // 2. Cambiar/Sincronizar Fondo
        if (!fundidoGradual)
        {
            // Cambio directo: El fondo es SIEMPRE 100% idéntico a la animación activa
            fondoA.sprite = seleccionado.sprite;
            Color colorA100 = fondoA.color;
            colorA100.a = 1f;
            fondoA.color = colorA100;

            if (fondoB != null)
            {
                Color colorB0 = fondoB.color;
                colorB0.a = 0f;
                fondoB.color = colorB0;
            }
        }
        else
        {
            // Fundido gradual entre fondos
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

    private void CambiarAnimacion(FondoPorTemperatura fondoActual)
    {
        // 1. Cambiar Material si están asignados el Renderer y el Material
        if (rendererObjetivo != null && fondoActual.material != null)
        {
            rendererObjetivo.material = fondoActual.material;
            Debug.Log($"[FondoTemperatura] Cambiando material a: {fondoActual.material.name} para el estado {fondoActual.nombre}");
        }

        if (animatorPersonaje == null)
        {
            Debug.LogWarning("FondoTemperatura: No se ha asignado el Animator del personaje en el Inspector.");
            return;
        }

        Debug.Log($"[FondoTemperatura] Cambiando animación a: {fondoActual.nombre} (idAnimacion = {fondoActual.idAnimacion}) mediante parámetro '{nombreParametroInt}'");

        // 2. Intentar cambiar por parámetro Int (Transiciones / Relaciones de Unity)
        if (!string.IsNullOrEmpty(nombreParametroInt))
        {
            animatorPersonaje.SetInteger(nombreParametroInt, fondoActual.idAnimacion);
        }
        // 3. Fallback: CrossFade directo por nombre de estado si no se usan parámetros
        else if (!string.IsNullOrEmpty(fondoActual.nombreAnimacion))
        {
            animatorPersonaje.CrossFade(fondoActual.nombreAnimacion, 0.25f);
        }
    }
}


