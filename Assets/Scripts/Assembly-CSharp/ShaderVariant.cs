using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ShaderVariant
{
	private Dictionary<string, bool> unityKeywords;

	private Dictionary<string, bool> waterKeywords;

	private Dictionary<string, string> shaderParts;

	public ShaderVariant()
	{
		unityKeywords = new Dictionary<string, bool>();
		waterKeywords = new Dictionary<string, bool>();
		shaderParts = new Dictionary<string, string>();
	}

	public void SetUnityKeyword(string keyword, bool value)
	{
		if (value)
		{
			unityKeywords[keyword] = true;
		}
		else
		{
			unityKeywords.Remove(keyword);
		}
	}

	public void SetWaterKeyword(string keyword, bool value)
	{
		if (value)
		{
			waterKeywords[keyword] = value;
		}
		else
		{
			waterKeywords.Remove(keyword);
		}
	}

	public void SetAdditionalCode(string keyword, string code)
	{
		if (code != null)
		{
			shaderParts[keyword] = code;
		}
		else
		{
			shaderParts.Remove(keyword);
		}
	}

	public bool IsUnityKeywordEnabled(string keyword)
	{
		bool value;
		if (unityKeywords.TryGetValue(keyword, out value))
		{
			return true;
		}
		return false;
	}

	public bool IsWaterKeywordEnabled(string keyword)
	{
		bool value;
		if (waterKeywords.TryGetValue(keyword, out value))
		{
			return true;
		}
		return false;
	}

	public string GetAdditionalCode()
	{
		StringBuilder stringBuilder = new StringBuilder(512);
		foreach (string value in shaderParts.Values)
		{
			stringBuilder.Append(value);
		}
		return stringBuilder.ToString();
	}

	public string[] GetUnityKeywords()
	{
		string[] array = new string[unityKeywords.Count];
		int num = 0;
		foreach (string key in unityKeywords.Keys)
		{
			array[num++] = key;
		}
		return array;
	}

	public string[] GetWaterKeywords()
	{
		string[] array = new string[waterKeywords.Count];
		int num = 0;
		foreach (string key in waterKeywords.Keys)
		{
			array[num++] = key;
		}
		return array;
	}

	public string GetKeywordsString()
	{
		StringBuilder stringBuilder = new StringBuilder(512);
		bool flag = false;
		foreach (string item in (IEnumerable<string>)Enumerable.OrderBy<string, string>((IEnumerable<string>)waterKeywords.Keys, (Func<string, string>)((string k) => k)))
		{
			if (flag)
			{
				stringBuilder.Append(' ');
			}
			else
			{
				flag = true;
			}
			stringBuilder.Append(item);
		}
		foreach (string item2 in (IEnumerable<string>)Enumerable.OrderBy<string, string>((IEnumerable<string>)unityKeywords.Keys, (Func<string, string>)((string k) => k)))
		{
			if (flag)
			{
				stringBuilder.Append(' ');
			}
			else
			{
				flag = true;
			}
			stringBuilder.Append(item2);
		}
		foreach (string item3 in (IEnumerable<string>)Enumerable.OrderBy<string, string>((IEnumerable<string>)shaderParts.Keys, (Func<string, string>)((string k) => k)))
		{
			if (flag)
			{
				stringBuilder.Append(' ');
			}
			else
			{
				flag = true;
			}
			stringBuilder.Append(item3);
		}
		return stringBuilder.ToString();
	}
}
