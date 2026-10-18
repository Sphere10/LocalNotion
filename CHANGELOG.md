# Changelog

## 1.7.0

- Add `create-cms` to create an empty Notion website database with the canonical CMS schema and preset Type, Status, Tags and Themes options.
- Add `git` to view and change an existing repository's automatic commit and push settings without publishing content.
- Preserve framing tags on CMS rows and inherit Gallery headers through `@UseParentHeader`, while respecting explicit header selection and `@HideHeader`.
- Remove Local Notion's Enterprise/DRM dependencies, activation and license-enforced quotas. Preserve the GPL license, renderer linking exception and separately licensed dependency notices.
- Update the Local Notion logo and native application icon.
- Apply shared ReadyToRun publishing defaults across native archives, Docker and local deployment, with bundle compression disabled for the local Windows deployment to reduce startup allocation.
- Expand the CMS guide with schema, page composition, themes, media, hosting and clean rebuild procedures; document Git configuration and update native and Docker installation examples.

The unified release provides eight self-contained native archives, the Windows Docker launcher bundle, SHA-256 checksums and release metadata. The official Docker image targets `linux/amd64`. Check the metadata for the release build number, source revision and immutable container digest.
