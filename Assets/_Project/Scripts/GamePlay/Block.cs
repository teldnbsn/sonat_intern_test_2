using System.Collections;
using UnityEngine;

public class Block : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private SpriteRenderer blockRenderer;
    [SerializeField] private SpriteRenderer arrowRenderer;
    [SerializeField] private GameObject specialMark;

    [Header("Direction")]
    [SerializeField] private BlockDirection direction = BlockDirection.Right;

    [Header("Visual Variants")]
    [SerializeField] private Sprite[] possibleBlockSprites;

    [Header("Special Block")]
    [SerializeField] private bool isSpecial = false;

    [Header("Ice Block")]
    [SerializeField] private bool isFrozen = false;
    [SerializeField] private GameObject iceOverlay;

public bool IsFrozen => isFrozen;

    [Header("Animation")]
    [SerializeField] private float exitSlideDuration = 0.45f;
    [SerializeField] private float hitSlideDuration = 0.18f;

    public Vector2Int GridPosition { get; private set; }
    public BlockDirection Direction => direction;
    public bool IsSpecial => isSpecial;
    public bool IsAnimating { get; private set; }

    private BoardManager boardManager;

    public void Initialize(
        BoardManager boardManager,
        Vector2Int gridPosition,
        BlockDirection blockDirection,
        bool isSpecialBlock,
        bool useRandomColor,
        int colorIndex,
        bool isFrozenBlock)
    {
        this.boardManager = boardManager;
        GridPosition = gridPosition;
        direction = blockDirection;
        isSpecial = isSpecialBlock;
        isFrozen = isFrozenBlock;

        SetupBlockSprite(useRandomColor, colorIndex);
        UpdateVisual();
    }

    public void SetGridPosition(Vector2Int newGridPosition)
    {
        GridPosition = newGridPosition;
    }

    private void OnMouseDown()
    {
        if (boardManager == null || IsAnimating)
            return;

        if (isFrozen)
        {
            BreakIce();
            return;
        }

        boardManager.TryRemoveBlock(this);
    }   

    private void SetupBlockSprite(bool useRandomColor, int colorIndex)
    {
        if (blockRenderer == null)
            return;

        if (possibleBlockSprites == null || possibleBlockSprites.Length == 0)
            return;

        if (useRandomColor)
        {
            blockRenderer.sprite = possibleBlockSprites[Random.Range(0, possibleBlockSprites.Length)];
        }
        else
        {
            int safeIndex = Mathf.Clamp(colorIndex, 0, possibleBlockSprites.Length - 1);
            blockRenderer.sprite = possibleBlockSprites[safeIndex];
        }
    }

    private void UpdateVisual()
    {
        UpdateArrowRotation();
        UpdateArrowSorting();

        if (specialMark != null)
        {
            specialMark.SetActive(isSpecial);
        }

        if (arrowRenderer != null)
        {
            arrowRenderer.color = isSpecial ? new Color32(200, 80, 255, 255) : Color.white;
        }

        if (iceOverlay != null)
        {
            iceOverlay.SetActive(isFrozen);
        }
    }

    private void UpdateArrowRotation()
    {
        if (arrowRenderer == null)
            return;

        float zRotation = direction switch
        {
            BlockDirection.Up => 90f,
            BlockDirection.Right => 0f,
            BlockDirection.Down => -90f,
            BlockDirection.Left => 180f,
            _ => 0f
        };

        arrowRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, zRotation);
    }

    private void UpdateArrowSorting()
    {
        if (blockRenderer == null || arrowRenderer == null)
            return;

        arrowRenderer.sortingLayerID = blockRenderer.sortingLayerID;
        arrowRenderer.sortingOrder = blockRenderer.sortingOrder + 1;
    }

    public void SetSortingOrder(int baseSortingOrder)
    {
        if (blockRenderer != null)
            blockRenderer.sortingOrder = baseSortingOrder;

        if (arrowRenderer != null)
            arrowRenderer.sortingOrder = baseSortingOrder + 1;

        if (iceOverlay != null)
        {
            SpriteRenderer iceRenderer = iceOverlay.GetComponent<SpriteRenderer>();
            if (iceRenderer != null)
                iceRenderer.sortingOrder = baseSortingOrder + 2;
        }
    }

    public void RotateToRandomDirection()
    {
        if (!isSpecial || IsAnimating)
            return;

        BlockDirection newDirection = direction;

        while (newDirection == direction)
        {
            newDirection = (BlockDirection)Random.Range(0, 4);
        }

        direction = newDirection;
        UpdateVisual();
    }

    public IEnumerator PlayMoveTo(Vector3 targetPosition, bool removeAtEnd, bool playBlockedSoundAtEnd)
    {
        IsAnimating = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySlide();
        }

        float duration = removeAtEnd ? exitSlideDuration : hitSlideDuration;

        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(startPos, targetPosition, t);
            yield return null;
        }

        transform.position = targetPosition;

        if (playBlockedSoundAtEnd && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBlocked();
        }

        if (removeAtEnd)
        {
            Destroy(gameObject);
        }
        else
        {
            IsAnimating = false;
        }
    }

    public void PlayBlockedFeedback()
    {
        if (!gameObject.activeInHierarchy || IsAnimating)
            return;

        StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        Vector3 originalPos = transform.position;
        float duration = 0.1f;
        float strength = 0.05f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Vector3 offset = (Vector3)Random.insideUnitCircle * strength;
            transform.position = originalPos + offset;
            yield return null;
        }

        transform.position = originalPos;
    }

    public void BreakIce()
    {
        if (!isFrozen)
            return;

        isFrozen = false;

        if (iceOverlay != null)
            iceOverlay.SetActive(false);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayIceBreak();
    }
}