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
    public static Stack<Vector3> BuildPath(Room room, Vector3Int startGridPosition, Vector3Int endGridPosition, bool useDungeonBuilderGrid = false)
    {
        Vector2Int lowerBoundsToUse = (useDungeonBuilderGrid) ? DungeonBuilder.Instance.dungeonLowerBounds : room.templateLowerBounds;
        Vector2Int upperBoundsToUse = (useDungeonBuilderGrid) ? DungeonBuilder.Instance.dungeonUpperBounds : room.templateUpperBounds;

        //Adjust positions by lower bnounds
        if(!useDungeonBuilderGrid)
        {
            startGridPosition -= (Vector3Int)lowerBoundsToUse;
            endGridPosition -= (Vector3Int)lowerBoundsToUse;
        }


        //Create open list and closed hashset
        NativeHeap minHeap = new NativeHeap(400); //change this to be a binary heap
        HashSet<Node> closedNodeHashSet = new HashSet<Node>();

        //Create gridnodes for path finding
        GridNodes gridNodes = new GridNodes(upperBoundsToUse.x - lowerBoundsToUse.x + 1, upperBoundsToUse.y - lowerBoundsToUse.y + 1);
        if (useDungeonBuilderGrid)
        {
            startGridPosition -= (Vector3Int)lowerBoundsToUse;
            endGridPosition -= (Vector3Int)lowerBoundsToUse;
        }
        Node startNode = gridNodes.GetGridNode(startGridPosition.x, startGridPosition.y);
        Node targetNode = gridNodes.GetGridNode(endGridPosition.x, endGridPosition.y);

        Node endPathNode = FindShortestPath(startNode, targetNode, gridNodes, minHeap, closedNodeHashSet, room.instantiatedRoom, useDungeonBuilderGrid);
        if(endPathNode != null)
        {
            return CreatePathStack(endPathNode, room);
        }
        return null;
    }

    private static Stack<Vector3> CreatePathStack(Node targetNode, Room room, bool useDungeonBuilderGrid = false)
    {
        Vector2Int lowerBoundsToUse = (useDungeonBuilderGrid) ? DungeonBuilder.Instance.dungeonLowerBounds : room.templateLowerBounds;
        Grid gridToUse = (useDungeonBuilderGrid) ? DungeonBuilder.Instance.dungeonGrid : room.instantiatedRoom.grid;

        Stack<Vector3> movementPathStack = new Stack<Vector3>();
        
        Node nextNode = targetNode;
        Vector3 cellMidPoint = gridToUse.cellSize * 0.5f;
        cellMidPoint.z = 0;

        while(nextNode != null)
        {
            //Converts grid position to world position
            Vector3 worldPostion = gridToUse.CellToWorld(new Vector3Int(nextNode.gridPosition.x + lowerBoundsToUse.x,
                nextNode.gridPosition.y + lowerBoundsToUse.y, 0));

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
    private static Node FindShortestPath(Node startNode, Node targetNode, GridNodes gridNodes, NativeHeap openNodeHeap, HashSet<Node> closedNodeHashSet, InstantiatedRoom instantiatedRoom, bool useDungeonBuilderGrid)
    {
        openNodeHeap.Enqueue(startNode);
        while(openNodeHeap.GetCount() > 0)
        {

            //current node = node in the open list withh the lowest fCost
            Node currentNode = openNodeHeap.Dequeue();

            if(currentNode == targetNode)
            {
                Debug.Log("found shortest path");
                return currentNode;
            }
            
            closedNodeHashSet.Add(currentNode);

            EvaluateCurrentNodeNeighbors(currentNode, targetNode, gridNodes, openNodeHeap, closedNodeHashSet, instantiatedRoom, useDungeonBuilderGrid);
        }
        return null;
    }

    private static void EvaluateCurrentNodeNeighbors(Node currentNode, Node targetNode, GridNodes gridNodes, NativeHeap openNodeHeap, HashSet<Node> closedNodeHashSet, InstantiatedRoom instantiatedRoom, bool useDungeonBuilderGrid)
    {
        Vector2Int currentNodeGridPosition = currentNode.gridPosition;
        Node validNeighborNode;

        // Loop through all directions
        for(int i = -1; i <= 1; i++)
        {
            for(int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue; //current node
                validNeighborNode = GetValidNeighbor(currentNodeGridPosition.x + i, currentNodeGridPosition.y + j, gridNodes, closedNodeHashSet, instantiatedRoom, useDungeonBuilderGrid);
                if(validNeighborNode != null)
                {
                    //calculate new gcost for neighbor
                    int newCostToNeighbor;

                    // Get the movement penalty
                    // Unwalkable paths have a value of 0. Default movement penalty is set in
                    // Settings and applies to other grid squares.
                    int movementPenaltyForGridSpace = (useDungeonBuilderGrid) ? DungeonBuilder.Instance.aStarMovementPenalty[validNeighborNode.gridPosition.x, validNeighborNode.gridPosition.y] : instantiatedRoom.aStarMovementPenalty[validNeighborNode.gridPosition.x, validNeighborNode.gridPosition.y];

                    newCostToNeighbor = currentNode.gCost + GetDistance(currentNode, validNeighborNode) + movementPenaltyForGridSpace;
                    bool isValidNeighborNodeInOpenList = openNodeHeap.Contains(validNeighborNode);
                    if(newCostToNeighbor < validNeighborNode.gCost || !isValidNeighborNodeInOpenList)
                    {
                        validNeighborNode.gCost = newCostToNeighbor;
                        validNeighborNode.hCost = GetDistance(validNeighborNode, targetNode);
                        validNeighborNode.parentNode = currentNode;

                        if(!isValidNeighborNodeInOpenList)
                        {
                            openNodeHeap.Enqueue(validNeighborNode);
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
    private static Node GetValidNeighbor(int neighborNodeXPosition, int neighborNodeYPosition, GridNodes gridNodes, HashSet<Node> closedNodeHashSet, InstantiatedRoom instantiatedRoom, bool useDungeonBuilderGrid)
    {
        Vector2Int lowerBoundsToUse = (useDungeonBuilderGrid) ? DungeonBuilder.Instance.dungeonLowerBounds : instantiatedRoom.room.templateLowerBounds;
        Vector2Int upperBoundsToUse = (useDungeonBuilderGrid) ? DungeonBuilder.Instance.dungeonUpperBounds : instantiatedRoom.room.templateUpperBounds;

        if (neighborNodeXPosition >= upperBoundsToUse.x - lowerBoundsToUse.x || neighborNodeXPosition < 0
            || neighborNodeYPosition >= upperBoundsToUse.y - lowerBoundsToUse.y || neighborNodeYPosition < 0)
        {
            return null;
        }

        Node neighborNode = gridNodes.GetGridNode(neighborNodeXPosition, neighborNodeYPosition);

        int movementPenaltyForGridSpace = (useDungeonBuilderGrid) ? DungeonBuilder.Instance.aStarMovementPenalty[neighborNodeXPosition, neighborNodeYPosition] : instantiatedRoom.aStarMovementPenalty[neighborNodeXPosition, neighborNodeYPosition];

        if (movementPenaltyForGridSpace == 0 || closedNodeHashSet.Contains(neighborNode))
        {
            return null;
        }
        else
        {
            return neighborNode;
        }
    }
}
