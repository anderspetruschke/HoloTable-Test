using UnityEngine;

public class HoloControlModeWand : HoloControlModeBase
{
  private float m_flyAccel = 2.0f; // m/s/s
  private float m_curSpeed = 0.0f; // m/s
  bool press = false;
  float pressTime = 0.0f;
  float heldTime = 0.2f;

  public override bool Apply(HoloTrackGlasses glasses, HoloTrackWand wand, HoloTrackDevice.ButtonState actionButtonState, float flySpeed, bool showLaser)
  {
    if (showLaser)
      wand.DrawLaser(Color.white, 100 * HoloDevice.active.GetWorldScale(), true);

    HoloDevice device = HoloDevice.active;
    Vector3 currentPosition = device.GetWorldPosition(true);
    float accel = (actionButtonState.down ? 1 : -1) * m_flyAccel * flySpeed * Time.deltaTime;

    if (actionButtonState.pressed)
    {
      pressTime = 0.0f;
      press = true;
    }
    if (press)
    {
      pressTime += Time.deltaTime;
      if (pressTime > heldTime)
      {
        m_curSpeed = Mathf.Clamp(m_curSpeed + accel, 0.0f, flySpeed);
        if (m_curSpeed > 0)
        {
          device.SetWorldPosition(currentPosition + (m_curSpeed * wand.transform.forward), true);
          return true; // Return true indicating that this control mode is active
        }
      }
      if (actionButtonState.released)
      {
        if (pressTime <= heldTime)
        {
          Vector3 wandRotation = wand.transform.rotation.eulerAngles;
          Vector3 newRotaion = new Vector3(0, wandRotation.y, 0);
          device.SetWorldRotation(newRotaion);
        }
        press = false;
      }
    }
    return false; // Return false indicating that this control mode is inactive
  }
}
