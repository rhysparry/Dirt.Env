set windows-shell := ["nu", "-c"]

version := "1.1.0"
configuration := env("BUILD_CONFIGURATION", "Release")
nugetApiKey := env("NUGET_API_KEY", "")

test_filter := if os_family() == "windows" { "Platform!=Nix" } else { "Platform!=Windows" }

restore:
    dotnet restore

build: restore
    dotnet build --no-restore --configuration {{configuration}} /p:Version={{version}} /p:AssemblyVersion={{version}}

test: build
    dotnet test --no-build --verbosity normal --configuration {{configuration}} --filter {{test_filter}}

pack: build
    dotnet pack --no-build --configuration {{configuration}} /p:PackageVersion={{version}}

clean:
    dotnet clean --configuration {{configuration}}

nuget-push: (is-not-empty nugetApiKey) pack
    @dotnet nuget push **/*.nupkg --source https://api.nuget.org/v3/index.json --api-key {{nugetApiKey}}

changelog:
    git-cliff --output CHANGELOG.md

[private]
[windows]
@is-not-empty value:
    use std assert; assert (not ("{{value}}" | is-empty))

[private]
[unix]
@is-not-empty value:
    [ -n "{{value}}" ]