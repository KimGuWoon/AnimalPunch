using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectToggle : MonoBehaviour
{
    public int CharacterIndex;

    public void ToggleSelected()
    {
        UserData.charIndex = CharacterIndex;
    }
}
