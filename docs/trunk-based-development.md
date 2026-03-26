# Trunk-Based CI/CD

This repository now includes a trunk-based GitHub Actions setup:

- `CI` validates every pull request targeting the trunk branch (`master` today, `main` later if you rename it), direct pushes to trunk, and merge queue builds through the `merge_group` event.
- `CD` packages the application and can publish a production container image to `ghcr.io/<owner>/<repo>`.
- `Release` runs `versionize` on trunk merges, updates the version and changelog, creates a git tag, and publishes a GitHub Release.
- `Vercel Preview` deploys pull requests to Vercel preview.
- `Release` also deploys tagged production releases to Vercel after `versionize` succeeds.
- Test execution is automatic once a `*Tests.csproj` or `*.Tests.csproj` project is added to the repo. Right now the workflow logs that no tests are present and continues.

## Recommended GitHub Settings

Configure the repository like this to keep the flow truly trunk-based:

1. Use a single trunk branch. This repo is currently on `master`; the workflows also support `main` if you rename it later.
2. Protect the trunk branch and require pull requests before merging.
3. Require the `Build and validate` status check before merge.
4. Require conventional commit style in squash commit titles or PR titles so `versionize` can calculate the next version reliably.
5. Enable merge queue if you want serialized merges into trunk; the workflow already supports it through `merge_group`.
6. Prefer squash merge or rebase merge so trunk stays linear and easy to reason about.
7. Keep feature branches short-lived and merge them back quickly.

## Deployment Notes

- The production image is built from [Opti.CMS/Docker/web.release.dockerfile](../Opti.CMS/Docker/web.release.dockerfile).
- The `container` job uses the built-in `GITHUB_TOKEN`, so no extra registry secret is required for GHCR.
- If you want a manual approval before publishing the container, create a GitHub environment named `production` and add the reviewers/rules you want.
- Vercel deployments require the repository secrets `VERCEL_TOKEN`, `VERCEL_ORG_ID`, and `VERCEL_PROJECT_ID`.
- If this Vercel project is still connected to Git with automatic deployments enabled, Vercel will also deploy on its own. Disable Vercel auto deployments with `git.deploymentEnabled: false` if you want GitHub Actions to be the single deployment path.
- `versionize` is installed as a local tool in [.config/dotnet-tools.json](../.config/dotnet-tools.json) and configured in [.versionize](../.versionize).
- The release workflow pushes the version/changelog commit with a `[skip ci]` suffix so the generated release commit does not loop back through every push workflow.
- If you later deploy somewhere other than Vercel, add that step after the packaging stage in [cd.yml](../.github/workflows/cd.yml) or replace the Vercel deployment job in [release.yml](../.github/workflows/release.yml).

## What Each Workflow Does

`CI`

- Restores dependencies from `nuget.org` and the Optimizely feed.
- Builds the web app in `Release`.
- Runs tests automatically when test projects exist.
- Publishes a smoke-test artifact on trunk so the merged commit is already packageable.

`CD`

- Publishes the app output as a workflow artifact.
- Builds a runtime container image from the repository.
- Pushes immutable image tags for the branch name, `latest`, and the commit SHA.

`Release`

- Restores the local `versionize` tool.
- Bumps the `<Version>` in [Opti.CMS.csproj](../Opti.CMS/Opti.CMS.csproj) and updates `CHANGELOG.md` when trunk contains releasable commits.
- Pushes the release commit and tag back to GitHub.
- Creates a GitHub Release and deploys the tagged build to Vercel production.

`Vercel Preview`

- Builds the current pull request with the Vercel CLI.
- Publishes a preview deployment URL for review before the branch is merged.
