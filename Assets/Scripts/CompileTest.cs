using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class CompileTest : MonoBehaviour 
{
	public Button compileButton;
	public InputField inputField;

	Compiler compiler = new Compiler();	
	Script script;

	// Use this for initialization
	void Awake () 
	{
		compileButton.onClick.AddListener(OnClick);	
	}

	void OnClick()
	{
		if (compiler.Compile(inputField.text))
		{
			MemoryStream ms = ByteCode.SaveToMemory(compiler.GetTables());
			
			ScriptContext context;

			if (ByteCode.Load(ms, out context))
			{
				script = new Script(context);
			}
		}
	}

	void Update()
	{
		if (script != null)
			script.RunStep();
	}
}
