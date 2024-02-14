#r "paket:
version 7.0.2
framework: net6.0
source https://api.nuget.org/v3/index.json

nuget Microsoft.Build 17.3.2
nuget Microsoft.Build.Framework 17.3.2
nuget Microsoft.Build.Tasks.Core 17.3.2
nuget Microsoft.Build.Utilities.Core 17.3.2
nuget Be.Vlaanderen.Basisregisters.Build.Pipeline 6.0.6 //"

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

let buildSolution = buildSolution assemblyVersionNumber
let buildSource = build assemblyVersionNumber
let buildTest = buildTest assemblyVersionNumber
let setVersions = (setSolutionVersions assemblyVersionNumber product copyright company)
let test = testSolution
let publishSource = publish assemblyVersionNumber
let pack = pack nugetVersionNumber
let containerize = containerize dockerRepository
let push = push dockerRepository

supportedRuntimeIdentifiers <- [ "msil"; "linux-x64" ]

// Solution -----------------------------------------------------------------------

Target.create "Restore_Solution" (fun _ -> restore "ParcelRegistry")

Target.create "Build_Solution" (fun _ ->
  setVersions "SolutionInfo.cs"
  buildSolution "ParcelRegistry"
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
    "ParcelRegistry.Importer.Grb"
    "ParcelRegistry.Projections.Legacy"
    "ParcelRegistry.Projections.Extract"
    "ParcelRegistry.Projections.LastChangedList"
    "ParcelRegistry.Projections.Syndication"
    "ParcelRegistry.Projections.BackOffice"
    "ParcelRegistry.Consumer.Address.Console"
    "ParcelRegistry.Migrator.Parcel"
    "ParcelRegistry.Producer"
    "ParcelRegistry.Producer.Snapshot.Oslo"
    "ParcelRegistry.Api.BackOffice"
    "ParcelRegistry.Api.BackOffice.Abstractions"
    "ParcelRegistry.Api.BackOffice.Handlers.Lambda"
    "ParcelRegistry.Snapshot.Verifier"
  ] |> List.iter publishSource)

Target.create "Pack_Solution" (fun _ ->
  [
    "ParcelRegistry.Api.Legacy"
    "ParcelRegistry.Api.Oslo"
    "ParcelRegistry.Api.Extract"
    "ParcelRegistry.Api.CrabImport"
    "ParcelRegistry.Api.BackOffice"
    "ParcelRegistry.Api.BackOffice.Abstractions"
    "ParcelRegistry.Importer.Grb"
  ] |> List.iter pack)

Target.create "Containerize_Projector" (fun _ -> containerize "ParcelRegistry.Projector" "projector")
Target.create "Containerize_ApiLegacy" (fun _ -> containerize "ParcelRegistry.Api.Legacy" "api-legacy")
Target.create "Containerize_ApiOslo" (fun _ -> containerize "ParcelRegistry.Api.Oslo" "api-oslo")
Target.create "Containerize_ApiExtract" (fun _ -> containerize "ParcelRegistry.Api.Extract" "api-extract")
Target.create "Containerize_ApiCrabImport" (fun _ -> containerize "ParcelRegistry.Api.CrabImport" "api-crab-import")
Target.create "Containerize_ProjectionsSyndication" (fun _ -> containerize "ParcelRegistry.Projections.Syndication" "projections-syndication")
Target.create "Containerize_ProjectionsBackOffice" (fun _ -> containerize "ParcelRegistry.Projections.BackOffice" "projections-backoffice")
Target.create "Containerize_ProjectionsLastChangedList" (fun _ -> containerize "ParcelRegistry.Projections.LastChangedList.Console" "projections-last-changed-list-console")
Target.create "Containerize_ConsumerAddress" (fun _ -> containerize "ParcelRegistry.Consumer.Address.Console" "consumer-address")
Target.create "Containerize_MigratorParcel" (fun _ -> containerize "ParcelRegistry.Migrator.Parcel" "migrator-parcel")
Target.create "Containerize_ApiBackOffice" (fun _ -> containerize "ParcelRegistry.Api.BackOffice" "api-backoffice")
Target.create "Containerize_Producer" (fun _ -> containerize "ParcelRegistry.Producer" "producer")
Target.create "Containerize_ProducerSnapshotOslo" (fun _ -> containerize "ParcelRegistry.Producer.Snapshot.Oslo" "producer-snapshot-oslo")
Target.create "Containerize_ImporterGrb" (fun _ -> containerize "ParcelRegistry.Importer.Grb" "importer-grb")
Target.create "Containerize_SnapshotVerifier" (fun _ -> containerize "ParcelRegistry.Snapshot.Verifier" "snapshot-verifier")

Target.create "SetAssemblyVersions" (fun _ -> setVersions "SolutionInfo.cs")
// --------------------------------------------------------------------------------

Target.create "Build" ignore
Target.create "Test" ignore
Target.create "Publish" ignore
Target.create "Pack" ignore
Target.create "Containerize" ignore

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
  // ==> "Containerize_Projector"
  // ==> "Containerize_ApiLegacy"
  // ==> "Containerize_ApiOslo"
  // ==> "Containerize_ApiExtract"
  // ==> "Containerize_ApiCrabImport"
  // ==> "Containerize_ProjectionsSyndication"
  // ==> "Containerize_ProjectionsBackOffice"
  // ==> "Containerize_ConsumerAddress"
  // ==> "Containerize_MigratorParcel"
  // ==> "Containerize_Producer"
  // ==> "Containerize_ProducerSnapshotOslo"
  // ==> "Containerize_ApiBackOffice"
  ==> "Containerize"
// Possibly add more projects to containerize here

// By default we build & test
Target.runOrDefault "Test"
