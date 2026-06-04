# GitHub Actions — Required Secrets

Both CI workflows require three Unity license secrets to be set in the repository.

## How to add secrets

GitHub repo → **Settings** → **Secrets and variables** → **Actions** → **New repository secret**

---

## Required secrets

| Secret name | Value |
|---|---|
| `UNITY_LICENSE` | Contents of your Unity `.ulf` license file (see below) |
| `UNITY_EMAIL` | Email address of your Unity account |
| `UNITY_PASSWORD` | Password of your Unity account |

---

## How to get your Unity license file

### Option A — Personal license (free Unity plan)

1. Install Unity 2022.3 LTS locally
2. Activate a personal license: **Unity Hub → Preferences → Licenses → Add**
3. Find the license file:
   - **Windows:** `C:\ProgramData\Unity\Unity_lic.ulf`
   - **macOS:** `/Library/Application Support/Unity/Unity_lic.ulf`
   - **Linux:** `~/.local/share/unity3d/Unity/Unity_lic.ulf`
4. Open the `.ulf` file in a text editor, copy the **entire contents**
5. Paste as the value for secret `UNITY_LICENSE`

### Option B — Use the GameCI activation workflow

GameCI provides an automated activation workflow. Run it once to generate the license:

```
https://game.ci/docs/github/activation
```

---

## Workflow summary

| Workflow | Trigger | Output |
|---|---|---|
| `build-android.yml` | Push to `main`, PRs, manual | `.apk` artifact (14-day retention) |
| `build-webgl.yml` | Push to `main`, manual | WebGL artifact + GitHub Pages deploy |

## Estimated build times

| Platform | Cold (no cache) | Warm (with cache) |
|---|---|---|
| Android APK | ~25–40 min | ~10–15 min |
| WebGL | ~20–35 min | ~8–12 min |
