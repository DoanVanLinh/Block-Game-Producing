using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GamePlay : MonoBehaviour
{
    [SerializeField] private int xSize;
    [SerializeField] private int ySize;
    [SerializeField] List<Gem> gemPrefabs;
    [SerializeField] float travelTime = .6f;
    [SerializeField] GameObject backgroundTile;
    [SerializeField] GameObject frame;
    [SerializeField] GameObject[] backgroundGrid;

    private Gem[,] grid;
    private List<Gem> twoChosenGems = new List<Gem>();
    private bool isUpdateDone = true;
    // private bool isDestroying = false;

    #region Singleton
    public static GamePlay Instance { get; set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }
    #endregion
    void Start()
    {
        InitalGrid();
        InitalBackground();
        Camera.main.transform.position = new Vector3(xSize / 2f, ySize / 2f, -10f);
        EndGame();
        StartCoroutine("CheckGrid");
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            CheckIsStuck();
        StartCoroutine("CheckGrid");
    }
    void InitalGrid()
    {
        grid = new Gem[xSize, ySize * 2];

        for (int i = 0; i < xSize; i++)
        {
            for (int j = 0; j < ySize; j++)
            {
                Gem gem = Instantiate(gemPrefabs[Random.Range(0, gemPrefabs.Count)],
                                    new Vector2(i, j),
                                    Quaternion.identity, transform) as Gem;
                gem.SetPosition(i, j, this);
                grid[i, j] = gem;
                if (i % 2 == 0)
                {
                    if (j % 2 == 0)
                        Instantiate(backgroundGrid[0], new Vector2(i, j), Quaternion.identity, transform);
                    else
                        Instantiate(backgroundGrid[1], new Vector2(i, j), Quaternion.identity, transform);
                }
                else
                {
                    if (j % 2 == 0)
                        Instantiate(backgroundGrid[1], new Vector2(i, j), Quaternion.identity, transform);
                    else
                        Instantiate(backgroundGrid[0], new Vector2(i, j), Quaternion.identity, transform);
                }

            }
        }
    }
    void InitalBackground()
    {
        int xBackgroundSize = xSize * 10;
        int yBackgroundSize = ySize * 10;

        for (int i = -xBackgroundSize; i < xBackgroundSize; i++)
        {
            for (int j = -yBackgroundSize; j < yBackgroundSize; j++)
            {
                if (i < 0 || i >= xSize || j < 0 || j >= ySize)
                    Instantiate(backgroundTile, new Vector2(i, j), Quaternion.identity, transform);
            }
        }
    }
    IEnumerator CheckGrid()
    {
        yield return new WaitUntil(() => isUpdateDone == true);
        yield return new WaitForSeconds(travelTime * 2);
        //StartCoroutine("DestroyLegalGem");
        DestroyLegalGem();
    }

    #region CHECK LEGAL GEMS >=3
    List<Gem> CheckVerticalGrid()
    {
        List<Gem> listGemCheck = new List<Gem>();
        List<Gem> listGemResult = new List<Gem>();
        int type = 0;

        for (int i = 0; i < xSize; i++)
        {
            for (int j = 0; j < ySize; j++)
            {
                if (grid[i, j] == null)
                {
                    listGemCheck.Clear();
                    continue;
                }
                if (grid[i, j].Type != type)
                {
                    if (listGemCheck.Count >= 3)
                        listGemResult.AddRange(listGemCheck);
                    listGemCheck.Clear();
                }
                type = grid[i, j].Type;
                listGemCheck.Add(grid[i, j]);
            }
            if (listGemCheck.Count >= 3)
                listGemResult.AddRange(listGemCheck);
            listGemCheck.Clear();
        }
        return listGemResult;
    }

    List<Gem> CheckHorizontalGrid()
    {
        List<Gem> listGemCheck = new List<Gem>();
        List<Gem> listGemResult = new List<Gem>();
        int type = 0;

        for (int j = 0; j < ySize; j++)
        {
            for (int i = 0; i < xSize; i++)
            {
                if (grid[i, j] == null)
                {
                    listGemCheck.Clear();
                    continue;
                }
                if (grid[i, j].Type != type)
                {
                    if (listGemCheck.Count >= 3)
                        listGemResult.AddRange(listGemCheck);
                    listGemCheck.Clear();
                }
                type = grid[i, j].Type;
                listGemCheck.Add(grid[i, j]);
            }
            if (listGemCheck.Count >= 3)
                listGemResult.AddRange(listGemCheck);
            listGemCheck.Clear();
        }
        return listGemResult;
    }

    void DestroyLegalGem()
    {
        HashSet<Gem> listGemToDestroy = new HashSet<Gem>();
        listGemToDestroy.UnionWith(CheckHorizontalGrid());
        listGemToDestroy.UnionWith(CheckVerticalGrid());

        if (listGemToDestroy.Count != 0)
        {
            int type = 0;
            List<Gem> listToCalculatorPoint = new List<Gem>();
            foreach (var item in listGemToDestroy)
            {
                if (item.Type != type || listGemToDestroy.Last() == item)
                {
                    if (listToCalculatorPoint.Count != 0)
                    {
                        GameManager.Instance.CalculatorPoint(listToCalculatorPoint.Count + 1);
                        UIManager.Instance.UpdatePoint();
                        listToCalculatorPoint.Clear();
                    }
                }
                type = item.Type;
                listToCalculatorPoint.Add(item);

                Gem gem = Instantiate(gemPrefabs[Random.Range(0, gemPrefabs.Count)],
                                        new Vector2(item.X, item.Y + ySize),
                                        Quaternion.identity, transform) as Gem;
                gem.SetPosition(item.X, item.Y + ySize, this);
                grid[item.X, item.Y + ySize] = gem;

                grid[item.X, item.Y] = null;
                item.GetComponent<Animator>().SetTrigger("Fade");
                Destroy(item, 1.5f);
                Destroy(item.gameObject, 1.5f);
            }
            StartCoroutine("UpdateGrid");
        }
        else
        {
            return;
        }
    }
    #endregion

    #region MOVE BLOCK AFTER BLOW UP
    IEnumerator GemFall(int x, int y)
    {
        if (x < 0 || x > xSize || y <= 0 || y > ySize * 2)//outside of grid
            yield break;

        if (grid[x, y] == null)//gem empty
            yield break;

        if (grid[x, y - 1] != null)//under gem not empty
            yield break;
        int tempY = y;

        while (tempY > 0 && grid[x, tempY - 1] == null)
        {
            grid[x, tempY].transform.DOMove(new Vector2(x, tempY - 1), travelTime);

            grid[x, tempY].SetPosition(x, tempY - 1, this);

            Gem tempGem;
            tempGem = grid[x, tempY];
            grid[x, tempY] = grid[x, tempY - 1];
            grid[x, tempY - 1] = tempGem;


            tempY--;
        }
        yield return new WaitForSeconds(1);
    }

    IEnumerator UpdateGrid()
    {
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < xSize; i++)
        {
            for (int j = 0; j < ySize * 2; j++)
            {
                StartCoroutine(GemFall(i, j));
            }
        }
    }
    #endregion

    #region  MOVE BLOCK
    void SwapPosition(int x1, int y1, int x2, int y2)
    {
        if (grid[x1, y1] == null || grid[x2, y2] == null)
            return;

        grid[x1, y1].transform.DOMove(new Vector2(x2, y2), travelTime / 2f);
        grid[x2, y2].transform.DOMove(new Vector2(x1, y1), travelTime / 2f);

        grid[x1, y1].SetPosition(x2, y2, this);
        grid[x2, y2].SetPosition(x1, y1, this);

        Gem tempGem;
        tempGem = grid[x1, y1];
        grid[x1, y1] = grid[x2, y2];
        grid[x2, y2] = tempGem;

    }
    public IEnumerator AddOrRemoveGemChosen(Gem gem)
    {
        if (GameManager.Instance.IsEnd)
            yield break;
        if (GameManager.Instance.IsPause)
            yield break;

        if (twoChosenGems.Contains(gem))//unchose the first one 
        {
            Destroy(twoChosenGems[0].transform.GetChild(0).gameObject);
            twoChosenGems.Remove(gem);
        }
        else
        {
            if (twoChosenGems.Count == 0)//the first
            {
                twoChosenGems.Add(gem);
                Instantiate(frame, twoChosenGems[0].transform.position, Quaternion.identity, twoChosenGems[0].transform);
            }
            else//the second
            {
                if (CheckNeighborGem(twoChosenGems[0], gem))
                {
                    twoChosenGems.Add(gem);
                    Destroy(twoChosenGems[0].transform.GetChild(0).gameObject);

                    SwapPosition(twoChosenGems[0].X, twoChosenGems[0].Y, twoChosenGems[1].X, twoChosenGems[1].Y);

                    HashSet<Gem> listGemToDestroy = new HashSet<Gem>();
                    listGemToDestroy.UnionWith(CheckHorizontalGrid());
                    listGemToDestroy.UnionWith(CheckVerticalGrid());

                    if (listGemToDestroy.Count == 0)
                    {
                        yield return new WaitForSeconds(travelTime / 2f);
                        SwapPosition(twoChosenGems[1].X, twoChosenGems[1].Y, twoChosenGems[0].X, twoChosenGems[0].Y);
                    }
                    else
                        StartCoroutine("CheckGrid");
                    EndGame();

                    twoChosenGems.Clear();
                }
                else
                {
                    Destroy(twoChosenGems[0].transform.GetChild(0).gameObject);
                    twoChosenGems.Clear();
                }
            }
        }
    }
    bool CheckNeighborGem(Gem gem, Gem gemToCheck)
    {
        List<Vector2> neighborPosition = new List<Vector2>() { Vector2.up, Vector2.right, Vector2.down, Vector2.left };
        foreach (var item in neighborPosition)
        {
            if (gem.X + item.x == gemToCheck.X && gem.Y + item.y == gemToCheck.Y)
                return true;
        }


        return false;
    }
    #endregion

    #region CHECK IS STUCK

    bool CheckIsStuck()
    {
        for (int i = 0; i < xSize; i++)//Check Vertical
        {
            for (int j = 0; j < ySize - 1; j++)
            {
                Swap(i, j, i, j + 1);
                if (CheckHorizontalGrid().Count != 0)
                {
                    Swap(i, j, i, j + 1);
                    return GameManager.Instance.IsEnd = false;
                }
                if (CheckVerticalGrid().Count != 0)
                {
                    Swap(i, j, i, j + 1);
                    return GameManager.Instance.IsEnd = false;
                }
                Swap(i, j, i, j + 1);
            }
        }

        for (int j = 0; j < ySize; j++)//Check Horizontal
        {
            for (int i = 0; i < xSize - 1; i++)
            {
                Swap(i, j, i + 1, j);
                if (CheckHorizontalGrid().Count != 0)
                {
                    Swap(i, j, i + 1, j);
                    return GameManager.Instance.IsEnd = false;
                }
                if (CheckVerticalGrid().Count != 0)
                {
                    Swap(i, j, i + 1, j);
                    return GameManager.Instance.IsEnd = false;
                }
                Swap(i, j, i + 1, j);
            }
        }

        return GameManager.Instance.IsEnd = true;

        void Swap(int x1, int y1, int x2, int y2)
        {
            if (grid[x1, y1] == null || grid[x2, y2] == null)
                return;

            grid[x1, y1].SetPosition(x2, y2, this);
            grid[x2, y2].SetPosition(x1, y1, this);

            Gem tempGem;
            tempGem = grid[x1, y1];
            grid[x1, y1] = grid[x2, y2];
            grid[x2, y2] = tempGem;
        }
    }
    #endregion

    #region  END GAME
    void EndGame()
    {
        Debug.Log((GameManager.Instance.IsEnd = CheckIsStuck()) == true ? "NO germs left to blow up!!!" : "Have germs to blow up!!!");
        if (GameManager.Instance.IsEnd)
            UIManager.Instance.Pause();
    }
    #endregion
}
