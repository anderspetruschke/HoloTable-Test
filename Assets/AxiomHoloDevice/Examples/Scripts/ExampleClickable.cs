﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/**
 * Clickable GameObject Implementation
 * 
 * This is an example of basic interaction with GameObjects using the Wand.
 * We detect a click event, and print the name of the object that was clicked.
 * 
 * To use this script:
 *   1. Ensure the GameObject you want to interact with has a valid Collider.
 *   2. Set the GameObject's Layer to 'UI' (This is the default Layer that the
 *      Wand will interact with).
 *   3. Attach this script to GameObject.
 *   4. Add a HoloEventSystem Prefab to the scene if one does not exist. You
 *      will find it in 'Assets/AxiomHoloDevice/HoloEventSystem.prefab'
 **/

[RequireComponent(typeof(Collider))] // To interact with a non-UI GameObject using
                                     // the wand, it needs a Collider.
public class ExampleClickable
  : MonoBehaviour        // Inherit from MonoBehaviour as this is a component.
  , IPointerClickHandler // Inherit from IPointerClickHandler to receive
                         // OnPointerClick events from the HoloEventSystem.
  , IPointerEnterHandler // Inherit from IPointerEnterHandler to receive
                         // OnPointerEnter events from the HoloEventSystem.
  , IPointerExitHandler  // Inherit from IPointerExitHandler to receive
                         // OnPointerExit events from the HoloEventSystem.
{
  // This function is called once when a button on the wand is Pressed, then
  // Released, while hovering the GameObject.
  public void OnPointerClick(PointerEventData eventData)
  {
    // Get the ID of the user that clicked the object.
    // If the event was not generated by the HoloEventSystem, GetUserID() returns -1.
    int userID = eventData.GetUserID();

    if (userID != -1)
    { // If the user ID is valid, the GameObject was clicked by a user.
      // Log the GameObject name and the userID.
      Debug.Log(string.Format("{0} was clicked by User {1}'s wand (button: {2}).", gameObject.name, userID, eventData.GetWandButton()));
    }
    else
    { // If the user ID is invalid, the GameObject was not clicked by a wand,
      // and was not generated by HoloEventSystem.
      Debug.Log(string.Format("{0} was clicked, but not by a wand.", gameObject.name));
    }
  }

  // The OnPointerEvent/OnPointerExit functions are implemented below, using the
  // same concepts outlined above.

  // This function is called once for each button when the GameObject is hovered
  // by the wand.
  public void OnPointerEnter(PointerEventData eventData)
  {
    if (eventData.button != PointerEventData.InputButton.Left)
      return; // A quirk of the system is that we get enter events for each input
              // button, so ignore all but one.

    int userID = eventData.GetUserID();

    if (userID != -1)
    {
      Debug.Log(string.Format("{0} is hovered by User {1}'s wand.", gameObject.name, userID));
    }
    else
    {
      Debug.Log(string.Format("{0} is hovered, but not by a wand.", gameObject.name));
    }
  }

  // This function is called once for each button when the wand stops hovering
  // this GameObject.
  public void OnPointerExit(PointerEventData eventData)
  {
    if (eventData.button != PointerEventData.InputButton.Left)
      return; // A quirk of the system is that we get exit events for each input
              // button, so ignore all but one.

    int userID = eventData.GetUserID();

    if (userID != -1)
    {
      Debug.Log(string.Format("{0} is no longer hovered by User {1}'s wand.", gameObject.name, userID));
    }
    else
    {
      Debug.Log(string.Format("{0} is no longer hovered.", gameObject.name));
    }
  }
}
