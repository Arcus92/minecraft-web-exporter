using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MinecraftWebExporter.Structs;
using MinecraftWebExporter.Utils;

namespace MinecraftWebExporter.Tests;

[TestClass]
public class CmdLineParserTests
{
    [TestMethod]
    public void TestProgramParameter()
    {
        var args = CmdLineToArgumentArray(
            "--minecraft appdata/.minecraft --world path/world --alias test-world --output path/output " +
            "--min -16 -16 --max 16 16 --home 8 8 --threads 8 " +
            "--culling false --culling-height 16 " +
            "--resourcepack path/pack1 path/pack2");
        var parameter = ProgramParameter.Parse(args);
        
        Assert.AreEqual("appdata/.minecraft", parameter.PathToMinecraft);
        Assert.AreEqual("path/world", parameter.PathToWorld);
        Assert.AreEqual("path/output", parameter.PathToOutput);
        
        Assert.AreEqual(new Vector3(-16, -64, -16), parameter.WorldMin);
        Assert.AreEqual(new Vector3(16, 255, 16), parameter.WorldMax);
        Assert.AreEqual(new Vector3(8, 0, 8), parameter.WorldHome);
        
        Assert.AreEqual(false, parameter.CaveCulling);
        Assert.AreEqual(16, parameter.CaveCullingHeight);
        
        Assert.AreEqual(2, parameter.PathToResourcePacks.Length);
        Assert.AreEqual("path/pack1", parameter.PathToResourcePacks[0]);
        Assert.AreEqual("path/pack2", parameter.PathToResourcePacks[1]);
    }
    
    [TestMethod]
    public void TestCmdLineParser()
    {
        var args = CmdLineToArgumentArray("-param1 value1 value2 -param2 -param3 1 2.0 -3");
        var parser = new CmdLineParser(args);

        var hasParam1 = false;
        var hasParam2 = false;
        var hasParam3 = false;

        while (parser.Next())
        {
            switch (parser.ParameterName)
            {
                case "-param1":
                    hasParam1 = true;
                    Assert.AreEqual(2, parser.ValueCount);
                    Assert.AreEqual("value1", parser.GetValue(0));
                    Assert.AreEqual("value2", parser.GetValue(1));
                    
                    Assert.AreEqual(2, parser.Values.Length);
                    Assert.AreEqual("value1", parser.Values[0]);
                    Assert.AreEqual("value2", parser.Values[1]);
                    break;
                
                case "-param2":
                    hasParam2 = true;
                    Assert.AreEqual(0, parser.ValueCount);
                    Assert.AreEqual(0, parser.Values.Length);
                    break;
                
                case "-param3":
                    hasParam3 = true;
                    Assert.AreEqual(3, parser.ValueCount);
                    Assert.AreEqual("1", parser.GetValue(0));
                    Assert.AreEqual("2.0", parser.GetValue(1));
                    Assert.AreEqual("-3", parser.GetValue(2));
                    
                    Assert.AreEqual(3, parser.Values.Length);
                    Assert.AreEqual("1", parser.Values[0]);
                    Assert.AreEqual("2.0", parser.Values[1]);
                    Assert.AreEqual("-3", parser.Values[2]);
                    break;
                
                default:
                    Assert.Fail($"Unknown command line argument: {parser.ParameterName}");
                    break;
            }
        }
        
        Assert.IsTrue(hasParam1);
        Assert.IsTrue(hasParam2);
        Assert.IsTrue(hasParam3);
    }
    
    /// <summary>
    /// Returns a string array based on the given <paramref name="arguments"/> string.
    /// </summary>
    /// <remarks>This is just a call for <c>arguments.Split</c> and will ignore all escape rules (like " or ')!</remarks>
    /// <param name="arguments"></param>
    /// <returns></returns>
    private static string[] CmdLineToArgumentArray(string arguments)
    {
        return arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }
}