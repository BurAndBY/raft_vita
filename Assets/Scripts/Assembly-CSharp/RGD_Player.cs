using System;

[Serializable]
public class RGD_Player
{
	public SerializableTransform serializableTransform;

	public float hunger;

	public float thirst;

	public float health;

	public float fatigue;

	public void RestorePlayer(PlayerStats stats)
	{
		serializableTransform.SetTransform(stats.transform);
		stats.Hunger = hunger;
		stats.Thirst = thirst;
		stats.Health = health;
		stats.Fatigue = fatigue;
	}
}
