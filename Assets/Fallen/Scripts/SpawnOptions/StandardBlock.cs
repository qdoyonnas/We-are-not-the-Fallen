using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fallen/Spawn Options/Standard", order = 1)]
public class StandardBlock : SpawnOption
{
	public float hazardBaseChance = 1f;

	[Header("Block Options")]
	public float minScale = 2.5f;
	public float scaleRange = 10f;
	public float offset = 5f;
	public float distance = 200f;
	public float safetyDistance = 100f;
	public float velocityRange = 15f;
	public float velocityRangeScaling = 10f;
	public float spinRange = 5f;
	public float spinRangeScaling = 5f;
	public float cushion = 5f;

	[Header("Prefabs")]
	GameObject blockPrefab;

	protected float scaledVelocity = 0f;
	protected float scaledSpin = 0f;
	protected float hazardChance = 0f;

	public override void OnAwake()
	{
		base.OnAwake();
		hazardChance = 0f;
	}

	public override void SetChance(int score)
	{
		chance = 0f;

		if( score >= startScore && (score < maxScore || maxScore == -1) ) {
			chance = baseChance;
		}

		hazardChance = ScaleValue(score, startScore, scoreRange, maxScore, hazardBaseChance, curvePower);
		scaledVelocity = ScaleValue(score, startScore, scoreRange, maxScore, velocityRangeScaling, curvePower);
		scaledSpin = ScaleValue(score, startScore, scoreRange, maxScore, spinRangeScaling, curvePower);
	}

	public override bool DoSpawn()
	{
		if( !base.DoSpawn() ) { return false; }

		PlayerJump player = GameManager.Instance.player;
        Vector3 spawnCenter = player.transform.position + (player.rigidbody.velocity.normalized * distance);
		SpawnRandomBlock(spawnCenter, offset, scaleRange, cushion, velocityRange, spinRange);

		return true;
	}

	protected virtual GameObject SpawnRandomBlock(Vector3 spawnCenter, float spawnRange, float scaleRange, float cushion, float velocityRange, float spinRange, bool checkDistance = true)
	{
		Vector3 spawnPosition = Vector3.zero;
		int failCount = 0;
		do {
			failCount++;
			if( failCount > 10 ) {
				return null;
			}
			spawnPosition = new Vector3(spawnCenter.x + Random.Range(-spawnRange, spawnRange), spawnCenter.y + Random.Range(-spawnRange, spawnRange), 0);
		} while(checkDistance && Vector3.Distance(spawnPosition, GameManager.Instance.player.transform.position) < safetyDistance);

		float speedMagnitude = Random.value * (velocityRange + scaledVelocity);
		float velocityAngle = Random.value * Mathf.PI * 2;
		Vector3 velocity = new Vector3(Mathf.Cos(velocityAngle) * speedMagnitude, Mathf.Sin(velocityAngle) * speedMagnitude);

		List<GameManager.BlockType> blockTypes = new List<GameManager.BlockType>();
		// Hazard
		if( hazardChance > 0 ) {
			float roll = Random.value;
			if( roll <= hazardChance ) {
				blockTypes.Add(GameManager.BlockType.hazard);
			}
		}

		float spin = Random.Range(-(spinRange + scaledSpin), (spinRange + scaledSpin));
		Vector3 spawnScale = new Vector3(Random.Range(minScale, scaleRange), Random.Range(minScale, scaleRange), 1);
		float spawnRadius = spawnScale.x > spawnScale.y ? spawnScale.x * 0.5f : spawnScale.y * 0.5f;

		if (!Physics.CheckSphere(spawnPosition, spawnRadius + cushion))
		{
			return GameManager.Instance.SpawnBlock(spawnPosition, spawnScale, velocity, spin, blockTypes);
		}

		return null;
	}
	
}
