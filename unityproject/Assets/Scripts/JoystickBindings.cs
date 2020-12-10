using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class JoystickBindings : MonoBehaviour
{
    public TMP_Text joystick;
    public TextMesh[] inputText;
    public TextMesh[] buttonText;

    void Start()
    {
        
    }
 
    /*
     * Read all axis of joystick inputs and display them for configuration purposes
     * Requires the following input managers
     *      Joy[N] Axis 1-9
     *      Joy[N] Button 0-20
     */
    void Update () {
        var joysticks = Input.GetJoystickNames();
        if (joysticks.Length > 0)
            joystick.text = joysticks[0];
        
        for (int i = 1; i <= 1; i++)
        {
            string inputs = "Joystick " + i + "\n";
            string stick = "Joy " + i + " Axis ";
            for (int a = 1; a <= 10; a++)
            {                
                inputs += "Axis "+ a +":" + Input.GetAxis(stick + a).ToString("0.00") + "\n";
            }
            inputText[i - 1].text = inputs;
        }
 
        string buttons = "Buttons 3\n";
        for (int b = 0; b <= 10; b++)
        {
            buttons += "Btn " + b + ":" + Input.GetButton("Joy 3 Button " + b) + "\n";
        }
 
        buttonText[2].text = buttons;
    }
}
