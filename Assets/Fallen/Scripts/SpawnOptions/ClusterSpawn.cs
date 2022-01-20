using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fallen/Spawn Options/Cluster", order = 2)]
public class ClusterSpawn : StandardBlock
{
	[Header("Cluster Options")]
	public int minAmount = 10;
	public int maxAmount = 30;
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
		if( !base.DoSpawn() ) { return false; }

		PlayerJump player = GameManager.Instance.player;
        Vector3 spawnCenter = player.transform.position + (player.rigidbody.velocity.normalized * distance);

        int amount = Mathf.RoundToInt(Random.Range(minAmount, maxAmount));
		int successCount = 0;
        int failCount = 0;
        do {
            if( !SpawnRandomBlock(spawnCenter, offset * amount, scaleRange, cushion, velocityRange, spinRange) ) {
                failCount++;
            } else {
				successCount++;
                failCount = 0;
            }
        } while( failCount <= failAmount && successCount < amount );

		return true;
	}
}
