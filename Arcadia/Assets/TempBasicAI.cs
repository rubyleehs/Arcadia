using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempBasicAI : MonoBehaviour {

    public float testFloat;

	public virtual void StartTurn()
    {
        if (!Attack())
        {
            StartCoroutine(Move());
        }
    }

    public virtual bool Attack()
    {
        Debug.Log("base.Attack() called");
        Debug.Log("testFloat = " + testFloat);
        return false;
    }

    public virtual IEnumerator Move()
    {
        Debug.Log("base.Move() called");
        yield return true;
    }
}
