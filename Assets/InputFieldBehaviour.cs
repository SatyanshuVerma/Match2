using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

//This script you attach to gameobject where is TMPro input field component and
//restrict the min and max values to be a valid input within maxValue & minValue
public class InputFieldBehaviour : MonoBehaviour                                                                                           
{
    InputField inputField;
    // [SerializeField]
    int maxValue = 6;
    // [SerializeField]
    int minValue = 2;
    private void Start()
    {
        inputField = GetComponent<InputField>();
        inputField.contentType = InputField.ContentType.IntegerNumber;
        inputField.onEndEdit.AddListener(delegate
        {
            Int32.TryParse(inputField.text, out int value);
            ValidateInput(value);
        });
    }
    void ValidateInput(int value)
    {
        

        if (value > maxValue)
        {
            inputField.text = maxValue.ToString();
        }
        else if (value <= minValue)
        {
            inputField.text = minValue.ToString();
        }
    }
}