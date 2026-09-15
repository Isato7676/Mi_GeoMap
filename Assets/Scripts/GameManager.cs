using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public enum GameState
{
    LevelStartPanel,
    Playing,
    MarkPlaced,
    Evaluating,
    QuestionResult,
    LevelEndPanel
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Configuración de Tiempos")]
    public float tiempoZoom = 0.8f;
    public float tiempoLinea = 1.0f;

    [Header("Referencias de Mapa y Cámara")]
    public MapController mapController;
    public Camera mainCamera;
    public Transform mapaTransform;

    [Header("Marcadores y Visuales")]
    public GameObject userMarker;
    public GameObject realMarker;
    public LineRenderer lineRenderer;
    public RectTransform cuentakilometrosUI;
    public TextMeshProUGUI txtCuentakilometros;

    [Header("Paneles UI")]
    public RectTransform panelInicioNivel;
    public RectTransform panelFinNivel;
    public GameObject panelPreguntaSuperior;
    public GameObject panelInferiorStats;

    [Header("UI Textos Panel Inicio")]
    public TextMeshProUGUI txtTituloInicioNivel;
    public TextMeshProUGUI txtNumPreguntasInicio;
    public TextMeshProUGUI txtPuntosReqInicio;

    [Header("UI Textos Panel Fin")]
    public TextMeshProUGUI txtTituloFinNivel;
    public TextMeshProUGUI txtResumenPuntosFin;

    [Header("UI Pregunta e Info")]
    public TextMeshProUGUI txtPregunta;
    public TextMeshProUGUI txtPista;
    public Button btnComprarPista;
    public TextMeshProUGUI txtPuntosTotales;
    public TextMeshProUGUI txtPuntosNivelReq;
    public TextMeshProUGUI txtProgresoPreguntas;

    [Header("Botones Flotantes")]
    public RectTransform btnConfirmar;
    public RectTransform btnSiguiente;

    [Header("Botones de Paneles")]
    public Button btnEmpezarNivel;
    public Button btnFinNivelSiguiente;

    // Variables del Juego
    private JuegoDataConfig datosJuego;
    private List<PreguntaData> preguntasNivelSeleccionadas = new List<PreguntaData>();
    private int nivelActualIdx = 0;
    private int preguntaActualIdx = 0;
    private int puntosTotales = 0;
    private int puntosNivelActual = 0;
    private int monedas = 100;

    private GameState estadoActual;
    private Vector2 posMarcaUsuarioWorld;
    private Vector2 posMarcaRealWorld;
    private float distanciaCalculadaKM;

    private Vector3 clickDownPos;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        CentrarElementosDescolocados();
        CargarJSONData();

        // Protección crítica para Android: Solo arrancar si el JSON se cargó con éxito
        if (datosJuego != null && datosJuego.niveles != null && datosJuego.niveles.Count > 0)
        {
            InicializarEstadoPartida();
        }
        else
        {
            Debug.LogError("Error: No se pudieron cargar los datos del JSON en Resources/PreguntasData.");
        }
    }

    private void CentrarElementosDescolocados()
    {
        if (panelInicioNivel != null) panelInicioNivel.anchoredPosition = Vector2.zero;
        if (panelFinNivel != null) panelFinNivel.anchoredPosition = Vector2.zero;
        if (btnConfirmar != null) btnConfirmar.anchoredPosition = new Vector2(0, -350);
        if (btnSiguiente != null) btnSiguiente.anchoredPosition = new Vector2(0, -350);
    }

    private void CargarJSONData()
    {
        TextAsset jsonText = Resources.Load<TextAsset>("PreguntasData");
        if (jsonText != null)
        {
            datosJuego = JsonUtility.FromJson<JuegoDataConfig>(jsonText.text);
        }
        else
        {
            Debug.LogError("No se encontró el archivo PreguntasData en la carpeta Resources.");
        }
    }

    private void InicializarEstadoPartida()
    {
        puntosTotales = 0;
        nivelActualIdx = 0;
        DatosPartida.Limpiar();
        PrepararInicioNivel();
    }

    private void PrepararInicioNivel()
    {
        if (datosJuego == null || datosJuego.niveles == null || nivelActualIdx >= datosJuego.niveles.Count) return;

        puntosNivelActual = 0;
        preguntaActualIdx = 0;

        NivelData nivel = datosJuego.niveles[nivelActualIdx];

        if (nivel.preguntas != null)
        {
            List<PreguntaData> bancoCopia = new List<PreguntaData>(nivel.preguntas);
            preguntasNivelSeleccionadas.Clear();

            for (int i = 0; i < 2 && bancoCopia.Count > 0; i++)
            {
                int rndIdx = Random.Range(0, bancoCopia.Count);
                preguntasNivelSeleccionadas.Add(bancoCopia[rndIdx]);
                bancoCopia.RemoveAt(rndIdx);
            }
        }

        if (txtTituloInicioNivel) txtTituloInicioNivel.text = nivel.tituloNivel;
        if (txtNumPreguntasInicio) txtNumPreguntasInicio.text = "Preguntas: " + preguntasNivelSeleccionadas.Count;
        if (txtPuntosReqInicio) txtPuntosReqInicio.text = "Puntos Necesarios: " + nivel.puntosRequeridos;

        CambiarEstado(GameState.LevelStartPanel);
    }

    public void CambiarEstado(GameState nuevoEstado)
    {
        estadoActual = nuevoEstado;

        if (btnConfirmar) btnConfirmar.gameObject.SetActive(false);
        if (btnSiguiente) btnSiguiente.gameObject.SetActive(false);

        switch (estadoActual)
        {
            case GameState.LevelStartPanel:
                if (panelInicioNivel) panelInicioNivel.gameObject.SetActive(true);
                if (panelFinNivel) panelFinNivel.gameObject.SetActive(false);
                if (panelPreguntaSuperior) panelPreguntaSuperior.SetActive(false);
                if (panelInferiorStats) panelInferiorStats.SetActive(false);
                if (mapController) mapController.SetInteractionEnabled(false);
                OcultarMarcasYLineas();
                break;

            case GameState.Playing:
                if (panelInicioNivel) panelInicioNivel.gameObject.SetActive(false);
                if (panelFinNivel) panelFinNivel.gameObject.SetActive(false);
                if (panelPreguntaSuperior) panelPreguntaSuperior.SetActive(true);
                if (panelInferiorStats) panelInferiorStats.SetActive(true);
                if (mapController) mapController.SetInteractionEnabled(true);
                OcultarMarcasYLineas();
                CargarPreguntaUI();
                break;

            case GameState.MarkPlaced:
                if (mapController) mapController.SetInteractionEnabled(true);
                if (btnConfirmar) btnConfirmar.gameObject.SetActive(true);
                break;

            case GameState.Evaluating:
                if (mapController) mapController.SetInteractionEnabled(false);
                StartCoroutine(RutinaEvaluacionRespuesta());
                break;

            case GameState.QuestionResult:
                if (mapController) mapController.SetInteractionEnabled(false);
                if (btnSiguiente) btnSiguiente.gameObject.SetActive(true);
                if (cuentakilometrosUI) cuentakilometrosUI.gameObject.SetActive(true);
                break;

            case GameState.LevelEndPanel:
                if (panelInicioNivel) panelInicioNivel.gameObject.SetActive(false);
                if (panelFinNivel) panelFinNivel.gameObject.SetActive(true);
                if (panelPreguntaSuperior) panelPreguntaSuperior.SetActive(false);
                if (panelInferiorStats) panelInferiorStats.SetActive(false);
                if (mapController) mapController.SetInteractionEnabled(false);
                OcultarMarcasYLineas();

                if (datosJuego != null && nivelActualIdx < datosJuego.niveles.Count)
                {
                    NivelData n = datosJuego.niveles[nivelActualIdx];
                    bool superado = puntosNivelActual >= n.puntosRequeridos;
                    if (txtTituloFinNivel) txtTituloFinNivel.text = superado ? "¡Nivel Superado!" : "Nivel No Superado";
                    if (txtResumenPuntosFin) txtResumenPuntosFin.text = $"{puntosNivelActual} / {n.puntosRequeridos} Puntos";
                }
                break;
        }
    }

    private void CargarPreguntaUI()
    {
        if (datosJuego == null || preguntasNivelSeleccionadas.Count == 0 || preguntaActualIdx >= preguntasNivelSeleccionadas.Count) return;

        NivelData nivel = datosJuego.niveles[nivelActualIdx];
        PreguntaData p = preguntasNivelSeleccionadas[preguntaActualIdx];

        if (txtPregunta) txtPregunta.text = p.pregunta;

        bool pistaActiva = nivel.pistaActivaPorDefecto;
        if (txtPista)
        {
            txtPista.gameObject.SetActive(pistaActiva);
            txtPista.text = "Pista: " + p.pista;
        }
        if (btnComprarPista) btnComprarPista.gameObject.SetActive(!pistaActiva);

        ActualizarStatsUI();
    }

    private void ActualizarStatsUI()
    {
        if (datosJuego == null || nivelActualIdx >= datosJuego.niveles.Count) return;

        NivelData nivel = datosJuego.niveles[nivelActualIdx];
        if (txtPuntosTotales) txtPuntosTotales.text = "Total: " + puntosTotales;
        if (txtPuntosNivelReq) txtPuntosNivelReq.text = $"Nivel: {puntosNivelActual}/{nivel.puntosRequeridos}";
        if (txtProgresoPreguntas) txtProgresoPreguntas.text = $"Pregunta: {preguntaActualIdx + 1}/{preguntasNivelSeleccionadas.Count}";
    }

    void Update()
    {
        if (estadoActual == GameState.Playing || estadoActual == GameState.MarkPlaced)
        {
            // Verificación totalmente segura de UI para Android y PC
            bool estaSobreUI = false;
            if (EventSystem.current != null)
            {
                estaSobreUI = EventSystem.current.IsPointerOverGameObject();
                if (Input.touchCount > 0)
                {
                    estaSobreUI = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
                }
            }

            if (Input.GetMouseButtonDown(0) && !estaSobreUI)
            {
                clickDownPos = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(0) && !estaSobreUI)
            {
                float distanciaArrastre = Vector3.Distance(clickDownPos, Input.mousePosition);

                if (distanciaArrastre < 10f && mainCamera != null)
                {
                    Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                    worldPos.z = 0;

                    posMarcaUsuarioWorld = worldPos;
                    if (userMarker)
                    {
                        userMarker.transform.position = posMarcaUsuarioWorld;
                        userMarker.SetActive(true);
                    }

                    if (estadoActual != GameState.MarkPlaced)
                    {
                        CambiarEstado(GameState.MarkPlaced);
                    }
                }
            }
        }
    }

    public void OnBtnEmpezarNivelClicked() => CambiarEstado(GameState.Playing);
    public void OnBtnConfirmarClicked() => CambiarEstado(GameState.Evaluating);

    public void OnBtnSiguienteClicked()
    {
        preguntaActualIdx++;

        if (preguntaActualIdx < preguntasNivelSeleccionadas.Count)
        {
            ResetearCamaraYMapa();
            CambiarEstado(GameState.Playing);
        }
        else
        {
            CambiarEstado(GameState.LevelEndPanel);
        }
    }

    public void OnBtnFinNivelSiguienteClicked()
    {
        if (datosJuego != null && nivelActualIdx < datosJuego.niveles.Count)
        {
            NivelData nivel = datosJuego.niveles[nivelActualIdx];

            DatosPartida.resultadosNiveles.Add(new DatosPartida.NivelResultado
            {
                tituloNivel = nivel.tituloNivel,
                puntosObtenidos = puntosNivelActual,
                puntosRequeridos = nivel.puntosRequeridos,
                superado = puntosNivelActual >= nivel.puntosRequeridos
            });

            if (puntosNivelActual >= nivel.puntosRequeridos && (nivelActualIdx + 1) < datosJuego.niveles.Count)
            {
                nivelActualIdx++;
                ResetearCamaraYMapa();
                PrepararInicioNivel();
            }
            else
            {
                SceneManager.LoadScene("EscenaFinJuego");
            }
        }
    }

    private IEnumerator RutinaEvaluacionRespuesta()
    {
        if (preguntasNivelSeleccionadas.Count == 0 || preguntaActualIdx >= preguntasNivelSeleccionadas.Count) yield break;

        PreguntaData p = preguntasNivelSeleccionadas[preguntaActualIdx];

        if (mapaTransform == null) yield break;

        Vector2 mapBoundsSize = mapaTransform.GetComponent<SpriteRenderer>().bounds.size;

        Vector2 posMapaLocalReal = GeoUtils.GeoToMapPoint(p.latitud, p.longitud, mapBoundsSize);
        posMarcaRealWorld = mapaTransform.TransformPoint(posMapaLocalReal);

        if (realMarker)
        {
            realMarker.transform.position = posMarcaRealWorld;
            realMarker.SetActive(true);
        }

        Vector3 centro = (posMarcaUsuarioWorld + posMarcaRealWorld) / 2f;
        if (mainCamera) centro.z = mainCamera.transform.position.z;

        float deltaX = Mathf.Abs(posMarcaUsuarioWorld.x - posMarcaRealWorld.x);
        float deltaY = Mathf.Abs(posMarcaUsuarioWorld.y - posMarcaRealWorld.y);

        float aspect = mainCamera ? mainCamera.aspect : 1.77f;

        float reqSizeY = (deltaY / 2f) + 1.2f;
        float reqSizeX = ((deltaX / 2f) + 1.2f) / aspect;

        float targetOrthographicSize = Mathf.Max(reqSizeY, reqSizeX);
        if (mapController) targetOrthographicSize = Mathf.Clamp(targetOrthographicSize, mapController.minZoom, mapController.maxZoom);

        Vector3 camStartPos = mainCamera ? mainCamera.transform.position : Vector3.zero;
        float camStartSize = mainCamera ? mainCamera.orthographicSize : 5f;

        float tiempo = 0f;
        while (tiempo < tiempoZoom)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / tiempoZoom;
            if (mainCamera)
            {
                mainCamera.transform.position = Vector3.Lerp(camStartPos, centro, t);
                mainCamera.orthographicSize = Mathf.Lerp(camStartSize, targetOrthographicSize, t);
            }
            if (mapController) mapController.ClampCamera();
            yield return null;
        }

        if (lineRenderer)
        {
            lineRenderer.gameObject.SetActive(true);
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, posMarcaRealWorld);
            lineRenderer.SetPosition(1, posMarcaRealWorld);
        }

        Vector2 localUserPos = mapaTransform.InverseTransformPoint(posMarcaUsuarioWorld);
        Vector2 geoUsuario = GeoUtils.MapPointToGeo(localUserPos, mapBoundsSize);
        distanciaCalculadaKM = GeoUtils.CalcularDistanciaKM(geoUsuario, new Vector2(p.latitud, p.longitud));
        int puntosGanados = GeoUtils.CalcularPuntos(distanciaCalculadaKM);

        puntosNivelActual += puntosGanados;
        puntosTotales += puntosGanados;

        PosicionarCuentakilometrosUI(posMarcaRealWorld, posMarcaUsuarioWorld);
        if (cuentakilometrosUI) cuentakilometrosUI.gameObject.SetActive(true);

        tiempo = 0f;
        while (tiempo < tiempoLinea)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / tiempoLinea;

            Vector3 posLineaInterp = Vector3.Lerp(posMarcaRealWorld, posMarcaUsuarioWorld, t);
            if (lineRenderer) lineRenderer.SetPosition(1, posLineaInterp);

            float kmAnimados = Mathf.Lerp(0, distanciaCalculadaKM, t);
            if (txtCuentakilometros) txtCuentakilometros.text = $"{kmAnimados:F0} km\n(+{Mathf.RoundToInt(puntosGanados * t)} pts)";

            yield return null;
        }

        ActualizarStatsUI();
        CambiarEstado(GameState.QuestionResult);
    }

    private void PosicionarCuentakilometrosUI(Vector2 posA, Vector2 posB)
    {
        Vector2 puntoMedio = (posA + posB) / 2f;
        Vector2 dir = (posB - posA).normalized;
        Vector2 normal = new Vector2(-dir.y, dir.x);

        Vector2 posFinalWorld = puntoMedio + normal * 1.5f;

        if (mainCamera && cuentakilometrosUI)
        {
            Vector2 screenPos = mainCamera.WorldToScreenPoint(posFinalWorld);
            cuentakilometrosUI.position = screenPos;
        }
    }

    private void ResetearCamaraYMapa()
    {
        // Notificar al controlador del mapa que limpie sus variables de objetivo
        if (mapController != null)
        {
            mapController.ResetMapPosition();
        }
        else if (mainCamera != null)
        {
            mainCamera.transform.position = new Vector3(0, 0, -10);
        }

        OcultarMarcasYLineas();
    }

    private void OcultarMarcasYLineas()
    {
        if (userMarker) userMarker.SetActive(false);
        if (realMarker) realMarker.SetActive(false);
        if (lineRenderer) lineRenderer.gameObject.SetActive(false);
        if (cuentakilometrosUI) cuentakilometrosUI.gameObject.SetActive(false);
    }

    public void OnBtnComprarPistaClicked()
    {
        int costoPista = 20;
        if (monedas >= costoPista)
        {
            monedas -= costoPista;
            if (txtPista) txtPista.gameObject.SetActive(true);
            if (btnComprarPista) btnComprarPista.gameObject.SetActive(false);
        }
    }
}