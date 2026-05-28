# Release Process

This project releases Windows artifacts from Git tags. Pushing a tag creates the GitHub release assets first, then opens a pull request that updates the NetSparkle appcast files. The in-app updater only sees the new version after that pull request is merged into `main`.

## Required GitHub Configuration

Create these values in `Settings` -> `Secrets and variables` -> `Actions`:

- Repository secret `SPARKLE_PRIVATE_KEY`
- Repository secret `SPARKLE_PUBLIC_KEY`

The private key signs update files and appcast files in GitHub Actions. The public key is embedded into the release build through MSBuild so the app can verify updates.

`SPARKLE_PUBLIC_KEY` can also be provided as a repository variable because it is not secret material, but the workflow also accepts the existing repository secret.

When Authenticode signing is added, also provide the expected publisher certificate thumbprint as `AUTHENTICODE_PUBLISHER_THUMBPRINT`. Until that value is configured, the integrated updater relies on NetSparkle Ed25519 verification only. Manual user downloads from the GitHub release are not blocked by the app.

## Local Release Publishes

Local `Release` publishes also need the NetSparkle public key. Without it, the app cannot verify signed appcasts and would start with unsafe update verification, so the project fails the publish instead of producing a broken updater build.

Use one of these options before publishing locally:

```powershell
$env:SPARKLE_PUBLIC_KEY = "<public key>"
dotnet publish src\StatisticsAnalysisTool\StatisticsAnalysisTool.csproj -c Release -p:Platform=x64
```

or pass the key directly:

```powershell
dotnet publish src\StatisticsAnalysisTool\StatisticsAnalysisTool.csproj -c Release -p:Platform=x64 -p:NetSparkleEd25519PublicKey="<public key>"
```

The private key is only needed by GitHub Actions for signing appcasts and download assets. Do not store or commit the private key locally.

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

Installed pre-release builds automatically check the pre-release appcast, even if the user has not enabled pre-release suggestions. This keeps beta, alpha, and rc builds on the channel that contains their signed appcast metadata.

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
2. The NetSparkle public key is embedded into the published application through the `NetSparkleEd25519PublicKey` MSBuild property.
3. The app is published for `win-x64`.
4. A ZIP file and Inno Setup installer are created.
5. The ZIP and installer are uploaded to the GitHub release.
6. The workflow checks out `main`.
7. The installer is signed with the NetSparkle private key.
8. The appcast XML file or files are generated with real file length values and Ed25519 download signatures.
9. The appcast `.signature` file or files are generated.
10. A pull request is opened against `main`.

The GitHub release is available before the appcast pull request is merged. Merging the pull request activates the update for the in-app updater.

## Updater Verification

Release builds require an embedded NetSparkle public key. If the key is missing, the release build fails before publishing artifacts.

The integrated updater verifies the appcast signature before offering an update and requires a download signature on the selected update item. Invalid or missing updater signatures prevent the in-app update prompt, but users can still manually download and install ZIP or installer assets from GitHub.

If `AUTHENTICODE_PUBLISHER_THUMBPRINT` is configured in a future Authenticode signing setup, the integrated updater also verifies the downloaded installer before starting it. If the Authenticode signature is invalid or signed by a different certificate, the integrated updater does not start the installer. This check is intentionally scoped to the integrated updater and does not block manual installation.
