using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TileMap : MonoBehaviour
{
    /*this is the selected object will have to make a function that can make it so one can change 
    unit that is selected*/
    public GameObject selectedUnit;

    public TileType[] tileTypes;

    int[,] tiles;
    Node[,] graph;

    List<Node> CurrentPath = null;

    public int mapSizeX = 10;
    public int mapSizeY = 10;
	// Use this for initialization
	void Start ()
    {
        selectedUnit.GetComponent<Unit>().tileX = (int)selectedUnit.transform.position.x;
        selectedUnit.GetComponent<Unit>().tileY = (int)selectedUnit.transform.position.y;
        selectedUnit.GetComponent<Unit>().map = this;
        MapDataGeneration();
        GeneratePathfindingGraph();
        GenerateMapVisuals();
	}

    public float CostToEnterTile(int sourceX, int sourceY,int targetX, int targetY)
    {

        TileType tt = tileTypes[tiles[targetX, targetY]];

        if(UnitCanEnterTile(targetX,targetY)==false)
        { return Mathf.Infinity; }
        float cost = tt.movementCost;
        if(sourceX!= targetX && sourceY!= targetY)
        {
            cost += 0.001f;
        }
        return cost;
    }
   
    void GeneratePathfindingGraph()
    {
        // initilaize the array;
        graph = new Node[mapSizeX, mapSizeY];
        
        //initialize a node in the each spot in the array
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                graph[x, y] = new Node();
                graph[x, y].x = x;
                graph[x, y].y = y;
            }
        }

        //now that all nodes exisat calculate thier neighbours;
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                /*we currently have a 4 way connected map, 
                  * this also works for a 6 way connected map,*/
                /* if (x > 0)
                 { graph[x, y].neighbours.Add(graph[x - 1, y]); }
                 if (x < mapSizeX - 1)
                 { graph[x, y].neighbours.Add(graph[x + 1, y]);}
                 if (y > 0)
                 { graph[x, y].neighbours.Add(graph[x, y - 1]);}
                 if (y < mapSizeY - 1)
                 { graph[x, y].neighbours.Add(graph[x, y + 1]);}*/

                if (x > 0)
                {
                    graph[x, y].neighbours.Add(graph[x - 1, y]);
                    if (y > 0)
                    { graph[x, y].neighbours.Add(graph[x - 1, y-1]); }
                    if (y < mapSizeY - 1)
                    { graph[x, y].neighbours.Add(graph[x-1, y + 1]); }
                }
                if (x < mapSizeX - 1)
                {
                    graph[x, y].neighbours.Add(graph[x + 1, y]);
                    if (y > 0)
                    { graph[x, y].neighbours.Add(graph[x + 1, y - 1]); }
                    if (y < mapSizeY - 1)
                    { graph[x, y].neighbours.Add(graph[x + 1, y + 1]); }
                }

                if (y > 0)
                { graph[x, y].neighbours.Add(graph[x, y - 1]); }
                if (y < mapSizeY - 1)
                { graph[x, y].neighbours.Add(graph[x, y + 1]); }

            }
        }
        
    }
    void MapDataGeneration()
    {

        tiles = new int[mapSizeX, mapSizeY];
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                tiles[x, y] = 0;
            }
        }
        tiles[2, 3] = 1;
        tiles[3, 3] = 1;
        tiles[4, 3] = 1;
        tiles[3, 4] = 2;
        tiles[4, 5] = 2;
        tiles[4, 6] = 2;
        tiles[4, 7] = 2;
        tiles[4, 8] = 2;

    }
    void GenerateMapVisuals()
    {

        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                //temporary
                TileType tt = tileTypes[tiles[x, y]];


                GameObject go = (GameObject)Instantiate(tt.tileVisualPrefab, new Vector3(x, y, 0), Quaternion.identity);

                ClickableTile ct = go.GetComponent<ClickableTile>();
                ct.tileX = x;
                ct.tileY = y;
                ct.map = this;
            }
        }
        
    }
    public Vector3 TileCoordToWorldCoord(int x, int y)
    {
        //this currently places the unit in its new coordinates
        return new Vector3(x, y, -1);
    }

    public bool UnitCanEnterTile(int x, int y)
    {
        return tileTypes[tiles[x,y]].isWalkable;
    }
    public void GeneratePathTo(int x, int y)
    {
        //clears out old path
        selectedUnit.GetComponent<Unit>().currentPath = null;

        if (UnitCanEnterTile(x, y) == false)
        {
            // if we clicked the mountains we should jsut return and not be able to
            return;
        }
        //this is the code that moves the unit.
        // selectedUnit.transform.position = TileCoordToWorldCoord(x, y);
        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

        //setup the "Q"-- the list of nodes that havent been chacked yet.
        List<Node> unvisited = new List<Node>();

        Node source = graph[
            selectedUnit.GetComponent<Unit>().tileX,
            selectedUnit.GetComponent<Unit>().tileY];

        Node target = graph[x, y];

        dist[source] = 0;
        prev[source] = null;

        //v = vertex
        /* initailize everything to have infitnity distance, since 
         * we dont know any better right now, also it it is possible 
         * that some nodes cant be reached from source, 
         * which would make infinity reasonable value*/
        foreach (Node v in graph)
        {
            if (v != source)
            {
                dist[v] = Mathf.Infinity;
                prev[v] = null;
            }
            unvisited.Add(v);
        }
        while (unvisited.Count > 0)
        {
            // u is going to be the unvisited node with the smallest distance


            //u stands for unvisited
            Node u = null;
            foreach(Node possibleU in unvisited)
            {
                if (u == null || dist[possibleU] < dist[u])
                {
                    u = possibleU;
                }
            }
                

            if(u == target)
            {
                break;
            }
            unvisited.Remove(u);
            foreach (Node v in u.neighbours)
            {
                //float alt = dist[u] + u.DistanceTo(v);
                float alt = dist[u] + CostToEnterTile(u.x,u.y,v.x,v.y);
                if (alt < dist[v])
                {
                    dist[v] = alt;
                    prev[v] = u;
                    //
                    
                    
                    //
                }
            }
        }
        if (prev[target] == null)
        {

            return;
        }
        List<Node> CurrentPath = new List<Node>();
        Node curr = target;
        // step through the " prev" chain and add it to our path
        while (curr != null)
        {
            CurrentPath.Add(curr);
            curr = prev[curr];


        }
        CurrentPath.Reverse();
        selectedUnit.GetComponent<Unit>().currentPath = CurrentPath;
    }
	
}
