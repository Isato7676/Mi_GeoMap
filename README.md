# 🗺️ Mi GeoMap - Mobile Geolocation Game (Android)

**Mi GeoMap** es un juego interactivo de geolocalización e itinerarios desarrollado en **Unity** para dispositivos Android. Los jugadores deben responder a preguntas sobre puntos geográficos reales colocando marcas sobre un mapa 2D, con cálculo de distancias en tiempo real y animación de resultados.

---

## 📱 Capturas del Juego

<p align="center">
  <img width="622" height="350" alt="Inicio" src="https://github.com/user-attachments/assets/69cd9aef-a057-4bf9-810a-27566a111dd6" />
  <img width="631" height="353" alt="juego" src="https://github.com/user-attachments/assets/47cbe329-8e31-47cd-a863-84e07195379e" />
</p>

---

## 🛠️ Arquitectura y Características Técnicas

- **Gestión de Estados y Controladores (`GameManager`):** Control del flujo global del juego mediante patrón *Singleton* blindado para dispositivos móviles.
- **Control de Cámara y Mapa (`MapController`):** Manipulación 2D avanzada con zoom táctil (*pinch-to-zoom*), arrastre (*drag*) suave con interpolación progresiva (`Mathf.Lerp`), rescate de coordenadas $Z$ y restricción de límites (*bounds clamping*).
- **Cálculo Geográfico en Tiempo Real (`GeoUtils`):** Conversión de coordenadas latitud/longitud a espacio local en el mapa 2D y cálculo de distancias en kilómetros mediante la fórmula de Haversine.
- **UI Responsiva en Canvas:** Interfaz ajustada mediante `Canvas Scaler` en `Scale With Screen Size` (1920x1080) independiente de la densidad de píxeles del teléfono.
- **Lectura de Datos Estructurada:** Parseo de niveles y preguntas desde archivos JSON dinámicos utilizando la carpeta `Resources`.

---

## ⚙️ Optimización para Android

- **API Gráfica:** Configurado exclusivamente en **OpenGLES3** para evitar problemas de compatibilidad con drivers Vulkan en diversos procesadores móviles.
- **Backend de Scripting:** Compilado en **IL2CPP** de **64 bits (ARM64)** garantizando máximo rendimiento.
- **Manejo Seguro de Inputs:** Prevención de excepciones por superposición de toques en UI (`IsPointerOverGameObject`) combinando compatibilidad con el sistema de entradas clásico.

---

## 🚀 Cómo probar el proyecto

1. Clona este repositorio:
   ```bash
   git clone [https://github.com/Isato7676/Mi_GeoMap.git](https://github.com/Isato7676/Mi_GeoMap.git)
