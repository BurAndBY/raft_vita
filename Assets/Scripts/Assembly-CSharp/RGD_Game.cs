using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class RGD_Game
{
	[OptionalField(VersionAdded = 2)]
	public float globalRaftTimeOffset;

	[OptionalField(VersionAdded = 2)]
	public SerializableTransform globalRaftSerializableTransform;

	[OptionalField(VersionAdded = 2)]
	public List<RGD_Block> blocks = new List<RGD_Block>();

	[OptionalField(VersionAdded = 2)]
	public List<RGD_CookingStand> cookingStands = new List<RGD_CookingStand>();

	[OptionalField(VersionAdded = 2)]
	public List<RGD_Cropplot> cropplots = new List<RGD_Cropplot>();

	[OptionalField(VersionAdded = 2)]
	public List<RGD_Inventory> inventories = new List<RGD_Inventory>();

	[OptionalField(VersionAdded = 3)]
	public List<RGD_ItemNet> itemNets = new List<RGD_ItemNet>();

	[OptionalField(VersionAdded = 3)]
	public RGD_Sky sky;

	[OptionalField(VersionAdded = 2)]
	public RGD_Player player = new RGD_Player();
}
