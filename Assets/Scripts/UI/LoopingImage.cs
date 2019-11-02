using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoopingImage : MonoBehaviour
{
    public Texture2D[] images;
    public float speed = 1f;
    public RawImage raw;
    int Current = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        Invoke("NextFrame", speed / 60);
    }
    void NextFrame()
    {
        Current += 1;
        if (Current >= images.Length)
            Current = 0;
        raw.texture = images[Current];
        Invoke("NextFrame", speed / 60);
    }
}
