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
    [SerializeField] private Camera mainCamera; // Drag your Main Camera here
    [SerializeField] private Transform originTransform;

    [Header("Transition Settings")]
    [SerializeField] private float transitionSpeed = 5f;
    [Range(0f, 1f)] public float targetMaskAmount = 0f;

    [Header("Noise & Jitter")]
    public Vector2 noiseTiling = new Vector2(10f, 10f);
    public Vector2 baseNoiseFlow = new Vector2(0.2f, 0.5f);
    public bool enableJitter = true;
    public float jitterFrequency = 2f;
    public float jitterAmplitude = 0.5f;
    
    [Header("Current Live Data")]
    public Vector2 currentNoiseSpeed;
    public Vector2 originScreenPoint;
    
    [Header("Edge Polish")]
    [Range(0f, 0.5f)] public float edgeDistortion = 0.05f;
    [Range(0f, 0.5f)] public float edgeSoftness = 0.1f;
    [Range(0f, 0.1f)] public float edgeWidth = 0.02f;
    [ColorUsage(true, true)] public Color edgeColor = new Color(2f, 0.5f, 0f, 1f);

    public float CurrentMaskAmount { get; private set; }

    private int shaderMaskId, noiseTilingId, noiseSpeedId, edgeDistId, edgeSoftId, edgeWidthId, edgeColorId, originId;
    
    // The actual reference to the feature in your Project Asset
    private ScriptableRendererFeature maskFeature;
    private Material runtimeMaterialInstance;
    private Material originalFeatureMaterial;
    private float randomOffsetX, randomOffsetY;
    // private bool wasFullyClosed = true;

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
        if (runtimeMaterialInstance != null) Destroy(runtimeMaterialInstance);
    }

    private void Awake()
    {
        CacheIDs();
        randomOffsetX = Random.Range(0f, 100f);
        randomOffsetY = Random.Range(100f, 200f);

        if (maskMaterialAsset != null)
            runtimeMaterialInstance = new Material(maskMaterialAsset);

        // Find the feature once and store it
        if (rendererData != null)
        {
            maskFeature = rendererData.rendererFeatures.Find(f => f.name == featureName);
            if (maskFeature is FullScreenPassRendererFeature fsf && runtimeMaterialInstance != null)
            {
                originalFeatureMaterial = fsf.passMaterial; 
                fsf.passMaterial = runtimeMaterialInstance; 
            }
        }
    }

    private void CacheIDs()
    {
        shaderMaskId = Shader.PropertyToID("_MaskAmount");
        noiseTilingId = Shader.PropertyToID("_NoiseTiling");
        noiseSpeedId = Shader.PropertyToID("_NoiseSpeed");
        edgeDistId = Shader.PropertyToID("_EdgeDistortion");
        edgeSoftId = Shader.PropertyToID("_EdgeSoftness");
        edgeWidthId = Shader.PropertyToID("_EdgeWidth");
        edgeColorId = Shader.PropertyToID("_EdgeColor");
        originId = Shader.PropertyToID("_Origin");
    }

    // THE MAGIC LOGIC:
    private void OnBeginCamera(ScriptableRenderContext context, Camera cam)
    {
        if (maskFeature == null) return;

        // If the camera is the Main Camera AND we have the mask open, turn it ON.
        // If the camera is the Alternate Camera (or Scene View), turn it OFF.
        if (cam == mainCamera && CurrentMaskAmount > 0)
        {
            maskFeature.SetActive(true);
        }
        else
        {
            maskFeature.SetActive(false);
        }
    }

    private void Update()
    {
        HandleJitter();
        UpdateOrigin();
        UpdateShaderProperties();

        if (CurrentMaskAmount != targetMaskAmount)
        {
            CurrentMaskAmount = Mathf.MoveTowards(CurrentMaskAmount, targetMaskAmount, transitionSpeed * Time.deltaTime);
            
            // if (CurrentMaskAmount <= 0 && targetMaskAmount <= 0)
            // {
            //     wasFullyClosed = true;
            // }
            // else
            // {
            //     wasFullyClosed = false;
            // }
        }
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
        if (originTransform != null && mainCamera != null)
        {
            // Convert 3D world position to 0-1 Screen Space (Viewport)
            originScreenPoint = mainCamera.WorldToViewportPoint(originTransform.position);
        }
        else
        {
            // Default to center if no transform is assigned
            originScreenPoint = new Vector2(0.5f, 0.5f);
        }

    }
    private void UpdateShaderProperties()
    {
        if (runtimeMaterialInstance != null)
        {
            runtimeMaterialInstance.SetFloat(shaderMaskId, CurrentMaskAmount);
            runtimeMaterialInstance.SetVector(noiseTilingId, noiseTiling);
            runtimeMaterialInstance.SetVector(noiseSpeedId, currentNoiseSpeed);
            runtimeMaterialInstance.SetFloat(edgeDistId, edgeDistortion);
            runtimeMaterialInstance.SetFloat(edgeSoftId, edgeSoftness);
            runtimeMaterialInstance.SetFloat(edgeWidthId, edgeWidth);
            runtimeMaterialInstance.SetColor(edgeColorId, edgeColor);
            runtimeMaterialInstance.SetVector(originId, originScreenPoint);
        }
    }
}