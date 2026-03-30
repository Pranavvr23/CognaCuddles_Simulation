using UnityEngine;

public class GridScript : MonoBehaviour
{
    public int Width = 24;
    public int Height = 10;
    public GameObject prefab;
    public GameObject enemy;
    public float cutoffRadius = 20f;

    private Pixel[,] grid;

    void Awake()
    {
        grid = new Pixel[Width, Height];
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                grid[i, j] = Instantiate(prefab, new Vector3(i, j, 0), Quaternion.identity).GetComponent<Pixel>();
            }
        }
    }

    void Update()
    {
        Vector3 enemyPos = enemy.transform.position;

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                if (i >= CONSTANTS.minX && i <= CONSTANTS.maxX && j >= CONSTANTS.minY && j <= CONSTANTS.maxY)
                {
                    float dist = Vector3.Distance(new Vector3(i, j, 0), enemyPos);
                    int value = dist <= cutoffRadius ? Mathf.Clamp((int)((1f - dist / cutoffRadius) * 1000f), 0, 1000) : 0;
                    setGrid(i, j, value);
                }
            }
        }

        // Border override — only raise values, never lower them
        for (int x = CONSTANTS.minX; x <= CONSTANTS.maxX; x++)
        {
            setGrid(x, CONSTANTS.minY, 500 + getGrid(x,CONSTANTS.minY));
            setGrid(x, CONSTANTS.maxY, 500 + getGrid(x, CONSTANTS.maxY));
        }
        for (int y = CONSTANTS.minY; y <= CONSTANTS.maxY; y++)
        {
            setGrid(CONSTANTS.minX, y, 500 + getGrid(CONSTANTS.minX, y));
            setGrid(CONSTANTS.maxX, y, 500 + getGrid(CONSTANTS.maxX, y));
        }
    }

    void setGrid(int x, int y, int v) => grid[x, y].value = v;
    public int getGrid(int x, int y) => grid[x, y].value;
    double getGridPosX(int x, int y) => grid[x, y].transform.position.x;
    double getGridPosY(int x, int y) => grid[x, y].transform.position.y;
}   