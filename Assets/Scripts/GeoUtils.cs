using UnityEngine;

public static class GeoUtils
{
    private const float EARTH_RADIUS_KM = 6371f;

    // Convierte Latitud/Longitud a Puntos en el espacio local del Mapa (Centrado en 0,0)
    // Ancho total en unidades = mapBoundsSize.x | Alto total = mapBoundsSize.y
    public static Vector2 GeoToMapPoint(float lat, float lon, Vector2 mapBoundsSize)
    {
        // Longitud: [-180, +180] -> Mapeado de -width/2 a +width/2
        float x = (lon / 180f) * (mapBoundsSize.x / 2f);

        // Latitud: [+90, -90] -> Mapeado de +height/2 (Norte) a -height/2 (Sur)
        float y = (lat / 90f) * (mapBoundsSize.y / 2f);

        return new Vector2(x, y);
    }

    // Convierte Punto Local del Mapa a Latitud/Longitud
    public static Vector2 MapPointToGeo(Vector2 localPoint, Vector2 mapBoundsSize)
    {
        float lon = (localPoint.x / (mapBoundsSize.x / 2f)) * 180f;
        float lat = (localPoint.y / (mapBoundsSize.y / 2f)) * 90f;

        return new Vector2(lat, lon);
    }

    // Distancia real en Kilómetros (Fórmula Haversine)
    public static float CalcularDistanciaKM(Vector2 geoA, Vector2 geoB)
    {
        float dLat = (geoB.x - geoA.x) * Mathf.Deg2Rad;
        float dLon = (geoB.y - geoA.y) * Mathf.Deg2Rad;

        float lat1 = geoA.x * Mathf.Deg2Rad;
        float lat2 = geoB.x * Mathf.Deg2Rad;

        float a = Mathf.Sin(dLat / 2f) * Mathf.Sin(dLat / 2f) +
                  Mathf.Sin(dLon / 2f) * Mathf.Sin(dLon / 2f) * Mathf.Cos(lat1) * Mathf.Cos(lat2);

        float c = 2f * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1f - a));
        return EARTH_RADIUS_KM * c;
    }

    // Puntuación en función de la distancia
    public static int CalcularPuntos(float distanciaKM)
    {
        float maxDistancia = 5000f;
        if (distanciaKM >= maxDistancia) return 0;

        float factor = 1f - (distanciaKM / maxDistancia);
        return Mathf.RoundToInt(1000f * Mathf.Pow(factor, 2));
    }
}