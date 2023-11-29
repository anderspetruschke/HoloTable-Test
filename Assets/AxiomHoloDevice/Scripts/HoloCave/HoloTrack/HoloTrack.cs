using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base HoloTrack class
public class HoloTrack : MonoBehaviour
{
  public bool m_enabled = true;
  public bool m_debugButtonInputs = false;
  public bool m_trackingSmoothed = true;
  public string m_server = "localhost";

  protected long m_lastActiveTime = -1;
  protected Vector3 m_lastTrackingPosition;
  protected int m_activeThreshold = 1000;
  protected bool m_trackingPositionInitialized = false;

  public bool m_forceTCP = false; ///< Connect to the VRPN server over TCP instead of UDP
  public bool m_enableDebugOutput = false; ///< Output the tracker position/orientation and button states to the Log.

  protected List<Vector3> m_positionHistory = new List<Vector3>();

  public virtual string GetUser() { return "Events"; }

  // Check if the tracking position is valid.
  // If the position has changed within the active threshold (40ms ?) then this
  // will return true, otherwise it will return false.
  // If m_enabled==false this function always returns false.
  public bool IsPositionValid()
  {
    return m_enabled && ((long)(Time.unscaledTime * 1000) - m_lastActiveTime) < m_activeThreshold;
  }

  // Get the full host address for this device
  public string Host()
  {
    if (m_forceTCP)
      return GetUser() + "@tcp://" + m_server;
    else
      return GetUser() + "@" + m_server;
  }

  // Returns the current state of the given button
  protected bool Button(int a_button)
  {
    if (!m_enabled)
      return false;
    
    bool state = HoloTrackInterface.vrpnButton(Host(), (int)a_button); /*Button0 is reserved*/
    if (m_enableDebugOutput)
      Debug.Log(string.Format("[{0}]: Button{1}={2}", Host(), a_button, state));
    return state;
  }

  // Returns the reported battery value
  protected double Battery()
  {
    if (!m_enabled)
      return 1;
    
    double state = HoloTrackInterface.vrpnAnalog(Host());
    if (m_enableDebugOutput)
      Debug.Log(string.Format("[{0}]: Battery={1}", Host(), state));
    return state;
  }

  // Returns the raw rotation of the device 
  public Quaternion RawRotation() { return HoloTrackInterface.vrpnTrackerQuat(Host()); }

  // Returns the raw position of the device
  public Vector3 RawPosition() { return HoloTrackInterface.vrpnTrackerPos(Host()); }

  // Returns the device rotation
  protected Quaternion Rotation()
  {
    if (!m_enabled)
      return transform.localRotation;

    if (!IsPositionValid())
    {
      if (m_enableDebugOutput)
        Debug.Log(string.Format("[{0}]: Error getting orientation. Tracker is enabled but no data has been received from server for a while", Host()));
      return transform.localRotation;
    }

    Quaternion state = RawRotation();
    if (m_enableDebugOutput)
      Debug.Log(string.Format("[{0}]: Orientation={1}", Host(), state));

    return state; 
  }

  // Returns the device position
  protected Vector3 Position()
  {
    if (m_enabled)
    {
      Vector3 rawTrackedPos = RawPosition();
      if (!m_trackingPositionInitialized)
      {
        m_lastTrackingPosition = rawTrackedPos;
        m_lastActiveTime = -2 * m_activeThreshold; // default m_lastActiveTime to a value that indicates the position is definitely not valid
        m_trackingPositionInitialized = true;
      }

      Vector3 filteredTrackedPos = rawTrackedPos;
      if (m_trackingSmoothed)
      {
        // Manage the history
        const int framesToCollect = 10;
        while (m_positionHistory.Count > framesToCollect)
          m_positionHistory.RemoveAt(0);

        // Store the new data
        m_positionHistory.Add(rawTrackedPos);

        // Generate averaged position
        Vector3 averagedPosition = Vector3.zero;
        float total = 0;
        const float exponent = 1.6f;
        for (int deviceIndex = 0; deviceIndex < m_positionHistory.Count; ++deviceIndex)
        {
          float weight = Mathf.Pow((float)deviceIndex, exponent);
          averagedPosition += m_positionHistory[deviceIndex] * weight;
          total += weight;
        }
        if (total > 0)
          averagedPosition /= total;

        // Apply averaged position
        filteredTrackedPos = averagedPosition;
      }

      // Update device validity if the position has changed
      if (rawTrackedPos != m_lastTrackingPosition)
        m_lastActiveTime = (long)(Time.unscaledTime * 1000);
      m_lastTrackingPosition = rawTrackedPos;

      if (IsPositionValid())
      {
        if (m_enableDebugOutput)
          Debug.Log(string.Format("[{0}]: Position={1}", Host(), filteredTrackedPos));
        return filteredTrackedPos;
      }
      else if (m_enableDebugOutput)
      {
        Debug.Log(string.Format("[{0}]: Error getting position. Tracker is enabled but no data has been received from server for a while", Host()));
      }
    }

    return transform.localPosition;
  }
}
