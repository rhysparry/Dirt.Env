set windows-shell := ["pwsh.exe", "-NoProfile", "-c"]

export NuGetApiKey := env("NUGET_API_KEY", "")
export Version := trim_start_match(`git-cliff --bumped-version`, "v")
build-cmd := if os_family() == "windows" { "./build.cmd" } else { "./build.sh" }
mkdir-p := if os_family() == "windows" { "New-Item -Type Directory -Force" } else { "mkdir -p" }

restore:
    {{ build-cmd }} -target Restore

build:
    {{ build-cmd }} -target Compile

test:
    {{ build-cmd }} -target Test

pack:
    {{ build-cmd }} -target Pack

clean:
    {{ build-cmd }} -target Clean

nuget-push:
    {{ build-cmd }} -target Publish

nuget-push-no-pack:
    {{ build-cmd }} -target Publish -skip Pack

[private]
artifacts-dir:
    {{ mkdir-p }} artifacts

release-notes: artifacts-dir
    git-cliff --latest --strip header --output artifacts/RELEASE-NOTES.md

changelog:
    git-cliff --output CHANGELOG.md

[windows, private]
assert-branch name="main":
    @if ((git branch --show-current) -ne "{{ name }}") { throw "Not on {{ name }} branch" }

[unix, private]
assert-branch name="main":
    [ "$(git branch --show-current)" = "{{ name }}" ]

[windows, private]
assert-no-pending-changes:
    @if ($null -ne (git status --porcelain)) { throw "There are pending changes" }

[unix, private]
assert-no-pending-changes:
    [ -z "$(git status --porcelain)" ]

release: (assert-branch "main") assert-no-pending-changes && push-release
    git pull
    git-cliff --bump --output CHANGELOG.md
    git-cliff --bump --unreleased --strip header --output artifacts/RELEASE-NOTES.md
    git add CHANGELOG.md
    git commit -m "chore(release): release {{ Version }}"
    git tag "v{{ Version }}" -F artifacts/RELEASE-NOTES.md --cleanup=whitespace

[private, confirm("Are you sure you want to push the release?")]
push-release:
    git push origin main --tags