// <file>
//     <copyright see="prj:///doc/copyright.txt">2002-2005 AlphaSierraPapa</copyright>
//     <license see="prj:///doc/license.txt">GNU General Public License</license>
//     <owner name="David Srbeck�" email="dsrbecky@gmail.com"/>
//     <version>$Revision$</version>
// </file>

using Debugger;
using Microsoft.CSharp;
using NUnit.Framework;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading;

namespace Debugger.Tests
{
	/// <summary>
	/// This class contains methods that test the debugger
	/// </summary>
	[TestFixture]
	public class DebuggerTests
	{
		NDebugger debugger;
		string log;
		string lastLogMessage;
		string assemblyFilename;
		string assemblyDir;
		string symbolsFilename;
		
		public DebuggerTests()
		{
			assemblyFilename = Assembly.GetExecutingAssembly().Location;
			assemblyDir = Path.GetDirectoryName(assemblyFilename);
			symbolsFilename = Path.Combine(assemblyDir, Path.GetFileNameWithoutExtension(assemblyFilename) + ".pdb");
			
			debugger = new NDebugger();
			debugger.MTA2STA.CallMethod = CallMethod.Manual;
			debugger.LogMessage += delegate(object sender, MessageEventArgs e) {
				log += e.Message;
				lastLogMessage = e.Message;
			};
		}
		
		void StartProgram(string programName)
		{
			StartProgram(assemblyFilename, programName);
		}
		
		void StartProgram(string exeFilename, string programName)
		{
			log = "";
			lastLogMessage = null;
			debugger.Start(exeFilename, Path.GetDirectoryName(exeFilename), programName);
		}
		
		void WaitForPause(PausedReason expectedReason, string expectedLastLogMessage)
		{
			if (expectedLastLogMessage != null) expectedLastLogMessage += "\r\n";
			debugger.WaitForPause();
			Assert.AreEqual(true, debugger.IsPaused);
			Assert.AreEqual(expectedReason, debugger.PausedReason);
			Assert.AreEqual(expectedLastLogMessage, lastLogMessage);
		}
		
		
		[Test]
		public void SimpleProgram()
		{
			StartProgram("SimpleProgram");
			debugger.WaitForPrecessExit();
		}
		
		[Test]
		public void HelloWorld()
		{
			StartProgram("HelloWorld");
			debugger.WaitForPrecessExit();
			Assert.AreEqual("Hello world!\r\n", log);
		}
		
		[Test]
		public void Break()
		{
			StartProgram("Break");
			WaitForPause(PausedReason.Break, null);
			
			debugger.Continue();
			debugger.WaitForPrecessExit();
		}
		
		[Test]
		public void Symbols()
		{
			Assert.AreEqual("debugger.tests.exe", Path.GetFileName(assemblyFilename).ToLower());
			Assert.IsTrue(File.Exists(symbolsFilename), "Symbols file not found (.pdb)");
			
			StartProgram("Symbols");
			WaitForPause(PausedReason.Break, null);
			Assert.AreEqual(true, debugger.GetModule(Path.GetFileName(assemblyFilename)).SymbolsLoaded, "Module symbols not loaded");
			
			debugger.Continue();
			debugger.WaitForPrecessExit();
		}
		
		[Test]
		public void Breakpoint()
		{
			Breakpoint b = debugger.AddBreakpoint(@"D:\corsavy\SharpDevelop\src\AddIns\Misc\Debugger\Debugger.Tests\Project\Src\TestPrograms\Breakpoint.cs", 18);
			
			StartProgram("Breakpoint");
			WaitForPause(PausedReason.Break, null);
			Assert.AreEqual(true, b.Enabled);
			Assert.AreEqual(true, b.HadBeenSet, "Breakpoint is not set");
			Assert.AreEqual(18, b.SourcecodeSegment.StartLine);
			
			debugger.Continue();
			WaitForPause(PausedReason.Breakpoint, "Mark 1");
			
			debugger.Continue();
			WaitForPause(PausedReason.Break, "Mark 2");
			
			debugger.Continue();
			debugger.WaitForPrecessExit();
			Assert.AreEqual("Mark 1\r\nMark 2\r\n", log);
		}
		
		[Test, Ignore("Works only if run alone")]
		public void FileRelease()
		{
			Assert.IsTrue(File.Exists(assemblyFilename), "Assembly file not found");
			Assert.IsTrue(File.Exists(symbolsFilename), "Symbols file not found (.pdb)");
			
			string tempPath = Path.Combine(Path.GetTempPath(), Path.Combine("DebeggerTest", new Random().Next().ToString()));
			Directory.CreateDirectory(tempPath);
			
			string newAssemblyFilename = Path.Combine(tempPath, Path.GetFileName(assemblyFilename));
			string newSymbolsFilename = Path.Combine(tempPath, Path.GetFileName(symbolsFilename));
			
			File.Copy(assemblyFilename, newAssemblyFilename);
			File.Copy(symbolsFilename, newSymbolsFilename);
			
			Assert.IsTrue(File.Exists(newAssemblyFilename), "Assembly file copying failed");
			Assert.IsTrue(File.Exists(newSymbolsFilename), "Symbols file copying failed");
			
			StartProgram(newAssemblyFilename, "FileRelease");
			debugger.WaitForPrecessExit();
			
			try {
				File.Delete(newAssemblyFilename);
			} catch (System.Exception e) {
				Assert.Fail("Assembly file not released\n" + e.ToString());
			}
			
			try {
				File.Delete(newSymbolsFilename);
			} catch (System.Exception e) {
				Assert.Fail("Symbols file not released\n" + e.ToString());
			}
		}
		
		[Test]
		public void Stepping()
		{
			StartProgram("Stepping");
			WaitForPause(PausedReason.Break, null);
			
			debugger.StepOver(); // Debugger.Break
			WaitForPause(PausedReason.StepComplete, null);
			
			debugger.StepOver(); // Debug.WriteLine 1
			WaitForPause(PausedReason.StepComplete, "1");
			
			debugger.StepInto(); // Method Sub
			WaitForPause(PausedReason.StepComplete, "1");
			
			debugger.StepInto(); // '{'
			WaitForPause(PausedReason.StepComplete, "1");
			
			debugger.StepInto(); // Debug.WriteLine 2
			WaitForPause(PausedReason.StepComplete, "2");
			
			debugger.StepOut(); // Method Sub
			WaitForPause(PausedReason.StepComplete, "4");
			
			debugger.StepOver(); // Method Sub
			WaitForPause(PausedReason.StepComplete, "4");
			
			debugger.StepOver(); // Method Sub2
			WaitForPause(PausedReason.StepComplete, "5");
			
			debugger.Continue();
			debugger.WaitForPrecessExit();
		}
		
		[Test]
		public void Callstack()
		{
			List<Function> callstack;
			
			StartProgram("Callstack");
			WaitForPause(PausedReason.Break, null);
			callstack = new List<Function>(debugger.CurrentThread.Callstack);
			Assert.AreEqual("Sub2", callstack[0].Name);
			Assert.AreEqual("Sub1", callstack[1].Name);
			Assert.AreEqual("Main", callstack[2].Name);
			
			debugger.StepOut();
			WaitForPause(PausedReason.StepComplete, null);
			callstack = new List<Function>(debugger.CurrentThread.Callstack);
			Assert.AreEqual("Sub1", callstack[0].Name);
			Assert.AreEqual("Main", callstack[1].Name);
			
			debugger.StepOut();
			WaitForPause(PausedReason.StepComplete, null);
			callstack = new List<Function>(debugger.CurrentThread.Callstack);
			Assert.AreEqual("Main", callstack[0].Name);
			
			debugger.Continue();
			debugger.WaitForPrecessExit();
		}
		
		[Test]
		public void FunctionArgumentVariables()
		{
			List<Variable> args;
			
			StartProgram("FunctionArgumentVariables");
			WaitForPause(PausedReason.Break, null);
			
			for(int i = 0; i < 2; i++) {
				debugger.Continue();
				WaitForPause(PausedReason.Break, null);
				args = new List<Variable>(debugger.CurrentFunction.ArgumentVariables);
				// Argument names
				Assert.AreEqual("i", args[0].Name);
				Assert.AreEqual("s", args[1].Name);
				Assert.AreEqual("args", args[2].Name);
				// Argument types
				Assert.AreEqual(typeof(PrimitiveValue), args[0].Value.GetType());
				Assert.AreEqual(typeof(PrimitiveValue), args[1].Value.GetType());
				Assert.AreEqual(typeof(ArrayValue),     args[2].Value.GetType());
				// Argument values
				Assert.AreEqual("0", args[0].Value.AsString);
				Assert.AreEqual("S", args[1].Value.AsString);
				Assert.AreEqual(0 ,((ArrayValue)args[2].Value).Lenght);
				
				debugger.Continue();
				WaitForPause(PausedReason.Break, null);
				args = new List<Variable>(debugger.CurrentFunction.ArgumentVariables);
				// Argument types
				Assert.AreEqual(typeof(PrimitiveValue), args[0].Value.GetType());
				Assert.AreEqual(typeof(PrimitiveValue), args[1].Value.GetType());
				Assert.AreEqual(typeof(ArrayValue),     args[2].Value.GetType());
				// Argument values
				Assert.AreEqual("1", args[0].Value.AsString);
				Assert.AreEqual("S", args[1].Value.AsString);
				Assert.AreEqual(1 ,((ArrayValue)args[2].Value).Lenght);
				
				debugger.Continue();
				WaitForPause(PausedReason.Break, null);
				args = new List<Variable>(debugger.CurrentFunction.ArgumentVariables);
				// Argument types
				Assert.AreEqual(typeof(PrimitiveValue), args[0].Value.GetType());
				Assert.AreEqual(typeof(NullValue), args[1].Value.GetType());
				Assert.AreEqual(typeof(ArrayValue),     args[2].Value.GetType());
				// Argument values
				Assert.AreEqual("2", args[0].Value.AsString);
				Assert.IsNotNull(args[1].Value.AsString);
				Assert.AreEqual(2 ,((ArrayValue)args[2].Value).Lenght);
			}
			
			debugger.Continue();
			debugger.WaitForPrecessExit();
		}
	}
}
