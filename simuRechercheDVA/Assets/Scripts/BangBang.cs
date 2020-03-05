using System.Collections;
using UnityEngine;

[System.Serializable]

public abstract class BangBang <T>
{

    protected bool isMoving = false;
    protected float movementStartTime;
    protected T startPos;
    protected T targetPos;


    public bool IsMoving { get => isMoving;}
    public abstract float TimeRemaining(float currentTime);
    public virtual void StartMovement(T startPos,T targetPos,float currentTime){
        this.isMoving = true;
        this.movementStartTime = currentTime;
        this.startPos = startPos;
        this.targetPos = targetPos;
    }
    public abstract T[] GetTarget(float currentTime);

}
