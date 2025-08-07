using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialGradient : MonoBehaviour
{

    public Material targetMaterial;
    public Color startColor = new Color(0.235f, 0.369f, 0.694f); // 3C5EB1
    public Color endColor = new Color(0.690f, 0.235f, 0.686f); // B03CAF
    public float gradientSpeed = 1f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float t = Mathf.PingPong(Time.time * gradientSpeed, 1f); // 시간에 따라서 0에서 1사이를 반복하는 값을 계산
        Color currentColor = Color.Lerp(startColor, endColor, t);
        targetMaterial.color = currentColor;
    }
}
