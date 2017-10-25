using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Tokenizer  
{
    public enum TokenType
    {
        Ident, 
        Colon, // Dos puntos
        Comma,
        String,
        Number,
        Rsvd_Var, // Reserved VAR
        EOL, // Fin de linea
        EOF, // Fin de archivo
        Unknown,
        Empty
    }

    public struct Token
    {
        public string Lexeme;
        public TokenType Type;
    }

    private List<string> lines = new List<string>(); 
    private int idStart = 0;
    private int idEnd = 0;
    private int currentLine = 0;
    private Token token = new Token();

    public void Start(string str)
    {
        Reset();
        
        string[] lineArr = str.Split(new string[] {System.Environment.NewLine , "\n"}, System.StringSplitOptions.None);
        
        lines.AddRange(lineArr);
    }

    public void Reset()
    {
        lines.Clear();
        Rewind();
    }

    public void Rewind()
    {
        idStart = 0;
        idEnd = 0;
        currentLine = 0;
    }

    public int CurrentLine
    {
        get { return currentLine; }
    }

    public Token GetCurrentToken()
    {
        return token;
    }

    public Token SkipToNextLine()
    {
        token.Lexeme = "";
        
        idStart = 0;
        idEnd = 0;

        currentLine++;

        if (currentLine >= lines.Count)
        {
            token.Type = TokenType.EOF;
        }
        else
        {
            token.Type = TokenType.EOL;
        }

        return token;        
    }

    public Token GetNextToken()
    {
        idStart = idEnd;

        if (lines.Count == 0)
        {
            token.Lexeme = "";
            token.Type = TokenType.Empty;
        }
        else
        {
            string currentString = StringUtil.StripComments(lines[currentLine]).Trim();

            // Skip whitespaces
            while (idStart < currentString.Length && StringUtil.IsCharWhiteSpace(currentString[idStart]))
            {
                idStart++;
            }
            
            // Skip to next line of code
            if (idStart >= currentString.Length)
            {
                SkipToNextLine();
            }
            else
            {
                if (StringUtil.IsCharNumeric(currentString[idStart]) || currentString[idStart] == '-')
                {
                    token.Type = TokenType.Number;
                    token.Lexeme = GetLexemeFromString(currentString);
                }
                else if (StringUtil.IsCharIdent(currentString[idStart]))
                {
                    token.Type = TokenType.Ident;
                    token.Lexeme = GetLexemeFromString(currentString);

                    if (token.Lexeme.ToUpper() == "VAR")
                        token.Type = TokenType.Rsvd_Var;
                }
                else if (currentString[idStart] == ':') 
                {
                    token.Lexeme = currentString[idStart].ToString();
                    token.Type = TokenType.Colon;
                    idEnd++;
                }
                else if (currentString[idStart] == ',')
                {
                    token.Lexeme = currentString[idStart].ToString();
                    token.Type = TokenType.Comma;
                    idEnd++;
                }
                else if (currentString[idStart] == '"')
                {
                    token.Lexeme = GetStringLiteralFromString(currentString);
                    token.Type = TokenType.String;
                    idEnd++;
                }
                else
                {
                    token.Lexeme = null;
                    token.Type = TokenType.Unknown;
                }
            }
        }        

        return token;
    }

    private string GetLexemeFromString(string str)
    {
        string lexeme = "";
        
        idEnd = idStart;

        while (idEnd != str.Length)
        {
            lexeme += str[idEnd++];
            
            if (idEnd >= str.Length || StringUtil.IsCharDelimiter(str[idEnd]))
            {
                break;
            }
        }

        return lexeme;
    }

    private string GetStringLiteralFromString(string str)
    {
        string lexeme = "";
        
        idEnd = idStart;

        if (str[idEnd] == '"') // Skip starting quote
            idEnd++;

        while (idEnd != str.Length)
        {
            lexeme += str[idEnd++];

            if (idEnd >= str.Length || str[idEnd] == '"')
            {
                break;
            }
        }

        return lexeme;
    }

}
