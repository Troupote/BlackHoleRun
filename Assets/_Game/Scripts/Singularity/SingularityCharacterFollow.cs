using BHR;
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
    internal bool IsPickedUp { get => m_isPickedUp; }
    public void SetKinematicState(bool isKinematic)
    {
         m_rigidbody.isKinematic = isKinematic;
         m_rigidbody.detectCollisions = !isKinematic;
         m_rigidbody.interpolation = isKinematic ? RigidbodyInterpolation.None : RigidbodyInterpolation.Interpolate;
    }

    public void PickupSingularity(bool shouldPickup)
    {
        if (m_isPickedUp == shouldPickup) return;
        m_isPickedUp = shouldPickup;

        if (shouldPickup)
        {
            m_isPickedUp = true;
            SetKinematicState(true);
            m_singularityTransform.SetParent(m_camRefPlacement);
            m_singularityTransform.localPosition = Vector3.zero;
            m_singularityTransform.localRotation = Quaternion.identity;
        }
        else
        {
            m_isPickedUp = false;
            m_singularityTransform.SetParent(null);
            SetKinematicState(false);
        }
    }
}
