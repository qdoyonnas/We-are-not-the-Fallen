using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get {
            return _instance;
        }
    }

    public CameraBase activeCamera;
    public PlayerJump player;
    public Pointer pointer;
    public Text scoreText;


    [Header("Block Spawning")]
    public float startZoneSize = 300f;
    public bool blockSpawning = true;
    public bool spawnGoals = true;
    public SpawnOption[] spawnOptions;

    //public float spawnMinScale = 2.5f;
    //public float spawnScaleRange = 10f;
    //public float spawnOffset = 5f;
    //public float spawnDistance = 20f;
    //public float spawnVelocityRange = 5f;
    //public float spawnSpinRange = 15f;
    //public float spawnCushion = 5f;
    //public float spawnBlockChance = 0.9f;


    //[Header("Spawn Chances")]
    //public float clusterChance = 0.005f;
    //public float rainChance = 0.01f;

    //public float rogueChance = 0.05f;
    //public float hazardChance = 0.15f;
    //public float fragileChance = 0.10f;

    //int rainCount = -1;

    //[Header("Special Spawning")]
    //public float rogueMinVelocityMult = 2f;
    //public float rogueMaxVelocityMult = 4f;
    //public float rainScaleRange = 2f;
    //public int rainMinCount = 3;
    //public int rainMaxCount = 12;
    //public float shotVelocity = 70f;
    

    [Header("Scoring")]
    public float heightMult = 0.25f;
    public float goalSpawnDistance = 600f;
    public int scorePerLevel = 1000;
    public int level {
        get {
            return Mathf.FloorToInt((float)score / (float)scorePerLevel);
        }
    }

    [HideInInspector] public GameObject goal = null;
    [HideInInspector] public int savedScore = 0;
    int score = 0;
    float startTime = 0f;

    GameObject blockPrefab = null;
    GameObject shotPrefab = null;

    [HideInInspector] public Transform blocksContainer;
    [HideInInspector] public Transform emitterContainer;

    public enum BlockType {
        rogue,
        hazard,
        fragile,
        goal
    }

    private void Awake()
    {
        if( _instance != null ) { Destroy(gameObject); return; }

        _instance = this;

        blockPrefab = Resources.Load<GameObject>("Prefabs/Block");
        shotPrefab = Resources.Load<GameObject>("Prefabs/Shot");
    }
    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        SetupGame();
    }
    private void SetupGame()
    {
        blocksContainer = new GameObject("blocksContainer").transform;
        emitterContainer = new GameObject("emitterContainer").transform;

        Vector3 spawnPosition = new Vector3(player.transform.position.x, player.transform.position.y - 5, 0);
        Block block = Instantiate<GameObject>(blockPrefab, spawnPosition, Quaternion.identity, blocksContainer).GetComponent<Block>();
        block.SetScale(new Vector3(3, 8, 1));

        spawnPosition = new Vector3(player.transform.position.x - 1.5f, player.transform.position.y + 8f, 0);
        block = Instantiate<GameObject>(blockPrefab, spawnPosition, Quaternion.AngleAxis(15f, transform.forward), blocksContainer).GetComponent<Block>();
        block.SetScale(new Vector3(5, 2, 1));

        foreach( SpawnOption spawnOption in spawnOptions ) {
            if( spawnOption.enabled ) {
                spawnOption.OnAwake();
                spawnOption.SetChance(score);
            }
        }

        int failCount = 0;
        do {
            if( !SpawnRandomBlock() ) {
                failCount++;
            } else {
                failCount = 0;
            }

        } while( failCount < 5 );
    }
    private GameObject SpawnRandomBlock()
    {
        Vector3 spawnCenter = player.transform.position;
        Vector3 spawnPosition = new Vector3(spawnCenter.x + Random.Range(-startZoneSize, startZoneSize), spawnCenter.y + Random.Range(-startZoneSize, startZoneSize), 0);

		float speedMagnitude = Random.value * 3;
		float velocityAngle = Random.value * Mathf.PI * 2;
		Vector3 velocity = new Vector3(Mathf.Cos(velocityAngle) * speedMagnitude, Mathf.Sin(velocityAngle) * speedMagnitude);

        float spin = Random.Range(-2, 2);
		Vector3 spawnScale = new Vector3(Random.Range(2.5f, 7f), Random.Range(2.5f, 7f), 1);
		float spawnRadius = spawnScale.x > spawnScale.y ? spawnScale.x * 0.5f : spawnScale.y * 0.5f;

		if (!Physics.CheckSphere(spawnPosition, spawnRadius + 4))
		{
			return GameManager.Instance.SpawnBlock(spawnPosition, spawnScale, velocity, spin);
		}

		return null;
    }

    private void Update()
    {
        //if( Time.time > spawnTimeStamp || rainCount > 0 ) {
        //    if( rainCount > 0 ) {
        //        SpawnShot();
        //    } else {
        //        if( blockSpawning ) {
        //            HandleBlockSpawning();
        //        }
        //    }
        //    spawnTimeStamp = Time.time + spawnTime;
        //}
        
        HandleBlockSpawning();

        if( player.isAlive ) {
            scoreText.text = UpdateScore().ToString();
        } else {
            scoreText.text = savedScore.ToString();
        }
        
        if( spawnGoals ) {
            if( goal == null && !player.superJump  ) {
                SpawnGoal();
            } else {
                if( goal != null && Vector3.Distance(goal.transform.position, player.transform.position) > goalSpawnDistance * 1.5f ) {
                    Destroy(goal.gameObject);
                    goal = null;
                    SpawnGoal();
                }
            }
        }

        if( Input.GetKeyDown(KeyCode.R) ) {
            ResetGame();
        }

        if( Input.GetKeyDown(KeyCode.Escape) ) {
            Application.Quit();
        }
    }

    public int UpdateScore()
    {
        int playerHeightScore =  Mathf.FloorToInt(player.transform.position.y * heightMult);

        if( playerHeightScore > score ) {
            score = playerHeightScore;
            savedScore = score;

            foreach( SpawnOption spawnOption in spawnOptions ) {
                if( spawnOption.enabled ) {
                    spawnOption.SetChance(score);
                }
            }
        }

        return score;
    }

	private void SpawnGoal()
	{
		Vector3 goalDirection = Vector3.zero;
		while (goalDirection.x == 0 || goalDirection.y == 0)
		{
			float x = Mathf.Round(Random.Range(-1f, 1f));
			goalDirection = new Vector3(x, Random.Range(-0.7f, 0.8f), 0).normalized;
		}
		Vector3 spawnScale = new Vector3(Random.Range(3, 8), Random.Range(3, 8), 1);
		float spin = 0;

		goal = SpawnBlock(player.transform.position + (goalDirection * goalSpawnDistance),
			spawnScale, Vector3.zero, spin, new List<BlockType>(new BlockType[] { BlockType.goal }));
	}

	private void HandleBlockSpawning()
    {
        {
        //Vector3 spawnCenter = player.transform.position + (player.rigidbody.velocity.normalized * spawnDistance);

        //// Rain
        //roll = Random.value;
        //if( roll <= rainChance ) {
        //    rainCount = Random.Range(rainMinCount, rainMaxCount);
        //}
        }

        foreach( SpawnOption spawnOption in spawnOptions ) {
            if( spawnOption.enabled ) {
                spawnOption.DoSpawn();
            }
        }
    }

    public GameObject SpawnBlock(Vector3 spawnPosition, Vector3 spawnScale, Vector3 velocity, float spin, List<BlockType> blockTypes = null)
	{
		Block block = Instantiate<GameObject>(blockPrefab, spawnPosition, Quaternion.AngleAxis(Random.Range(0, 360), Vector3.forward), blocksContainer).GetComponent<Block>();
		block.SetScale(spawnScale);
        block.SetTypes(blockTypes);

		block.rigidbody.velocity = velocity;
		block.rigidbody.angularVelocity = new Vector3(0f, 0f, spin);

		return block.gameObject;
	}

    //private GameObject SpawnShot()
    //{
    //    Vector3 spawnCenter = player.transform.position + (-player.rigidbody.velocity.normalized * spawnDistance);
    //    Vector3 spawnPosition = new Vector3(spawnCenter.x + Random.Range(-spawnOffset, spawnOffset), spawnCenter.y + Random.Range(-spawnOffset, spawnOffset), 0);

    //    if( Vector3.Distance(spawnPosition, player.transform.position) < spawnDistance ) { return null; }

    //    Vector3 target = new Vector3(player.transform.position.x + Random.Range(-spawnOffset * 0.7f, spawnOffset * 0.7f), player.transform.position.y + Random.Range(-spawnOffset * 0.7f, spawnOffset * 0.7f), 0);
    //    Vector3 attackVector = (target - spawnPosition).normalized;
    //    Vector3 velocity = attackVector * shotVelocity;

    //    Shot shot = Instantiate<GameObject>( shotPrefab, spawnPosition, Quaternion.identity ).GetComponent<Shot>();
    //    shot.rigidbody.velocity = velocity;

    //    rainCount--;
    //    return shot.gameObject;
    //}

    public void ResetGame()
    {
        Destroy(blocksContainer.gameObject);
        Destroy(emitterContainer.gameObject);
        score = 0;
        startTime = Time.time;
        scoreText.text = "0";

        Destroy(goal);

        player.ResetPlayer();

        SetupGame();
    }
}
