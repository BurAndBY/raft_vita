using System.Collections.Generic;
using UnityEngine;

public class BlockQuad : MonoBehaviour
{
	public List<BlockType> acceptableBlocks = new List<BlockType>();

	public bool AcceptsBlock(Block block)
	{
		foreach (BlockType acceptableBlock in acceptableBlocks)
		{
			if (acceptableBlock == block.type)
			{
				return true;
			}
		}
		return false;
	}
}
