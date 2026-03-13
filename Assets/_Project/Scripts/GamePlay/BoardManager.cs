using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [System.Serializable]
    public class BlockSpawnData
    {
        public Vector2Int position;
        public BlockDirection direction;
        public bool isSpecial;
        public bool useRandomColor;
        public int colorIndex;
        public bool isFrozen;

        public BlockSpawnData(
            Vector2Int position,
            BlockDirection direction,
            bool isSpecial = false,
            bool isFrozen = false,
            bool useRandomColor = true,
            int colorIndex = 0)
        {
            this.position = position;
            this.direction = direction;
            this.isSpecial = isSpecial;
            this.isFrozen = isFrozen;
            this.useRandomColor = useRandomColor;
            this.colorIndex = colorIndex;
        }
    }

    [System.Serializable]
    public class LevelData
    {
        public string levelName;
        public int maxMoves;
        public List<BlockSpawnData> blocks = new List<BlockSpawnData>();
        public Color cameraBackgroundColor = Color.gray;

        public LevelData(string levelName, int maxMoves, Color backgroundColor)
        {
            this.levelName = levelName;
            this.maxMoves = maxMoves;
            cameraBackgroundColor = backgroundColor;
        }
    }

    private enum MoveResultType
    {
        ImmediateBlocked,
        HitBlock,
        ExitBoard
    }

    private struct MoveResult
    {
        public MoveResultType type;
        public Vector2Int targetGridPosition;
        public Vector3 targetWorldPosition;
    }

    [Header("References")]
    [SerializeField] private Block blockPrefab;
    [SerializeField] private Camera mainCamera;

    [Header("Board Settings")]
    [SerializeField] private float cellSize = 1.2f;

    [Header("UI")]
    [SerializeField] private UIManager uiManager;

    private Dictionary<Vector2Int, Block> blocks = new Dictionary<Vector2Int, Block>();
    private List<LevelData> levels = new List<LevelData>();

    private int currentLevelIndex = 0;
    private int remainingMoves;
    private int totalMovesMade;
    private bool isTransitioning;

    private void Start()
    {
        if (uiManager != null)
        {
            uiManager.Initialize(this);
        }

        CreateLevels();
        LoadLevel(0);
    }

    private Vector3 GetOffScreenTarget(Block block, Vector2Int moveOffset)
    {
        Camera cam = mainCamera != null ? mainCamera : Camera.main;

        if (cam == null)
        {
            return block.transform.position + (Vector3)(Vector2)moveOffset * 10f;
        }

        float zDistance = Mathf.Abs(cam.transform.position.z - block.transform.position.z);

        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0f, 0f, zDistance));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1f, 1f, zDistance));

        float margin = cellSize * 2f;
        Vector3 currentPos = block.transform.position;

        if (moveOffset == Vector2Int.right)
            return new Vector3(topRight.x + margin, currentPos.y, currentPos.z);

        if (moveOffset == Vector2Int.left)
            return new Vector3(bottomLeft.x - margin, currentPos.y, currentPos.z);

        if (moveOffset == Vector2Int.up)
            return new Vector3(currentPos.x, topRight.y + margin, currentPos.z);

        if (moveOffset == Vector2Int.down)
            return new Vector3(currentPos.x, bottomLeft.y - margin, currentPos.z);

        return currentPos;
    }

    private void CreateLevels()
    {
        levels.Clear();

        Color normalBg = new Color(0.22f, 0.25f, 0.32f);
        Color yellowBg = new Color(1f, 0.88f, 0.35f);

        // LEVEL 1
        LevelData level1 = new LevelData("Level 1", 18, normalBg);

        level1.blocks.Add(new BlockSpawnData(new Vector2Int(0, 0), BlockDirection.Left));
        level1.blocks.Add(new BlockSpawnData(new Vector2Int(1, 0), BlockDirection.Down));
        level1.blocks.Add(new BlockSpawnData(new Vector2Int(2, 0), BlockDirection.Right));

        level1.blocks.Add(new BlockSpawnData(new Vector2Int(0, 1), BlockDirection.Up));
        level1.blocks.Add(new BlockSpawnData(new Vector2Int(1, 1), BlockDirection.Up));
        level1.blocks.Add(new BlockSpawnData(new Vector2Int(2, 1), BlockDirection.Down));

        level1.blocks.Add(new BlockSpawnData(new Vector2Int(0, 2), BlockDirection.Left));
        level1.blocks.Add(new BlockSpawnData(new Vector2Int(1, 2), BlockDirection.Up));
        level1.blocks.Add(new BlockSpawnData(new Vector2Int(2, 2), BlockDirection.Right));

        level1.blocks.Add(new BlockSpawnData(new Vector2Int(-1, -1), BlockDirection.Up));
        level1.blocks.Add(new BlockSpawnData(new Vector2Int(3, -1), BlockDirection.Left));
        level1.blocks.Add(new BlockSpawnData(new Vector2Int(-1, 3), BlockDirection.Right));
        level1.blocks.Add(new BlockSpawnData(new Vector2Int(3, 3), BlockDirection.Down));



        levels.Add(level1);

        // LEVEL 2
        LevelData level2 = new LevelData("Level 2", 42, normalBg);

        level2.blocks.Add(new BlockSpawnData(new Vector2Int(0, 0), BlockDirection.Left));
        level2.blocks.Add(new BlockSpawnData(new Vector2Int(1, 0), BlockDirection.Down));
        level2.blocks.Add(new BlockSpawnData(new Vector2Int(2, 0), BlockDirection.Down));
        level2.blocks.Add(new BlockSpawnData(new Vector2Int(3, 0), BlockDirection.Down));
        level2.blocks.Add(new BlockSpawnData(new Vector2Int(4, 0), BlockDirection.Right));

        level2.blocks.Add(new BlockSpawnData(new Vector2Int(0, 1), BlockDirection.Left));
        level2.blocks.Add(new BlockSpawnData(new Vector2Int(1, 1), BlockDirection.Left, true));
        level2.blocks.Add(new BlockSpawnData(new Vector2Int(2, 1), BlockDirection.Up));
        level2.blocks.Add(new BlockSpawnData(new Vector2Int(3, 1), BlockDirection.Right, true));
        level2.blocks.Add(new BlockSpawnData(new Vector2Int(4, 1), BlockDirection.Right));

        level2.blocks.Add(new BlockSpawnData(new Vector2Int(0, 2), BlockDirection.Left));
        level2.blocks.Add(new BlockSpawnData(new Vector2Int(1, 2), BlockDirection.Up));
        level2.blocks.Add(new BlockSpawnData(new Vector2Int(2, 2), BlockDirection.Right));
        level2.blocks.Add(new BlockSpawnData(new Vector2Int(3, 2), BlockDirection.Down));
        level2.blocks.Add(new BlockSpawnData(new Vector2Int(4, 2), BlockDirection.Right));

        level2.blocks.Add(new BlockSpawnData(new Vector2Int(0, 3), BlockDirection.Left));
        level2.blocks.Add(new BlockSpawnData(new Vector2Int(1, 3), BlockDirection.Down, true));
        level2.blocks.Add(new BlockSpawnData(new Vector2Int(2, 3), BlockDirection.Left));
        level2.blocks.Add(new BlockSpawnData(new Vector2Int(3, 3), BlockDirection.Up, true));
        level2.blocks.Add(new BlockSpawnData(new Vector2Int(4, 3), BlockDirection.Right));

        level2.blocks.Add(new BlockSpawnData(new Vector2Int(0, 4), BlockDirection.Left));
        level2.blocks.Add(new BlockSpawnData(new Vector2Int(1, 4), BlockDirection.Up));
        level2.blocks.Add(new BlockSpawnData(new Vector2Int(2, 4), BlockDirection.Up));
        level2.blocks.Add(new BlockSpawnData(new Vector2Int(3, 4), BlockDirection.Up));
        level2.blocks.Add(new BlockSpawnData(new Vector2Int(4, 4), BlockDirection.Right));

        level2.blocks.Add(new BlockSpawnData(new Vector2Int(-2, 0), BlockDirection.Down));
        level2.blocks.Add(new BlockSpawnData(new Vector2Int(-2, 1), BlockDirection.Left));
        level2.blocks.Add(new BlockSpawnData(new Vector2Int(-2, 2), BlockDirection.Right, true));
        level2.blocks.Add(new BlockSpawnData(new Vector2Int(-2, 3), BlockDirection.Up));
        level2.blocks.Add(new BlockSpawnData(new Vector2Int(-2, 4), BlockDirection.Up));

        level2.blocks.Add(new BlockSpawnData(new Vector2Int(6, 0), BlockDirection.Down));
        level2.blocks.Add(new BlockSpawnData(new Vector2Int(6, 1), BlockDirection.Down));
        level2.blocks.Add(new BlockSpawnData(new Vector2Int(6, 2), BlockDirection.Left, true));
        level2.blocks.Add(new BlockSpawnData(new Vector2Int(6, 3), BlockDirection.Up));
        level2.blocks.Add(new BlockSpawnData(new Vector2Int(6, 4), BlockDirection.Up));


        levels.Add(level2);

        LevelData level3 = new LevelData("Level 3", 32, normalBg);


        level3.blocks.Add(new BlockSpawnData(new Vector2Int(0, 0), BlockDirection.Down, false, true));
        level3.blocks.Add(new BlockSpawnData(new Vector2Int(2, 0), BlockDirection.Up, false, true));
        level3.blocks.Add(new BlockSpawnData(new Vector2Int(4, 0), BlockDirection.Left, false, true));


        level3.blocks.Add(new BlockSpawnData(new Vector2Int(1, 1), BlockDirection.Right, false, true));
        level3.blocks.Add(new BlockSpawnData(new Vector2Int(3, 1), BlockDirection.Right, false, true));
        level3.blocks.Add(new BlockSpawnData(new Vector2Int(5, 1), BlockDirection.Down, false, true));


        level3.blocks.Add(new BlockSpawnData(new Vector2Int(0, 2), BlockDirection.Left, false, true));
        level3.blocks.Add(new BlockSpawnData(new Vector2Int(2, 2), BlockDirection.Up, false, true));
        level3.blocks.Add(new BlockSpawnData(new Vector2Int(4, 2), BlockDirection.Left, false, true));


        level3.blocks.Add(new BlockSpawnData(new Vector2Int(1, 3), BlockDirection.Down, false, true));
        level3.blocks.Add(new BlockSpawnData(new Vector2Int(3, 3), BlockDirection.Right, false, true));
        level3.blocks.Add(new BlockSpawnData(new Vector2Int(5, 3), BlockDirection.Up, false, true));


        level3.blocks.Add(new BlockSpawnData(new Vector2Int(0, 4), BlockDirection.Up, false, true));
        level3.blocks.Add(new BlockSpawnData(new Vector2Int(2, 4), BlockDirection.Up, false, true));
        level3.blocks.Add(new BlockSpawnData(new Vector2Int(4, 4), BlockDirection.Left, false, true));


        level3.blocks.Add(new BlockSpawnData(new Vector2Int(1, 5), BlockDirection.Up, false, true));
        level3.blocks.Add(new BlockSpawnData(new Vector2Int(3, 5), BlockDirection.Right, false, true));
        level3.blocks.Add(new BlockSpawnData(new Vector2Int(5, 5), BlockDirection.Right, false, true));

        levels.Add(level3);
    }

    private BlockDirection GetPatternDirection(int x, int y)
    {
        int value = (x + y) % 4;

        switch (value)
        {
            case 0: return BlockDirection.Up;
            case 1: return BlockDirection.Right;
            case 2: return BlockDirection.Down;
            default: return BlockDirection.Left;
        }
    }

    private void LoadLevel(int levelIndex)
    {
        ClearCurrentBoard();

        if (uiManager != null)
            uiManager.HideAllPanels();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopSfx();
            AudioManager.Instance.PlayGameplayBgm();
        }

        isTransitioning = false;
        currentLevelIndex = levelIndex;
        remainingMoves = levels[levelIndex].maxMoves;
        totalMovesMade = 0;

        if (uiManager != null)
        {
            uiManager.UpdateMovesText(remainingMoves);
            uiManager.UpdateLevelText(levels[levelIndex].levelName);
        }

        if (mainCamera != null)
        {
            mainCamera.backgroundColor = levels[levelIndex].cameraBackgroundColor;
        }

        LevelData level = levels[levelIndex];
        Vector3 boardOffset = GetBoardOffset(level.blocks);

        foreach (BlockSpawnData data in level.blocks)
        {
            CreateBlock(data, boardOffset);
        }

        Debug.Log("Loaded " + level.levelName + " | Moves: " + remainingMoves);
    }

    private void ClearCurrentBoard()
    {
        foreach (Block block in blocks.Values)
        {
            if (block != null)
            {
                Destroy(block.gameObject);
            }
        }

        blocks.Clear();
    }

    private void CreateBlock(BlockSpawnData data, Vector3 boardOffset)
    {
        Vector3 worldPosition = GridToWorld(data.position, boardOffset);

        Block block = Instantiate(blockPrefab, worldPosition, Quaternion.identity, transform);
        block.Initialize(this, data.position, data.direction, data.isSpecial, data.useRandomColor, data.colorIndex, data.isFrozen);

        int sortingOrder = -data.position.y * 10;
        block.SetSortingOrder(sortingOrder);

        blocks.Add(data.position, block);
    }

    public void TryRemoveBlock(Block block)
    {
        if (remainingMoves <= 0 || block == null || block.IsAnimating || isTransitioning)
            return;

        remainingMoves--;
        totalMovesMade++;

        if (uiManager != null)
        {
            uiManager.UpdateMovesText(remainingMoves);
        }

        MoveResult moveResult = CalculateMoveResult(block);

        if (moveResult.type == MoveResultType.ImmediateBlocked)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBlocked();
            }

            block.PlayBlockedFeedback();
            
            RotateSpecialBlocks();

            CheckGameState();
            return;
        }

        StartCoroutine(HandleBlockMoveRoutine(block, moveResult));
    }

    private IEnumerator HandleBlockMoveRoutine(Block block, MoveResult moveResult)
    {
        Vector2Int oldGridPosition = block.GridPosition;
        bool removeAtEnd = moveResult.type == MoveResultType.ExitBoard;
        bool playBlockedAtEnd = moveResult.type == MoveResultType.HitBlock;

        blocks.Remove(oldGridPosition);

        yield return StartCoroutine(block.PlayMoveTo(
            moveResult.targetWorldPosition,
            removeAtEnd,
            playBlockedAtEnd));

        if (moveResult.type == MoveResultType.HitBlock)
        {
            Vector2Int nextPos = moveResult.targetGridPosition + GetDirectionOffset(block.Direction);

            if (blocks.ContainsKey(nextPos))
            {
                Block hitBlock = blocks[nextPos];

                if (hitBlock.IsFrozen)
                {
                    hitBlock.BreakIce();
                }
            }
        }

        if (!removeAtEnd)
        {
            block.SetGridPosition(moveResult.targetGridPosition);

            int sortingOrder = -moveResult.targetGridPosition.y * 10;
            block.SetSortingOrder(sortingOrder);

            blocks[moveResult.targetGridPosition] = block;
        }
            RotateSpecialBlocks();

        CheckGameState();
    }

    private MoveResult CalculateMoveResult(Block block)
    {
        LevelData level = levels[currentLevelIndex];
        Vector3 boardOffset = GetBoardOffset(level.blocks);
        Vector2Int offset = GetDirectionOffset(block.Direction);

        Vector2Int nextPos = block.GridPosition + offset;

        if (blocks.ContainsKey(nextPos))
        {
            return new MoveResult
            {
                type = MoveResultType.ImmediateBlocked,
                targetGridPosition = block.GridPosition,
                targetWorldPosition = block.transform.position
            };
        }

        Vector2Int current = nextPos;
        Vector2Int lastFreeCell = block.GridPosition;

        while (IsInsideBoard(current, level.blocks))
        {
            if (blocks.ContainsKey(current))
        {
            return new MoveResult
            {
                type = MoveResultType.HitBlock,
                targetGridPosition = lastFreeCell,
                targetWorldPosition = GridToWorld(lastFreeCell, boardOffset)
            };
        }

            lastFreeCell = current;
            current += offset;
        }

        Vector3 exitTarget = GetOffScreenTarget(block, offset);

        return new MoveResult
        {
            type = MoveResultType.ExitBoard,
            targetGridPosition = current,
            targetWorldPosition = exitTarget
        };
    }

    private void RotateSpecialBlocks()
    {
        foreach (Block block in blocks.Values)
        {
            if (block != null && block.IsSpecial)
            {
                block.RotateToRandomDirection();
            }
        }
    }

    private Vector2Int GetDirectionOffset(BlockDirection direction)
    {
        switch (direction)
        {
            case BlockDirection.Up: return Vector2Int.up;
            case BlockDirection.Down: return Vector2Int.down;
            case BlockDirection.Left: return Vector2Int.left;
            case BlockDirection.Right: return Vector2Int.right;
            default: return Vector2Int.zero;
        }
    }

    private bool IsInsideBoard(Vector2Int pos, List<BlockSpawnData> blockDataList)
    {
        int minX = int.MaxValue;
        int maxX = int.MinValue;
        int minY = int.MaxValue;
        int maxY = int.MinValue;

        foreach (BlockSpawnData data in blockDataList)
        {
            if (data.position.x < minX) minX = data.position.x;
            if (data.position.x > maxX) maxX = data.position.x;
            if (data.position.y < minY) minY = data.position.y;
            if (data.position.y > maxY) maxY = data.position.y;
        }

        return pos.x >= minX && pos.x <= maxX && pos.y >= minY && pos.y <= maxY;
    }

    private Vector3 GridToWorld(Vector2Int gridPosition, Vector3 boardOffset)
    {
        return new Vector3(
            gridPosition.x * cellSize,
            gridPosition.y * cellSize,
            0f) + boardOffset;
    }

    private void CheckGameState()
    {
        if (blocks.Count == 0)
        {
            bool hasNextLevel = currentLevelIndex < levels.Count - 1;
            isTransitioning = true;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopBgm();
                AudioManager.Instance.PlayWin();
            }

            if (uiManager != null)
            {
                uiManager.ShowWin(hasNextLevel);
            }

            return;
        }

        if (remainingMoves <= 0)
        {
            isTransitioning = true;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopBgm();
                AudioManager.Instance.PlayLose();
            }

            if (uiManager != null)
            {
                uiManager.ShowLose();
            }
        }
    }

    private Vector3 GetBoardOffset(List<BlockSpawnData> blockDataList)
    {
        if (blockDataList == null || blockDataList.Count == 0)
            return Vector3.zero;

        int minX = int.MaxValue;
        int maxX = int.MinValue;
        int minY = int.MaxValue;
        int maxY = int.MinValue;

        foreach (BlockSpawnData data in blockDataList)
        {
            if (data.position.x < minX) minX = data.position.x;
            if (data.position.x > maxX) maxX = data.position.x;
            if (data.position.y < minY) minY = data.position.y;
            if (data.position.y > maxY) maxY = data.position.y;
        }

        float width = (maxX - minX + 1) * cellSize;
        float height = (maxY - minY + 1) * cellSize;

        float offsetX = -width / 2f + cellSize / 2f;
        float offsetY = -height / 2f + cellSize / 2f;

        return new Vector3(offsetX, offsetY, 0f);
    }

    public void RestartCurrentLevelFromUI()
    {
        LoadLevel(currentLevelIndex);
    }

    public void LoadNextLevelFromUI()
    {
        if (currentLevelIndex < levels.Count - 1)
        {
            LoadLevel(currentLevelIndex + 1);
        }
    }
}