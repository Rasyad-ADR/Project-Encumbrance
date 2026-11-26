using System.Collections.Generic;
using UnityEngine;

public class Terrain_Moving : MonoBehaviour
{
    public float speed = 5f;          // move left
    public float downSpeed = 2f;      // move down
    public int totalSprites = 6;      // how many sprites exist at once
    public float deleteBuffer = 2f;   // sprite-widths past the start before deleting
    public Transform terrain;
    public SpriteRenderer spriteA;
    public SpriteRenderer spriteB;

    List<SpriteRenderer> sprites = new List<SpriteRenderer>();
    Vector3 step;
    float leftLimit;
    bool nextIsA = true;

    void Start()
    {
        step = spriteB.bounds.center - spriteA.bounds.center;
        leftLimit = spriteA.bounds.min.x - Mathf.Abs(step.x) * deleteBuffer;

        // hidden templates, so Instantiate still works after the originals are destroyed
        SpriteRenderer templateA = Instantiate(spriteA, terrain, true);
        SpriteRenderer templateB = Instantiate(spriteB, terrain, true);
        templateA.gameObject.SetActive(false);
        templateB.gameObject.SetActive(false);

        sprites.Add(spriteA);
        sprites.Add(spriteB);

        spriteA = templateA;
        spriteB = templateB;
    }

    void Update()
    {
        Vector3 move = new Vector3(-speed, -downSpeed, 0f) * Time.deltaTime;
        terrain.Translate(move, Space.World);

        // delete the sprite that is too far to the left
        while (sprites.Count > 0 && sprites[0].bounds.max.x < leftLimit)
        {
            Destroy(sprites[0].gameObject);
            sprites.RemoveAt(0);
        }

        // generate new sprites on the right (alternating A, B, A, B)
        while (sprites.Count < totalSprites)
        {
            SpriteRenderer last = sprites[sprites.Count - 1];
            SpriteRenderer source = nextIsA ? spriteA : spriteB;

            SpriteRenderer copy = Instantiate(source, terrain, true);
            copy.gameObject.SetActive(true);
            copy.transform.position = last.transform.position + step;

            sprites.Add(copy);
            nextIsA = !nextIsA;
        }
    }
}