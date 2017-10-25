using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Compiler 
{
	Tables tables = new Tables();
	Parser parser;

	public Compiler()
	{
		ErrorLogger logger = new ErrorLogger();
		parser = new Parser(tables,logger);

		tables.AddInstrLookUp("MOV", OpCodes.INSTR_MOV, 2);
		tables.SetOpType("MOV", 0, 
			OpFlags.MemIdx
		);
		tables.SetOpType("MOV", 1, 
			OpFlags.Literal |
			OpFlags.MemIdx
		);

		tables.AddInstrLookUp("ADD", OpCodes.INSTR_ADD, 2);
		tables.SetOpType("ADD", 0, 
			OpFlags.MemIdx
		);
		tables.SetOpType("ADD", 1, 
			OpFlags.Literal |
			OpFlags.MemIdx
		);


		tables.AddInstrLookUp("JE", OpCodes.INSTR_JE, 3);
		tables.SetOpType("JE", 0, 
			OpFlags.InstrIdx
		);
		tables.SetOpType("JE", 1, 
			OpFlags.Literal |
			OpFlags.MemIdx
		);
		tables.SetOpType("JE", 2, 
			OpFlags.Literal |
			OpFlags.MemIdx
		);

		tables.AddInstrLookUp("EXIT", OpCodes.INSTR_EXIT, 0);

		tables.AddInstrLookUp("PAUSE", OpCodes.INSTR_PAUSE, 1);
		tables.SetOpType("PAUSE", 0, 
			OpFlags.Literal |
			OpFlags.MemIdx
		);

		tables.AddInstrLookUp("POP", OpCodes.INSTR_POP, 1);
		tables.SetOpType("POP", 0, 
			OpFlags.MemIdx
		);
		
		tables.AddInstrLookUp("PUSH", OpCodes.INSTR_PUSH, 1);
		tables.SetOpType("PUSH", 0, 
			OpFlags.MemIdx |
			OpFlags.Literal 
		);

	}

	public Tables GetTables()
	{
		return tables;
	}

	public bool Compile(string str)
	{
		parser.Reset();
		
		if (!parser.Parse(str))
		{
			Debug.Log("Error while parsing...");
			return false;		
		}

		return true;
	}

}
