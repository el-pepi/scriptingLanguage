using System;

public class StringUtil
{
	const char COMMENT_CHAR = ';';

	public static bool IsCharNumeric(char chr) 
	{
		if (chr >= '0' && chr <= '9')
			return true;
		
		return false;
	}
	
	public static bool IsCharWhiteSpace(char chr)
	{
		if (chr == ' ' || chr == '\t' || chr == '\n')
			return true;
		
		return false;
	}

	public static bool IsCharIdent(char chr)
	{
		if ((chr >= '0' && chr <= '9') ||
			(chr >= 'A' && chr <= 'Z') ||
			(chr >= 'a' && chr <= 'z') ||
			chr == '_')
			return true;
		
		return false;
	}

	public static bool IsCharDelimiter(char chr)
	{
		if ( chr == ':' || chr == ',' || chr == '"' ||
			chr == '[' || chr == ']' || 
			chr == '{' || chr == '}' ||
			chr == '\n' ||
			IsCharWhiteSpace(chr))
			return true;
		
		return false;
	}

	public static bool IsStringInt(string str)
	{
		if (string.IsNullOrEmpty(str))
			return false;
		
		int ch = 0;
		
		for(ch = 0; ch < str.Length; ch++) {
			if (!IsCharNumeric(str[ch]) && str[ch] != '-')
				return false;
		}
		
		for(ch = 1; ch < str.Length; ch++) {
			if (str[ch] == '-')
				return false;
		}
		
		return true;
	}
	
	public static bool IsStringFloat(string str)
	{
		if (string.IsNullOrEmpty(str))
			return false;

		int ch = 0;
		
		for(ch = 0; ch < str.Length; ch++) {
			if (!IsCharNumeric(str[ch]) && 
				str[ch] != '-' &&
				str[ch] != '.')
				return false;
		}
		
		bool radixPointFound = false;
		
		for(ch = 1; ch < str.Length; ch++)
		{
			if (str[ch] == '.')
				if (radixPointFound)
					return false;
			else
				radixPointFound = true;
		}
		
		for(ch = 1; ch < str.Length; ch++)
		{
			if (str[ch] == '-')
				return false;
		}
		
		if (!radixPointFound)
			return false;
		
		return true;
	}

	public static bool IsStringWhiteSpace(string str)
	{
		if (string.IsNullOrEmpty(str))
			return false;
		
		int ch = 0;
		
		for(ch = 0; ch < str.Length; ch++) 
		{
			if (!IsCharWhiteSpace(str[ch]))
				return false;
		}
		
		return true;
	}

	public static bool IsStringIdent(string str)
	{
		if (string.IsNullOrEmpty(str))
			return false;

		if (str[0] >= '0' && str[0] <= '9')
			return false;
		
		int ch = 0;
		
		for(ch = 0; ch < str.Length; ch++) 
		{
			if (!IsCharIdent(str[ch]))
				return false;
		}
		
		return true;
	}
	
	public static string StripComments(string str) 
	{
		int id = 0;
		bool inString = false;
		string res = "";

		if (str.Length <= 0)
			return str;
		
		for (id = 0; id < str.Length; id++) 
		{
			if (str[id] == '"') 
			{
				res += str[id];
				inString = !inString;
			}
			else if (str[id] == COMMENT_CHAR) 
			{
				if (!inString) 
				{
					break;
				}
				else
				{
					res += str[id];
				}
			}
			else
			{
				res += str[id];
			}
		}

		return res;
	}

	public static bool IsCharHexValue(char ch)
	{
		
		if (IsCharNumeric(ch) || 
			(ch >= 'A' && ch <= 'F') ||
			(ch >= 'a' && ch <= 'f'))
			return true;
		
		return false;
	}

	public static bool IsStringHex(string str)
	{
		if (string.IsNullOrEmpty(str))
			return false;
		
		if (str.Length <= 2 || str.Length > 10)
			return false;
		
		int ch = 0;
		
		if (str[0] != '0' || (str[1] != 'x' && str[1] != 'X')) 
		{
			return false;
		}
		
		for(ch = 2; ch < str.Length; ch++) 
		{
			if (!IsCharHexValue(str[ch]))
				return false;
		}
		return true;
	}

	public static int StrHexToInt(string str)
	{
		if (str.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
			str = str.Substring(2);

		return int.Parse(str, System.Globalization.NumberStyles.HexNumber);
	}
}

