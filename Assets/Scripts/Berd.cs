using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Berd : MonoBehaviour
{
    //public event EventHandler OnDied = delegate{};
    public event EventHandler OnDied ;
    private const float JUMP_POWER = 75f;
    private Rigidbody2D berdrigidbody2D;

    private static Berd instance;
    public static Berd GetInstance()
    {
        return instance;
    }

    private void Awake()
    {
        instance = this;
        berdrigidbody2D = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButton(0))
        {
            jump();
        }
    }

    private void jump()
    {
        berdrigidbody2D.velocity = Vector2.up*JUMP_POWER;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        berdrigidbody2D.bodyType = RigidbodyType2D.Static;
        OnDied(this,EventArgs.Empty);
        /*if(OnDied !=null) 
        {
            OnDied(this,EventArgs.Empty);
        }*/
    }
}
