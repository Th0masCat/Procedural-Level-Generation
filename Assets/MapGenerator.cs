using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    int[,] generatedMap;

    public int width;
    public int height;

    public string seed;
    private System.Random pseudoRandom;

    [Range(0, 100)]
    public int fillPercentage;

    void Update()
    {
        CellularAutomata();
    }

    void CellularAutomata()
    {
        seed = (seed.Length <= 0) ? Time.time.ToString() : seed;
        pseudoRandom = new System.Random(seed.GetHashCode());

        GenerateMap();

        for (int i = 0; i < 5; i++)
            SmoothMap();

        RemoveUnreachableCells();
        RestoreEdge();
    }

    void GenerateMap()
    {
        generatedMap = new int[width, height];

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                generatedMap[x, y] = (pseudoRandom.Next(0, 100) < fillPercentage) ? 1 : 0;
            }
        }
    }

    void SmoothMap()
    {
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                //Rules CA
                // T > 4 => C = true
                // T = 4 => C = C
                // T < 4 => C = false
                int neighbours = getNeighbours(x, y, generatedMap);

                if (neighbours > 4)
                    generatedMap[x, y] = 1;
                else if (neighbours < 4)
                    generatedMap[x, y] = 0;
            }
        }
    }


    int getNeighbours(int x, int y, int[,] m)
    {
        int neighbours = 0;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                neighbours += m[i + x, j + y];
            }
        }
        neighbours -= m[x, y];

        return neighbours;
    }

    void RestoreEdge()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                    generatedMap[x, y] = 0;
            }
        }
    }

    void RemoveUnreachableCells()
    {
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                generatedMap[x, y] = (getNeighbours(x, y, generatedMap) <= 0) ? 0 : generatedMap[x, y];
            }
        }
    }

    private void OnDrawGizmos()
    {
        if(generatedMap != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gizmos.color = (generatedMap[x, y] == 1) ? Color.white : Color.black;
                    Vector3 pos = new Vector3(x + .5f, 0, y + .5f);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }
    }

}
