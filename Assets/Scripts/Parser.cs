using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
	; COMMENT

	FUNC PEPE 

		VAR W

		POP C
		POP B
		POP A


		RET
	ENDFUNC

	LABEL:
		VAR X

		MOV X, 5

		PUSH A
		PUSH B
		PUSH C
		JSR PEPE

		JMP LABEL

 */

public class Parser 
{
	Tokenizer tokenizer = new Tokenizer();

	Tables tables;

	ConsoleLogger logger;

	public Parser(Tables tables,ConsoleLogger logger)
	{
		this.tables = tables;
		this.logger = logger;
	}

	public void Reset()
	{
		tables.Clear();
	}

	public bool Parse(string str)
	{
		Reset();

		tokenizer.Start(str);

		// Parse Vars and Labels
		if (!Pass1())
			return false;

		// Rewind tokenizer to start the
		// second pass from the beginning
		tokenizer.Rewind();

		// Parse instructions
		if (!Pass2())
			return false;

		return true;
	}

	// Parse Vars and Labels
	bool Pass1()
	{
		Tokenizer.Token currentToken = tokenizer.GetNextToken();

		if (currentToken.Type == Tokenizer.TokenType.Empty)
		{
			return false;
		}

		int instrIdx = 0;
		
		while (currentToken.Type != Tokenizer.TokenType.EOF && currentToken.Type != Tokenizer.TokenType.Unknown)
		{
			// ===================================================================
			// Skip end of lines
			if (currentToken.Type == Tokenizer.TokenType.EOL)
			{
				currentToken = tokenizer.GetNextToken();
			}
			// ===================================================================
			// Parse variables
 			else if (currentToken.Type == Tokenizer.TokenType.Rsvd_Var)
			{
				currentToken = tokenizer.GetNextToken();
				
				if (currentToken.Type == Tokenizer.TokenType.Ident)
				{
					if (!tables.AddVar(currentToken.Lexeme))
					{
						logger.Log("Parser Error: Var already exists");
						return false;
					}
				}
				else
				{
					logger.Log("Parser Error: Ident expected");
					return false;
				}

				currentToken = tokenizer.GetNextToken();
			}
			// ===================================================================
			// Parse instructions and labels			
			else if (currentToken.Type == Tokenizer.TokenType.Ident)
			{
				string ident = currentToken.Lexeme;

				currentToken = tokenizer.GetNextToken();

				// ===================================================================
				// Is it a label?
				if (currentToken.Type == Tokenizer.TokenType.Colon)
				{
					tables.AddLabel(ident, instrIdx);

					currentToken = tokenizer.GetNextToken();
				}
				// ===================================================================
				// It's an instruction
				else
				{
					instrIdx++; // Increment counter

					// Skip to next line
					currentToken = tokenizer.SkipToNextLine();
				}

			}
			else
			{
				logger.Log("Parser Error: unexpected token");
				return false;
			}
		}

		return true;
	}

	// Parse instructions
	bool Pass2()
	{
		Instruction currentInstruction;
		Tokenizer.Token currentToken = tokenizer.GetNextToken();

		if (currentToken.Type == Tokenizer.TokenType.Empty)
		{
			return false;
		}
		
		while (currentToken.Type != Tokenizer.TokenType.EOF && currentToken.Type != Tokenizer.TokenType.Unknown)
		{
			// ===================================================================
			// Skip end of lines
			if (currentToken.Type == Tokenizer.TokenType.EOL)
			{
				currentToken = tokenizer.GetNextToken();
			}
			// ===================================================================
			// Skip variables declaration 
 			else if (currentToken.Type == Tokenizer.TokenType.Rsvd_Var)
			{
				currentToken = tokenizer.GetNextToken(); // Skip the VAR reserved word
				
				currentToken = tokenizer.GetNextToken(); // Skip VAR's identifier
			}
			// ===================================================================
			// Parse instructions and labels			
			else if (currentToken.Type == Tokenizer.TokenType.Ident)
			{
				string ident = currentToken.Lexeme;

				currentToken = tokenizer.GetNextToken();

				// ===================================================================
				// Is it a label? Skip it
				if (currentToken.Type == Tokenizer.TokenType.Colon)
				{
					currentToken = tokenizer.GetNextToken();
				}
				// ===================================================================
				// It's an instruction
				else
				{
					InstrDecl instr;

					if (!tables.GetInstrLookUp(ident, out instr))
					{
						logger.Log("Parser Error: syntax error");
						return false;
					}

					currentInstruction = new Instruction();
					currentInstruction.OpCode = instr.OpCode;

					if (instr.ParamsCount > 0)
						currentInstruction.Values = new Value[instr.ParamsCount];

					// ===================================================================
					// Parse params
					for (int i = 0; i < instr.ParamsCount; i++)
					{
						// We have to skip the ','
						if (i > 0)
						{
							currentToken = tokenizer.GetNextToken();
							if (currentToken.Type != Tokenizer.TokenType.Comma)
							{
								logger.Log("Parser Error: Comma expected");
								return false;
							}
							
							currentToken = tokenizer.GetNextToken();
						}

						Tokenizer.TokenType t = currentToken.Type;
						int flags = instr.ParamsFlags[i];

						// ===================================================================
						// Is it a variable or label?
						if (t == Tokenizer.TokenType.Ident)
						{
							if ((flags & OpFlags.MemIdx) != 0)
							{
								VarDecl varDecl;
								
								if (!tables.GetVarByIdent(currentToken.Lexeme, out varDecl))
								{
									logger.Log("Parser Error: Variable doesn't exist");
									return false;
								}
								currentInstruction.Values[i].Type = OpType.MemIdx;
								currentInstruction.Values[i].StackIndex = varDecl.Idx;
							}
							else if ((flags & OpFlags.InstrIdx) != 0)
							{
								LabelDecl label;
								
								if (!tables.GetLabelByName(currentToken.Lexeme, out label))
								{
									logger.Log("Parser Error: Label doesn't exist");
									return false;
								}

								currentInstruction.Values[i].Type = OpType.InstrIdx;
								currentInstruction.Values[i].InstrIndex = label.Idx;
							}
							else if ((flags & OpFlags.HostAPICallIdx) != 0)
							{
								// TODO: host api calls
							}
						}
						// ===================================================================
						// Is it a literal value?
						else if (t == Tokenizer.TokenType.Number || t == Tokenizer.TokenType.String)
						{
							if ((flags & OpFlags.Literal) == 0)
							{	
								// TODO: Log error: doesn´t allow literals
								return false;
							}

							if (t == Tokenizer.TokenType.Number)
							{
								if (StringUtil.IsStringFloat(currentToken.Lexeme))
								{
									float val = 0;
									
									currentInstruction.Values[i].Type = OpType.Float;

									if (float.TryParse(currentToken.Lexeme, out val))
										currentInstruction.Values[i].FloatLiteral = val;
									else
									{
										// TODO: log error: error parsing float value
										return false;
									}
								}
								else if (StringUtil.IsStringInt(currentToken.Lexeme))
								{
									int val = 0;
									
									currentInstruction.Values[i].Type = OpType.Int;

									if (int.TryParse(currentToken.Lexeme, out val))
										currentInstruction.Values[i].IntLiteral = val;
									else
									{
										// TODO: log error: error parsing int value
										return false;
									}
								}
								else if (StringUtil.IsStringHex(currentToken.Lexeme))
								{
									currentInstruction.Values[i].Type = OpType.Int;
									currentInstruction.Values[i].IntLiteral = StringUtil.StrHexToInt(currentToken.Lexeme);
								}
								else 
								{
									// TODO: log error: error parsing literal value
									return false;
								}
							}
							else
							{
								currentInstruction.Values[i].Type = OpType.String;
								currentInstruction.Values[i].StringLiteral = currentToken.Lexeme;
							}
						}
						else
						{
							// TODO: log error: unexpected token
							return false;
						}
					}

					// Add the instruction to the stream
					tables.AddInstrToStream(currentInstruction);
					
					// Skip to next token
					currentToken = tokenizer.GetNextToken();
				}
			}
			else
			{
				// TODO: log error: unexpected token
				return false;
			}
		}

		return true;
	}

}
