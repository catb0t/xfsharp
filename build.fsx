// Include Fake library
#r "./packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.Testing.NUnit3

// Directories
let buildDir  = "./build/"
let sourceDir  = "./exercises/"

// Files
let solutionFile = buildDir @@ "/exercises.fsproj"
let compiledOutput = buildDir @@ "xfsharp.dll"

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir]
)

Target "Copy" (fun _ -> CopyDir buildDir sourceDir allFiles)

Target "BuildUnchanged" (fun _ ->
    MSBuildRelease buildDir "Build" [solutionFile]
    |> Log "AppBuild-Output: "
)

Target "PrepareTests" (fun _ ->
    let ignorePattern = "(\[<Ignore\(\"Remove to run test\"\)>]|, Ignore = \"Remove to run test case\")"

    !! (buildDir @@ "**/*Test?.fs")
    |> RegexReplaceInFilesWithEncoding ignorePattern "" System.Text.Encoding.UTF8
)

Target "BuildWithAllTests" (fun _ ->
    MSBuildRelease buildDir "Build" [solutionFile]
    |> Log "AppBuild-Output: "
)

Target "Test" (fun _ ->
    if getEnvironmentVarAsBool "APPVEYOR" then
        [compiledOutput]
        |> NUnit3 (fun p -> { p with 
                                ShadowCopy = false
                                ToolPath = @"C:\Tools\NUnit3\bin\nunit3-console.exe"
                                ResultSpecs = ["myresults.xml;format=AppVeyor"] })
    else
        [compiledOutput]
        |> NUnit3 (fun p -> { p with ShadowCopy = false })
)

Target "CleanTest" (fun _ ->
    DeleteFiles ["iliad.txt"; "midsummer-night.txt"; "paradise-lost.txt"]
)

// Build order
"Clean" 
  ==> "Copy"
  ==> "BuildUnchanged"
  ==> "PrepareTests"
  ==> "BuildWithAllTests"    
  ==> "Test"
  ==> "CleanTest"

// start build
RunTargetOrDefault "CleanTest"