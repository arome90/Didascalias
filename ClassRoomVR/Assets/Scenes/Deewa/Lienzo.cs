using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lienzo : MonoBehaviour
{
    public Camera _camera;
    public Shader _drawShader;

    private RenderTexture _splatMap;
    private Material _drawMaterial;
    private RaycastHit _hit;

    // Start is called before the first frame update
    void Start()
    {
        _drawMaterial = new Material(_drawShader);
        _drawMaterial.SetVector("_Color", Color.white);

        // Asignar una textura inicial al material
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.material.mainTexture = _splatMap = new RenderTexture(2048, 2048, 0, RenderTextureFormat.ARGBFloat);
        Graphics.Blit(Texture2D.blackTexture, _splatMap); // Rellenar la textura con un color inicial (negro en este caso)
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Debug.Log(_camera.ScreenPointToRay(Input.mousePosition));
            if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out _hit))
            {
                Vector2 pixelUV = _hit.textureCoord;

                _drawMaterial.SetVector("_Coordinates", new Vector4(pixelUV.x, pixelUV.y, 0, 0));

                // Pintar en la textura del material directamente
                RenderTexture temp = RenderTexture.GetTemporary(_splatMap.width, _splatMap.height, 0, RenderTextureFormat.ARGBFloat);
                Graphics.Blit(_splatMap, temp);
                Graphics.Blit(temp, _splatMap, _drawMaterial); // Acumular el dibujo en la textura del material
                RenderTexture.ReleaseTemporary(temp);
            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Marker"))
        {
            Debug.Log("aaaaa");
            
            Ray ray = new Ray(other.transform.position, other.transform.localRotation.eulerAngles);
            Debug.Log(ray);

            if (Physics.Raycast(ray, out _hit))
            {
                Debug.Log("aaaaajjjj");
                Vector2 pixelUV = _hit.textureCoord;

                _drawMaterial.SetVector("_Coordinates", new Vector4(pixelUV.x, pixelUV.y, 0, 0));

                // Pintar en la textura del material directamente
                RenderTexture temp = RenderTexture.GetTemporary(_splatMap.width, _splatMap.height, 0, RenderTextureFormat.ARGBFloat);
                Graphics.Blit(_splatMap, temp);
                Graphics.Blit(temp, _splatMap, _drawMaterial); // Acumular el dibujo en la textura del material
                RenderTexture.ReleaseTemporary(temp);
            }

        }
    }
    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag("Marker")) 
        {
            Debug.Log("aaaaa");
           
            if (Physics.Raycast(_camera.ScreenPointToRay(collision.transform.position), out _hit))
            {
                Vector2 pixelUV = _hit.textureCoord;

                _drawMaterial.SetVector("_Coordinates", new Vector4(pixelUV.x, pixelUV.y, 0, 0));

                // Pintar en la textura del material directamente
                RenderTexture temp = RenderTexture.GetTemporary(_splatMap.width, _splatMap.height, 0, RenderTextureFormat.ARGBFloat);
                Graphics.Blit(_splatMap, temp);
                Graphics.Blit(temp, _splatMap, _drawMaterial); // Acumular el dibujo en la textura del material
                RenderTexture.ReleaseTemporary(temp);
            }

        }
    }
    // No necesitas el método OnGUI para dibujar la textura del material

    // Asegúrate de liberar los recursos cuando destruyas el objeto
    private void OnDestroy()
    {
        _splatMap.Release();
        Destroy(_splatMap);
    }
}
