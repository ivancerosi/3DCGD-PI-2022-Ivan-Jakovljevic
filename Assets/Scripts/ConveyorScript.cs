using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorScript : MonoBehaviour
{
    public float Speed = 0.02f;

    MeshRenderer meshRenderer;
    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        meshRenderer.material.mainTextureOffset += new Vector2(0f, Speed);
        for (int x=0; x<transform.childCount; x++)
        {
            Transform child = transform.GetChild(x);
            if (child.name == "Spawn" || child.name == "Despawn") continue;
            child.position -= transform.forward*Speed;
        }
    }
}
