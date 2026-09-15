using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class FinJuegoSceneController : MonoBehaviour
{
    [Header("UI Textos")]
    public TextMeshProUGUI txtRangoTitulo;
    public TextMeshProUGUI txtRangoDescripcion;
    public TextMeshProUGUI txtResumenTabla;

    void Start()
    {
        MostrarResumenPartida();
    }

    private void MostrarResumenPartida()
    {
        // Si no hay datos guardados (por ejemplo, al probar la escena suelta directamente)
        if (DatosPartida.resultadosNiveles == null || DatosPartida.resultadosNiveles.Count == 0)
        {
            if (txtRangoTitulo) txtRangoTitulo.text = "SIN DATOS DE PARTIDA";
            if (txtRangoDescripcion) txtRangoDescripcion.text = "Juega una partida desde la portada para ver tus resultados.";
            if (txtResumenTabla) txtResumenTabla.text = "";
            return;
        }

        // Puntos máximos posibles (2 preguntas por nivel * 1000 pts = 2000 por nivel)
        int maxPuntosPosibles = DatosPartida.resultadosNiveles.Count * 2000;

        var (titulo, descripcion) = DatosPartida.ObtenerRangoJugador(maxPuntosPosibles);

        if (txtRangoTitulo) txtRangoTitulo.text = "RANGO: " + titulo;
        if (txtRangoDescripcion) txtRangoDescripcion.text = descripcion;

        // Construcción de la tabla resumen
        string tabla = "<b>RESUMEN DE LA PARTIDA:</b>\n\n";
        foreach (var res in DatosPartida.resultadosNiveles)
        {
            string estado = res.superado ? "<color=#22FF22>[SUPERADO]</color>" : "<color=#FF2222>[NO SUPERADO]</color>";
            tabla += $"{res.tituloNivel}: {res.puntosObtenidos} / {res.puntosRequeridos} pts  {estado}\n";
        }

        tabla += $"\n<b>PUNTOS TOTALES: {DatosPartida.PuntosTotalesPartida}</b>";

        if (txtResumenTabla) txtResumenTabla.text = tabla;
    }

    public void OnBtnSalirClicked()
    {
        SceneManager.LoadScene("EscenaInicio");
    }
}