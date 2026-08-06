using UnityEngine;
using UnityEngine.UI;

public class VHSNoise : MonoBehaviour
{
    [SerializeField] private float noiseIntensity = 0.05f;
    [SerializeField] private float scanlineSpeed = 2f;
    
    private Image bgImage;
    private float scanlineOffset;
    
    void Start()
    {
        bgImage = GetComponent<Image>();
    }
    
    void Update()
    {
        // Лёгкое мерцание фона
        float noise = 1f - Random.Range(0f, noiseIntensity);
        bgImage.color = new Color(noise, noise, noise);
        
        // Симуляция строк развёртки (scanlines)
        scanlineOffset += Time.deltaTime * scanlineSpeed;
        if (Mathf.Sin(scanlineOffset * 10f) > 0.95f)
        {
            bgImage.color = new Color(0.02f, 0.02f, 0.02f);
        }
    }
}