using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour {
    public static bool HasActiveEnemies = true;
    public static float enemyMoveSpeed;

    public GameObject tempWarrior;

    public float I_enemyMoveSpeed;
    public GridGen gridGen;

    public List<BasicEnemyAI> enemies;
    public static int enemiesInTurnPhaseCount = 0 ;
    // Use this for initialization
	void Start () {
        enemyMoveSpeed = I_enemyMoveSpeed;
        enemies = new List<BasicEnemyAI>();
        
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown("Fire2"))
        {
            SummonRandomMob();
        }
	}

    public void SummonRandomMob()
    {
        GridCell randomCell = gridGen.RandomActiveFreeCell();
        Transform enemy = Instantiate(tempWarrior, randomCell.transform.position, Quaternion.identity).transform;
        randomCell.entity = enemy;
        randomCell.Walkable = false;
        enemy.GetComponent<BasicEnemyAI>().gridCell = randomCell;
        enemy.SetParent(randomCell.transform);
        if(enemy.parent.GetComponent<SpriteRenderer>().enabled == false)
        {
            enemy.GetComponent<SpriteRenderer>().enabled = false;
        }

        enemies.Add(enemy.GetComponent<BasicEnemyAI>());
        HasActiveEnemies = true;
    }

    public IEnumerator EnemyPhase()
    {
        while (InputManager.PlayerIsMoving)
        {
            yield return null;
        }
        
        if (enemies.Count == 0)
        {
            HasActiveEnemies = false;
            InputManager.AllowInput = true;
            yield return true;
        }
        else HasActiveEnemies = true;

        enemiesInTurnPhaseCount = enemies.Count;
        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].StartTurn();
        }
        yield return true;
    }

    public void MoveEnemiesToNextGrid()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            if (!enemies[i].MoveToNextGrid())
            {
                Destroy(enemies[i].gameObject);
                enemies.RemoveAt(i);
                i--;
            }
        }
    }

    public void PrepareMovingToNextGrid()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].transform.SetParent(enemies[i].gridCell.transform);
        }
    }

    public static IEnumerator TryEndEnemyPhase()
    {
        enemiesInTurnPhaseCount--;
        if (enemiesInTurnPhaseCount <= 0)
        {
            //yield return new WaitForSeconds(0.075f);
            //Debug.Log("EnemyPhaseEnd");
            InputManager.AllowInput = true;
            yield return true;
        }
        yield return true;
    }
}
