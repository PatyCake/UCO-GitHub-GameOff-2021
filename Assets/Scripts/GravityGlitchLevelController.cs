using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class GravityGlitchLevelController : MonoBehaviour
{

    // controls the glitch frequency
    [SerializeField]
    [Range(0, 10)]
    private float gravityChangeFrequency = 3.0f;
    [SerializeField]
    private float glitchDuration = 0.5f;

    [SerializeField]
    private float gravChangeWarningDuration = 1f;

    private float glitchDurationCounter = 0f;
    private float gravFreqCounter = 0.0f;
    private const float grav = 9.8f;

    // sequence key:
    //  - u: up (player head facing up)
    //  - r: right (player head facing right)
    //  - d: down (player head facing down)
    //  - l: left (player head facing left)
    private string currentSequence;
    private string lvl1Sequence = "dlur";
    private int gravSequenceCounter = 0;

    private GameObject player;

    static public Vector3 playerHeadUpDirection = Vector3.up;
    private PostProcessVolume postProcessGlitch;


    // PostProcess component is attached to the MainCamera. We use this to adjust the variables on 
    //  the material for the gravity change warning effect
    private PostProcess glitchPostFX;
    float t = 0.0f; // time value for lerping between effect off to full effect

    // Start is called before the first frame update
    void Start()
    {
        GameObject cam = GameObject.FindGameObjectWithTag("MainCamera");
        currentSequence = lvl1Sequence;
        player = GameObject.FindGameObjectWithTag("Player");
        postProcessGlitch = cam.GetComponent<PostProcessVolume>();
        postProcessGlitch.enabled = true;

        glitchPostFX = cam.GetComponent<PostProcess>();
    }

    // Update is called once per frame
    void Update()
    {
        gravFreqCounter += Time.deltaTime;
        glitchDurationCounter += Time.deltaTime;

        // gravity change warning started here
        if (gravFreqCounter >= gravityChangeFrequency - gravChangeWarningDuration)
        {
            glitchPostFX.waveLength = Mathf.Lerp(5000, 700, t);
            postProcessGlitch.weight = Mathf.Lerp(0, 0.5f, t);
            t += Time.deltaTime;
            Debug.Log(glitchPostFX.waveLength);
        }

        // gravity change occurs here (every [gravityChangeFrequency] seconds)
        if (gravFreqCounter >= gravityChangeFrequency)
        {
            char d = currentSequence[gravSequenceCounter];

            ChangeGravity(d);
            
            gravFreqCounter = 0;
            t = 0;
            gravSequenceCounter = (gravSequenceCounter + 1) % currentSequence.Length;
        }

    }

    
    void ChangeGravity(char d)
    {
        postProcessGlitch.weight = 1f;

        glitchDurationCounter = 0f;
        playerHeadUpDirection = Vector3.up;
        switch (d)
        {
            case 'd': // Gravity Down (player head facing down)
                playerHeadUpDirection = Vector3.down;
                Physics2D.gravity = new Vector2(0, grav);
                break;
            case 'u': // Gravity Up (player head facing up)
                playerHeadUpDirection = Vector3.up;
                Physics2D.gravity = new Vector2(0, grav * -1);
                break;
            case 'r': // Gravity Right (player head facing right)
                playerHeadUpDirection = Vector3.right;
                Physics2D.gravity = new Vector2(grav * -1, 0);
                break;
            case 'l': // Gravity Left (player head facing left)
                playerHeadUpDirection = Vector3.left; 
                Physics2D.gravity = new Vector2(grav, 0);
                break;
        }

        player.transform.up = playerHeadUpDirection;

        // Turn off grav glitch warning
        glitchPostFX.waveLength = 5000;
        postProcessGlitch.weight = 0f;
    }
}
