using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterializeEffect : MonoBehaviour
{
    public IEnumerator MaterialiseRoutine(Shader materializeShader, Color materializeColor, float materializeTime, SpriteRenderer[] spriteRendererArray, Material normalMat)
    {
        Material materializeMaterial = new Material(materializeShader);

        materializeMaterial.SetColor("_EmissionColor", materializeColor);

        float dissolveAmmount = 0;

        foreach (SpriteRenderer spriteRenderer in spriteRendererArray)
        {
            spriteRenderer.material = materializeMaterial;
        }

        while(dissolveAmmount < 1f)
        {
            dissolveAmmount += (Time.deltaTime / materializeTime);
            materializeMaterial.SetFloat("_DissolveAmount", dissolveAmmount);
            yield return null;
        }

        foreach(SpriteRenderer spriteRenderer in spriteRendererArray)
        {
            spriteRenderer.material = normalMat;
        }
    }
}
