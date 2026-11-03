# Official Local Notion Docker
# Build with: docker build --platform linux/amd64 -t local-notion .
FROM mcr.microsoft.com/dotnet/sdk:10.0.400-noble AS build
ARG VERSION=dev
ARG BUILD_NUMBER=0
ARG VCS_REF=
WORKDIR /src
RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates git \
    && rm -rf /var/lib/apt/lists/*
COPY build/framework-source.ref build/visual-renderer-source.ref build/
# Native CI and standalone Docker builds use the same immutable public source pins.
# Cone-mode sparse checkout includes root MSBuild files and only these dependency projects.
RUN set -eu; \
    framework_pin="$(sed 's/\r$//' build/framework-source.ref)"; \
    [ "$(printf '%s\n' "$framework_pin" | wc -l)" -eq 1 ]; \
    printf '%s\n' "$framework_pin" | grep -Eq '^HermanSchoenfeld/Sphere10\.Framework [0-9a-f]{40}$'; \
    framework_repository="${framework_pin% *}"; \
    framework_revision="${framework_pin#* }"; \
    [ "$framework_revision" != 0000000000000000000000000000000000000000 ] || { echo 'Framework source pin is pending.' >&2; exit 1; }; \
    git init /Framework; \
    git -C /Framework remote add origin "https://github.com/$framework_repository.git"; \
    git -C /Framework sparse-checkout init --cone; \
    git -C /Framework sparse-checkout set src/Sphere10.Framework src/Sphere10.Framework.Data src/Sphere10.Framework.Application resources/branding; \
    git -C /Framework fetch --depth=1 --filter=blob:none origin "$framework_revision"; \
    git -C /Framework checkout --detach "$framework_revision"; \
    [ "$(git -C /Framework rev-parse HEAD)" = "$framework_revision" ]; \
    [ ! -d /Framework/src/Sphere10.Framework.Application/DRM ]
ENV Sphere10FrameworkRoot=/Framework
RUN set -eu; \
    renderer_pin="$(sed 's/\r$//' build/visual-renderer-source.ref)"; \
    [ "$(printf '%s\n' "$renderer_pin" | wc -l)" -eq 1 ]; \
    printf '%s\n' "$renderer_pin" | grep -Eq '^HermanSchoenfeld/Sphere10\.VisualRenderer [0-9a-f]{40}$'; \
    renderer_repository="${renderer_pin% *}"; \
    renderer_revision="${renderer_pin#* }"; \
    [ "$renderer_revision" != 0000000000000000000000000000000000000000 ] || { echo 'VisualRenderer source pin is pending.' >&2; exit 1; }; \
    git init /VisualRenderer; \
    git -C /VisualRenderer remote add origin "https://github.com/$renderer_repository.git"; \
    git -C /VisualRenderer sparse-checkout init --cone; \
    git -C /VisualRenderer sparse-checkout set src/Sphere10.VisualRenderer resources/branding; \
    git -C /VisualRenderer fetch --depth=1 --filter=blob:none origin "$renderer_revision"; \
    git -C /VisualRenderer checkout --detach "$renderer_revision"; \
    [ "$(git -C /VisualRenderer rev-parse HEAD)" = "$renderer_revision" ]
ENV Sphere10VisualRendererRoot=/VisualRenderer
COPY global.json Version.props Directory.Build.props Directory.Build.targets ./
COPY build/Sphere10.Framework.References.targets build/
COPY LICENSE COPYRIGHT COPYING.EXCEPTION ./
COPY assets/branding/local-notion.ico assets/branding/
COPY LocalNotion.CLI/LocalNotion.CLI.csproj LocalNotion.CLI/
COPY LocalNotion.Core/LocalNotion.Core.csproj LocalNotion.Core/
COPY Notion.Client/Notion.Client.csproj Notion.Client/
RUN env -u VERSION dotnet restore LocalNotion.CLI/LocalNotion.CLI.csproj -r linux-x64 -p:Configuration=Release -p:BuildRevision=0
COPY LocalNotion.CLI/ LocalNotion.CLI/
COPY LocalNotion.Core/ LocalNotion.Core/
COPY Notion.Client/ Notion.Client/
# JSON polymorphism uses reflection, so trimming must remain disabled.
# Copy the complete publish output, including native dependencies.
RUN set --; \
    if [ -n "$VERSION" ] && [ "$VERSION" != dev ]; then set -- "$@" "-p:ReleaseVersion=$VERSION"; fi; \
    if [ -n "$VCS_REF" ] && [ "$VCS_REF" != unknown ]; then set -- "$@" "-p:SourceRevisionId=$VCS_REF"; fi; \
    env -u VERSION dotnet publish LocalNotion.CLI/LocalNotion.CLI.csproj \
    -c Release -r linux-x64 --self-contained true --no-restore \
    -p:PublishSingleFile=true \
    -p:DebugType=None -p:DebugSymbols=false -p:BuildRevision=0 "-p:BuildNumber=$BUILD_NUMBER" "$@" -o /out

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-noble
ARG VERSION=dev
ARG VCS_REF=unknown
ARG BUILD_NUMBER=0
LABEL org.opencontainers.image.title="Official Local Notion Docker" \
      org.opencontainers.image.description="The official Local Notion CLI for Notion backups, synchronization, and HTML export." \
      org.opencontainers.image.source="https://github.com/Sphere10/LocalNotion" \
      org.opencontainers.image.url="https://sphere10.com/products/localnotion" \
      com.sphere10.localnotion.portable-paths="1" \
      com.sphere10.localnotion.build-number="${BUILD_NUMBER}" \
      org.opencontainers.image.vendor="Sphere10" \
      org.opencontainers.image.licenses="GPL-3.0-or-later" \
      org.opencontainers.image.version="${VERSION}" \
      org.opencontainers.image.revision="${VCS_REF}"
RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates git openssh-client \
    && rm -rf /var/lib/apt/lists/*
ENV HOME=/var/lib/localnotion
# Container Git uses OpenSSH even when a Windows repository selects Plink.
ENV GIT_SSH_COMMAND=ssh GIT_SSH_VARIANT=ssh
# OpenSSH finds ~/.ssh from the account database, not just the HOME variable.
RUN usermod --home /var/lib/localnotion app
RUN mkdir -p /repo "$HOME/.local/share" \
    && chown -R app:app /repo "$HOME"
COPY --from=build /out/ /opt/localnotion/
COPY docker/entrypoint.sh /usr/local/bin/localnotion-entrypoint
RUN ln -s /opt/localnotion/localnotion /usr/local/bin/localnotion \
    && chmod 755 /usr/local/bin/localnotion-entrypoint
USER app
WORKDIR /repo
STOPSIGNAL SIGINT
ENTRYPOINT ["/usr/local/bin/localnotion-entrypoint"]
CMD ["--help"]
