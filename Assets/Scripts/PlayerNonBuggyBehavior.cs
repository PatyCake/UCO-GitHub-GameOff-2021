using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNonBuggyBehavior : MonoBehaviour
{
    [SerializeField] private float playerMovementSpeed = 14.0f;
    [SerializeField] private float gravity = 9.8f;
    [SerializeField] private float jumpHeight = 160.0f;
    [SerializeField] private GameObject binaryBullet;
    [SerializeField] private float bulletSpeed = 2000;

    private Rigidbody2D rb2D;
    private bool isGrounded;
    private bool playerJumpKeyDown;
    private float velocity;
    private GameObject healthBar;

    private string binaryGunString = "0101010101000011010011110010000001000010011010010110011100100000010000100111001001100001011010010110111001110011"; // UCO Big Brains
    private int currentBinaryBullet;
    private int bulletsRemaining;

    private bool binaryGunIsShooting;
    private float lastFacingDirection;

    private float timeSinceLastBulletFired;
    
    private const float SECONDS_BETWEEN_BINARY_BULLETS = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        rb2D = transform.GetComponent<Rigidbody2D>();
        rb2D.gravityScale = 9;
        isGrounded = false;
        playerJumpKeyDown = false;
        velocity = 1;
        healthBar = GameObject.Find("/Main Camera/Health Bar Parent/Health Bar");
        if (Constants.DEV_MODE)
        {
            Debug.Assert(healthBar != null);
        }
        currentBinaryBullet = 0;
        binaryGunIsShooting = false;
        bulletsRemaining = binaryGunString.Length;
        timeSinceLastBulletFired = SECONDS_BETWEEN_BINARY_BULLETS;
        lastFacingDirection = 1;
    }

    // Update is called once per frame
    void Update()
    {
        velocity = -0.1f;

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            playerJumpKeyDown = true;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            binaryGunIsShooting = true;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            binaryGunIsShooting = false;
        }
    }

    private void FixedUpdate()
    {
        if (isGrounded)
        {
            velocity = 0;

            if (playerJumpKeyDown)
            {
                Jump();
            }
        }

        MovePlayer();
        
        if (binaryGunIsShooting)
        {
            float currentTime = Time.time;
            if (currentTime - timeSinceLastBulletFired > SECONDS_BETWEEN_BINARY_BULLETS)
            {
                timeSinceLastBulletFired = currentTime;
                FireBinaryBullet();
            }
        }
    }

    private void FireBinaryBullet()
    {
        // fire as shotgun
        if (currentBinaryBullet == binaryGunString.Length)
        {
            currentBinaryBullet = 0;
        }

        string binary = binaryGunString[currentBinaryBullet].ToString();

        Debug.Log("FIRE");
        GameObject bullet = Instantiate(binaryBullet, transform.position, new Quaternion()); // TODO figure out rotation next
        Debug.Log("FIRE 2");
        bullet.GetComponent<BinaryBulletBehavior>().SetText(binary);
        bullet.GetComponent<Rigidbody2D>().velocity = Vector2.left * -lastFacingDirection * bulletSpeed * Time.deltaTime;


        currentBinaryBullet++;
    }

    
    private void MovePlayer()
    {
        var h = Input.GetAxisRaw("Horizontal");
        rb2D.velocity = new Vector2(h * playerMovementSpeed, rb2D.velocity.y);
        if (h != 0) lastFacingDirection = h;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = true;
        }

        else if (collision.gameObject.tag.Equals("Bug"))
        {
            HealthBarBehavior hbb = healthBar.GetComponent<HealthBarBehavior>();
            hbb.TakeDamage(0.23f);
            if (hbb.GetHealth() <= 0)
            {
                PlayerDeath();
            }
        }

        else if (collision.gameObject.tag.Equals("Bug Boss"))
        {
            HealthBarBehavior hbb = healthBar.GetComponent<HealthBarBehavior>();
            hbb.TakeDamage(0.46f);
            if (hbb.GetHealth() <= 0)
            {
                PlayerDeath();
            }
        }
    }

    private void PlayerDeath()
    {
        GameObject bugBoss = GameObject.Find("/BossFolder/Bug Boss");
        if (Constants.DEV_MODE)
        {
            Debug.Assert(bugBoss != null);
        }

        bugBoss.GetComponent<BugBossBehavior>().PlayerLost();
        Destroy(this.gameObject);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = false;
        }
    }

    private void Jump()
    {
        velocity = jumpHeight;
        rb2D.AddForce(new Vector2(0, velocity * gravity));
        playerJumpKeyDown = false;
    }
}
