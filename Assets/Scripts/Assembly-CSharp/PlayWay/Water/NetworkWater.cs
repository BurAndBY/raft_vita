using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace PlayWay.Water
{
	[AddComponentMenu("Water/Network Synchronization", 2)]
	public class NetworkWater : NetworkBehaviour
	{
		[SyncVar]
		private float time;

		private Water water;

		public float Networktime
		{
			get
			{
				return time;
			}
			[param: In]
			set
			{
				SetSyncVar(value, ref time, 1u);
			}
		}

		private void Start()
		{
			water = GetComponent<Water>();
			if (water == null)
			{
				base.enabled = false;
			}
		}

		private void Update()
		{
			if (base.isServer)
			{
				Networktime = Time.time;
			}
			else
			{
				Networktime = time + Time.deltaTime;
			}
			water.Time = time;
		}

		private void UNetVersion()
		{
		}

		public override bool OnSerialize(NetworkWriter writer, bool forceAll)
		{
			if (forceAll)
			{
				writer.Write(time);
				return true;
			}
			bool flag = false;
			if ((base.syncVarDirtyBits & (true ? 1u : 0u)) != 0)
			{
				if (!flag)
				{
					writer.WritePackedUInt32(base.syncVarDirtyBits);
					flag = true;
				}
				writer.Write(time);
			}
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
			}
			return flag;
		}

		public override void OnDeserialize(NetworkReader reader, bool initialState)
		{
			if (initialState)
			{
				time = reader.ReadSingle();
				return;
			}
			int num = (int)reader.ReadPackedUInt32();
			if (((uint)num & (true ? 1u : 0u)) != 0)
			{
				time = reader.ReadSingle();
			}
		}
	}
}
