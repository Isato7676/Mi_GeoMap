using System.Collections.Generic;

public static class DatosPartida
{
    public class NivelResultado
    {
        public string tituloNivel;
        public int puntosObtenidos;
        public int puntosRequeridos;
        public bool superado;
    }

    public static List<NivelResultado> resultadosNiveles = new List<NivelResultado>();
    public static int PuntosTotalesPartida => ObtenerPuntosTotales();

    public static void Limpiar()
    {
        resultadosNiveles.Clear();
    }

    private static int ObtenerPuntosTotales()
    {
        int total = 0;
        foreach (var res in resultadosNiveles)
        {
            total += res.puntosObtenidos;
        }
        return total;
    }

    // Calcula el rango humorístico según el porcentaje sobre el total de puntos posibles
    public static (string titulo, string descripcion) ObtenerRangoJugador(int maxPuntosPosibles)
    {
        float porcentaje = maxPuntosPosibles > 0 ? ((float)PuntosTotalesPartida / maxPuntosPosibles) * 100f : 0f;

        if (porcentaje >= 90f)
            return ("Willy Fog / Magallanes", "¡Impresionante! Te conoces el planeta al milímetro. La Tierra no tiene secretos para ti.");
        else if (porcentaje >= 75f)
            return ("Trotamundos Experimentado", "Tienes una excelente orientación global. Pocas veces te desvías de la ruta.");
        else if (porcentaje >= 50f)
            return ("Turista de Mochila", "Te defiendes bien con el mapa, aunque algún viaje se te ha ido un poco de las manos.");
        else if (porcentaje >= 25f)
            return ("Peligro en el Centro Comercial", "Si vas a por pan, existe un riesgo real de que termines en otro municipio. Ve de la mano.");
        else
            return ("No salgas de casa", "Tu sentido de la orientación es legendario... por lo malo. ¡No te separes de tus padres!");
    }
}