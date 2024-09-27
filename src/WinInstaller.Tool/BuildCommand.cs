﻿using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using SharpDevLib;
using WinInstaller.Tool.Extensions;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Text;

namespace WinInstaller.Tool;

/// <summary>
/// build command
/// </summary>
[Command]
public class BuildCommand : ICommand
{
    /// <summary>
    /// app name
    /// </summary>
    [CommandOption("name", 'n', IsRequired = false, Description = "应用名称,默认为【entry-point】的文件名")]
    public string Name { get; set; }

    /// <summary>
    /// output file path
    /// </summary>
    [CommandOption("output", 'o', IsRequired = false, Description = "输出文件路径,默认为【binary-folder】上级目录")]
    public string Output { get; set; }

    /// <summary>
    /// app version
    /// </summary>
    [CommandOption("app-version", 'v', IsRequired = false, Description = "应用版本,默认为1.0.0.0")]
    public string AppVersion { get; set; }

    /// <summary>
    /// app binary folder
    /// </summary>
    [CommandOption("binary-folder", 'b', IsRequired = true, Description = "应用目录")]
    public string BinaryFolder { get; set; }

    /// <summary>
    /// app entrypoint file path
    /// </summary>
    [CommandOption("entry-point", 'e', IsRequired = true, Description = "可执行文件路径,【binary-folder】的相对地址")]
    public string EntrypointPath { get; set; }

    /// <summary>
    /// icon file path
    /// </summary>
    [CommandOption("icon", 'i', IsRequired = false, Description = "图标路径,如果为空将尝试从【entry-point】中提取,如果提取失败则用默认图标")]
    public string IconPath { get; set; }

    /// <summary>
    /// ovveride tostring
    /// </summary>
    /// <returns>string</returns>
    public override string ToString()
    {
        var builder = new StringBuilder();
        var properties = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        foreach (var property in properties)
        {
            builder.AppendLine($"{property.Name}={property.GetValue(this)}");
        }
        return builder.ToString();
    }

    /// <summary>
    /// build
    /// </summary>
    /// <param name="console">console interface</param>
    /// <returns>task</returns>
    public async ValueTask ExecuteAsync(IConsole console)
    {
        try
        {
            if (!EnsureParameterCorrect(console)) return;
            var tempFolder = PrepareProject(console);
            PrepareBinaryFiles(console, tempFolder);
            BuildProject(console, tempFolder);
            CreateSingleExe(console, tempFolder);
        }
        catch (Exception ex)
        {
            console.WriteError(ex.Message);
            Environment.Exit(0);
        }
        await Task.CompletedTask;
    }

    bool EnsureParameterCorrect(IConsole console)
    {

        if (!Directory.Exists(BinaryFolder))
        {
            console.WriteError($"directory '{BinaryFolder}' not found");
            return false;
        }

        var entrypointPath = Path.Combine(BinaryFolder, EntrypointPath);
        if (!File.Exists(entrypointPath))
        {
            console.WriteError($"file '{entrypointPath}' not found");
            return false;
        }

        if (Name.IsNullOrWhiteSpace()) Name = entrypointPath.GetFileName(false);
        if (Output.IsNullOrWhiteSpace()) Output = new DirectoryInfo(BinaryFolder).Parent.FullName;
        if (AppVersion.IsNullOrWhiteSpace()) AppVersion = "1.0.0.0";

        return true;
    }

    string PrepareProject(IConsole console)
    {
        var tempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"WinInstaller\\{Name}\\{AppVersion}");
        tempFolder.CreateDirectoryIfNotExist();
        CopyProjectFiles(console, tempFolder);
        ReplaceProjectInformation(tempFolder);
        return tempFolder;
    }

    static void CopyProjectFiles(IConsole console, string tempFolder)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var names = assembly.GetManifestResourceNames().ToList();

        foreach (var name in names)
        {
            console.WriteInformation($"准备文件:{name}");
            var stream = assembly.GetManifestResourceStream(name);
            if (stream != null)
            {
                var path = tempFolder.CombinePath(name.TrimStart("..\\WinInstaller\\"));
                new FileInfo(path).DirectoryName.CreateDirectoryIfNotExist();
                stream.SaveToFile(path);
                stream.Dispose();
            }
        }
    }

    void ReplaceProjectInformation(string tempFolder)
    {
        var appXamlCs = tempFolder.CombinePath("App.xaml.cs");
        var appXamlText = File.ReadAllText(appXamlCs);
        appXamlText = appXamlText.Replace("Name = \"SampleApp\"", $"Name = \"{Name}\"");
        appXamlText = appXamlText.Replace("Version = \"1.0.0\"", $"Version = \"{AppVersion}\"");
        appXamlText = appXamlText.Replace("EntryPoint = \"SampleApp.exe\"", $"EntryPoint = \"{EntrypointPath}\"");
        File.WriteAllText(appXamlCs, appXamlText);

        var csproj = tempFolder.CombinePath("WinInstaller.csproj");
        var csprojText = File.ReadAllText(csproj);
        csprojText = csprojText.Replace("<AssemblyName>win-installer</AssemblyName>", $"<AssemblyName>{Name}</AssemblyName>");
        File.WriteAllText(csproj, csprojText);
    }

    void PrepareBinaryFiles(IConsole console, string tempFolder)
    {
        var sourceZipFile = tempFolder.CombinePath("Source.zip");
        if (File.Exists(sourceZipFile)) File.Delete(sourceZipFile);
        ZipFile.CreateFromDirectory(BinaryFolder, sourceZipFile);

        //icon
        var targetIconPath = tempFolder.CombinePath("favicon.ico");
        if (File.Exists(IconPath))
        {
            File.Copy(IconPath, targetIconPath, true);
        }
        else
        {
            try
            {
                IconExtension.SaveIcon(BinaryFolder.CombinePath(EntrypointPath), targetIconPath);
            }
            catch (Exception ex)
            {
                console.WriteWarning($"尝试从EntryPoint中获取Icon失败:{ex.Message}");
                console.WriteInformation("使用默认图标");
            }
        }
    }

    static void BuildProject(IConsole console, string tempFolder)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo("dotnet", $"build -c Release {tempFolder}")
        };
        process.OutputDataReceived += (s, e) => console.WriteInformation(e.Data);
        process.Start();
        process.WaitForExit();
    }

    void CreateSingleExe(IConsole console, string tempFolder)
    {
        EnsureSingleExeToolInstalled(console);

        var process = new Process
        {
            StartInfo = new ProcessStartInfo("single-exe", $"-b {tempFolder.CombinePath($"bin\\Release\\net472")} -e {Name}.exe -o {Output} -n {Name}")
        };
        process.OutputDataReceived += (s, e) => console.WriteInformation(e.Data);
        process.Start();
        process.WaitForExit();
    }

    static void EnsureSingleExeToolInstalled(IConsole console)
    {
        var toolList = new List<string>();
        var process = new Process
        {
            StartInfo = new ProcessStartInfo("dotnet", "tool list -g")
        };
        process.OutputDataReceived += (s, e) => toolList.Add(e.Data);
        process.Start();
        process.WaitForExit();

        if (toolList.Any(x => x.StartsWith("single-exe ")))
        {
            process = new Process
            {
                StartInfo = new ProcessStartInfo("dotnet", "tool update -g single-exe")
            };
            process.OutputDataReceived += (s, e) => console.WriteInformation(e.Data);
            process.Start();
            process.WaitForExit();
            if (process.ExitCode != 0) throw new Exception("安装single-exe工具失败");
        }
        else
        {
            process = new Process
            {
                StartInfo = new ProcessStartInfo("dotnet", "tool install -g single-exe")
            };
            process.OutputDataReceived += (s, e) => console.WriteInformation(e.Data);
            process.Start();
            process.WaitForExit();
            if (process.ExitCode != 0) throw new Exception("安装single-exe工具失败");
        }
    }
}