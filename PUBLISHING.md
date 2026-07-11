# Publishing

Two independent, server-less pipelines. Both run on GitHub Actions.

| Goal | Channel | Workflow | Trigger |
|------|---------|----------|---------|
| Install the library (`dotnet add package FluentGraphQL`) | [nuget.org](https://www.nuget.org/) | `.github/workflows/release.yml` | **Run workflow** button (manual) |
| Docs + interactive playground online | GitHub Pages | `.github/workflows/pages.yml` | pushing to `main` (changes under `docs/`) |

Continuous build & test on every push/PR is already handled by `.github/workflows/ci.yml`.

## One-time setup

### NuGet
1. Create an API key on <https://www.nuget.org/account/apikeys> (scope: *Push new packages and package versions*, glob `FluentGraphQL`).
2. In the repo: **Settings → Secrets and variables → Actions → New repository secret**, name it **`NUGET_API_KEY`**, paste the key.

### GitHub Pages
1. **Settings → Pages → Build and deployment → Source: GitHub Actions**.
2. That's it — the first push touching `docs/` publishes the site to:
   `https://nayruuu.github.io/graphql-generator/`

## Versioning — automatic

The version number is **computed automatically** by [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning) — you never type one.

- The base `major.minor` lives in `version.json` (currently `2.0`).
- The **patch** is the git height (number of commits), computed at build time → `2.0.<N>`.
- On `main` it is a clean public release (`2.0.4`); on branches/PRs it is a prerelease (`2.0.4-<branch>.g<commit>`).

To move to the next **minor/major**, edit one line in `version.json` (e.g. `"version": "2.1"`) and commit — the patch counter restarts from that commit.

## Releasing to NuGet

Go to **Actions → Release (NuGet) → Run workflow**. It tests, packs (`FluentGraphQL.<auto>.nupkg` + `.snupkg` symbols) and pushes to nuget.org with `--skip-duplicate`. No tag, no version to type.

> Re-running without new commits produces the same version, which nuget.org already has — `--skip-duplicate` makes that a no-op instead of an error.

## Updating the online docs / playground

`docs/index.html` is a self-contained page (the API reference + the C# → GraphQL playground). Edit it and push to `main`; the Pages workflow redeploys automatically. No build step — it is plain HTML/CSS/JS.
