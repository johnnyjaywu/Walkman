using System.Collections.Generic;
using UnityEngine;

public class AlternateWorldObjectManager : MonoBehaviour
{
    [SerializeField] private FullscreenMaskController maskController;
    [SerializeField] private Transform originTransform;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask altWorldLayer;

    // We store the objects here so we don't have to search for them every frame
    private readonly List<AltObject> managedObjects = new List<AltObject>();

    private struct AltObject
    {
        public GameObject gameObject;
        public Renderer renderer;
    }

    private void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include);

        foreach (GameObject obj in allObjects)
        {
            if ((altWorldLayer.value & (1 << obj.layer)) > 0)
            {
                // Grab the SpriteRenderer (or MeshRenderer) to get the Bounding Box
                Renderer rend = obj.GetComponent<Renderer>();
                if (rend != null)
                {
                    managedObjects.Add(new AltObject { gameObject = obj, renderer = rend });
                    obj.SetActive(false);
                }
            }
        }
    }

    private void Update()
    {
        if (!maskController || !originTransform || !mainCamera) return;

        // 1. Grab the exact Screen Space radius the shader is currently drawing
        float currentRadius = maskController.CurrentViewportRadius;

        // 2. Where is the center of the portal on the screen?
        Vector3 anchorViewport = mainCamera.WorldToViewportPoint(originTransform.position);

        foreach (AltObject altObj in managedObjects)
        {
            if (altObj.gameObject == null) continue;

            // 3. Find the closest physical 3D edge of the object
            Vector3 closestPoint = altObj.renderer.bounds.ClosestPoint(originTransform.position);

            // 4. Convert that specific 3D edge into a 2D Screen Space coordinate
            Vector3 objViewport = mainCamera.WorldToViewportPoint(closestPoint);

            // 5. Apply the EXACT same Aspect Ratio correction the Shader Graph uses!
            float dx = (objViewport.x - anchorViewport.x) * mainCamera.aspect;
            float dy = objViewport.y - anchorViewport.y;

            // Measure the distance purely on the 2D monitor
            float screenDistance = Mathf.Sqrt(dx * dx + dy * dy);

            // 6. If the screen distance is less than the shader radius, it is visibly inside the portal
            // (We also check objViewport.z > 0 to ensure the object isn't behind the camera)
            bool shouldBeActive = screenDistance <= currentRadius && objViewport.z > 0;

            if (altObj.gameObject.activeSelf != shouldBeActive)
            {
                altObj.gameObject.SetActive(shouldBeActive);
            }
        }
    }
}