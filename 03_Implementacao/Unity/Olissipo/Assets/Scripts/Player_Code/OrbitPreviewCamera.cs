/*
 * Este script permite que a câmera orbite em torno de um alvo (target) com controle de rotação e zoom.
 * Calcula automaticamente os bounds do alvo para definir a distância inicial da câmera.
 * 
 * Funcionalidades:
 * - Arrastar com o botão esquerdo do rato para orbitar ao redor do alvo.
 * - Usar scroll para dar zoom in/out.
 * - Respeita limites de ângulo vertical e distância de zoom.
 * - Pode ser usado em runtime para definir um novo alvo com SetTarget().
 * 
 * Requisitos:
 * - O GameObject deve ter um componente Camera.
 */

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class OrbitPreviewCamera : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Transform do modelo ou do Empty parent")]
    [SerializeField] private Transform target;

    [Header("Orbit Settings")]
    [SerializeField] private float orbitSensitivity = 0.4f;
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 60f;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSensitivity = 0.15f;
    [SerializeField]
    [Range(0.01f, 0.99f)]
    private float minZoomFactor = 0.25f; // Percentagem do raio calculado
    [SerializeField]
    [Range(1.01f, 5f)]
    private float maxZoomFactor = 2.5f; // Percentagem do raio calculado

    [Header("Smoothing")]
    [SerializeField][Range(1f, 30f)] private float orbitSmoothing = 12f;
    [SerializeField][Range(1f, 30f)] private float zoomSmoothing = 10f;

    [Header("Initial Angles")]
    [SerializeField] private float initialYaw = 0f;
    [SerializeField] private float initialPitch = 15f;

    private Camera _cam;
    private Vector3 _pivotPoint; // Centro da bounding box
    private float _boundingRadius; // Raio calculado

    private float _yaw;
    private float _pitch;
    private float _currentYaw;
    private float _currentPitch;

    private float _targetDistance;
    private float _currentDistance;

    private bool _isDragging;
    private Vector2 _lastMousePos;

    // Guarda o alvo se SetTarget for chamado antes de Start()
    private Transform _pendingTarget;
    private bool _isInitialized;

    private void Awake()
    {
        EnsureCameraRef();
    }

    private void Start()
    {
        // Se SetTarget() foi chamado antes de Start(),
        // _pendingTarget já foi guardado e aplica-o agora que tudo está inicializado
        if (_pendingTarget != null)
        {
            CalculateBounds(_pendingTarget);
            _pendingTarget = null;
        }
        else if (target != null)
        {
            CalculateBounds(target);
        }

        ResetAngles();
        _isInitialized = true;
    }

    private void Update()
    {
        HandleInput();
        ApplySmoothing();
        UpdateCameraTransform();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LevelManager.Instance.LoadScene("Menu", "CrossFade");
        }
    }

    // Define o alvo em runtime e recalcula os bounds
    // Pode ser chamado a qualquer momento, antes ou depois de Start()
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target == null) return;

        EnsureCameraRef();

        if (!_isInitialized)
        {
            // Se Start() ainda não correu, guarda para aplicar depois
            _pendingTarget = newTarget;
            return;
        }

        CalculateBounds(target);
        ResetAngles();
    }

    // Mete a camara na posição inicial sem mudar o alvo
    public void ResetView()
    {
        ResetAngles();
    }

    private void EnsureCameraRef()
    {
        if (_cam == null)
            _cam = GetComponent<Camera>();
    }

    private void CalculateBounds(Transform root)
    {
        // Recolhe todos os renderers no root e nos seus filhos
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: false);

        if (renderers.Length == 0)
        {
            // Usa a posição do próprio transform
            _pivotPoint = root.position;
            _boundingRadius = 1f;
            Debug.LogWarning($"[OrbitPreviewCamera] Nenhum Renderer encontrado em '{root.name}'. A usar bounds padrão.");
            return;
        }

        // Calcula bounds unificados em world space
        Bounds combined = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            combined.Encapsulate(renderers[i].bounds);

        _pivotPoint = combined.center;
        _boundingRadius = combined.extents.magnitude; // Diagonal do cubo / 2

        float minDist = _boundingRadius * minZoomFactor;
        float maxDist = _boundingRadius * maxZoomFactor;

        // Posição inicial, afasta a camara de modo a ver o modelo todo
        float fovRad = _cam.fieldOfView * Mathf.Deg2Rad * 0.5f;
        float idealDist = (_boundingRadius / Mathf.Tan(fovRad)) * 1.1f;
        _targetDistance = Mathf.Clamp(idealDist, minDist, maxDist);
        _currentDistance = _targetDistance;
    }

    private void ResetAngles()
    {
        _yaw = _currentYaw = initialYaw;
        _pitch = _currentPitch = Mathf.Clamp(initialPitch, minVerticalAngle, maxVerticalAngle);
    }
    private void HandleInput()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        // Arrastar com botão esquerdo do rato
        if (mouse.leftButton.wasPressedThisFrame)
        {
            _isDragging = true;
            _lastMousePos = mouse.position.ReadValue();
        }
        if (mouse.leftButton.wasReleasedThisFrame)
            _isDragging = false;

        if (_isDragging)
        {
            Vector2 currentPos = mouse.position.ReadValue();
            Vector2 delta = currentPos - _lastMousePos;
            _lastMousePos = currentPos;

            _yaw += delta.x * orbitSensitivity;
            _pitch -= delta.y * orbitSensitivity;   // Invertido, ou seja, arrastar para cima = olhar para baixo
            _pitch = Mathf.Clamp(_pitch, minVerticalAngle, maxVerticalAngle);
        }

        // Zoom com roda do rato
        float scroll = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.001f)
        {
            float minDist = _boundingRadius * minZoomFactor;
            float maxDist = _boundingRadius * maxZoomFactor;

            _targetDistance -= scroll * zoomSensitivity * _boundingRadius;
            _targetDistance = Mathf.Clamp(_targetDistance, minDist, maxDist);
        }
    }

    private void ApplySmoothing()
    {
        float t = Time.deltaTime;
        _currentYaw = Mathf.LerpAngle(_currentYaw, _yaw, orbitSmoothing * t);
        _currentPitch = Mathf.LerpAngle(_currentPitch, _pitch, orbitSmoothing * t);
        _currentDistance = Mathf.Lerp(_currentDistance, _targetDistance, zoomSmoothing * t);
    }

    private void UpdateCameraTransform()
    {
        if (target == null) return;

        Quaternion rotation = Quaternion.Euler(_currentPitch, _currentYaw, 0f);
        Vector3 offset = rotation * new Vector3(0f, 0f, -_currentDistance);

        transform.position = _pivotPoint + offset;
        transform.LookAt(_pivotPoint);
    }
}