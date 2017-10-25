using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class Script 
{
	enum State
	{
		None,
		Running,
		Pause,
	}
	private delegate void InstrDlg(Value[] values);

	ScriptContext context;

	InstrDlg[] instrExec = new InstrDlg[OpCodes.COUNT];
	State state = State.None;

	public Script()
	{
		SetInstrDlg();
	}

	public Script(ScriptContext context) 
	{
		this.context = context;
		SetInstrDlg();
	}

	private void SetInstrDlg()
	{
		instrExec[OpCodes.INSTR_MOV] = InstrMov;
		instrExec[OpCodes.INSTR_ADD] = InstrAdd;
		instrExec[OpCodes.INSTR_EXIT] = InstrExit;
		instrExec[OpCodes.INSTR_JE] = InstrJe;
		instrExec[OpCodes.INSTR_PAUSE] = InstrPause;
		instrExec[OpCodes.INSTR_POP] = InstrPop;
		instrExec[OpCodes.INSTR_PUSH] = InstrPush;
		
	}

	public void RunStep()
	{
		if (state == State.Running)
		{
			if (context.instrStream.Instructions == null)
				return;

			if (context.instrStream.PC >= 0 && context.instrStream.PC < context.instrStream.Instructions.Length)
			{
				Instruction op = context.instrStream.Instructions[context.instrStream.PC];

				if (instrExec[op.OpCode] != null)
					instrExec[op.OpCode](op.Values);

				context.instrStream.PC++;
			}
			else
			{
				Stop();
			}
		}
	}

	void ResetContext()
	{
		context.instrStream.PC = context.instrStream.StartPC;
		context.stack.TopStackIdx = 0;
	}

	public void Start()
	{
		if (state == State.Running)
			ResetContext();
			
		state = State.Running;
	}

	public void Stop()
	{
		state = State.None;
		ResetContext();
	}

	public void Pause(bool pause)
	{
		if (pause)
		{
			state = State.Pause;
		}
		else
		{
			state = State.Running;
		}
	}

#region Instructions
	private void InstrMov(Value[] values)
	{
		Value val = ResolveOpValue(values[1]);
		ResolveOpValueAndSet(0, val);
	}

	private void InstrAdd(Value[] values)
	{
		Value val1 = ResolveOpValue(values[0]);
		Value val2 = ResolveOpValue(values[1]);

		switch(val1.Type)
		{
			case OpType.Int:
				if (val2.Type == OpType.Int)
					val1.IntLiteral += val2.IntLiteral;
				else if (val2.Type == OpType.Float)
				{
					val1.Type = OpType.Float;
					val1.FloatLiteral = (float)val1.IntLiteral + val2.FloatLiteral;
				}
				else if (val2.Type == OpType.String)
				{
					val1.Type = OpType.String;
					val1.StringLiteral = val1.IntLiteral + val2.StringLiteral;
				}
			break;
			case OpType.Float:
				if (val2.Type == OpType.Int)
					val1.FloatLiteral += (float)val2.IntLiteral;
				else if (val2.Type == OpType.Float)
					val1.FloatLiteral += (float)val2.FloatLiteral;
				else if (val2.Type == OpType.String)
				{
					val1.Type = OpType.String;
					val1.StringLiteral = val1.FloatLiteral + val2.StringLiteral;
				}
			break;
			case OpType.String:
				val1.StringLiteral = val1.FloatLiteral + val2.StringLiteral;
			break;
		}
		
		ResolveOpValueAndSet(0, val1);
	}

	private void InstrJe(Value[] values)
	{
		Value val1 = ResolveOpValue(values[0]);
		Value val2 = ResolveOpValue(values[1]);
		Value val3 = ResolveOpValue(values[2]);
		
		bool shouldJump = false;

		switch(val2.Type)
		{
			case OpType.Int:
				if (val3.Type == OpType.Int)
					shouldJump = val2.IntLiteral == val3.IntLiteral;
				else if (val3.Type == OpType.Float)
					shouldJump = val2.IntLiteral == (int)val3.FloatLiteral;
				else if(val3.Type == OpType.String)
					shouldJump = val2.IntLiteral.ToString() == val3.StringLiteral;
			break;
			case OpType.Float:
				if (val3.Type == OpType.Int)
					shouldJump = val2.FloatLiteral == val3.IntLiteral;
				else if(val3.Type == OpType.Float)
					shouldJump = val2.FloatLiteral == val3.FloatLiteral;
				else if(val3.Type == OpType.String)
					shouldJump = val2.FloatLiteral.ToString() == val3.StringLiteral;
			break;
			case OpType.String:
				if (val3.Type == OpType.Int)
					shouldJump = val2.StringLiteral == val3.IntLiteral.ToString();
				else if(val3.Type == OpType.Float)
					shouldJump = val2.StringLiteral == val3.FloatLiteral.ToString();
				else if (val3.Type == OpType.String)
					shouldJump = val2.StringLiteral == val3.StringLiteral;
			break;
		}

		if (shouldJump)
			context.instrStream.PC = val1.IntLiteral;
	}

	private void InstrPush(Value[] values)
	{
		Value val = ResolveOpValue(values[0]);
		Push(val);
	}

	private void InstrPop(Value[] values)
	{
		Value val = Pop();
		ResolveOpValueAndSet(0, val);
	}

	private void InstrExit(Value[] values)
	{
		Stop();
	}

	private void InstrPause(Value[] values)
	{
		Value val = ResolveOpValue(values[0]);

		Pause(val.IntLiteral > 0);
	}

#endregion 

	private void Push(Value val)
	{
		int start = context.stack.StackStartIdx;
		int top = context.stack.TopStackIdx;
		int idx = start + top;

		if (idx < context.stack.Elements.Length)
		{
			context.stack.Elements[idx] = val;
			context.stack.TopStackIdx++;
		}
		else
		{
			// TODO: Log stack overflow error!
		}
	}

	private Value Pop()
	{
		int start = context.stack.StackStartIdx;
		int top = context.stack.TopStackIdx;
		int idx = top - 1;
		
		Value val; 

		if (idx >= start)
		{
			val = context.stack.Elements[idx];
			context.stack.TopStackIdx--;
		}
		else
		{
			val = new Value();

			// TODO: Log stack overflow error!
		}

		return val;
	}

	private Value ResolveOpValue(Value val)
	{
		switch(val.Type)
		{
			case OpType.MemIdx:
				return context.stack.Elements[val.IntLiteral];

			default:
				return val;
		}
	}

	private void ResolveOpValueAndSet(int idx, Value val)
	{
		context.stack.Elements[GetOpValue(idx).IntLiteral] = val;
	}

	private Value GetOpValue(int idx)
	{
		return context.instrStream.Instructions[context.instrStream.PC].Values[idx];
	}
}
