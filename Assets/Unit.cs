using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public int tileX;
    public int tileY;
    public TileMap map;
    public List<Node> currentPath = null;

    public float moveSpeed = 2;
    public float canMove;
    public float remainingMovement = 0;
    public bool newTileSelected;
    public GameObject gameobj;
    public List<GameObject> whatTile;

    public int[,] placeTile;



    private void Start()
    {
        canMove = moveSpeed;

        SetTileforShadow();

    }
    void SetTileforShadow()
    {
        placeTile = new int[map.mapSizeX, map.mapSizeY];
        for (int x = 0; x < map.mapSizeX; x++)
        {
            for (int y = 0; y < map.mapSizeY; y++)
            {
                placeTile[x, y] = 0;
            }
        }
    }
    private void Update()
    {

        MovementVisual();
    }
    void MovementVisual()
    {
        if (currentPath != null)
        {
            int currNode = 0;

            while (currNode < currentPath.Count - 1)
            {
                Vector3 start = map.TileCoordToWorldCoord(currentPath[currNode].x, currentPath[currNode].y);
                Vector3 end = map.TileCoordToWorldCoord(currentPath[currNode + 1].x, currentPath[currNode + 1].y);

                Debug.DrawLine(start, end, Color.red);



                currNode++;

            }
            if (Vector3.Distance(transform.position, map.TileCoordToWorldCoord(tileX, tileY)) < 0.1f)
            {
                AdvancePathing();
            }
        }
        if (newTileSelected != true)
        {
            return;
        }
        transform.position = Vector3.MoveTowards(transform.position, map.TileCoordToWorldCoord(tileX, tileY), 5f * Time.deltaTime);
    }

    public void WhereCanMove()
    {


        whatTile = new List<GameObject>();
        for (int x = 0; x < map.mapSizeX; x++)
        {


            for (int y = 0; y < map.mapSizeY; y++)
            {



                map.GeneratePathTo(x, y);

                if (currentPath != null)
                {
                    int currNode = 0;


                    while (currNode < currentPath.Count - 1)
                    {
                        Vector3 start = map.TileCoordToWorldCoord(currentPath[currNode].x, currentPath[currNode].y);
                        Vector3 end = map.TileCoordToWorldCoord(currentPath[currNode + 1].x, currentPath[currNode + 1].y);

                        Debug.DrawLine(start, end, Color.red);



                        currNode++;
                        if (canMove >= 0)
                        {


                            //if (currentPath == null)
                            // return;
                            remainingMovement = canMove;

                            if (remainingMovement <= 0)
                            {
                                break;
                            }

                            //if (newTileSelected != true)
                            //  return;


                            remainingMovement -= map.CostToEnterTile(currentPath[0].x, currentPath[0].y, currentPath[1].x, currentPath[1].y);
                            canMove -= map.CostToEnterTile(currentPath[0].x, currentPath[0].y, currentPath[1].x, currentPath[1].y);

                            if (placeTile[currentPath[currNode].x, currentPath[currNode].y] == 0)
                            {
                                whatTile.Add((GameObject)Instantiate(gameobj, new Vector3(currentPath[currNode].x, currentPath[currNode].y, -1), Quaternion.identity));

                                placeTile[currentPath[currNode].x, currentPath[currNode].y] = 1;
                            }

                            // Instantiate(gameobj, new Vector3(currentPath[currNode].x, currentPath[currNode].y, 0), Quaternion.identity);


                        }

                    }

                    canMove = moveSpeed;
                }

            }


            currentPath = null;
        }
    }

    void AdvancePathing()
    {
        if (canMove >= 0)
        {
            if (currentPath == null)
                return;
            if (remainingMovement <= 0)
            {
                return;
            }
            if (newTileSelected != true)
                return;
            transform.position = map.TileCoordToWorldCoord(tileX, tileY);

            remainingMovement -= map.CostToEnterTile(currentPath[0].x, currentPath[0].y, currentPath[1].x, currentPath[1].y);
            canMove -= map.CostToEnterTile(currentPath[0].x, currentPath[0].y, currentPath[1].x, currentPath[1].y);

            tileX = currentPath[1].x;
            tileY = currentPath[1].y;

            currentPath.RemoveAt(0);

            if (currentPath.Count == 1)
            {
                currentPath = null;

                remainingMovement = 0;
            }
            //when the new shade must be placed
            if (remainingMovement <= 0)
            {
                SetTileforShadow();
                whatTile.Clear();
                WhereCanMove();
                remainingMovement = 0;
            }

        }
    }

    public void MoveUnit()
    {
        StartCoroutine(MoveTheUnit());
    }
    private IEnumerator MoveTheUnit()
    {
        newTileSelected = true;
        if (canMove > 0)
        {
            while (currentPath != null && remainingMovement > 0)
            {
                AdvancePathing();
                yield return new WaitForSeconds(1.5f);
            }
            remainingMovement = moveSpeed;
        }
    }
    public void ResetMovement()
    {
        newTileSelected = false;
        canMove = moveSpeed;

        //this part is related to the list of gameobjects of tiles that is created so it can be removed later
        for (int i = 0; i < whatTile.Count; i++)
        {
            Destroy(whatTile[i]);
        }
        whatTile.Clear();




    }
    /*  public void MoveToNextTile()
      {
          float remainingMovement = moveSpeed;
          while (remainingMovement > 0)
          {
              if (currentPath == null)
                  return;

              remainingMovement -= map.CostToEnterTile(currentPath[0].x, currentPath[0].y, currentPath[1].x, currentPath[1].y);

              tileX = currentPath[1].x;
              tileY = currentPath[1].y;
              transform.position = map.TileCoordToWorldCoord(tileX,tileY);

              currentPath.RemoveAt(0);

              if (currentPath.Count == 1)
              {
                  currentPath = null;
              }
          }
      }*/


}
