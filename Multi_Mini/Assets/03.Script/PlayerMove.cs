using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMove : MonoBehaviourPunCallbacks
{
    public AudioSource atAudio;
    public AudioSource dfAudio;
    public AudioSource jumpAudio;
    public AudioSource moveAudio;

    bool isMoveS = false;

    // 플레이어 이동 / 체력
    public float playerMoveSpeed = 5;
    float hAxis;
    float vAxis;
    bool rDown;
    public float maxHealth = 30;
    public float curHealth = 30;

    // 플레이어 점프
    public float gravity = -10;
    float yVelocity = 0;
    public float jumpPower = 3f;
    public bool isJumping = false;

    // 플레이어 공격
    bool aDown;
    float attackDelay;
    bool isAttackReady = true;

    // 플레이어 스턴 / 사망
    public bool stern = false;
    public bool isDie = false;

    // 플레이어 섬광    
    bool bDown;
    public GameObject grenadeObj;
    public GameObject firePos;

    // 플레이어 방어
    public GameObject effect;
    public BoxCollider scol;
    bool dDown;

    // 플레이어 스킬 쿨타임
    public Image bomb_CoolTime;
    bool bomb;
    public GameObject bombCoolTime;
    //float bombTime = 3f;

    public Image shield_CoolTime;
    bool shield;
    public GameObject shieldCoolTime;
    //float shieldTime = 5f;

    // 플레이어 체력
    public Slider hp_Slider;

    CharacterController cc;
    Animator anim;
    Weapon weapon;
    GameObject gameManager;

    // 포톤 연동 변수
    public PhotonView pv;
    public float rotSpeed = 200;
    float mx = 0;
    private Renderer[] renderers;

    bool isCursor = false;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        renderers = GetComponentsInChildren<Renderer>();

        gameManager = GameObject.Find("GameManager");
        bomb_CoolTime = gameManager.GetComponent<GameManager>().bomb_CoolTime;
        bombCoolTime = gameManager.GetComponent<GameManager>().bombCoolTime;
        shield_CoolTime = gameManager.GetComponent<GameManager>().shield_CoolTime;
        shieldCoolTime = gameManager.GetComponent<GameManager>().shieldCoolTime;
        hp_Slider = gameManager.GetComponent<GameManager>().hp_Slider;

        cc = gameObject.GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        weapon = GetComponentInChildren<Weapon>();

        curHealth = maxHealth;
    }

    private void Start()
    {
        Cursor.visible = false;

        hp_Slider.value = (float)curHealth / (float)maxHealth;
        bombCoolTime.SetActive(false);
        shieldCoolTime.SetActive(false);
        bomb_CoolTime.fillAmount = 1;
        bomb = false;
        bomb_CoolTime.enabled = false;
        shield_CoolTime.fillAmount = 1;
        shield = false;
        shield_CoolTime.enabled = false;
        effect.SetActive(false);
    }

    private void Update()
    {
        if (pv.IsMine)
        {
            GetInput();
            Move();

            if (aDown && isAttackReady && stern == false && isDie == false && !isJumping)
            {
                Attack(pv.Owner.ActorNumber);

                pv.RPC("Attack", RpcTarget.OthersBuffered, pv.Owner.ActorNumber);
            }
            if (curHealth <= 0)
            {
                Die();
                pv.RPC("Die", RpcTarget.OthersBuffered, null);
            }
            HandleHp();
            if (pv.IsMine && bDown && stern == false && isDie == false && bomb == false)
            {
                ThrowBomb();
                pv.RPC("ThrowBomb", RpcTarget.OthersBuffered, null);
            }
            if (dDown && stern == false && isDie == false && shield == false)
            {
                Defend();
                pv.RPC("Defend", RpcTarget.OthersBuffered, null);
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(isCursor == false)
            {
                isCursor = true;
                Cursor.visible = true;
            }
            else
            {
                isCursor = false;
                Cursor.visible = false;
            }
        }
    }

    private void LateUpdate()
    {
        if (pv.IsMine)
        {
            float mouse_X = Input.GetAxisRaw("Mouse X");

            mx += mouse_X * rotSpeed * Time.deltaTime;

            transform.eulerAngles = new Vector3(0, mx, 0);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Sword")
        {
            curHealth -= 10f;
            anim.SetTrigger("damage");
            if (curHealth <= 0)
            {
                if (photonView.IsMine)
                {
                    var actorNo = other.GetComponent<Weapon>().actorNumber;
                    Player lastAttack = PhotonNetwork.CurrentRoom.GetPlayer(actorNo);
                    string msg = string.Format(
                        "\n<color=#ff0000>{0}</color> 가 <color=#00ff00>{1}</color>를 처치",
                        lastAttack.NickName, photonView.Owner.NickName);

                    photonView.RPC("KillMessage", RpcTarget.AllBufferedViaServer, msg);
                }
            }
        }
    }
    [PunRPC]
    void KillMessage(string msg)
    {
        gameManager.GetComponent<GameManager>().msgList.text += msg;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Bomb")
        {
            StartCoroutine(BombAttack());
        }
        if (collision.gameObject.tag == "Shield")
        {
            dfAudio.Play();
            StartCoroutine(BombAttack());
        }
    }
    IEnumerator BombAttack()
    {
        stern = true;
        anim.SetTrigger("Hit");
        yield return new WaitForSeconds(2f);
        stern = false;
    }
    [PunRPC]
    void HandleHp()
    {
        hp_Slider.value = (float)curHealth / (float)maxHealth;
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        rDown = Input.GetKey(KeyCode.LeftShift);
        aDown = Input.GetMouseButtonDown(0);
        bDown = Input.GetMouseButtonDown(1);
        dDown = Input.GetKeyDown(KeyCode.E);
    }

    void Move()
    {
        Vector3 dir = new Vector3(hAxis, 0, vAxis).normalized;

        anim.SetBool("isWalk", dir != Vector3.zero);
        anim.SetBool("isRun", rDown);

        dir = Camera.main.transform.TransformDirection(dir);

        if(dir == Vector3.zero)
        {
            isMoveS = true;
            if (isMoveS == true)
            {
                moveAudio.Play();
            }
            isMoveS = false;
        }
        

        if (cc.collisionFlags == CollisionFlags.Below)
        {
            if (isJumping)
            {
                isJumping = false;
                yVelocity = 0;
            }
        }

        if (Input.GetButtonDown("Jump") && !isJumping && stern == false && isDie == false)
        {
            yVelocity = jumpPower;
            isJumping = true;
            jumpAudio.Play();
        }

        yVelocity += gravity * Time.deltaTime;
        dir.y = yVelocity;

        if (stern == true || isDie == true)
        {
            dir = Vector3.zero;
        }

        cc.Move(dir * playerMoveSpeed * (rDown ? 1.5f : 1f) * Time.deltaTime);        
    }
    [PunRPC]
    void Attack(int actorNo)
    {
        attackDelay += Time.deltaTime;
        weapon.Use();
        anim.SetTrigger("Attack");
        attackDelay = 0;
        weapon.GetComponent<Weapon>().actorNumber = actorNo;
        atAudio.Play();
    }
    [PunRPC]
    void ThrowBomb()
    {
        if (pv.IsMine)
        {
            bomb = true;
            bomb_CoolTime.enabled = true;
            bomb_CoolTime.fillAmount = 1;
            StartCoroutine(LightCoolTime());
        }        

        GameObject instantBomb = Instantiate(grenadeObj, firePos.transform.position,
            firePos.transform.rotation);
        Rigidbody rigidBomb = instantBomb.GetComponent<Rigidbody>();
        rigidBomb.AddForce(firePos.transform.forward * 13f, ForceMode.Impulse);
    }
    IEnumerator LightCoolTime()
    {
        while (bomb_CoolTime.fillAmount > 0)
        {
            bomb_CoolTime.fillAmount -= 1 * Time.smoothDeltaTime / 3f;
            yield return null;
            bomb = true;
        }
        bomb = false;
        bomb_CoolTime.enabled = false;
    }
    [PunRPC]
    void Die()
    {
        isDie = true;
        //anim.SetBool("Die", true);
        //Destroy(this.gameObject, 3f);
        StartCoroutine(PlayerDIe());
    }
    IEnumerator PlayerDIe()
    {
        cc.enabled = false;
        anim.SetBool("Die", true);
        curHealth = 30;

        yield return new WaitForSeconds(2.0f);
        SetPlayerVisible(false);

        Transform[] points = GameObject.Find("SpawnPointGroup").GetComponentsInChildren<Transform>();
        int idx = Random.Range(1, points.Length);
        transform.position = points[idx].position;

        anim.SetBool("Die", false);
        anim.SetTrigger("Recovery");
        SetPlayerVisible(true);
        cc.enabled = true;
        isDie = false;
    }
    void SetPlayerVisible(bool isVisible)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = isVisible;
        }
    }
    [PunRPC]
    void Defend()
    {
        if (pv.IsMine)
        {
            shieldCoolTime.SetActive(true);
            shield = true;
            shield_CoolTime.enabled = true;
            shield_CoolTime.fillAmount = 1;
            StartCoroutine(DefendCoolTime());
        }

        UseS();
        scol.enabled = true;
        anim.SetTrigger("Guard");
    }
    IEnumerator DefendCoolTime()
    {
        while (shield_CoolTime.fillAmount > 0)
        {
            shield_CoolTime.fillAmount -= 1 * Time.smoothDeltaTime / 5f;
            yield return null;
            shield = true;
        }
        shield = false;
        shield_CoolTime.enabled = false;
    }
    [PunRPC]
    void UseS()
    {
        StartCoroutine(ShieldTime());
    }
    IEnumerator ShieldTime()
    {
        scol.enabled = true;
        effect.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        effect.SetActive(false);
        scol.enabled = false;
    }
}
