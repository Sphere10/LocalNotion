# Git configuration

Local Notion can stage and commit repository changes automatically after supported content operations. It can also push those commits using the repository's configured Git remote and upstream. These behaviors are controlled by two settings in **`.localnotion/registry.json`**:

| Setting | Effect |
| --- | --- |
| `git.enabled` | Enables automatic Git staging and commits during supported Local Notion operations. |
| `git.push` | Also pushes after an automatic commit, when `git.enabled` is true. |

Both settings default to `false`. A stored `push=true` has no effect while `enabled=false`; disabling tracking retains the push preference unless you change it too. Pull, render and sync can therefore commit or publish when these settings are enabled. Automatic staging includes changes throughout the selected repository, so keep unrelated work elsewhere.

## Command availability

The `git` configuration command is available from Local Notion **1.7.0** in native builds and Docker. Earlier releases, including `v1.6.3` (version `1.6.3.7`) and `1.5.0.12`, do not include it. Check the executable you actually run with:

```sh
localnotion git --help
```

If it reports an unknown command, update to version 1.7.0 or later. For Docker, pull the updated image and select it in the launcher or service configuration; an existing installation does not update automatically. See [native downloads](../README.md#native-downloads) and [Docker updates](../docker/README.md#backups-and-updates).

## View the settings

```sh
localnotion git --path "./website"
```

With no setting options, the command displays only the supported Git settings and leaves the registry unchanged:

```json
{"git":{"enabled":false,"push":false}}
```

The current directory is used when `--path` is omitted. Select the existing repository folder, not the registry file. The command opens the repository through the same load lifecycle as other repository commands. Opening can restore missing themes, initialize repository logs and stores, and complete an interrupted registry save. It does not render content, initialize Git, stage files, commit, push or contact Notion, and does not require Git or Notion credentials.

## Change the settings

Pass explicit `true` or `false` values. Omitted settings keep their existing values.

```sh
# Disable automatic commits and pushes for subsequent commands.
localnotion git --path "./website" --enabled false --push false

# Enable automatic commits without automatic push.
localnotion git --path "./website" --enabled true --push false

# Enable automatic push after commits, once the target is configured.
localnotion git --path "./website" --enabled true --push true

# Change only the push preference.
localnotion git --path "./website" --push false
```

For example, enabling automatic commits without push prints the effective settings:

```json
{"git":{"enabled":true,"push":false}}
```

The command updates settings through the repository API, saves through its normal registry transaction, then displays their effective values. An unchanged assignment does not rewrite the registry. A change uses the normal serializer: it retains the other Git setting, supported repository configuration, and unknown properties at the registry root and inside `git`. Formatting, comments and the original encoding are not preserved when the registry is rewritten. Unknown properties within other typed configuration or resource models follow those models' normal serialization behavior. Errors return a nonzero exit code without printing the registry or its secrets.

Stop an existing Local Notion sync process and other repository writers before changing configuration. A running process can retain its loaded settings or overwrite later edits when it saves; restart it after the configuration change. For local previews, use a separate repository or isolated copy with both settings disabled and no deployment remote.

Registry recovery follows the existing repository load process. A completed `registry.json.commit` is copied to the registry before settings are loaded, and abandoned `registry.json.persist` data is cleaned up. The primary registry must still be readable for repository type detection. There is no separate configuration lock or settings-specific recovery protocol; use the same single-writer discipline as for other repository changes.

Cancellation is checked before opening the repository and before changing its settings. Once a save starts, the command lets the normal save transaction finish; the repository save API does not support cancellation.

## Configure Git itself

This command manages Local Notion's two existing Git settings. Git identity, remotes, branches, upstream selection, SSH credentials and other Git options remain standard Git configuration. Inspect an existing Git repository before enabling automatic publication:

```sh
git -C "./website" status --short
git -C "./website" remote -v
git -C "./website" branch -vv
git -C "./website" config --local --get user.name
git -C "./website" config --local --get user.email
```

A remote can deploy a website, store a private backup or serve another purpose. Inspect its URL and intended branch; its name alone does not identify the destination. For a repository that has not yet been configured, run the following from its folder:

```sh
git init
git config --local user.name "Your Name"
git config --local user.email "you@example.com"
git remote add origin "YOUR_PRIVATE_GIT_REMOTE"
```

If Git is already initialized or the remote already exists, inspect and adjust that setup rather than repeating initialization. Configure and test the intended branch/upstream with ordinary Git before enabling Local Notion's automatic push. Changing Local Notion's settings does not create a Git repository or verify a remote, authentication or server deployment.

For a new Local Notion repository, the existing `init --git` and `init --git-push` options enable these behaviors at initialization. Use `localnotion git` to inspect or change an existing repository without reinitializing it. A successful configuration change is separate from a later content operation, Git transfer and activation of the hosted website.
