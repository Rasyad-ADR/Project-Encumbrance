using System.Collections.Generic;
using UnityEngine;
using Encumbrance.Journal;

public class Mountain_Moving : MonoBehaviour
{
    [System.Serializable]
    public class Layer
    {
        public Transform root;                  // parent of this mountain layer
        public SpriteRenderer sprite;           // the mountain sprite of this layer
        public float leftPerCharacter = 0.1f;   // far = small, mid = bigger
        public int totalSprites = 4;            // how many copies exist at once
        public float deleteBuffer = 1f;         // sprite-widths past the start before deleting

        List<SpriteRenderer> sprites = new List<SpriteRenderer>();
        SpriteRenderer template;
        float width;
        float leftLimit;
        float pending;

        public void Init()
        {
            width = sprite.bounds.size.x;
            leftLimit = sprite.bounds.min.x - width * deleteBuffer;

            template = Object.Instantiate(sprite, root, true);
            template.gameObject.SetActive(false);

            sprites.Add(sprite);
        }

        public void ClearPending() => pending = 0f;

        public void OnTyped()
        {
            pending += leftPerCharacter;
        }

        public void Tick(float smoothSpeed)
        {
            if (pending > 0f)
            {
                float delta = Mathf.MoveTowards(0f, pending, smoothSpeed * Time.deltaTime);
                pending -= delta;
                root.Translate(new Vector3(-delta, 0f, 0f), Space.World);
            }

            while (sprites.Count > 0 && sprites[0].bounds.max.x < leftLimit)
            {
                Object.Destroy(sprites[0].gameObject);
                sprites.RemoveAt(0);
            }

            while (sprites.Count < totalSprites)
            {
                SpriteRenderer last = sprites[sprites.Count - 1];
                SpriteRenderer copy = Object.Instantiate(template, root, true);
                copy.gameObject.SetActive(true);
                copy.transform.position = last.transform.position + new Vector3(width, 0f, 0f);
                sprites.Add(copy);
            }
        }
    }

    public TypingController typing;     // drag the object that has TypingController
    public float smoothSpeed = 4f;      // how fast it glides after each key
    public Layer farMountain;
    public Layer midMountain;

    private bool movementEnabled = true;

    public void SetMovementEnabled(bool value)
    {
        movementEnabled = value;
        if (!value)
        {
            farMountain?.ClearPending();
            midMountain?.ClearPending();
        }
    }

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
        if (!movementEnabled) return;
        farMountain.OnTyped();
        midMountain.OnTyped();
    }

    void Start()
    {
        farMountain.Init();
        midMountain.Init();
    }

    void Update()
    {
        if (!movementEnabled) return;
        farMountain.Tick(smoothSpeed);
        midMountain.Tick(smoothSpeed);
    }
}