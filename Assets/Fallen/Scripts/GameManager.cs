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
    public Text scoreText;

    int score = 0;

    [Header("Block Spawning")]
    public float spawnMinScale = 2.5f;
    public float spawnScaleRange = 10f;
    public float spawnOffset = 5f;
    public float spawnDistance = 20f;
    public float spawnVelocityRange = 5f;
    public float spawnSpinRange = 15f;
    public float spawnTime = 0.1f;
    public float spawnCushion = 5f;

    float spawnTimeStamp = -1f;

    [Header("Spawn Chances")]
    public float clusterChance = 0.005f;
    public float rainChance = 0.01f;

    public float rogueChance = 0.05f;
    public float hazardChance = 0.15f;
    public float fragileChance = 0.10f;

    int rainCount = -1;

    [Header("Special Spawning")]
    public float rogueMinVelocityMult = 2f;
    public float rogueMaxVelocityMult = 4f;
    public float rainScaleRange = 2f;
    public int rainMinCount = 3;
    public int rainMaxCount = 12;
    public float spawnClusterRange = 30f;

    GameObject blockPrefab;

    [HideInInspector] public Transform blocksContainer;
    [HideInInspector] public Transform emitterContainer;

    public enum BlockType {
        rogue,
        hazard,
        fragile
    }

    private void Awake()
    {
        if( _instance != null ) { Destroy(gameObject); return; }

        _instance = this;

        blockPrefab = Resources.Load<GameObject>("Prefabs/Block");
    }
    private void Start()
    {
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

        int failCount = 0;
        do {
            if( !SpawnRandomBlock( Vector3.zero, spawnDistance, spawnScaleRange * 0.5f, 15f, spawnVelocityRange * 0.3f, spawnSpinRange, new List<BlockType>(), new List<BlockType>(), false) ) {
                failCount++;
            } else {
                failCount = 0;
            }

        } while( failCount < 5 );
    }

    private void Update()
    {
        if( Time.time > spawnTimeStamp || rainCount > 0 ) {
            HandleBlockSpawning();
            spawnTimeStamp = Time.time + spawnTime;
        }

        if( player.isAlive && player.transform.position.y > score ) {
            score = Mathf.FloorToInt(player.transform.position.y);
            scoreText.text = score.ToString();
        }

        if( Input.GetKeyDown(KeyCode.R) ) {
            ResetGame();
        }

        if( Input.GetKeyDown(KeyCode.Escape) ) {
            Application.Quit();
        }
    }
    
    private void HandleBlockSpawning()
    {
        if( player.rigidbody.velocity.magnitude <= 0 ) { return; }

        Vector3 spawnCenter = player.transform.position + (player.rigidbody.velocity.normalized * spawnDistance);

        // Continued Rain
        if( rainCount > 0 ) {
            int failCount = 0;
            do {
                if( !SpawnRandomBlock( spawnCenter, spawnOffset, rainScaleRange, spawnCushion, spawnVelocityRange, spawnSpinRange, new List<BlockType>(new BlockType[] { BlockType.hazard }), new List<BlockType>(new BlockType[] { BlockType.rogue, BlockType.fragile }) ) ) {
                    failCount++;
                } else {
                    break;
                }
            } while( failCount <= 4 );

            rainCount--;

            return;
        }

        // Cluster
        float roll = Random.value;
        if( roll <= clusterChance ) {
            int failCount = 0;
            do {
                if( !SpawnRandomBlock( spawnCenter, spawnClusterRange, spawnScaleRange, spawnCushion, spawnVelocityRange, spawnSpinRange, new List<BlockType>(new BlockType[] { BlockType.hazard, BlockType.fragile }), new List<BlockType>() ) ) {
                    failCount++;
                } else {
                    failCount = 0;
                }
            } while( failCount <= 10 );

            return;
        }

        // Rain
        roll = Random.value;
        if( roll <= rainChance ) {
            rainCount = Random.Range(rainMinCount, rainMaxCount);
            
            return;
        }

        SpawnRandomBlock( spawnCenter, spawnOffset, spawnScaleRange, spawnCushion, spawnVelocityRange, spawnSpinRange, new List<BlockType>(new BlockType[] { BlockType.rogue, BlockType.hazard, BlockType.fragile }), new List<BlockType>() );
    }
    private bool SpawnRandomBlock(Vector3 spawnCenter, float spawnRange, float scaleRange, float cushion, float velocityRange, float spinRange, List<BlockType> possibleblockTypes, List<BlockType> forceBlockTypes, bool checkDistance = true)
    {
        Vector3 spawnPosition = new Vector3(spawnCenter.x + Random.Range(-spawnRange, spawnRange), spawnCenter.y + Random.Range(-spawnRange, spawnRange), 0);

        if( checkDistance && Vector3.Distance(spawnPosition, player.transform.position) < spawnDistance ) { return false; }

        float speedMagnitude = Random.value * spawnVelocityRange;
        float velocityAngle = Random.value * Mathf.PI*2;
        Vector3 velocity = new Vector3(Mathf.Cos(velocityAngle) * velocityRange, Mathf.Sin(velocityAngle) * velocityRange);

        List<BlockType> blockTypes = new List<BlockType>();
        // Rogue
        if( possibleblockTypes.Contains(BlockType.rogue) || forceBlockTypes.Contains(BlockType.rogue) ) {
            float roll = Random.value;
            if( roll <= rogueChance || forceBlockTypes.Contains(BlockType.rogue) ) {
                speedMagnitude = Random.Range(spawnVelocityRange * rogueMinVelocityMult, spawnVelocityRange * rogueMaxVelocityMult);
                Vector3 attackVector = (player.transform.position - spawnPosition).normalized;
                velocity = attackVector * speedMagnitude;
            }
        }
        
        // Hazard
        if( possibleblockTypes.Contains(BlockType.hazard) || forceBlockTypes.Contains(BlockType.hazard) ) {
            float roll = Random.value;
            if( roll <= hazardChance || forceBlockTypes.Contains(BlockType.hazard) ) {
                blockTypes.Add(BlockType.hazard);
            }
        }

        // Fragile
        if( possibleblockTypes.Contains(BlockType.fragile) || forceBlockTypes.Contains(BlockType.fragile) ) {
            float roll = Random.value;
            if( roll <= fragileChance || forceBlockTypes.Contains(BlockType.fragile) ) {
                blockTypes.Add(BlockType.fragile);
            }
        }

        float spin = Random.Range(-spinRange, spinRange);

        Vector3 spawnScale = new Vector3(Random.Range(spawnMinScale, scaleRange), Random.Range(spawnMinScale, scaleRange), 1);
        float spawnRadius = spawnScale.x > spawnScale.y ? spawnScale.x * 0.5f : spawnScale.y * 0.5f;
        if( !Physics.CheckSphere(spawnPosition, spawnRadius + cushion) ) {
            return SpawnBlock(spawnPosition, spawnScale, velocity, spin, blockTypes);
        }
        
        return false;
    }
    private bool SpawnBlock(Vector3 spawnPosition, Vector3 spawnScale, Vector3 velocity, float spin, List<BlockType> blockTypes)
    {
        Block block = Instantiate<GameObject>( blockPrefab, spawnPosition, Quaternion.AngleAxis(Random.Range(0, 360), transform.forward), blocksContainer ).GetComponent<Block>();
        block.SetScale(spawnScale);
        block.SetTypes(blockTypes);
            
        block.rigidbody.velocity = velocity;
        block.rigidbody.angularVelocity = new Vector3(0f, 0f, spin);

        return true;
    }

    public void ResetGame()
    {
        Destroy(blocksContainer.gameObject);
        Destroy(emitterContainer.gameObject);
        score = 0;
        scoreText.text = "0";

        player.ResetPlayer();

        SetupGame();
    }
}
