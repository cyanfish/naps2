namespace NAPS2.App.Tests.Targets;

public record AppTestExe(string DefaultRootPath, string ExeSubPath, string ArgPrefix = null, string TestRootSubPath = null);