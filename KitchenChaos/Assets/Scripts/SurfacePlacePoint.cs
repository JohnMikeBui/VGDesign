using UnityEngine;

public class SurfacePlacePoint : MonoBehaviour
{
    public Transform placeAnchor; // exact spot to drop plates/items

    void Reset()
    {
        // Auto-create anchor if missing
        if (placeAnchor == null)
        {
            GameObject anchor = new GameObject("PlaceAnchor");
            anchor.transform.SetParent(transform);
            anchor.transform.localPosition = Vector3.zero + Vector3.up * 0.02f;
            placeAnchor = anchor.transform;
        }
    }
}
