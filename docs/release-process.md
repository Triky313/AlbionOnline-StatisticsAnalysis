# Release Process

This project releases Windows artifacts from Git tags. Pushing a tag creates the GitHub release assets first, then opens a pull request that updates the NetSparkle appcast files. The in-app updater only sees the new version after that pull request is merged into `main`.

## Required GitHub Configuration

Create these values in `Settings` -> `Secrets and variables` -> `Actions`:

- Repository secret `SPARKLE_PRIVATE_KEY`
- Repository variable `SPARKLE_PUBLIC_KEY`

The private key signs update files and appcast files in GitHub Actions. The public key is embedded into the release build so the app can verify updates.

## Tag Modes

The workflow decides which appcast files to update from the tag name.

| Tag pattern | Appcast mode | Updated files |
| --- | --- | --- |
| `v9.4.0` | Stable release | `ao-netsparkle-update-check.xml` |
| `v9.4.0-beta.1` | Pre-release | `ao-netsparkle-pre-release-update-check.xml` |
| `v9.4.0-rc.1` | Pre-release | `ao-netsparkle-pre-release-update-check.xml` |
| `v9.4.0-alpha.1` | Pre-release | `ao-netsparkle-pre-release-update-check.xml` |
| `v9.4.0+both` | Both | Both appcast files |
| `v9.4.0-rc.1+both` | Both | Both appcast files |

The `+both` suffix is release metadata. It is used only by the workflow and is not written into the appcast version. For example, `v9.4.0+both` is published to NetSparkle as version `9.4.0`.

## Stable Release

1. Make sure `main` contains the code to release.
2. Create a stable tag:

   ```powershell
   git tag v9.4.0
   git push origin v9.4.0
   ```

3. Wait for the release workflow to finish.
4. Verify the ZIP and installer EXE in the GitHub release.
5. Review the generated appcast pull request.
6. Merge the pull request when the update should become visible in the app.

## Pre-release

Use a pre-release suffix such as `-alpha`, `-beta`, or `-rc`. Tags with other hyphen suffixes are rejected by the workflow so accidental release channels do not update an appcast.

```powershell
git tag v9.4.0-beta.1
git push origin v9.4.0-beta.1
```

The workflow marks the GitHub release as a pre-release and opens a pull request for `ao-netsparkle-pre-release-update-check.xml`.

## Both Appcasts

Use `+both` when the same artifact should be offered to stable users and pre-release users.

```powershell
git tag v9.4.0+both
git push origin v9.4.0+both
```

The workflow opens one pull request that updates both appcast files and both appcast signature files.

The release assets use the version before `+both` in their file names. For example, `v9.4.0+both` creates artifacts named like `StatisticsAnalysis-AlbionOnline-v9.4.0-windows-x64.exe` under the `v9.4.0+both` GitHub release.

## What Happens in the Workflow

1. GitHub Actions checks out the tagged commit.
2. The NetSparkle public key is embedded into `AutoUpdateSecurity.cs` for the release build only.
3. The app is published for `win-x64`.
4. A ZIP file and Inno Setup installer are created.
5. The ZIP and installer are uploaded to the GitHub release.
6. The workflow checks out `main`.
7. The installer is signed with the NetSparkle private key.
8. The appcast XML file or files are generated.
9. The appcast `.signature` file or files are generated.
10. A pull request is opened against `main`.

The GitHub release is available before the appcast pull request is merged. Merging the pull request activates the update for the in-app updater.
