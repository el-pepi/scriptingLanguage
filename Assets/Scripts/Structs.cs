using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices; // For aligning layouts

// ==================================================
// File format structs
public static class HeaderConst
{
	public const string Magic = "ZS"; // For Zak Script
	public const byte MajorVersion = 1; // Major version of my script
	public const byte MinorVersion = 0; // Minor version
}

public struct Header
{
	public string Magic;
	public byte MajorVersion;
	public byte MinorVersion;
	public int StackSize;
	public int GlobalVarsSize;
	public int PCStartIdx;
	public int InstructionsCount;
}

// ==================================================
// Compiler structs
public struct LabelDecl
{
	public int Idx;
	public string Ident;
}

public struct InstrDecl
{
	public int OpCode;
	public int ParamsCount;
	public int[] ParamsFlags;
}

public struct VarDecl
{
	public string Ident;
	public int Idx;

	// TODO: add func id
}

// ==================================================
// Runtime structs
public struct InstrStream
{
	public Instruction[] Instructions;
	public int PC;
	public int StartPC;
}

public struct Instruction
{
	public int OpCode;
	public Value[] Values;

	// TODO: add function id
}


public static class OpFlags
{
	public const int Literal 		= 1;
	public const int MemIdx 		= 2;
	public const int InstrIdx 		= 4; 
	public const int HostAPICallIdx	= 8;
}

public enum OpType
{
	Null,
	Int,
	Float, 
	String, 
	MemIdx,
	InstrIdx, 
	HostAPICallIdx,
}


// Pack = 1 > Pack aligned on 1 byte boundaries 
// (no gaps between fields)
// Explicit means we can specify the offsets manually
[StructLayout(LayoutKind.Explicit, Pack = 1)] 
public struct Value
{
	[FieldOffset(0)]
	public OpType 	Type;

	[FieldOffset(4)]
	public int 		IntLiteral;
	[FieldOffset(4)]
	public float 	FloatLiteral;
	[FieldOffset(4)]
	public int 		StackIndex;
	[FieldOffset(4)]
	public int		InstrIndex;
	[FieldOffset(4)]
	public int		HostAPICallIndex;

	[FieldOffset(8)]
	public string 	StringLiteral;
}

public struct RuntimeStack 
{
	public Value[] Elements;
	public int StackStartIdx; // stack starts after global vars
	public int TopStackIdx;
}

public class ScriptContext
{
	public RuntimeStack stack;
	public InstrStream instrStream;
}

public class OpCodes
{
	public const int INSTR_MOV					= 0;
	public const int INSTR_ADD					= 1;
	public const int INSTR_SUB					= 2;
	public const int INSTR_MUL					= 3;
	public const int INSTR_DIV					= 4;
	public const int INSTR_MOD					= 5;
	public const int INSTR_EXP					= 6;
	public const int INSTR_NEG					= 7;
	public const int INSTR_INC					= 8;
	public const int INSTR_DEC					= 9;
	public const int INSTR_AND					= 10;
	public const int INSTR_OR					= 11;
	public const int INSTR_XOR					= 12;
	public const int INSTR_NOT					= 13;
	public const int INSTR_SHL					= 14;
	public const int INSTR_SHR					= 15;
	public const int INSTR_JMP					= 16;
	public const int INSTR_JE					= 17;
	public const int INSTR_JNE					= 18;
	public const int INSTR_JG					= 19;
	public const int INSTR_JL					= 20;
	public const int INSTR_JGE					= 21;
	public const int INSTR_JLE					= 22;
	public const int INSTR_PUSH					= 23;
	public const int INSTR_POP					= 24;
	public const int INSTR_PAUSE				= 25;
	public const int INSTR_EXIT					= 26;
	public const int INSTR_JSR					= 27;
	public const int INSTR_RET					= 28;
	public const int INSTR_CALLHOST				= 29;
	public const int INSTR_LN					= 30;

	public const int COUNT		 				= 31;
}
