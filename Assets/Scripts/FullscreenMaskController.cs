using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;

public class FullscreenMaskController : MonoBehaviour
{
    [Header("Pipeline Control")]
    [SerializeField] private ScriptableRendererData rendererData;

    [SerializeField] private string featureName = "Fullscreen Mask";
    [SerializeField] private Material maskMaterialAsset;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform originTransform;

    [Header("Mask Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float maskAmount = 0f;

    [Tooltip("Value is applied in UV space")]
    [SerializeField] private Vector2 originOffset = new Vector2(0f, 0f);

    [Header("Noise & Jitter")]
    [SerializeField] private Vector2 noiseTiling = new Vector2(10f, 10f);

    [SerializeField] private Vector2 baseNoiseFlow = new Vector2(0.2f, 0.5f);
    [SerializeField] private bool enableJitter = true;
    [SerializeField] private float jitterFrequency = 2f;
    [SerializeField] private float jitterAmplitude = 0.5f;

    [Header("Edge Polish")]
    [Range(0f, 0.5f)]
    [SerializeField] private float edgeDistortion = 0.05f;

    [Range(0f, 0.5f)]
    [SerializeField] private float edgeSoftness = 0.1f;

    [Range(0f, 0.1f)]
    [SerializeField] private float edgeWidth = 0.02f;

    [ColorUsage(true, true)]
    [SerializeField] private Color edgeColor = new Color(2f, 0.5f, 0f, 1f);

    [Header("Lighting Control")]
    [Tooltip("The 2D light you want to hide from a specific camera.")]
    [SerializeField] private Light2D lightToHide;

    [ShowNonSerializedField] private Vector2 currentNoiseSpeed;
    [ShowNonSerializedField] private Vector3 originScreenPoint;

    private int maskAmountId,
        noiseTilingId,
        noiseSpeedId,
        edgeDistId,
        edgeSoftId,
        edgeWidthId,
        edgeColorId,
        originId,
        aspectRatioId;

    // The actual reference to the feature in your Project Asset
    private ScriptableRendererFeature maskFeature;
    private Material runtimeMaterialInstance;
    private Material originalFeatureMaterial;
    private float randomOffsetX, randomOffsetY;

    public float MaskAmount => maskAmount;

    public float CurrentViewportRadius { get; private set; }

    private void OnEnable()
    {
        // Subscribe to the render loop
        RenderPipelineManager.beginCameraRendering += OnBeginCamera;
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent errors
        RenderPipelineManager.beginCameraRendering -= OnBeginCamera;

        if (maskFeature != null)
        {
            maskFeature.SetActive(false);
            if (maskFeature is FullScreenPassRendererFeature fsf)
                fsf.passMaterial = originalFeatureMaterial;
        }

        if (runtimeMaterialInstance != null)
        {
            Destroy(runtimeMaterialInstance);
        }
    }

    private void Awake()
    {
        CacheIDs();
        randomOffsetX = Random.Range(0f, 100f);
        randomOffsetY = Random.Range(100f, 200f);

        if (maskMaterialAsset != null)
            runtimeMaterialInstance = new Material(maskMaterialAsset);

        // Find the feature once and store it
        if (rendererData == null) return;
        maskFeature = rendererData.rendererFeatures.Find(f => f.name == featureName);

        if (maskFeature is not FullScreenPassRendererFeature fsf) return;
        originalFeatureMaterial = fsf.passMaterial;
        if (runtimeMaterialInstance != null)
            fsf.passMaterial = runtimeMaterialInstance;
    }

    private void CacheIDs()
    {
        maskAmountId = Shader.PropertyToID("_MaskAmount");
        noiseTilingId = Shader.PropertyToID("_NoiseTiling");
        noiseSpeedId = Shader.PropertyToID("_NoiseSpeed");
        edgeDistId = Shader.PropertyToID("_EdgeDistortion");
        edgeSoftId = Shader.PropertyToID("_EdgeSoftness");
        edgeWidthId = Shader.PropertyToID("_EdgeWidth");
        edgeColorId = Shader.PropertyToID("_EdgeColor");
        originId = Shader.PropertyToID("_Origin");
        aspectRatioId = Shader.PropertyToID("_AspectRatio");
    }

    private void OnBeginCamera(ScriptableRenderContext context, Camera cam)
    {
        if (maskFeature == null) return;

        // This fixes an issue where the edge color is bleeding into the alternate camera texture
        // by disabling the shader while we are rendering the alternate camera

        // If the camera is the Main Camera AND we have the mask open, turn it ON.
        // If the camera is the Alternate Camera (or Scene View), turn it OFF.
        if (cam == mainCamera && maskAmount > 0)
        {
            maskFeature.SetActive(true);
        }
        else
        {
            maskFeature.SetActive(false);
        }

        if (lightToHide != null)
        {
            // If the camera currently drawing is the Main Camera, turn the light OFF.
            if (cam == mainCamera)
            {
                lightToHide.enabled = false;
            }
            else
            {
                lightToHide.enabled = true;
            }
        }
    }

    private void Update()
    {
        HandleJitter();
        UpdateOrigin();
        UpdateShaderProperties();
    }

    private void HandleJitter()
    {
        if (enableJitter)
        {
            float jitterX = (Mathf.PerlinNoise(Time.time * jitterFrequency, randomOffsetX) * 2f) - 1f;
            float jitterY = (Mathf.PerlinNoise(Time.time * jitterFrequency, randomOffsetY) * 2f) - 1f;
            currentNoiseSpeed = baseNoiseFlow + (new Vector2(jitterX, jitterY) * jitterAmplitude);
        }
        else currentNoiseSpeed = baseNoiseFlow;
    }

    private void UpdateOrigin()
    {
        originScreenPoint = (originTransform && mainCamera)
            ? mainCamera.WorldToViewportPoint(originTransform.position)
            : new Vector3(0.5f, 0.5f, 0);
    }

    private void UpdateShaderProperties()
    {
        if (!runtimeMaterialInstance || !mainCamera) return;

        // start with base mask amount (0 to 1)
        float finalMaskAmount = maskAmount;
        // If the object is in front of the camera
        if (originScreenPoint.z > 0)
        {
            // 1. Calculate Frustum dimensions at this depth
            float frustumHeight = 2.0f * originScreenPoint.z * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float frustumWidth = frustumHeight * mainCamera.aspect;

            // --- THE OFF-CENTER FIX ---
            // 2. Find the longest physical distance from the anchor to the left/right and top/bottom edges
            float maxDistX = Mathf.Max(originScreenPoint.x, 1f - originScreenPoint.x) * frustumWidth;
            float maxDistY = Mathf.Max(originScreenPoint.y, 1f - originScreenPoint.y) * frustumHeight;

            // 3. Use Pythagorean theorem to find the distance to the farthest corner
            float maxWorldRadius = Mathf.Sqrt((maxDistX * maxDistX) + (maxDistY * maxDistY));

            // 4. Scale by our slider
            float dynamicWorldRadius = maxWorldRadius * maskAmount * 1.2f;

            // 5. Convert back to Viewport Space for the Shader
            float fovScale = 1f / (2f * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad));
            CurrentViewportRadius = (dynamicWorldRadius * fovScale) / originScreenPoint.z;

            finalMaskAmount = CurrentViewportRadius;
        }

        runtimeMaterialInstance.SetFloat(maskAmountId, finalMaskAmount);
        runtimeMaterialInstance.SetVector(noiseTilingId, noiseTiling);
        runtimeMaterialInstance.SetVector(noiseSpeedId, currentNoiseSpeed);
        runtimeMaterialInstance.SetFloat(edgeDistId, edgeDistortion);
        runtimeMaterialInstance.SetFloat(edgeSoftId, edgeSoftness);
        runtimeMaterialInstance.SetFloat(edgeWidthId, edgeWidth);
        runtimeMaterialInstance.SetColor(edgeColorId, edgeColor);
        runtimeMaterialInstance.SetVector(originId,
            new Vector2(originScreenPoint.x, originScreenPoint.y) + originOffset);
        runtimeMaterialInstance.SetFloat(aspectRatioId, mainCamera.aspect);
    }

    public void SetMaskAmount(float amount)
    {
        maskAmount = amount;
    }
}