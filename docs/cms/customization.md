# CMS customization

Use ordinary blocks/properties for editorial work and theme overrides for reusable rendering changes. Internal pages and raw HTML are mechanisms for reviewed custom markup, styles or scripts.

## Internal token pages

An Internal row is route-based framing. Code-block **captions** become token names and the raw code text becomes each value. The current implementation traverses its block graph and reads nonblank captions; language highlighting is not checked by this mechanism.

| Caption/token | Intended location |
| --- | --- |
| `html_head_start` | Start of head |
| `html_head_end` | End of head |
| `html_body_start` | Start of body |
| `html_body_end` | End of body |
| `google_tag` | Tracking-related template token when consumed |

The selected template must consume a token. Preserve exact spelling and use one deliberate block per token; later encountered duplicate captions can replace earlier values.

For example, put this code in a root Internal row with caption `html_head_end`:

```html
<style>
  .cms-note { border-left: 0.25rem solid #477457; padding-left: 1rem; }
</style>
```

That is a token value, not a body paragraph. Preserve existing head content and inspect final HTML. A nearer Internal row replaces the inherited source; ancestor token sets are not automatically merged.

Internal bypasses ordinary publication gating, and its own Tags/Summary are cleared. Put `@HideInternal` on ordinary content that should omit it.

## Ordinary code and explicit HTML injection

Normal code blocks become code content. The explicitly supported raw-HTML path requires both Notion language **HTML** and exact trimmed caption **`{INJECT}`**. Example block content:

```html
<aside class="cms-note">Additional information for this page.</aside>
```

A similar caption in a plain-text or JavaScript block does not select this path. Review injected markup, external resources, responsive layout and accessibility before publishing. Never include integration keys or authentication headers in public HTML/scripts/tokens.

## Code themes and renderer versions

The inspected website installation gives these two presets different jobs:

- `code_inject` replaces the Code-block template with raw code output. Affected markup, styles or scripts can become active content rather than a readable listing.
- `code_all` replaces the usual page-content wrapper with its child content. It removes that wrapper's normal cover/title/container presentation; by itself, it does not make every code block raw or remove all outer framing.

Do not select these themes merely to improve the appearance of documentation examples. An example that should remain literal needs escaped/readable rendering. Their exact effect depends on the installed templates and overrides; the website installation inspected for this guide and the newer source renderer are different versions.

The HTML-language plus `{INJECT}` projection described above is explicit in current source. Check the version-matched rendered result before choosing it for an existing website. On the older installation used for the initial guide, literal HTML code examples needed paragraphs with rich-text code annotation to remain visible as text. A source rule and a theme-file inspection explain their mechanisms; they do not replace a runtime check of the final page.

## Choose the owner

| Change | Edit |
| --- | --- |
| Text, images, links | Owning Notion blocks |
| Route/publication/composition | CMS properties |
| Shared menu/footer copy | Selected framing row |
| Reusable component/template styling | Theme override |
| Small head/body token | Internal code block with exact caption |
| Reviewed body markup | HTML block captioned `{INJECT}` |

Do not repair an editorial link in generated HTML; regeneration can overwrite it. See [themes and layout](themes-and-layout.md).
