using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fallen/Spawn Options/Rogue", order = 3)]
public class RogueSpawn : StandardBlock
{
	[Header("Rogue Options")]
	public float minVelocityMult = 4f;
	public float maxVelocityMult = 8f;
	public int failAmount = 20;

	public override void OnAwake()
	{
		base.OnAwake();
	}

	public override void SetChance(int score)
	{
		chance = ScaleValue(score, startScore, scoreRange, maxScore, baseChance, curvePower);
		hazardChance = ScaleValue(score, startScore, scoreRange, maxScore, hazardBaseChance, curvePower);
	}

	public override bool DoSpawn()
	{
		return base.DoSpawn();
	}

	protected override GameObject SpawnRandomBlock(Vector3 spawnCenter, float spawnRange, float scaleRange, float cushion, float velocityRange, float spinRange, bool checkDistance = true)
	{
		Vector3 spawnPosition = new Vector3(spawnCenter.x + Random.Range(-spawnRange, spawnRange), spawnCenter.y + Random.Range(-spawnRange, spawnRange), 0);

		if (checkDistance && Vector3.Distance(spawnPosition, GameManager.Instance.player.transform.position) < distance) { return null; }

		List<GameManager.BlockType> blockTypes = new List<GameManager.BlockType>();

		blockTypes.Add(GameManager.BlockType.rogue);
		float scaledVelocityRange = velocityRange + scaledVelocity;
		float speedMagnitude = Random.Range(scaledVelocityRange * minVelocityMult, scaledVelocityRange * maxVelocityMult);
		Vector3 attackVector = (GameManager.Instance.player.transform.position - spawnPosition).normalized;
		Vector3 velocity = attackVector * speedMagnitude;

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

		int failCount = 0;
		do {
			if (!Physics.CheckSphere(spawnPosition, spawnRadius + cushion)) {
				return GameManager.Instance.SpawnBlock(spawnPosition, spawnScale, velocity, spin, blockTypes);
			} else {
				failCount++;
			}
		} while(failCount < failAmount);

		return null;
	}
}
