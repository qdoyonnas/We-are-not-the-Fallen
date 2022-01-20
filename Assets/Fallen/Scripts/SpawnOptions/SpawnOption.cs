using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnOption : ScriptableObject
{
	[Header("Spawn Options")]
	public bool enabled = true;
	public float spawnTime = 0.05f;
	public int startScore = 0;
	public int scoreRange = -1;
	public int maxScore = -1;
	public float baseChance = 0.5f;
	public float curvePower = 1;
	public bool clampScales = true;
	public bool spawnsDuringSuperJump = true;

	[HideInInspector] protected float chance = 0f;
	protected float spawnTimestamp = 0f;

	public virtual void OnAwake()
	{
		spawnTimestamp = spawnTime;
	}

	public virtual void SetChance(int score)
	{
		chance = ScaleValue(score, startScore, scoreRange, maxScore, baseChance, curvePower);
	}
	public virtual float ScaleValue(int score, int startScore, int scoreRange, int maxScore, float baseValue, float curvePower)
	{
		if( (score < startScore)
			|| (maxScore != -1 && score > maxScore)) {
			return 0f;
		}

		int relativeScore = score - startScore;
		if( (scoreRange == -1)
			|| (clampScales && relativeScore > scoreRange) ) {
			return baseValue;
		} else {
			float calculatedChance = Mathf.Pow((float)relativeScore / (float)scoreRange, curvePower) * baseValue;
			return calculatedChance;
		}
	}

	public virtual bool DoSpawn()
	{
		if( chance <= 0 ) { return false; }
		if( !spawnsDuringSuperJump && GameManager.Instance.player.superJump ) { return false; }

		if( Time.time > spawnTimestamp ) {
			float roll = Random.value;
			if( roll <= chance ) {
				return true;
			}
			spawnTimestamp = Time.time + spawnTime;
		}

		return false;
	}
}