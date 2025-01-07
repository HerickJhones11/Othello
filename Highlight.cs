using UnityEngine;

public class Highlight : MonoBehaviour 
{
    [SerializeField]
    private Color normalColor;

    [SerializeField]
    private Color mouseOverColor;

    private Material material;

    private void Start()
    {
        material = GetComponent<MeshRenderer>().material;    
        material.color = normalColor;
    }

    private void OnMouseEnter()
    {
        material.color = mouseOverColor;
    }
    private void OnMouseExit()
    {
        material.color = normalColor;   
    }
    private void OnDestroy()
    {
        Destroy(material);
    }
}

