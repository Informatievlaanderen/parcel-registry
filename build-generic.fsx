#I "packages/Newtonsoft.Json/lib/net45"
#r "Newtonsoft.Json.dll"

open Fake.Core
open Fake.Core.TargetOperators
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.DotNet
open Fake.JavaScript

open System
open System.IO
open Newtonsoft.Json.Linq

Target.initEnvironment()

let currentDirectory = Directory.GetCurrentDirectory()
let buildNumber = Environment.environVarOrDefault "CI_BUILD_NUMBER" "0.0.0"
let gitHash = Environment.environVarOrDefault "GIT_COMMIT" ""
let buildDir = Environment.environVarOrDefault "BUILD_STAGINGDIRECTORY" (currentDirectory @@ "dist")
let dockerRegistry = Environment.environVarOrDefault "BUILD_DOCKER_REGISTRY" "dev.local"

let mutable supportedRuntimeIdentifiers = [ "linux-x64"; "win-x64" ]
let mutable customDotnetExePath : Option<string> = None

let getDotnetExePath defaultPath : string =
  match customDotnetExePath with
  | None -> defaultPath
  | Some dotnetExePath -> dotnetExePath

let getDotNetClrVersionFromGlobalJson() : string =
    if not (File.Exists "global.json") then
        failwithf "global.json not found"
    try
        let content = File.ReadAllText "global.json"
        let json = JObject.Parse content
        let sdk = json.Item("clr") :?> JObject
        let version = sdk.Property("version").Value.ToString()
        version
    with
    | exn -> failwithf "Could not parse global.json: %s" exn.Message

let determineInstalledFxVersions () : string list =
  printfn "Determining CLR Versions using %s" (getDotnetExePath "dotnet")

  let clrVersions =
    try
      let dotnetCommand = getDotnetExePath "dotnet"

      ["--list-runtimes"]
      |> CreateProcess.fromRawCommand dotnetCommand
      |> CreateProcess.withWorkingDirectory Environment.CurrentDirectory
      |> CreateProcess.withTimeout (TimeSpan.FromMinutes 30.)
      |> CreateProcess.redirectOutput
      |> Proc.run
      |> fun output -> output.Result.Output.Split([| Environment.NewLine |], StringSplitOptions.None)
      |> Seq.filter (fun line -> line.Contains("Microsoft.NETCore.App"))
      |> Seq.map (fun line -> line.Split([| " " |], StringSplitOptions.None).[1].Trim())
      |> Seq.sortDescending
      |> Seq.toList
    with
      | _ -> []

  printfn "Determined CLR Versions: %A" clrVersions
  clrVersions

let determineInstalledSdkVersions () : string list =
  printfn "Determining SDK Versions using %s" (getDotnetExePath "dotnet")

  let sdkVersions =
    try
      let dotnetCommand = getDotnetExePath "dotnet"

      ["--list-sdks"]
      |> CreateProcess.fromRawCommand dotnetCommand
      |> CreateProcess.withWorkingDirectory Environment.CurrentDirectory
      |> CreateProcess.withTimeout (TimeSpan.FromMinutes 30.)
      |> CreateProcess.redirectOutput
      |> Proc.run
      |> fun output -> output.Result.Output.Split([| Environment.NewLine |], StringSplitOptions.None)
      |> Seq.map (fun line -> line.Split([| " " |], StringSplitOptions.None).[0].Trim())
      |> Seq.sortDescending
      |> Seq.toList
    with
      | _ -> []

  printfn "Determined SDK Versions: %A" sdkVersions
  sdkVersions

let determineInstalledFxVersion () =
  printfn "Determining CLR Version using %s" (getDotnetExePath "dotnet")

  let clrVersion =
    try
      let dotnetCommand = getDotnetExePath "dotnet"

      ["--list-runtimes"]
      |> CreateProcess.fromRawCommand dotnetCommand
      |> CreateProcess.withWorkingDirectory Environment.CurrentDirectory
      |> CreateProcess.withTimeout (TimeSpan.FromMinutes 30.)
      |> CreateProcess.redirectOutput
      |> Proc.run
      |> fun output -> output.Result.Output.Split([| Environment.NewLine |], StringSplitOptions.None)
      |> Seq.filter (fun line -> line.Contains("Microsoft.NETCore.App"))
      |> Seq.map (fun line -> line.Split([| " " |], StringSplitOptions.None).[1].Trim())
      |> Seq.sortDescending
      |> Seq.head
    with
      | _ -> "0.0.0"

  printfn "Determined CLR Version: %s" clrVersion
  clrVersion

let determineInstalledSdkVersion () =
  printfn "Determining SDK Version using %s" (getDotnetExePath "dotnet")

  let sdkVersion =
    try
      let dotnetCommand = getDotnetExePath "dotnet"

      ["--list-sdks"]
      |> CreateProcess.fromRawCommand dotnetCommand
      |> CreateProcess.withWorkingDirectory Environment.CurrentDirectory
      |> CreateProcess.withTimeout (TimeSpan.FromMinutes 30.)
      |> CreateProcess.redirectOutput
      |> Proc.run
      |> fun output -> output.Result.Output.Split([| Environment.NewLine |], StringSplitOptions.None)
      |> Seq.map (fun line -> line.Split([| " " |], StringSplitOptions.None).[0].Trim())
      |> Seq.sortDescending
      |> Seq.head
    with
      | _ -> "0.0.0"

  printfn "Determined SDK Version: %s" sdkVersion
  sdkVersion

let setCommonOptions (dotnet: DotNet.Options) =
  { dotnet with 
        DotNetCliPath = getDotnetExePath dotnet.DotNetCliPath        
  }

let setCommonOptionsTest (dotnet: DotNet.Options) =
  { dotnet with 
        DotNetCliPath = getDotnetExePath dotnet.DotNetCliPath
        Verbosity = Some DotNet.Verbosity.Detailed
  }

let merge a b =
  a @ b |> List.distinct

let addVersionArguments version args =
  let versionArgs =
    [
      "AssemblyVersion", version
      "FileVersion", version
      "InformationalVersion", version
      "PackageVersion", version
    ]
  merge args versionArgs

let addRuntimeFrameworkVersion args =
  let fxVersion = getDotNetClrVersionFromGlobalJson()
  let runtimeFrameworkVersionArgs = ["RuntimeFrameworkVersion", fxVersion]
  merge args runtimeFrameworkVersionArgs

let addReadyToRun readyToRun args =
  let readyToRunArgs =
    [
      "PublishReadyToRun", readyToRun.ToString()
    ]
  merge args readyToRunArgs

let testWithDotNet path =
  let setMsBuildParams (msbuild: MSBuild.CliArguments) =
    { msbuild with Properties = List.empty |> addRuntimeFrameworkVersion }

  DotNet.test (fun p ->
  { p with
      Common = setCommonOptionsTest p.Common
      Configuration = DotNet.Release
      NoBuild = true
      NoRestore = true
      Logger = Some "trx"
      Diag = Some "diag.txt"
      MSBuildParams = setMsBuildParams p.MSBuildParams
  }) path

let test project =
  testWithDotNet ("test" @@ project @@ (sprintf "%s.csproj" project))

let testSolution sln =
  testWithDotNet (sprintf "%s.sln" sln)

let setSolutionVersions formatAssemblyVersion product copyright company x =
  AssemblyInfoFile.createCSharp x
      [AssemblyInfo.Version (formatAssemblyVersion buildNumber)
       AssemblyInfo.FileVersion (formatAssemblyVersion buildNumber)
       AssemblyInfo.InformationalVersion gitHash
       AssemblyInfo.Product product
       AssemblyInfo.Copyright copyright
       AssemblyInfo.Company company]

let buildNeutral formatAssemblyVersion x =
  let setMsBuildParams (msbuild: MSBuild.CliArguments) readyToRun =
    { msbuild with Properties = List.empty |> addRuntimeFrameworkVersion |> addReadyToRun readyToRun |> addVersionArguments (formatAssemblyVersion buildNumber) }

  DotNet.build (fun p ->
  { p with
      Common = setCommonOptions p.Common
      Configuration = DotNet.Release
      NoRestore = true
      MSBuildParams = (setMsBuildParams p.MSBuildParams false)
  }) x

  for runtimeIdentifier in supportedRuntimeIdentifiers do
    let rid =
      match runtimeIdentifier with
      | "linux-x64" -> Some "linux-x64"
      | "win-x64" -> Some "win-x64"
      | "msil" -> None
      | _ -> failwithf "RuntimeIdentifier %s is not supported" runtimeIdentifier

    let readyToRun =
      match runtimeIdentifier with
      | "linux-x64" -> Environment.isLinux
      | "win-x64" -> Environment.isWindows
      | "msil" -> false
      | _ -> failwithf "RuntimeIdentifier %s is not supported" runtimeIdentifier

    DotNet.build (fun p ->
    { p with
        Common = setCommonOptions p.Common
        Configuration = DotNet.Release
        NoRestore = true
        Runtime = rid
        MSBuildParams = (setMsBuildParams p.MSBuildParams readyToRun)
    }) x

let build formatAssemblyVersion project =
  buildNeutral formatAssemblyVersion ("src" @@ project @@ (sprintf "%s.csproj" project))

let buildTest formatAssemblyVersion project =
  buildNeutral formatAssemblyVersion ("test" @@ project @@ (sprintf "%s.csproj" project))

let buildSolution formatAssemblyVersion sln =
  buildNeutral formatAssemblyVersion (sprintf "%s.sln" sln)

let publish formatAssemblyVersion project =
  let setMsBuildParams (msbuild: MSBuild.CliArguments) readyToRun =
    { msbuild with Properties = List.empty |> addRuntimeFrameworkVersion |> addReadyToRun readyToRun |> addVersionArguments (formatAssemblyVersion buildNumber) }

  for runtimeIdentifier in supportedRuntimeIdentifiers do
    let rid =
      match runtimeIdentifier with
      | "linux-x64" -> Some "linux-x64"
      | "win-x64" -> Some "win-x64"
      | "msil" -> None
      | _ -> failwithf "RuntimeIdentifier %s is not supported" runtimeIdentifier

    let readyToRun =
      match runtimeIdentifier with
      | "linux-x64" -> Environment.isLinux
      | "win-x64" -> Environment.isWindows
      | "msil" -> false
      | _ -> failwithf "RuntimeIdentifier %s is not supported" runtimeIdentifier

    let outputDirectory =
      match runtimeIdentifier with
      | "linux-x64" -> "linux"
      | "win-x64" -> "win"
      | "msil" -> "msil"
      | _ -> failwithf "RuntimeIdentifier %s is not supported" runtimeIdentifier

    let selfContained =
      match runtimeIdentifier with
      | "linux-x64" -> Some true
      | "win-x64" -> Some true
      | "msil" -> None
      | _ -> failwithf "RuntimeIdentifier %s is not supported" runtimeIdentifier

    DotNet.publish (fun p ->
    { p with
        Common = setCommonOptions p.Common
        Configuration = DotNet.Release
        NoBuild = true
        NoRestore = true
        Runtime = rid
        SelfContained = selfContained
        OutputPath = Some (buildDir @@ project @@ outputDirectory)
        MSBuildParams = (setMsBuildParams p.MSBuildParams readyToRun)
    }) ("src" @@ project @@ (sprintf "%s.csproj" project))

// TODO: Refactor publishSolution to work with msil rid as well
let publishSolution formatAssemblyVersion sln =
  let setMsBuildParams (msbuild: MSBuild.CliArguments) runtimeIdentifier publishDir readyToRun =
    { msbuild with
        MaxCpuCount = Some (Some 1)
        Targets = ["Publish"]
        Properties = [
          "NoBuild", "true"
          "SelfContained", "true"
          "configuration", "Release"
          "RuntimeIdentifier", runtimeIdentifier
          "PublishDir", publishDir
        ] |> addRuntimeFrameworkVersion |> addReadyToRun readyToRun |> addVersionArguments (formatAssemblyVersion buildNumber)
    }

  for runtimeIdentifier in supportedRuntimeIdentifiers do
    let readyToRun =
      match runtimeIdentifier with
      | "linux-x64" -> Environment.isLinux
      | "win-x64" -> Environment.isWindows
      | _ -> failwithf "RuntimeIdentifier %s is not supported" runtimeIdentifier

    let outputDirectory =
      match runtimeIdentifier with
      | "linux-x64" -> "linux"
      | "win-x64" -> "win"
      | _ -> failwithf "RuntimeIdentifier %s is not supported" runtimeIdentifier

    DotNet.msbuild (fun p ->
    { p with
        Common = setCommonOptions p.Common
        MSBuildParams = (setMsBuildParams p.MSBuildParams runtimeIdentifier (buildDir @@ sln @@ outputDirectory) readyToRun)
    }) (sprintf "%s.sln" sln)

let containerize dockerRepository project containerName =
  let result1 =
    [ "build"; "--no-cache"; "--tag"; sprintf "%s/%s/%s:%s" dockerRegistry dockerRepository containerName buildNumber; "."]
    |> CreateProcess.fromRawCommand "docker"
    |> CreateProcess.withWorkingDirectory (buildDir @@ project @@ "linux")
    |> CreateProcess.withTimeout (TimeSpan.FromMinutes 5.)
    |> Proc.run

  if result1.ExitCode <> 0 then failwith "Failed result from Docker Build"

  let result2 =
    [ "tag"; sprintf "%s/%s/%s:%s" dockerRegistry dockerRepository containerName buildNumber; sprintf "%s/%s/%s:latest" dockerRegistry dockerRepository containerName]
    |> CreateProcess.fromRawCommand "docker"
    |> CreateProcess.withTimeout (TimeSpan.FromMinutes 5.)
    |> Proc.run

  if result2.ExitCode <> 0 then failwith "Failed result from Docker Tag"

let push dockerRepository containerName =
  let result1 =
    [ "push"; sprintf "%s/%s/%s:%s" dockerRegistry dockerRepository containerName buildNumber]
    |> CreateProcess.fromRawCommand "docker"
    |> CreateProcess.withTimeout (TimeSpan.FromMinutes 5.)
    |> Proc.run

  if result1.ExitCode <> 0 then failwith "Failed result from Docker Push"

  let result2 =
    [ "push"; sprintf "%s/%s/%s:latest" dockerRegistry dockerRepository containerName]
    |> CreateProcess.fromRawCommand "docker"
    |> CreateProcess.withTimeout (TimeSpan.FromMinutes 5.)
    |> Proc.run

  if result2.ExitCode <> 0 then failwith "Failed result from Docker Push Latest"

let pack formatNugetVersion project =
  let nugetVersion = formatNugetVersion buildNumber
  if List.contains "msil" supportedRuntimeIdentifiers then
    Paket.pack(fun p ->
      { p with
          ToolType = ToolType.CreateLocalTool()
          BuildConfig = "Release"
          OutputPath = buildDir @@ "nuget"
          Version = nugetVersion
          WorkingDir = buildDir @@ project @@ "msil"
          TemplateFile = buildDir @@ project @@ "msil" @@ "paket.template"
      }
    )
  else
    Paket.pack(fun p ->
      { p with
          ToolType = ToolType.CreateLocalTool()
          BuildConfig = "Release"
          OutputPath = buildDir @@ "nuget"
          Version = nugetVersion
          WorkingDir = buildDir @@ project @@ "linux"
          TemplateFile = buildDir @@ project @@ "linux" @@ "paket.template"
      }
    )

let packSolution formatNugetVersion sln =
  let nugetVersion = formatNugetVersion buildNumber
  Paket.pack(fun p ->
    { p with
        ToolType = ToolType.CreateLocalTool()
        BuildConfig = "Release"
        OutputPath = buildDir @@ sln
        Version = nugetVersion
    }
  )

Target.create "DotNetCli" (fun _ ->
  let desiredFxVersion = getDotNetClrVersionFromGlobalJson()
  let installedFxVersions = determineInstalledFxVersions()
  let desiredSdkVersion = DotNet.getSDKVersionFromGlobalJson()
  let installedSdkVersions = determineInstalledSdkVersions()

  if not (List.contains desiredSdkVersion installedSdkVersions) then
    printfn "Invalid SDK Version, Desired: %s Installed: %A" desiredSdkVersion installedSdkVersions

    let install = DotNet.install (fun p ->
      { p with
          Version = DotNet.getSDKVersionFromGlobalJson() |> DotNet.Version
      })

    let installOptions = DotNet.Options.Create() |> install

    customDotnetExePath <- Some installOptions.DotNetCliPath
    printfn "Custom SDK Path: %s" customDotnetExePath.Value
  else
    printfn "Desired SDK Version %s found" desiredSdkVersion

  if not (List.contains desiredFxVersion installedFxVersions) then
    failwithf "Invalid CLR Version, Desired: %s Installed: %A" desiredFxVersion installedFxVersions
  else
    printfn "Desired CLR Version %s found" desiredFxVersion
)

Target.create "Clean" (fun _ -> Shell.cleanDir buildDir)

let restore sln =
  let fxVersion = getDotNetClrVersionFromGlobalJson()
  let dotnetCommand = getDotnetExePath "dotnet"

  let restore =
    ["restore"; (sprintf "-p:RuntimeFrameworkVersion=%s" fxVersion); (sprintf "%s.sln" sln)]
    |> CreateProcess.fromRawCommand dotnetCommand
    |> CreateProcess.withWorkingDirectory Environment.CurrentDirectory
    |> CreateProcess.withTimeout (TimeSpan.FromMinutes 30.)
    |> CreateProcess.redirectOutput
    |> Proc.run

  if restore.ExitCode <> 0 then failwith "Failed result from SLN Restore"

Target.create "Restore" (fun _ ->
  let fxVersion = getDotNetClrVersionFromGlobalJson()
  let dotnetCommand = getDotnetExePath "dotnet"

  let restore =
    ["restore"; (sprintf "-p:RuntimeFrameworkVersion=%s" fxVersion)]
    |> CreateProcess.fromRawCommand dotnetCommand
    |> CreateProcess.withWorkingDirectory Environment.CurrentDirectory
    |> CreateProcess.withTimeout (TimeSpan.FromMinutes 30.)
    |> CreateProcess.redirectOutput
    |> Proc.run

  if restore.ExitCode <> 0 then failwith "Failed result from Restore"
)

Target.create "NpmInstall" (fun _ ->
  Npm.install |> ignore
)

Target.create "DockerLogin" (fun _ ->
  let dockerLogin =
    [ "packages/Be.Vlaanderen.Basisregisters.Build.Pipeline/Content/ci-docker-login.sh"]
    |> CreateProcess.fromRawCommand "bash"
    |> CreateProcess.withTimeout (TimeSpan.FromMinutes 5.)
    |> Proc.run

  if dockerLogin.ExitCode <> 0 then failwith "Failed result from Docker Login"
)
