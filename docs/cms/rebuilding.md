# Rebuild a staging website from scratch

An ordinary update needs a pull and render, not deletion. A clean rebuild deliberately discards the local resource cache and generated content, then fetches the CMS again. Publishing an empty website first is an additional downtime test: it proves that the previous deployed payload is unavailable before the replacement is published.

The commands below use Local Notion **1.7.0 or later**, including its `git` settings command. Check `localnotion --version` and `localnotion git --help` before starting. An installed native executable and a Docker image are separate installations; select the updated one you intend to use.

> [!INFO]
> Use this procedure only for an authorized staging outage. Stop automatic publishers and other repository writers first. Preserve a private recovery copy outside the active folder, including configuration, custom themes and Git history. The empty publication can stop Nginx when its generated include files disappear; a timeout, proxy error or 404 can be the expected outage result. Record the observed result rather than assuming a particular status.

## 1. Preserve the configuration and retire the local folder

The examples use PowerShell 7 on the sync machine, a private `DEPLOY_REMOTE`, branch `main`, and `https://staging.example.com`. Replace these placeholders with verified staging targets. Do not reuse a production destination. Keep the recovery copy and logs outside the directory served by the website.

```powershell
$site = [IO.Path]::GetFullPath('C:\Websites\staging.example.com')
$recovery = [IO.Path]::GetFullPath('C:\WebsiteRecovery\staging.example.com-before-rebuild')
$remote = 'DEPLOY_REMOTE'
$branch = 'main'
if ($site -ne 'C:\Websites\staging.example.com') { throw 'Unexpected active path' }
if (Test-Path -LiteralPath $recovery) { throw 'Preserve the existing recovery folder' }
if (-not (Test-Path -LiteralPath (Join-Path $site '.git'))) { throw 'Expected Git repository' }
New-Item -ItemType Directory -Path (Split-Path -Parent $recovery) -Force | Out-Null
Copy-Item -LiteralPath $site -Destination $recovery -Recurse
$savedRegistry = Join-Path $recovery '.localnotion\registry.json'
$activeRegistry = Join-Path $site '.localnotion\registry.json'
if ((Get-FileHash -LiteralPath $savedRegistry).Hash -ne (Get-FileHash -LiteralPath $activeRegistry).Hash) { throw 'Registry backup differs' }
if (-not (Test-Path -LiteralPath (Join-Path $recovery '.git'))) { throw 'Git history backup missing' }
```

Verify that the private backup is complete and restorable, including any untracked files and custom theme package. Preserve site configuration such as CMS connection, paths, Nginx settings, original Git policy and repository-specific Git configuration. The registry can contain an integration key; do not print it or attach it to a report. Once recovery is confirmed, remove the exact active folder:

```powershell
if ($site -ne 'C:\Websites\staging.example.com') { throw 'Unexpected deletion target' }
Remove-Item -LiteralPath $site -Recurse -Force
```

Moving the whole folder to a private recovery location is an alternative when removal is unavailable. The active path must be absent before rebuilding, and the old cache must not be copied back. A backup stored inside the active folder does not satisfy this requirement.

## 2. Publish an empty payload and confirm the outage

Clone the private remote into the now-absent active path, remove its tracked payload, and publish that removal as a normal commit. This preserves the remote's history; it does not reset or force the receiving branch. Check the intended remote and branch before continuing.

```powershell
git clone --branch $branch $remote $site
git -C $site remote -v
git -C $site branch --show-current
git -C $site rm -r --ignore-unmatch -- .
git -C $site status --short
git -C $site commit -m 'Temporarily empty staging website'
git -C $site push origin "HEAD:$branch"
git -C $site rev-parse HEAD
```

Check the receiver's revision and worktree. Git removes tracked files, but ignored or untracked files can remain. If the test requires zero non-Git files, first preserve any needed receiver-only logs or files privately, then remove the remaining payload within the verified receiver directory. The following commands run on the example web server; the path must be the actual staging checkout, with `.git` preserved:

```bash
site=$(realpath -e -- /srv/localnotion/staging.example.com/repository)
test "$site" = /srv/localnotion/staging.example.com/repository || exit 1
test -d "$site/.git" || exit 1
git -C "$site" rev-parse HEAD
git -C "$site" status --porcelain
find "$site" -mindepth 1 -maxdepth 1 ! -name .git -print
# After checking the listed staging payload and its private recovery copy:
find "$site" -mindepth 1 -maxdepth 1 ! -name .git -exec rm -rf -- {} +
find "$site" -mindepth 1 -maxdepth 1 ! -name .git -print
```

From outside the server, make a bounded request and verify that the old website is unavailable. A nonzero curl exit code or HTTP 000 can mean a connection timeout; inspect the actual error and proxy/container state. If a CDN still serves the old page, bypass or purge the relevant cache and test the origin separately.

```powershell
curl.exe --silent --show-error --max-time 15 --output NUL --write-out '%{http_code}' 'https://staging.example.com/'
```

## 3. Start again from the empty remote

Remove the temporary local empty checkout using the same exact-path guard, then clone the empty remote again. This new checkout contains Git history but no previous local cache or generated website. Restore repository-specific Git settings and verified transport/identity configuration from the private recovery copy as needed; do not overwrite another user's global Git configuration.

```powershell
if ($site -ne 'C:\Websites\staging.example.com') { throw 'Unexpected deletion target' }
Remove-Item -LiteralPath $site -Recurse -Force
git clone --branch $branch $remote $site
```

Seed only the existing connection/configuration and reusable theme packages before opening Local Notion. Clear the resource and CMS indexes and disable both automatic Git settings. Retain the saved CMS ID, key, paths and Nginx policy in memory/file storage; no API key is passed on the command line.

```powershell
$seed = Get-Content -LiteralPath $savedRegistry -Raw | ConvertFrom-Json
$savedEnabled = $seed.git.enabled.ToString().ToLowerInvariant()
$savedPush = $seed.git.push.ToString().ToLowerInvariant()
$seed.resources = @()
$seed.cms_items = @()
$seed.git.enabled = $false
$seed.git.push = $false
New-Item -ItemType Directory -Path (Join-Path $site '.localnotion') -Force | Out-Null
$json = $seed | ConvertTo-Json -Depth 100
[IO.File]::WriteAllText((Join-Path $site '.localnotion\registry.json'), $json, [Text.UTF8Encoding]::new($true))
foreach ($relative in @('.localnotion\themes', 'themes')) {
    $source = Join-Path $recovery $relative
    if (Test-Path -LiteralPath $source) {
        Copy-Item -LiteralPath $source -Destination (Join-Path $site $relative) -Recurse
    }
}
Remove-Variable seed, json
```

Check the custom theme package's private definitions and public asset destinations; both are required when the package uses both. Do not restore the old object cache, graphs, downloaded content, generated routes, HTML, sitemap or generated Nginx mappings. The preserved registry must describe the same active path or use valid relative paths. A UTF-8 BOM retains compatibility with older executables.

## 4. Pull the entire CMS and render it again

Check that the selected executable supports the documented options and `git` command. Run Local Notion commands sequentially. The existing private registry supplies the connection; the following commands contain no key argument. Keep their logs private because content or authentication details may appear in diagnostic output.

```powershell
localnotion git --help
localnotion git --path $site
$pullLog = Join-Path $recovery 'fresh-pull.log'
$renderLog = Join-Path $recovery 'fresh-render.log'
localnotion pull --all --force --render true --fault-tolerant false --path $site *> $pullLog
if ($LASTEXITCODE -ne 0) { throw 'Fresh pull failed; inspect its private log' }
localnotion render --all --fault-tolerant false --path $site *> $renderLog
if ($LASTEXITCODE -ne 0) { throw 'Full render failed; inspect its private log' }
```

Read the complete logs locally. Confirm error totals, explain warnings and check whether resources were skipped; an exit code alone is insufficient. Verify that the fresh registry now contains the expected resources and CMS routes, and that the generated site includes its framing, documents, images, theme assets and route mappings. Inspect representative pages, section order, galleries, breadcrumbs and links at wide and narrow widths. Do not publish a partially successful pull merely because a subsequent command returned success.

## 5. Restore the Git policy and publish the replacement

Restore the original automatic Git policy only after the generated result passes review. The configuration command changes settings without making a Git commit or push. If the original policy was false/false, retain it; a rebuild is not permission to enable automatic publication permanently.

```powershell
localnotion git --path $site --enabled $savedEnabled --push $savedPush
git -C $site status --short
git -C $site add --all
git -C $site diff --cached --stat
git -C $site commit -m 'Rebuild staging website from Notion'
git -C $site push origin "HEAD:$branch"
git -C $site rev-parse HEAD
```

Review all staged files before the commit. Keep this repository private: the registry, cached Notion objects, authorization mappings and history can contain secrets. Compare the new local revision with the receiver, confirm a clean receiver worktree and verify the required files. Use a normal fast-forward push; investigate a rejected push instead of forcing it.

## 6. Validate and activate Nginx

Restore the correct serving arrangement, including its host-owned wrapper and the newly generated mappings. Use the Compose project and service defined for this website; the example Hosting setup uses service `web`. Fresh validation tests the new files even if the old container stopped during the empty publication.

```bash
sudo docker compose --project-directory /srv/localnotion/staging.example.com -p example-site config --quiet
sudo docker compose --project-directory /srv/localnotion/staging.example.com -p example-site run --rm --no-deps web nginx -t
sudo docker compose --project-directory /srv/localnotion/staging.example.com -p example-site ps
```

Compare hashes of the intended host configuration/mappings with the files visible inside the container. Adapt the paths to the actual mounts. With this guide's directory mounts, compare the host-owned wrapper with `/etc/nginx/nginx.conf`, and the generated `ln-urls.conf` and `mime.types` with their paths under `/usr/share/nginx/html/.localnotion/nginx/conf`. Inspect hashes rather than displaying authorization mapping contents.

If a mounted source directory or an individually bound file was replaced, recreate the affected website container so it receives current bindings. Start a stopped service before testing it. Otherwise validate and reload the existing service. Apply only the action required by the verified state:

```bash
# Recreate only when stopped or mount identity requires it:
sudo docker compose --project-directory /srv/localnotion/staging.example.com -p example-site up -d --force-recreate web
# Verify the running service, then reload when appropriate:
sudo docker compose --project-directory /srv/localnotion/staging.example.com -p example-site exec -T web nginx -t
sudo docker compose --project-directory /srv/localnotion/staging.example.com -p example-site exec -T web nginx -s reload
```

Recheck host/container hashes after activation and inspect the intended site's reload service or watcher and logs. A receiver commit, successful fresh validation or reload does not individually prove that the running service uses the correct files.

## 7. Verify HTTPS and record the result

Check the public homepage and representative documents, images, navigation and downloads. Confirm that private paths remain unavailable and protected pages reject unauthenticated requests. Test the actual website's routes; do not assume every site has a particular article or gallery.

```powershell
curl.exe --silent --show-error --max-time 15 --output NUL --write-out '%{http_code}' 'https://staging.example.com/'
curl.exe --silent --show-error --max-time 15 --output NUL --write-out '%{http_code}' 'https://staging.example.com/.git/HEAD'
curl.exe --silent --show-error --max-time 15 --output NUL --write-out '%{http_code}' 'https://staging.example.com/.localnotion/registry.json'
```

Record the empty-deployment revision and observed outage, fresh pull/render commands and exit/error summaries, replacement revision, receiver state, Nginx/mount checks and public results. Include sanitized commands and paths; omit API keys, credentials, raw private configuration and sensitive log lines. Resume scheduled sync only after the replacement is verified and the original Git policy is restored.
