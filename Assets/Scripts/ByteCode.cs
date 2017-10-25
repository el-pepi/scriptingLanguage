using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ByteCode 
{
	public static bool Save(string filename, Tables tables)
	{
		FileStream fs = new FileStream(filename, FileMode.Create);

		fs = (FileStream) Save(fs, tables);

		return true;
	}

	public static MemoryStream SaveToMemory(Tables tables)
	{
		MemoryStream s = new MemoryStream();

		s = (MemoryStream) Save(s, tables);

		return s;
	}

	public static Stream Save(Stream s, Tables tables)
	{
		BinaryWriter bw = new BinaryWriter(s);

		Header header = new Header();

		header.Magic = HeaderConst.Magic;
		header.MajorVersion = HeaderConst.MajorVersion;
		header.MinorVersion = HeaderConst.MinorVersion;

		header.InstructionsCount = tables.GetInstrStream().Count;
		header.PCStartIdx = 0;
		header.StackSize = 1024;
		
		header.GlobalVarsSize = tables.GetVarsTable().Count;

		bw.Write(header.Magic);
		bw.Write(header.MajorVersion);
		bw.Write(header.MinorVersion);
		bw.Write(header.InstructionsCount);
		bw.Write(header.PCStartIdx);
		bw.Write(header.StackSize);
		bw.Write(header.GlobalVarsSize);

		for (int i = 0; i < header.InstructionsCount; i++)
		{
			Instruction inst = tables.GetInstrStream()[i];
			bw.Write(inst.OpCode);

			if (inst.Values != null)
			{
				bw.Write(inst.Values.Length);
				for (int j = 0; j < inst.Values.Length; j++)
				{
					Value v = inst.Values[j];

					bw.Write((int)v.Type);

					switch(v.Type)
					{
						case OpType.Int:
						case OpType.MemIdx:
						case OpType.InstrIdx:
							bw.Write(v.IntLiteral);
						break;
						case OpType.Float:
							bw.Write(v.FloatLiteral);
						break;
						case OpType.String:
							bw.Write(v.StringLiteral);
						break;
					}
				}
			}
			else
			{
				bw.Write(0);
			}
		}
		
		bw.Flush();

		return s; 
	}

	public static bool Load(string filename, out ScriptContext context)
	{
		try
		{
			FileStream fs = new FileStream(filename, FileMode.Open);

			return Load(fs, out context);
		}
		catch (System.Exception e)
		{
			// TODO: Log error
			context = new ScriptContext();

			return false;
		}
	}
	

	public static bool Load(Stream s, out ScriptContext context)
	{
		context = new ScriptContext();

		s.Seek(0, SeekOrigin.Begin);

		BinaryReader br = new BinaryReader(s);

		Header header = new Header();

		header.Magic = br.ReadString();

		if (header.Magic != HeaderConst.Magic)
		{
			br.Close();

			// TODO: Log error: Bad Magic
			return false;
		}

		// TODO: check for version number
		header.MajorVersion = br.ReadByte();
		header.MinorVersion = br.ReadByte();

		header.InstructionsCount = br.ReadInt32();
		header.PCStartIdx = br.ReadInt32();
		
		header.StackSize = br.ReadInt32();
		header.GlobalVarsSize = br.ReadInt32();

		context.instrStream.StartPC = context.instrStream.PC = header.PCStartIdx;
		context.instrStream.Instructions = new Instruction[header.InstructionsCount];

		context.stack.Elements = new Value[header.StackSize];
		context.stack.StackStartIdx = header.GlobalVarsSize;
		context.stack.TopStackIdx = 0;

		for (int i = 0; i < header.InstructionsCount; i++)
		{
			Instruction inst = new Instruction();

			inst.OpCode = br.ReadInt32();

			int valuesCount = br.ReadInt32();

			if (valuesCount > 0)
			{
				inst.Values = new Value [valuesCount];

				for (int j = 0; j < inst.Values.Length; j++)
				{
					OpType type  = (OpType)br.ReadInt32();

					inst.Values[j].Type = type;

					switch(type)
					{
						case OpType.Int:
						case OpType.MemIdx:
						case OpType.InstrIdx:
							inst.Values[j].IntLiteral = br.ReadInt32();
						break;
						case OpType.Float:
							inst.Values[j].FloatLiteral = br.ReadSingle();
						break;
						case OpType.String:
							inst.Values[j].StringLiteral = br.ReadString();
						break;
					}
				}
			}

			context.instrStream.Instructions[i] = inst;
		}

		br.Close();

		return true; 
	}
}
