using UnityEngine;
using UnityEngine.EventSystems;

public class MapController : MonoBehaviour
{
    [Header("Límites y Cámara")]
    public SpriteRenderer mapSpriteRenderer;
    public Camera mainCamera;

    [Header("Ajustes de Zoom")]
    public float zoomSpeedMouse = 2f;
    public float zoomSpeedTouch = 0.05f;
    public float minZoom = 1.0f;
    public float maxZoom = 10.24f;
    public float zoomSmoothness = 10f;
    private float initialZoom; // Guardará automáticamente el tamaño de inicio de la escena

    [Header("Ajustes de Arrastre")]
    public float dragSmoothness = 12f;

    private Vector3 touchStartPos;
    private Vector3 targetCameraPos;
    private float targetOrthographicSize;
    private bool isDragging = false;
    private bool canInteract = false;


    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        if (mainCamera != null)
        {
            // Guardar el tamaño exacto que le diste a la cámara en el editor
            initialZoom = mainCamera.orthographicSize;
            targetOrthographicSize = initialZoom;
            targetCameraPos = mainCamera.transform.position;
        }
    }

    public void SetInteractionEnabled(bool state)
    {
        canInteract = state;
        if (!state) isDragging = false;
    }

    void Update()
    {
        if (!canInteract || mainCamera == null) return;

        // Evitar mover el mapa si interactuamos con elementos de la UI (verificación segura)
        if (EventSystem.current != null)
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    return;
            }
        }

        HandleZoom();
        HandleDrag();
        AplicarSuavizado();
    }

    private void HandleDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            touchStartPos = GetWorldPosWithCamZ(Input.mousePosition);
            targetCameraPos = mainCamera.transform.position;
            isDragging = true;
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 currentWorldPos = GetWorldPosWithCamZ(Input.mousePosition);
            Vector3 direction = touchStartPos - currentWorldPos;

            // Mantener Z constante para no descolocar la cámara en Android
            targetCameraPos += new Vector3(direction.x, direction.y, 0);
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    private Vector3 GetWorldPosWithCamZ(Vector3 screenPos)
    {
        // Conserva la distancia Z entre la cámara y el plano 0
        screenPos.z = -mainCamera.transform.position.z;
        return mainCamera.ScreenToWorldPoint(screenPos);
    }

    private void HandleZoom()
    {
        // 1. Zoom con Rueda del Ratón (PC / Editor)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            targetOrthographicSize -= scroll * zoomSpeedMouse;
        }

        // 2. Zoom Gestual / Táctil (Pinch to Zoom en Móvil)
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;

            targetOrthographicSize -= difference * zoomSpeedTouch;
        }

        targetOrthographicSize = Mathf.Clamp(targetOrthographicSize, minZoom, maxZoom);
    }

    private void AplicarSuavizado()
    {
        if (mainCamera == null) return;

        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetOrthographicSize, Time.deltaTime * zoomSmoothness);
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetCameraPos, Time.deltaTime * dragSmoothness);

        ClampCamera();
    }

    public void ClampCamera()
    {
        if (mapSpriteRenderer == null || mainCamera == null) return;

        Bounds bounds = mapSpriteRenderer.bounds;

        float vertExtent = mainCamera.orthographicSize;
        float horizExtent = vertExtent * mainCamera.aspect;

        float minX = bounds.min.x + horizExtent;
        float maxX = bounds.max.x - horizExtent;
        float minY = bounds.min.y + vertExtent;
        float maxY = bounds.max.y - vertExtent;

        // Protección adicional por si el zoom es más grande que el mapa
        if (minX > maxX) minX = maxX = bounds.center.x;
        if (minY > maxY) minY = maxY = bounds.center.y;

        Vector3 pos = mainCamera.transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        mainCamera.transform.position = pos;
        targetCameraPos.x = Mathf.Clamp(targetCameraPos.x, minX, maxX);
        targetCameraPos.y = Mathf.Clamp(targetCameraPos.y, minY, maxY);
    }

    public void ResetMapPosition()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        if (mainCamera != null)
        {
            // Restaurar exactamente al tamaño y posición con los que arrancó el juego
            targetOrthographicSize = initialZoom;
            targetCameraPos = new Vector3(0, 0, -10);

            mainCamera.orthographicSize = initialZoom;
            mainCamera.transform.position = targetCameraPos;

            ClampCamera();
        }
    }

}