using System;
using System.Collections.Generic;

[Serializable]
public class PreguntaData
{
    public string id;
    public string pregunta;
    public string pista;
    public float latitud;
    public float longitud;
}

[Serializable]
public class NivelData
{
    public int numeroNivel;
    public string tituloNivel;
    public int puntosRequeridos;
    public bool pistaActivaPorDefecto;
    public List<PreguntaData> preguntas;
}

[Serializable]
public class JuegoDataConfig
{
    public List<NivelData> niveles;
}