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

    //Hilbert Curve:
    Vector2[] hilbertPts;
    public int hilbertOrder;
    public int hilbertSize;
    int hilbertIndex;

    void Update()
    {
        CellularAutomata();
    }

    void CellularAutomata()
    {
        seed = (seed.Length <= 0) ? Time.time.ToString() : seed;
        pseudoRandom = new System.Random(seed.GetHashCode());

        hilbertPts = new Vector2[(int)Mathf.Pow(4, hilbertOrder)];
        hilbertIndex = 0;

        HilbertCurve(0.0f, 0.0f, 1.0f * Mathf.Max(width, height),
                     0.0f, 0.0f, 1.0f * Mathf.Max(width, height),
                     hilbertOrder);


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


    void HilbertCurve(float x, float y, float xi, float xj, float yi, float yj, int n)
    {
        /*
         * Hilbert Curve - Original algorithm by Andrew Cumming: 
         * http://www.fundza.com/algorithmic/space_filling/hilbert/basics/
         * def hilbert(x0, y0, xi, xj, yi, yj, n):
         *   if n <= 0:
         *     X = x0 + (xi + yi)/2
         *     Y = y0 + (xj + yj)/2
         *
         *   # Output the coordinates of the cv
         *   print("%s %s 0" % (X, Y))
         *   else:
         *     hilbert(x0,               y0,               yi/2, yj/2, xi/2, xj/2, n - 1)
         *     hilbert(x0 + xi/2,        y0 + xj/2,        xi/2, xj/2, yi/2, yj/2, n - 1)
         *     hilbert(x0 + xi/2 + yi/2, y0 + xj/2 + yj/2, xi/2, xj/2, yi/2, yj/2, n - 1)
         *     hilbert(x0 + xi/2 + yi,   y0 + xj/2 + yj,  -yi/2,-yj/2,-xi/2,-xj/2, n - 1)
         */

        if (n <= 0)
        {
            float X = x + (xi + yi) / 2;
            float Y = y + (xj + yj) / 2;
            hilbertPts[hilbertIndex] = new Vector2((int)X * hilbertSize, (int)Y * hilbertSize);
            hilbertIndex++;
        }
        else
        {
            HilbertCurve(x, y, yi / 2, yj / 2, xi / 2, xj / 2, n - 1);
            HilbertCurve(x + xi / 2, y + xj / 2, xi / 2, xj / 2, yi / 2, yj / 2, n - 1);
            HilbertCurve(x + xi / 2 + yi / 2, y + xj / 2 + yj / 2, xi / 2, xj / 2, yi / 2, yj / 2, n - 1);
            HilbertCurve(x + xi / 2 + yi, y + xj / 2 + yj, -yi / 2, -yj / 2, -xi / 2, -xj / 2, n - 1);
        }
    }

    private void OnDrawGizmos()
    {
        if (generatedMap != null)
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

        if (hilbertPts != null)
        {
            for (int i = 0; i < hilbertPts.Length - 1; i++)
            {
                if (hilbertPts[i].Equals(Vector2.zero) ||
                    hilbertPts[i + 1].Equals(Vector2.zero))
                    continue;

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(new Vector3(hilbertPts[i].x + .5f, 0, hilbertPts[i].y + .5f),
                                new Vector3(hilbertPts[i + 1].x + .5f, 0, hilbertPts[i + 1].y + .5f));
            }
        }
    }

}
