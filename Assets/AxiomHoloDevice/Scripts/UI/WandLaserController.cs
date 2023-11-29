using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(LineRenderer))]
public class WandLaserController : MonoBehaviour
{
  HoloWandInputModule HoloWandInputModule;
  public HoloTrackWand m_TargetWand = null;
  public bool m_AllowObjectsToBlockLaser = true;
  public bool m_UseUserColour = true;
  public Color m_LaserColour = Color.cyan;
  public float m_Length = 10.0f;
  public float m_Width = 0.01f;
  protected LineRenderer m_LineRenderer = null;

  // Start is called before the first frame update
  void Start()
  {
    HoloWandInputModule = GameObject.FindObjectOfType<HoloWandInputModule>();
    if (HoloWandInputModule == null)
      Debug.LogWarning("HoloEventSystem was not found in Scene. Please add this to use the WandLaser.");

    m_LineRenderer = gameObject.GetComponent<LineRenderer>();

    if (!m_TargetWand)
      Debug.Log("No target wand has been set. Please select a wand from the AxiomHoloCave GameObject.");
  }

  // Update is called once per frame
  void Update()
  {
    if (!m_TargetWand)
      return; // No wand, just return

    // Test for the laser intersection if blocking is enabled
    float maxLen = float.MaxValue;
    if (m_AllowObjectsToBlockLaser)
    {
      maxLen = m_TargetWand.GetLaserHitDist();

      RaycastHit hit;
      if (Physics.Raycast(m_TargetWand.transform.position, m_TargetWand.transform.forward, out hit, maxLen, ~(1 << 31)))
        maxLen = hit.distance;

      RaycastResult result;
      if (HoloWandInputModule != null && HoloWandInputModule.GetWandRayCastResult(m_TargetWand.m_id, out result))
      {
        float uiMaxLen = result.distance;
        if (uiMaxLen < maxLen && uiMaxLen != 0)
          maxLen = uiMaxLen;
      }
    }

    // Calculate line renderer points (in world space)
    float worldScale = HoloDevice.active.GetWorldScale();
    Vector3[] positions = new Vector3[2];
    positions[0] = m_TargetWand.transform.position;
    positions[1] = positions[0] + m_TargetWand.transform.forward * Mathf.Min(maxLen, m_Length * worldScale);
    m_LineRenderer.widthMultiplier = worldScale * m_Width;
    m_LineRenderer.SetPositions(positions);

    // Get the colour of the laser
    bool buttonDown = m_TargetWand.IsTriggerDown() || m_TargetWand.IsButtonADown() || m_TargetWand.IsButtonBDown();
    Color laserColour = m_UseUserColour ? HoloDevice.active.GetUserColour(m_TargetWand.m_id) : m_LaserColour;

    if (buttonDown)
      laserColour.a /= 2;

    m_LineRenderer.startColor = laserColour;
    m_LineRenderer.endColor = laserColour;
  }
}
