using UnityEngine;

// Taken from this thread: https://forum.unity.com/threads/test-if-ui-element-is-visible-on-screen.276549/

public static class RectTransformExtension
{
    /// <summary>
    /// Counts the bounding box corners of the given RectTransform that are visible in screen space.
    /// </summary>
    /// <returns>The amount of bounding box corners that are visible.</returns>
    /// <param name="rectTransform">Rect transform.</param>
    /// <param name="camera">Camera. Leave it null for Overlay Canvasses.</param>
    private static int CountCornersVisibleFrom(this RectTransform rectTransform, Camera camera = null)
    {
        Rect screenBounds = new Rect(0f, 0f, Screen.width, Screen.height); // Screen space bounds (assumes camera renders across the entire screen)
        Vector3[] objectCorners = new Vector3[4];
        rectTransform.GetWorldCorners(objectCorners);

        int visibleCorners = 0;
        Vector3 tempScreenSpaceCorner; // Cached
        for (var i = 0; i < objectCorners.Length; i++) // For each corner in rectTransform
        {
            if (camera != null)
                tempScreenSpaceCorner = camera.WorldToScreenPoint(objectCorners[i]); // Transform world space position of corner to screen space
            else
                tempScreenSpaceCorner = objectCorners[i]; // If no camera is provided we assume the canvas is Overlay and world space == screen space

            if (screenBounds.Contains(tempScreenSpaceCorner)) // If the corner is inside the screen
            {
                visibleCorners++;
            }
        }
        return visibleCorners;
    }

    /// <summary>
    /// Determines if this RectTransform is fully visible.
    /// Works by checking if each bounding box corner of this RectTransform is inside the screen space view frustrum.
    /// </summary>
    /// <returns><c>true</c> if is fully visible; otherwise, <c>false</c>.</returns>
    /// <param name="rectTransform">Rect transform.</param>
    /// <param name="camera">Camera. Leave it null for Overlay Canvasses.</param>
    public static bool IsFullyVisibleFrom(this RectTransform rectTransform, Camera camera = null)
    {
        if (!rectTransform.gameObject.activeInHierarchy)
            return false;

        return CountCornersVisibleFrom(rectTransform, camera) == 4; // True if all 4 corners are visible
    }

    /// <summary>
    /// Determines if this RectTransform is at least partially visible.
    /// Works by checking if any bounding box corner of this RectTransform is inside the screen space view frustrum.
    /// </summary>
    /// <returns><c>true</c> if is at least partially visible; otherwise, <c>false</c>.</returns>
    /// <param name="rectTransform">Rect transform.</param>
    /// <param name="camera">Camera. Leave it null for Overlay Canvasses.</param>
    public static bool IsVisibleFrom(this RectTransform rectTransform, Camera camera = null)
    {
        if (!rectTransform.gameObject.activeInHierarchy)
            return false;

        return CountCornersVisibleFrom(rectTransform, camera) > 0; // True if any corners are visible
    }
}