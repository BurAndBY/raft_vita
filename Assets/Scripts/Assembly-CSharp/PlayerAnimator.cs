using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
	public List<AnimationConnection> connections = new List<AnimationConnection>();

	private static Animator anim;

	private static int selectedIndex = 0;

	private static List<AnimationClip> allClips = new List<AnimationClip>();

	private void Awake()
	{
		anim = GetComponent<Animator>();
		allClips.AddRange(anim.runtimeAnimatorController.animationClips);
		foreach (AnimationConnection connection in connections)
		{
			AnimationClip clipByComparison = GetClipByComparison(connection.clip);
			if (clipByComparison != null)
			{
				AnimationEvent animationEvent = new AnimationEvent();
				animationEvent.objectReferenceParameter = connection.objectReciever;
				animationEvent.functionName = "OnEvent";
				animationEvent.time = connection.timeOfEvent;
				animationEvent.messageOptions = SendMessageOptions.DontRequireReceiver;
				clipByComparison.AddEvent(animationEvent);
			}
		}
	}

	private void OnValidate()
	{
		foreach (AnimationConnection connection in connections)
		{
			if (connection.clip != null)
			{
				connection.name = connection.clip.name;
			}
		}
	}

	public static void SetAnimation(PlayerAnimation animation, bool triggering = false)
	{
		string text = animation.ToString();
		string[] array = text.Split('_');
		if (array.Length == 0)
		{
			return;
		}
		string text2 = array[0];
		if (text2 == "Index" && array.Length == 3)
		{
			int index = int.Parse(array[1]);
			SetAnimationIndex(index, false);
		}
		else if (text2 == "Trigger" && array.Length == 2)
		{
			string trigger = array[1];
			anim.SetTrigger(trigger);
			if (triggering)
			{
				anim.SetBool("Triggering", triggering);
			}
		}
	}

	public static void SetAnimation(PlayerAnimation animation, bool overrideIndex, bool triggering)
	{
		string text = animation.ToString();
		string[] array = text.Split('_');
		if (array.Length == 0)
		{
			return;
		}
		string text2 = array[0];
		if (text2 == "Index" && array.Length == 3)
		{
			int index = int.Parse(array[1]);
			SetAnimationIndex(index, overrideIndex);
		}
		else if (text2 == "Trigger" && array.Length == 2)
		{
			string trigger = array[1];
			anim.SetTrigger(trigger);
			if (triggering)
			{
				anim.SetBool("Triggering", triggering);
			}
		}
	}

	public static void SetAnimationMoving(bool state)
	{
		if (anim != null)
		{
			anim.SetBool("Moving", state);
		}
	}

	public static void SetAnimationAxeHit(bool state)
	{
		anim.SetBool("AxeHit", state);
	}

	public static void SetAnimationWater(bool state)
	{
		anim.SetBool("InWater", state);
	}

	private static void SetAnimationIndex(int index, bool overrideIndex)
	{
		if (overrideIndex || selectedIndex != index)
		{
			selectedIndex = index;
			anim.SetInteger("Index", selectedIndex);
			anim.SetTrigger("Switch");
		}
	}

	private static AnimationClip GetClipByComparison(AnimationClip clip)
	{
		foreach (AnimationClip allClip in allClips)
		{
			if (allClip == clip)
			{
				return allClip;
			}
		}
		return null;
	}

	private AnimationConnection GetConnectionByObjectReciever(GameObject reciever)
	{
		foreach (AnimationConnection connection in connections)
		{
			if (reciever == connection.objectReciever)
			{
				return connection;
			}
		}
		return null;
	}

	public void OnEvent(GameObject reciever)
	{
		AnimationConnection connectionByObjectReciever = GetConnectionByObjectReciever(reciever);
		if (connectionByObjectReciever != null)
		{
			connectionByObjectReciever.objectReciever.SendMessage(connectionByObjectReciever.functionName, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void StopTriggering()
	{
		anim.SetBool("Triggering", false);
	}
}
