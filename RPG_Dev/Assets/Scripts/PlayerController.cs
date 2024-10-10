using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;

public class PlayerController : MonoBehaviourPun
{
    [HideInInspector]
    public int id;

    [Header("Info")]
    public float moveSpeed;
    public int gold;
    public int curHp;
    public int maxHp;
    public bool dead;

    [Header("Attack")]
    public int damage;
    public float attackRange;
    public float attackRate;
    private float lastAttackTime;

    [Header("Components")]
    public Rigidbody2D rig;
    public Player photonPlayer;
    public SpriteRenderer sr;
    public Animator weaponAnim;

    //local player
    public static PlayerController me;

    private void Awake()
    {
        rig = gameObject.GetComponent<Rigidbody2D>();
        sr = gameObject.GetComponent<SpriteRenderer>();
        weaponAnim = gameObject.GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        //mouse position is calculated from the top left, this adjusts to the middle of the screen
        float mouseX = (Screen.width / 2) - Input.mousePosition.x;

        if(mouseX < 0)
        {
            weaponAnim.transform.parent.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            weaponAnim.transform.parent.localScale = new Vector3(1, 1, 1);
        }

        Move();

        //my mouse is only rated for so many clicks
        if(Input.GetMouseButton(0) && Time.time - lastAttackTime > attackRate)
        {
            Attack();
        }
    }

    private void Attack()
    {
        lastAttackTime = Time.time;

        //this vomit gets the direction clicked in, as a normalized vector3
        Vector3 dir = (Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position)).normalized;

        //The raycast approach makes the weapon have no sweep. good for spears and such.
        RaycastHit2D hit = Physics2D.Raycast(transform.position + dir, dir, attackRange);

        //if enemy hit
        if(hit.collider != null && hit.collider.gameObject.CompareTag("Enemy"))
        {
            //TODO: damage enemy
        }

        weaponAnim.SetTrigger("Attack");
    }

    private void Move()
    {
        //inputs
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        //setting velocity
        rig.velocity = new Vector2(x, y) * moveSpeed;
    }
}
