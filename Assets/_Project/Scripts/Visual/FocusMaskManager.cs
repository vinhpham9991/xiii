using UnityEngine;
using System.Collections.Generic;

public class FocusMaskManager : MonoBehaviour
{
    private static FocusMaskManager _instance;
    public static FocusMaskManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("FocusMaskManager");
                _instance = go.AddComponent<FocusMaskManager>();
                _instance.CreateMask();
            }
            return _instance;
        }
    }

    private SpriteRenderer maskRenderer;
    
    // Store target GameObject to restore position later
    private struct RendererData
    {
        public Transform targetTransform;
        public Vector3 originalPos;
    }
    
    private List<RendererData> focusedRenderers = new List<RendererData>();
    
    // Sorting order for the mask. Focused objects will be MaskOrder + 1
    private const int MaskOrder = 10000;

    private void CreateMask()
    {
        // Create a 1x1 white texture
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        
        // Create a Sprite from the texture with 1 Pixel Per Unit so it's exactly 1x1 unit in size
        Sprite maskSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        
        // Create the GameObject
        GameObject maskObj = new GameObject("FocusMask");
        maskObj.transform.SetParent(Camera.main.transform);
        maskObj.transform.localPosition = new Vector3(0, 0, 10f); // 10 units in front of camera
        
        // Scale it to cover the screen
        // Orthographic size is half the height of the screen in world units
        float height = Camera.main.orthographicSize * 2f;
        float width = height * Camera.main.aspect;
        // Add some padding to be safe
        maskObj.transform.localScale = new Vector3(width * 1.5f, height * 1.5f, 1f);
        
        maskRenderer = maskObj.AddComponent<SpriteRenderer>();
        maskRenderer.sprite = maskSprite;
        maskRenderer.color = new Color(0f, 0f, 0f, 0.85f); // 85% Black
        maskRenderer.sortingOrder = MaskOrder;
        
        if (maskRenderer.material != null)
        {
            maskRenderer.material.renderQueue = 3000;
        }
        
        maskObj.SetActive(false); // Hide by default
    }

    public void FocusOn(Transform target)
    {
        // First, clear any existing focus
        ClearFocus();
        
        if (target == null) return;
        
        // Show mask
        if (maskRenderer != null)
        {
            maskRenderer.gameObject.SetActive(true);
        }
        
        // In URP 3D Orthographic, we move the target towards the camera to force it to render on top.
        // Because the camera is orthographic, moving it perfectly along the camera's forward vector
        // will NOT change its X/Y screen coordinates, so it won't appear to move visually,
        // and the Action Menu (which uses WorldToScreenPoint) will remain in the correct place!
        
        Vector3 camForward = Camera.main.transform.forward;
        
        focusedRenderers.Add(new RendererData
        {
            targetTransform = target,
            originalPos = target.position
        });
        
        // Move target 10 units towards the camera
        target.position = target.position - camForward * 10f;
    }

    public void ClearFocus()
    {
        // Hide mask
        if (maskRenderer != null)
        {
            maskRenderer.gameObject.SetActive(false);
        }
        
        // Restore position
        foreach (RendererData data in focusedRenderers)
        {
            if (data.targetTransform != null)
            {
                data.targetTransform.position = data.originalPos;
            }
        }
        focusedRenderers.Clear();
    }
}
