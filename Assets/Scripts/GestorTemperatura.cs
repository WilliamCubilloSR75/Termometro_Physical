using UnityEngine;
using TMPro;
using System;
using System.IO.Ports;
using System.Threading;
using System.Globalization;

public class GestorTemperatura : MonoBehaviour
{
    [Header("Referencias a los otros scripts")]
    public Termometro termometro;
    public FondoTemperatura fondo;

    [Header("Referencias de texto (TextMeshPro)")]
    public TMP_Text textoTemperatura; // arrastra aquí el TextMeshPro que mostrará la temperatura
    public TMP_Text textoHumedad;     // arrastra aquí el TextMeshPro que mostrará la humedad
    public TMP_Text textoEstadoPuerto; // opcional: mostrará "Conectando...", "Conectado", etc.

    [Header("Configuración del puerto serial (Arduino)")]
    public string nombrePuerto = "COM7"; // cámbialo por el puerto que use tu Arduino
    public int baudRate = 9600;

    private SerialPort puertoSerial;
    private Thread hiloLectura;
    private volatile bool leyendo = false;

    // Últimos valores leídos del Arduino. Se escriben desde el hilo de
    // lectura y se leen desde Update(), por eso usamos "lock" para
    // evitar que ambos hilos los toquen al mismo tiempo.
    private readonly object candado = new object();
    private float ultimaTemperatura = 0f;
    private float ultimaHumedad = 0f;
    private bool hayDatoNuevo = false;

    void Start()
    {
        ActualizarEstadoTexto("Conectando...");
        ListarPuertosDisponibles();
        AbrirPuerto();
    }

    /// <summary>
    /// Función pública asignable a un Botón UI para recargar/reintentar la conexión.
    /// </summary>
    public void ReconectarPuerto()
    {
        Debug.Log("[GestorTemperatura] Reintentando conexión al puerto serial...");
        ActualizarEstadoTexto("Conectando...");
        CerrarPuerto();
        ListarPuertosDisponibles();
        AbrirPuerto();
    }

    void ListarPuertosDisponibles()
    {
        string[] puertos = SerialPort.GetPortNames();
        Debug.Log("Puertos COM detectados en el sistema: " + string.Join(", ", puertos));
    }

    void AbrirPuerto()
    {
        try
        {
            puertoSerial = new SerialPort(nombrePuerto, baudRate);
            puertoSerial.ReadTimeout = 2000;
            puertoSerial.Open();

            leyendo = true;
            hiloLectura = new Thread(LeerDatosSerial);
            hiloLectura.IsBackground = true;
            hiloLectura.Start();

            Debug.Log("Puerto serial abierto correctamente: " + nombrePuerto);
            ActualizarEstadoTexto($"Conectado ({nombrePuerto})");
        }
        catch (Exception e)
        {
            Debug.LogError("No se pudo abrir el puerto serial (" + nombrePuerto + "): " + e.Message);
            ActualizarEstadoTexto($"Error al conectar ({nombrePuerto})");
        }
    }

    // Corre en un hilo aparte para no congelar Unity mientras espera datos.
    void LeerDatosSerial()
    {
        while (leyendo && puertoSerial != null && puertoSerial.IsOpen)
        {
            try
            {
                string linea = puertoSerial.ReadLine();
                ProcesarLinea(linea);
            }
            catch (TimeoutException)
            {
                // No llegó ningún dato a tiempo, se vuelve a intentar en la siguiente vuelta.
            }
            catch (Exception e)
            {
                Debug.LogWarning("Error leyendo el puerto serial: " + e.Message);
            }
        }
    }

    // Formato esperado desde el Arduino: DATA:T:25.30,H:60.20
    void ProcesarLinea(string linea)
    {
        if (string.IsNullOrEmpty(linea)) return;
        linea = linea.Trim();

        if (!linea.StartsWith("DATA:")) return; // ignora cualquier otra línea (mensajes, separadores, etc.)

        string contenido = linea.Substring("DATA:".Length); // T:25.30,H:60.20
        string[] partes = contenido.Split(',');
        if (partes.Length < 2) return;

        string parteTemp = partes[0].Replace("T:", "").Trim();
        string parteHum = partes[1].Replace("H:", "").Trim();

        float temp, hum;
        bool okTemp = float.TryParse(parteTemp, NumberStyles.Float, CultureInfo.InvariantCulture, out temp);
        bool okHum = float.TryParse(parteHum, NumberStyles.Float, CultureInfo.InvariantCulture, out hum);

        if (okTemp && okHum)
        {
            lock (candado)
            {
                ultimaTemperatura = temp;
                ultimaHumedad = hum;
                hayDatoNuevo = true;
            }
        }
    }

    void Update()
    {
        bool hayNuevo;
        float temp, hum;

        lock (candado)
        {
            hayNuevo = hayDatoNuevo;
            temp = ultimaTemperatura;
            hum = ultimaHumedad;
            hayDatoNuevo = false;
        }

        if (hayNuevo)
        {
            ActualizarTodo(temp, hum);
        }
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

    void ActualizarEstadoTexto(string mensaje)
    {
        if (textoEstadoPuerto != null)
            textoEstadoPuerto.text = mensaje;
    }

    void OnApplicationQuit()
    {
        CerrarPuerto();
    }

    void OnDestroy()
    {
        CerrarPuerto();
    }

    void CerrarPuerto()
    {
        leyendo = false;

        if (hiloLectura != null && hiloLectura.IsAlive)
            hiloLectura.Join(500);

        if (puertoSerial != null && puertoSerial.IsOpen)
            puertoSerial.Close();
    }
}