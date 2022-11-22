#r "paket:
version 7.0.2
framework: net6.0
source https://api.nuget.org/v3/index.json
nuget Be.Vlaanderen.Basisregisters.Build.Pipeline 6.0.3 //"

#load "packages/Be.Vlaanderen.Basisregisters.Build.Pipeline/Content/build-generic.fsx"

open Fake
open Fake.Core
open Fake.Core.TargetOperators
open Fake.IO.FileSystemOperators
open ``Build-generic``

let product = "Basisregisters Vlaanderen"
let copyright = "Copyright (c) Vlaamse overheid"
let company = "Vlaamse overheid"

let dockerRepository = "parcel-registry"
let assemblyVersionNumber = (sprintf "2.%s")
let nugetVersionNumber = (sprintf "%s")

let buildSource = build assemblyVersionNumber
let buildTest = buildTest assemblyVersionNumber
let setVersions = (setSolutionVersions assemblyVersionNumber product copyright company)
let publishSource = publish assemblyVersionNumber
let pack = pack nugetVersionNumber
let containerize = containerize dockerRepository
let push = push dockerRepository

supportedRuntimeIdentifiers <- [ "msil"; "linux-x64" ]

// Solution -----------------------------------------------------------------------

Target.create "Restore_Solution" (fun _ -> restore "ParcelRegistry")

Target.create "Build_Solution" (fun _ ->
  setVersions "SolutionInfo.cs"
  buildSource "ParcelRegistry.Projector"
  buildSource "ParcelRegistry.Api.Legacy"
  buildSource "ParcelRegistry.Api.Oslo"
  buildSource "ParcelRegistry.Api.Extract"
  buildSource "ParcelRegistry.Api.CrabImport"
  buildSource "ParcelRegistry.Projections.Legacy"
  buildSource "ParcelRegistry.Projections.Extract"
  buildSource "ParcelRegistry.Projections.LastChangedList"
  buildSource "ParcelRegistry.Projections.Syndication"
  buildSource "ParcelRegistry.Consumer.Address"
  buildSource "ParcelRegistry.Migrator.Parcel"
  buildTest "ParcelRegistry.Tests"
)

Target.create "Test_Solution" (fun _ ->
    [
        "test" @@ "ParcelRegistry.Tests"
    ] |> List.iter testWithDotNet
)

Target.create "Publish_Solution" (fun _ ->
  [
    "ParcelRegistry.Projector"
    "ParcelRegistry.Api.Legacy"
    "ParcelRegistry.Api.Oslo"
    "ParcelRegistry.Api.Extract"
    "ParcelRegistry.Api.CrabImport"
    "ParcelRegistry.Projections.Legacy"
    "ParcelRegistry.Projections.Extract"
    "ParcelRegistry.Projections.LastChangedList"
    "ParcelRegistry.Projections.Syndication"
    "ParcelRegistry.Consumer.Address"
    "ParcelRegistry.Migrator.Parcel"
  ] |> List.iter publishSource)

Target.create "Pack_Solution" (fun _ ->
  [
    "ParcelRegistry.Projector"
    "ParcelRegistry.Api.Legacy"
    "ParcelRegistry.Api.Oslo"
    "ParcelRegistry.Api.Extract"
    "ParcelRegistry.Api.CrabImport"
    "ParcelRegistry.Migrator.Parcel"
  ] |> List.iter pack)

Target.create "Containerize_Projector" (fun _ -> containerize "ParcelRegistry.Projector" "projector")
Target.create "PushContainer_Projector" (fun _ -> push "projector")

Target.create "Containerize_ApiLegacy" (fun _ -> containerize "ParcelRegistry.Api.Legacy" "api-legacy")
Target.create "PushContainer_ApiLegacy" (fun _ -> push "api-legacy")

Target.create "Containerize_ApiOslo" (fun _ -> containerize "ParcelRegistry.Api.Oslo" "api-oslo")
Target.create "PushContainer_ApiOslo" (fun _ -> push "api-oslo")

Target.create "Containerize_ApiExtract" (fun _ -> containerize "ParcelRegistry.Api.Extract" "api-extract")
Target.create "PushContainer_ApiExtract" (fun _ -> push "api-extract")

Target.create "Containerize_ApiCrabImport" (fun _ -> containerize "ParcelRegistry.Api.CrabImport" "api-crab-import")
Target.create "PushContainer_ApiCrabImport" (fun _ -> push "api-crab-import")

Target.create "Containerize_ProjectionsSyndication" (fun _ -> containerize "ParcelRegistry.Projections.Syndication" "projections-syndication")
Target.create "PushContainer_ProjectionsSyndication" (fun _ -> push "projections-syndication")

Target.create "Containerize_ConsumerAddress" (fun _ -> containerize "ParcelRegistry.Consumer.Address" "consumer-address")
Target.create "PushContainer_ConsumerAddress" (fun _ -> push "consumer-address")

Target.create "Containerize_MigratorParcel" (fun _ -> containerize "ParcelRegistry.Migrator.Parcel" "migrator-parcel")
Target.create "PushContainer_MigratorParcel" (fun _ -> push "migrator-parcel")

// --------------------------------------------------------------------------------

Target.create "Build" ignore
Target.create "Test" ignore
Target.create "Publish" ignore
Target.create "Pack" ignore
Target.create "Containerize" ignore
Target.create "Push" ignore

"NpmInstall"
  ==> "DotNetCli"
  ==> "Clean"
  ==> "Restore_Solution"
  ==> "Build_Solution"
  ==> "Build"

"Build"
  ==> "Test_Solution"
  ==> "Test"

"Test"
  ==> "Publish_Solution"
  ==> "Publish"

"Publish"
  ==> "Pack_Solution"
  ==> "Pack"

"Pack"
  ==> "Containerize_Projector"
  ==> "Containerize_ApiLegacy"
  ==> "Containerize_ApiOslo"
  ==> "Containerize_ApiExtract"
  ==> "Containerize_ApiCrabImport"
  ==> "Containerize_ProjectionsSyndication"
  ==> "Containerize_ConsumerAddress"
  ==> "Containerize_MigratorParcel"
  ==> "Containerize"
// Possibly add more projects to containerize here

"Containerize"
  ==> "DockerLogin"
  ==> "PushContainer_Projector"
  ==> "PushContainer_ApiLegacy"
  ==> "PushContainer_ApiOslo"
  ==> "PushContainer_ApiExtract"
  ==> "PushContainer_ApiCrabImport"
  ==> "PushContainer_ProjectionsSyndication"
  ==> "PushContainer_ConsumerAddress"
  ==> "PushContainer_MigratorParcel"
  ==> "Push"
// Possibly add more projects to push here

// By default we build & test
Target.runOrDefault "Test"
