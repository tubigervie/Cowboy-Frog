using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyObjectPoolTest : MonoBehaviour
{
    [SerializeField] private EnemyAnimationDetails[] enemyAnimationDetailsArray;
    [SerializeField] GameObject enemyExamplePrefab;
    private float timer = 1f;

    [System.Serializable]
    public struct EnemyAnimationDetails
    {
        public RuntimeAnimatorController animatorController;
        public Color spriteColor;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if(timer <= 0f)
        {
            GetEnemyExample();
            timer = 1f;
        }
    }

    private void GetEnemyExample()
    {
        Room currentRoom = GameManager.Instance.GetCurrentRoom();

        Vector3 spawnPosition = new Vector3(Random.Range(currentRoom.lowerBounds.x, currentRoom.upperBounds.x), Random.Range(currentRoom.lowerBounds.y, currentRoom.upperBounds.y), 0f);

        EnemyAnimation enemyAnimation = (EnemyAnimation)PoolManager.Instance.ReuseComponent(enemyExamplePrefab, HelperUtilities.GetSpawnPositionNearestToPlayer(spawnPosition), Quaternion.identity);

        int random_index = Random.Range(0, enemyAnimationDetailsArray.Length);
        enemyAnimation.gameObject.SetActive(true);
        enemyAnimation.SetAnimation(enemyAnimationDetailsArray[random_index].animatorController, enemyAnimationDetailsArray[random_index].spriteColor);
    }
}
