using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AStar
{
    /// <summary>
    /// Builds a path for the room, from the startGridPosition to the endGridPosition and adds movement steps to the returned Stack.
    /// Returns null if no path is found.
    /// </summary>
    /// <param name="room"></param>
    /// <param name="startGridPosition"></param>
    /// <param name="endGridPosition"></param>
    /// <returns></returns>
    public static Stack<Vector3> BuildPath(Room room, Vector3Int startGridPosition, Vector3Int endGridPosition)
    {
        //Adjust positions by lower bnounds
        startGridPosition -= (Vector3Int)room.templateLowerBounds;
        endGridPosition -= (Vector3Int)room.templateLowerBounds;

        //Create open list and closed hashset
        List<Node> openNodeList = new List<Node>(); //change this to be a binary heap
        HashSet<Node> closedNodeHashSet = new HashSet<Node>();

        //Create gridnodes for path finding
        GridNodes gridNodes = new GridNodes(room.templateUpperBounds.x - room.templateLowerBounds.x + 1, room.templateUpperBounds.y - room.templateLowerBounds.y + 1);

        Node startNode = gridNodes.GetGridNode(startGridPosition.x, startGridPosition.y);
        Node targetNode = gridNodes.GetGridNode(endGridPosition.x, endGridPosition.y);

        Node endPathNode = FindShortestPath(startNode, targetNode, gridNodes, openNodeList, closedNodeHashSet, room.instantiatedRoom);
        if(endPathNode != null)
        {
            return CreatePathStack(endPathNode, room);
        }
        return null;
    }

    private static Stack<Vector3> CreatePathStack(Node targetNode, Room room)
    {
        Stack<Vector3> movementPathStack = new Stack<Vector3>();
        
        Node nextNode = targetNode;
        Vector3 cellMidPoint = room.instantiatedRoom.grid.cellSize * 0.5f;
        cellMidPoint.z = 0;

        while(nextNode != null)
        {
            //Converts grid position to world position
            Vector3 worldPostion = room.instantiatedRoom.grid.CellToWorld(new Vector3Int(nextNode.gridPosition.x + room.templateLowerBounds.x,
                nextNode.gridPosition.y + room.templateLowerBounds.y, 0));

            //Set world position to middle of the grid cell;
            worldPostion += cellMidPoint;

            movementPathStack.Push(worldPostion);

            nextNode = nextNode.parentNode;
        }

        return movementPathStack;
    }

    /// <summary>
    /// Find the shortest path - returns the end Node if a path has been found else returns null
    /// </summary>
    /// <param name="startNode"></param>
    /// <param name="targetNode"></param>
    /// <param name="gridNodes"></param>
    /// <param name="openNodeList"></param>
    /// <param name="closedNodeHashSet"></param>
    /// <param name="instantiatedRoom1"></param>
    /// <param name="instantiatedRoom2"></param>
    /// <returns></returns>
    private static Node FindShortestPath(Node startNode, Node targetNode, GridNodes gridNodes, List<Node> openNodeList, HashSet<Node> closedNodeHashSet, InstantiatedRoom instantiatedRoom)
    {
        openNodeList.Add(startNode);
        while(openNodeList.Count > 0)
        {
            openNodeList.Sort();

            //current node = node in the open list withh the lowest fCost
            Node currentNode = openNodeList[0];
            openNodeList.RemoveAt(0);

            if(currentNode == targetNode)
            {
                return currentNode;
            }
            
            closedNodeHashSet.Add(currentNode);

            EvaluateCurrentNodeNeighbors(currentNode, targetNode, gridNodes, openNodeList, closedNodeHashSet, instantiatedRoom);
        }
        return null;
    }

    private static void EvaluateCurrentNodeNeighbors(Node currentNode, Node targetNode, GridNodes gridNodes, List<Node> openNodeList, HashSet<Node> closedNodeHashSet, InstantiatedRoom instantiatedRoom)
    {
        Vector2Int currentNodeGridPosition = currentNode.gridPosition;
        Node validNeighborNode;

        // Loop through all directions
        for(int i = -1; i <= 1; i++)
        {
            for(int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue; //current node
                validNeighborNode = GetValidNeighbor(currentNodeGridPosition.x + i, currentNodeGridPosition.y + j, gridNodes, closedNodeHashSet, instantiatedRoom);
                if(validNeighborNode != null)
                {
                    //calculate new gcost for neighbor
                    int newCostToNeighbor;

                    // Get the movement penalty
                    // Unwalkable paths have a value of 0. Default movement penalty is set in
                    // Settings and applies to other grid squares.
                    int movementPenaltyForGridSpace = instantiatedRoom.aStarMovementPenalty[validNeighborNode.gridPosition.x, validNeighborNode.gridPosition.y];

                    newCostToNeighbor = currentNode.gCost + GetDistance(currentNode, validNeighborNode) + movementPenaltyForGridSpace;
                    bool isValidNeighborNodeInOpenList = openNodeList.Contains(validNeighborNode);
                    if(newCostToNeighbor < validNeighborNode.gCost || !isValidNeighborNodeInOpenList)
                    {
                        validNeighborNode.gCost = newCostToNeighbor;
                        validNeighborNode.hCost = GetDistance(validNeighborNode, targetNode);
                        validNeighborNode.parentNode = currentNode;

                        if(!isValidNeighborNodeInOpenList)
                        {
                            openNodeList.Add(validNeighborNode);
                        }
                    }
                }
            }
        }
    }

    private static int GetDistance(Node currentNode, Node validNeighborNode)
    {
        int dstX = Mathf.Abs(currentNode.gridPosition.x - validNeighborNode.gridPosition.x);
        int dstY = Mathf.Abs(currentNode.gridPosition.y - validNeighborNode.gridPosition.y);
        if(dstX > dstY)
        {
            return 14 * dstY + 10 * (dstX - dstY); //10 used instead of 1, and 14 is a pythagoras approximation SQRT(10*10 + 10*10) - to avoid using floats
        }
        return 14 * dstX + 10 * (dstY - dstX);
    }

    /// <summary>
    /// Evaluate a neighbor node at neighborNodeXPosition, neighborNodeYPosition using the
    /// specified gridNodes, closedNodeHashSet and instantiatedRoom. Returns null if the node isn't valid
    /// </summary>
    /// <param name="neighborNodeXPosition"></param>
    /// <param name="neighborNodeYPosition"></param>
    /// <param name="gridNodes"></param>
    /// <param name="closedNodeHashSet"></param>
    /// <param name="instantiatedRoom"></param>
    /// <returns></returns>
    private static Node GetValidNeighbor(int neighborNodeXPosition, int neighborNodeYPosition, GridNodes gridNodes, HashSet<Node> closedNodeHashSet, InstantiatedRoom instantiatedRoom)
    {

        if(neighborNodeXPosition >= instantiatedRoom.room.templateUpperBounds.x - instantiatedRoom.room.templateLowerBounds.x || neighborNodeXPosition < 0
            || neighborNodeYPosition >= instantiatedRoom.room.templateUpperBounds.y - instantiatedRoom.room.templateLowerBounds.y || neighborNodeYPosition < 0)
        {
            return null;
        }

        Node neighborNode = gridNodes.GetGridNode(neighborNodeXPosition, neighborNodeYPosition);

        int movementPenaltyForGridSpace = instantiatedRoom.aStarMovementPenalty[neighborNodeXPosition, neighborNodeYPosition];

        if(movementPenaltyForGridSpace == 0 || closedNodeHashSet.Contains(neighborNode))
        {
            return null;
        }
        else
        {
            return neighborNode;
        }
    }
}
