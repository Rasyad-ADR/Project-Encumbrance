using System.Collections.Generic;
using UnityEngine;
using Encumbrance.Journal;

public class Terrain_Moving : MonoBehaviour
{
    public TypingController typing;      // drag the object that has TypingController
    public float leftPerCharacter = 0.5f;   // how far left per correct character
    public float downPerCharacter = 0.2f;   // how far down per correct character
    public float smoothSpeed = 4f;          // how fast it glides to the new position
    public int totalSprites = 6;
    public float deleteBuffer = 2f;
    public Transform terrain;
    public SpriteRenderer spriteA;
    public SpriteRenderer spriteB;

    List<SpriteRenderer> sprites = new List<SpriteRenderer>();
    Vector3 step;
    float leftLimit;
    bool nextIsA = true;
    Vector2 pending;   // distance still left to move

    void OnEnable()
    {
        if (typing != null) typing.CharacterTyped += OnCharacterTyped;
    }

    void OnDisable()
    {
        if (typing != null) typing.CharacterTyped -= OnCharacterTyped;
    }

    void OnCharacterTyped(char c)
    {
        pending += new Vector2(leftPerCharacter, downPerCharacter);
    }

    void Start()
    {
        step = spriteB.bounds.center - spriteA.bounds.center;
        leftLimit = spriteA.bounds.min.x - Mathf.Abs(step.x) * deleteBuffer;

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
        if (pending != Vector2.zero)
        {
            Vector2 delta = Vector2.MoveTowards(Vector2.zero, pending, smoothSpeed * Time.deltaTime);
            pending -= delta;
            terrain.Translate(new Vector3(-delta.x, -delta.y, 0f), Space.World);
        }

        while (sprites.Count > 0 && sprites[0].bounds.max.x < leftLimit)
        {
            Destroy(sprites[0].gameObject);
            sprites.RemoveAt(0);
        }

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