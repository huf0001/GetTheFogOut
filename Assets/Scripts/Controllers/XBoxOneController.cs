using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//////////////////////////////////////////////////////////////////////////////
//
// Sources: https://answers.unity.com/questions/1350081/xbox-one-controller-mapping-solved.html
// Sources: https://ritchielozada.com/2016/01/08/part-9-configure-input-settings-for-camera-controls/
// Sources: https://ritchielozada.com/2016/01/16/part-11-using-an-xbox-one-controller-with-unity-on-windows-10/
//
//  A                                  joystick button 0
//  B                                  joystick button 1
//  X                                  joystick button 2
//  Y                                  joystick button 3
//  Left Bumper                        joystick button 4
//  Right Bumper                       joystick button 5
//  View (Back)                        joystick button 6
//  Menu (Start)                       joystick button 7
//  Left Stick Button                  joystick button 8
//  Right Stick Button                 joystick button 9
//  Left Stick “Horizontal”            X Axis               -1 to 1
//  Left Stick “Vertical”              Y Axis                1 to -1
//  Left Trigger Shared Axis           3rd Axis              0 to 1
//  Right Trigger Shared Axis          3rd Axis              0 to -1 
//  Right Stick “HorizontalTurn”       4th Axis             -1 to 1
//  Right Stick “VerticalTurn”         5th Axis              1 to -1
//  DPAD – Horizontal                  6th Axis             -1 (.64) 1
//  DPAD – Vertical                    7th Axis             -1 (.64) 1
//  Left Trigger                       9th Axis              0 to 1
//  Right Trigger                      10th Axis             0 to 1
// 
// 
//  Settings for both Left and Right Sticks' Horizontal and Vertical
//  
//  Gravity:      1000
//  Dead:         0.04
//  Sensitivity:  1
//  
//////////////////////////////////////////////////////////////////////////////

public class XBoxOneController : MonoBehaviour
{
	private float XboxLSH, XboxLSV, XboxRSH, XboxRSV, xboxLTAxis, xboxRTAxis, xboxDpadHAxis, xboxDpadVAxis;
    private bool xboxA, xboxB, xboxX, xboxY, xboxLB, xboxRB, xboxLS, xboxRS, xboxView, xboxMenu;

    [SerializeField] protected TextMeshProUGUI xboxText;

    void Start()
    {
        
    }

    void controllerCheck()
    {
    	XboxLSH = Input.GetAxis("Xbox_LS_H");
		XboxLSV = Input.GetAxis("Xbox_LS_V");
		XboxRSH = Input.GetAxis("Xbox_RS_H");
		XboxRSV = Input.GetAxis("Xbox_RS_V");

    	xboxLTAxis = Input.GetAxis("Xbox_LT");
		xboxRTAxis = Input.GetAxis("Xbox_RT");
		xboxDpadHAxis = Input.GetAxis("Xbox_DPAD_H");
		xboxDpadVAxis = Input.GetAxis("Xbox_DPAD_V");

		xboxA = Input.GetButton("Xbox_A");
		xboxB = Input.GetButton("Xbox_B");
		xboxX = Input.GetButton("Xbox_X");
		xboxY = Input.GetButton("Xbox_Y");
		xboxLB = Input.GetButton("Xbox_LB");
		xboxRB = Input.GetButton("Xbox_RB");
		xboxLS = Input.GetButton("Xbox_LS_B");
		xboxRS = Input.GetButton("Xbox_RS_B");
		xboxView = Input.GetButton("Xbox_View");
		xboxMenu = Input.GetButton("Xbox_Menu");
    }

    // Update is called once per frame
    void Update()
    {
    	controllerCheck();

        xboxText.text = string.Format(
            "L Stick H: {14:0.000}\n" +
            "L Stick V: {15:0.000}\n" +
            "R Stick H: {16:0.000}\n" +
            "R Stick V: {17:0.000}\n" +
            "LTrigger: {0:0.000}\n" +
            "RTrigger: {1:0.000}\n" +
            "A: {2}\n" +
            "B: {3}\n" +
            "X: {4}\n" +
            "Y: {5}\n" +
            "LB: {6}\n" +
            "RB: {7}\n" +
            "LS: {8}\n" +
            "RS: {9}\n" +
            "View: {10}\n" +
            "Menu: {11}\n" +
            "Dpad-H: {12:0.000}\n" +
            "Dpad-V: {13:0.000}",
            xboxLTAxis, xboxRTAxis,
            xboxA, xboxB, xboxX, xboxY,
            xboxLB, xboxRB, xboxLS, xboxRS,
            xboxView, xboxMenu,
            xboxDpadHAxis, xboxDpadVAxis,
            XboxLSH, XboxLSV,
            XboxRSH, XboxRSV);
    }
}
