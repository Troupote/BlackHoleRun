using UnityEngine;

public class SingularityCharacterFollowComponent : MonoBehaviour
{
    #region Dependencies
    private Rigidbody m_rigidbody;
    private Transform m_camRefPlacement;
    private Transform m_singularityTransform;
    #endregion

    private bool m_isPickedUp = false;

    public void InititializeDependencies(Transform a_singularityTransform, Rigidbody a_rb)
    {
        m_rigidbody = a_rb;
        m_singularityTransform = a_singularityTransform;
        m_camRefPlacement = CameraManager.Instance.SingularityPlacementRefTransform;

        SetKinematicState(true);
    }
    public bool IsKinematicEnabled() => m_rigidbody.isKinematic;
    private void SetKinematicState(bool isKinematic)
    {
         m_rigidbody.isKinematic = isKinematic;
         m_rigidbody.detectCollisions = !isKinematic;
         m_rigidbody.interpolation = isKinematic ? RigidbodyInterpolation.None : RigidbodyInterpolation.Interpolate;
    }

    public void PickupSingularity(bool shouldPickup)
    {
        m_isPickedUp = shouldPickup;

        if (shouldPickup)
        {
            SetKinematicState(true);
            m_singularityTransform.SetParent(m_camRefPlacement);
            m_singularityTransform.localPosition = Vector3.zero;
        }
        else
        {
            m_singularityTransform.SetParent(null);
            SetKinematicState(false);
        }
    }
}
