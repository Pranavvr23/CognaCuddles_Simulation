using UnityEngine;

public class Pixel : MonoBehaviour
{
    [Range(0, 1000f)]
    public int value;
    private SpriteRenderer sr;
    

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        
    }

    void Update()
    {
        Color c = sr.color;
        c.a = value / 1000f;
        sr.color = c;
        
    }
}