using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;

public class FullscreenMaskController : MonoBehaviour
{
    [Header("Pipeline Control")]
    [SerializeField] private ScriptableRendererData rendererData;
    [SerializeField] private string featureName = "Fullscreen Mask";
    
    [Tooltip("The master material asset. We will clone this at runtime.")]
    [SerializeField] private Material maskMaterialAsset;

    [Header("Transition Settings")]
    [SerializeField] private float transitionSpeed = 2f;
    [SerializeField] private bool startMasked = false;

    [SerializeField] private float currentMaskAmount;
    public bool IsMaskActive { get; private set; }

    private const string kShaderMaskProperty = "_MaskAmount"; 
    private int shaderMaskId;
    
    private ScriptableRendererFeature maskFeature;
    
    // --- NEW: Instancing Variables ---
    private Material runtimeMaterialInstance;
    private Material originalFeatureMaterial; // To put things back how we found them

    private void Awake()
    {
        shaderMaskId = Shader.PropertyToID(kShaderMaskProperty);

        // 1. Create a unique, temporary clone of the material
        if (maskMaterialAsset != null)
        {
            runtimeMaterialInstance = new Material(maskMaterialAsset);
            runtimeMaterialInstance.name = "MAT_FullscreenMask_CLONE";
        }

        // 2. Find the feature and swap its material to our clone
        if (rendererData != null)
        {
            maskFeature = rendererData.rendererFeatures.Find(f => f.name == featureName);
            
            // We have to cast it to a FullScreenPassRendererFeature to access the passMaterial property
            if (maskFeature is FullScreenPassRendererFeature fullScreenFeature && runtimeMaterialInstance != null)
            {
                // Save the original asset so we don't break the Unity Editor
                originalFeatureMaterial = fullScreenFeature.passMaterial; 
                // Inject our unique clone
                fullScreenFeature.passMaterial = runtimeMaterialInstance; 
            }
        }

        IsMaskActive = startMasked;
        currentMaskAmount = IsMaskActive ? 1f : 0f;
        
        if (runtimeMaterialInstance != null)
        {
            runtimeMaterialInstance.SetFloat(shaderMaskId, currentMaskAmount);
        }
        
        if (maskFeature != null)
        {
            maskFeature.SetActive(IsMaskActive);
        }
    }

    public void OnToggleMask(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            IsMaskActive = !IsMaskActive;

            if (IsMaskActive && maskFeature != null)
            {
                maskFeature.SetActive(true);
            }
        }
    }

    private void Update()
    {
        float targetMaskAmount = IsMaskActive ? 1f : 0f;

        if (currentMaskAmount != targetMaskAmount)
        {
            currentMaskAmount = Mathf.MoveTowards(currentMaskAmount, targetMaskAmount, transitionSpeed * Time.deltaTime);
            
            // Push the value directly to our unique clone!
            if (runtimeMaterialInstance != null)
            {
                runtimeMaterialInstance.SetFloat(shaderMaskId, currentMaskAmount);
            }

            if (currentMaskAmount <= 0f && maskFeature != null)
            {
                maskFeature.SetActive(false);
            }
        }
    }

    private void OnDisable()
    {
        // --- NEW: Safe Cleanup ---
        
        // 1. Restore the master material to the Render Feature so the project asset isn't corrupted
        if (maskFeature is FullScreenPassRendererFeature fullScreenFeature)
        {
            fullScreenFeature.passMaterial = originalFeatureMaterial;
            fullScreenFeature.SetActive(false);
        }
        
        // 2. Destroy the clone to prevent memory leaks!
        if (runtimeMaterialInstance != null)
        {
            Destroy(runtimeMaterialInstance);
        }
    }
}