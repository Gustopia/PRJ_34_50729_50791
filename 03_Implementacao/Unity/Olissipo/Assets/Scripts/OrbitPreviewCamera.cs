using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Orbital preview camera à la Fallout 4 / Fortnite character screen.
/// Suporta um único modelo ou vários filhos de um Empty GameObject.
/// Compatível com Unity 6 + New Input System.
/// </summary>
[RequireComponent(typeof(Camera))]
public class OrbitPreviewCamera : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  Inspector
    // ─────────────────────────────────────────────

    [Header("Target")]
    [Tooltip("Arrasta aqui o Transform do modelo (ou do Empty pai dos meshes).")]
    [SerializeField] private Transform target;

    [Header("Orbit Settings")]
    [SerializeField] private float orbitSensitivity = 0.4f;
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 60f;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSensitivity = 0.15f;
    [SerializeField]
    [Range(0.01f, 0.99f)]
    private float minZoomFactor = 0.25f;   // % do raio calculado
    [SerializeField]
    [Range(1.01f, 5f)]
    private float maxZoomFactor = 2.5f;    // % do raio calculado

    [Header("Smoothing")]
    [SerializeField][Range(1f, 30f)] private float orbitSmoothing = 12f;
    [SerializeField][Range(1f, 30f)] private float zoomSmoothing = 10f;

    [Header("Initial Angles")]
    [SerializeField] private float initialYaw = 0f;
    [SerializeField] private float initialPitch = 15f;

    // ─────────────────────────────────────────────
    //  Estado interno
    // ─────────────────────────────────────────────

    private Camera _cam;
    private Vector3 _pivotPoint;          // centro da bounding box
    private float _boundingRadius;      // raio calculado

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

    // ─────────────────────────────────────────────
    //  Unity lifecycle
    // ─────────────────────────────────────────────

    private void Awake()
    {
        EnsureCameraRef();
    }

    private void Start()
    {
        // Se SetTarget() foi chamado antes de Start() (ex.: pelo Bootstrapper no Awake),
        // _pendingTarget já foi guardado — aplica-o agora que tudo está inicializado.
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

    /// <summary>Garante que _cam está preenchida, mesmo antes do Awake.</summary>
    private void EnsureCameraRef()
    {
        if (_cam == null)
            _cam = GetComponent<Camera>();
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

    // ─────────────────────────────────────────────
    //  API Pública
    // ─────────────────────────────────────────────

    /// <summary>
    /// Define o alvo em runtime e recalcula os bounds.
    /// Pode ser chamado a qualquer momento — antes ou depois de Start().
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target == null) return;

        EnsureCameraRef();

        if (!_isInitialized)
        {
            // Start() ainda não correu: guarda para aplicar depois
            _pendingTarget = newTarget;
            return;
        }

        CalculateBounds(target);
        ResetAngles();
    }

    /// <summary>Repõe câmara na posição inicial sem mudar o alvo.</summary>
    public void ResetView()
    {
        ResetAngles();
    }

    // ─────────────────────────────────────────────
    //  Bounds
    // ─────────────────────────────────────────────

    private void CalculateBounds(Transform root)
    {
        // Recolhe todos os Renderers no root e nos seus filhos
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: false);

        if (renderers.Length == 0)
        {
            // Fallback: usa a posição do próprio Transform
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
        _boundingRadius = combined.extents.magnitude;   // diagonal do cubo / 2

        float minDist = _boundingRadius * minZoomFactor;
        float maxDist = _boundingRadius * maxZoomFactor;

        // Posição inicial: afasta a câmara de modo a ver o modelo todo
        float fovRad = _cam.fieldOfView * Mathf.Deg2Rad * 0.5f;
        float idealDist = (_boundingRadius / Mathf.Tan(fovRad)) * 1.1f;
        _targetDistance = Mathf.Clamp(idealDist, minDist, maxDist);
        _currentDistance = _targetDistance;
    }

    // ─────────────────────────────────────────────
    //  Ângulos iniciais
    // ─────────────────────────────────────────────

    private void ResetAngles()
    {
        _yaw = _currentYaw = initialYaw;
        _pitch = _currentPitch = Mathf.Clamp(initialPitch, minVerticalAngle, maxVerticalAngle);
    }

    // ─────────────────────────────────────────────
    //  Input (New Input System via polling)
    // ─────────────────────────────────────────────

    private void HandleInput()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        // ── Arrastar com botão esquerdo ──
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
            _pitch -= delta.y * orbitSensitivity;   // invertido: arrastar para cima = olhar para baixo
            _pitch = Mathf.Clamp(_pitch, minVerticalAngle, maxVerticalAngle);
        }

        // ── Zoom com roda do rato ──
        float scroll = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.001f)
        {
            float minDist = _boundingRadius * minZoomFactor;
            float maxDist = _boundingRadius * maxZoomFactor;

            _targetDistance -= scroll * zoomSensitivity * _boundingRadius;
            _targetDistance = Mathf.Clamp(_targetDistance, minDist, maxDist);
        }
    }

    // ─────────────────────────────────────────────
    //  Smoothing
    // ─────────────────────────────────────────────

    private void ApplySmoothing()
    {
        float t = Time.deltaTime;
        _currentYaw = Mathf.LerpAngle(_currentYaw, _yaw, orbitSmoothing * t);
        _currentPitch = Mathf.LerpAngle(_currentPitch, _pitch, orbitSmoothing * t);
        _currentDistance = Mathf.Lerp(_currentDistance, _targetDistance, zoomSmoothing * t);
    }

    // ─────────────────────────────────────────────
    //  Posicionamento da câmara
    // ─────────────────────────────────────────────

    private void UpdateCameraTransform()
    {
        if (target == null) return;

        Quaternion rotation = Quaternion.Euler(_currentPitch, _currentYaw, 0f);
        Vector3 offset = rotation * new Vector3(0f, 0f, -_currentDistance);

        transform.position = _pivotPoint + offset;
        transform.LookAt(_pivotPoint);
    }

    // ─────────────────────────────────────────────
    //  Gizmos (editor only)
    // ─────────────────────────────────────────────

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(_pivotPoint, _boundingRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(_pivotPoint, 0.04f);
    }
#endif
}