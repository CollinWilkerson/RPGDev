using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

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
    public HeaderInfo headerInfo;

    //local player
    public static PlayerController me;

    [PunRPC]
    public void Initialize(Player player)
    {
        //this allows up to referance this instance of this script to photon
        id = player.ActorNumber;
        photonPlayer = player;

        GameManager.instance.players[id - 1] = this;

        headerInfo.Initialize(player.NickName, maxHp);

        if (player.IsLocal)
        {
            me = this;
        }
        else
        {
            rig.isKinematic = true;
        }
    }

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
            weaponAnim.transform.parent.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            weaponAnim.transform.parent.localScale = new Vector3(-1, 1, 1);
        }

        Move();

        if(Input.GetMouseButtonDown(0) && Time.time - lastAttackTime > attackRate)
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
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            enemy.photonView.RPC("TakeDamage", RpcTarget.MasterClient, damage);
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

    [PunRPC]
    public void TakeDamage(int damageTaken)
    {
        curHp -= damageTaken;

        if(curHp <= 0)
        {
            Die();
        }
        else
        {

            headerInfo.photonView.RPC("UpdateHealthBar", RpcTarget.All, curHp);

            StartCoroutine(DamageFlash());

            //save this tidbit for later
            IEnumerator DamageFlash()
            {
                sr.color = Color.red;
                yield return new WaitForSeconds(0.05f);
                sr.color = Color.white;
            }
        }
    }

    private void Die()
    {
        dead = true;
        rig.isKinematic = true;
        sr.color = Color.clear;
        transform.position = new Vector3(0, 99, 0);


        Vector3 spawnPos = GameManager.instance.spawnPoints[Random.Range(0, GameManager.instance.spawnPoints.Length)].position;

        StartCoroutine(Spawn(spawnPos, GameManager.instance.respawnTime));
    }

    private IEnumerator Spawn(Vector3 spawnPos, float timeToRespawn)
    {
        yield return new WaitForSeconds(timeToRespawn);

        dead = false;
        transform.position = spawnPos;
        curHp = maxHp;
        sr.color = Color.white;
        rig.isKinematic = false;

        headerInfo.photonView.RPC("UpdateHealthBar", RpcTarget.All, curHp);
    }

    [PunRPC]
    private void Heal(int healAmount)
    {
        curHp = Mathf.Clamp(curHp + healAmount, 0, maxHp);

        headerInfo.photonView.RPC("UpdateHealthBar", RpcTarget.All, curHp);
    }

    [PunRPC]
    private void GiveGold (int goldGained)
    {
        gold += goldGained;

        //ui
        GameUI.instance.UpdateGoldText(gold);
    }
}
