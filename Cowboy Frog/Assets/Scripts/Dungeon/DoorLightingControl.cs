using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class DoorLightingControl : MonoBehaviour
{
    private bool isLit = false;
    private Door door;

    private void Awake()
    {
        door = GetComponentInParent<Door>();
    }

    public void FadeInDoor(Door door)
    {
        Material material = new Material(GameResources.Instance.variableLitShader);
        if(!isLit)
        {
            SpriteRenderer[] spriteRenderers = GetComponentsInParent<SpriteRenderer>();
            foreach(SpriteRenderer sprite in spriteRenderers)
            {
                StartCoroutine(FadeInDoorRoutine(sprite, material));
            }

            isLit = true;
        }
    }

    private IEnumerator FadeInDoorRoutine(SpriteRenderer sprite, Material material)
    {
        sprite.material = material;

        for (float i = 0.05f; i <= 1f; i += Time.deltaTime / Settings.fadeInTime)
        {
            material.SetFloat("Alpha_Slider", i);
            yield return null;
        }

        sprite.material = GameResources.Instance.litMaterial;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        FadeInDoor(door);
    }
}
