# Themes and layout

Notion blocks define content structure; CMS types define composition; themes define rendering. Start with readable default content, add themes deliberately and check the whole route at desktop and narrow widths.

For an illustrated, practical introduction, start with [Design a website in Notion](visual-design.md). This page explains the underlying theme mechanics.

## Three layout levels

| Level | Control |
| --- | --- |
| Notion blocks | Paragraphs, headings, images, columns, lists and their order |
| CMS composition | Page/Section/Gallery, framing, hierarchy and Sequence |
| Themes | Templates, spacing, alignment, typography, colors and effects |

A theme cannot publish a Draft page, repair a slug or guarantee a contact-form backend.

## Selecting themes

The Themes field presets [228 names](theme-reference.md). A preset is an available choice, not proof that every installed renderer supplies it or that every combination works.

Local Notion adds appropriate CMS base themes for a page, section, header or navbar and layers selected choices. Empty selection uses those defaults; you do not need to invent a default option in the Notion palette.

Composition is ordered. Later overlays can replace templates, tokens or resources from earlier ones. Prefer one clear alignment/spacing/color choice over several contradictory options, and verify the actual effective order.

## Theme families

These are naming guides rather than a per-theme visual test report. Select exact names from the reference instead of inventing wildcard variants.

| Examples/family | Typical intent |
| --- | --- |
| `text_left`, `text_center`, `text_small`, `text_muted` | Text treatment |
| `h1_*`, `h2_*`, `h3_*`, `h123_*` | Heading styling |
| `section_padding_top_*`, `section_padding_bottom_*` | Section spacing |
| `columns_*`, `columns_middle` | Column sizing/alignment |
| `image_align_*`, `image_width_*`, `image_height_*`, `image_min*`, `image_max*` | Image sizing/alignment |
| `header_*`, `navbar_*` | Banner/navigation presentation |
| `color_text_*`, `color_bg_*`, `color_aux_*`, `color_card_*` | Color treatments |
| `animate_*`, `shape_*` | Motion and decoration |
| `button_*`, `link_*`, `callout_*`, `badge*`, `shadow_*` | Components |

Names are catalog/file inputs; preserve exact spelling across operating systems. Editor choice colors do not specify website colors.

## The empty theme

`empty` suppresses its part's rendered content. A metadata-only first Section can use it intentionally. Paragraphs in that row can save and pull successfully while displaying nothing.

This behavior was observed on staging and is explicit in the current CMS render-model builder. For visible-copy tests, choose a normal Page or Section without `empty` before diagnosing synchronization.

## Compose a landing page

Create Published Section rows with the same category path and distinct Sequence values: Welcome 10, Services 20, Contact 30. Their titles supply anchors. Add root NavBar/Footer framing and inspect the combined output. A Header is separate framing, not simply the first body Section.

Use only the base page path in a Section's Custom Slug, without a literal `#`; [routing](routing.md) explains why a hand-entered fragment can change the path instead of making an anchor. For an ordinary Page with exact-route Header/NavBar framing on installed 1.5.0.12, follow the [framing compatibility note](page-types-and-publication.md) rather than publishing both as ordinary page parts.

## Overrides and versions

The current renderer embeds built-in templates/assets. Local Notion also deploys missing built-in files into its configured themes folder on repository creation/opening. Existing files are preserved and can mask newer defaults.

Edit intended theme overrides for reusable styling rather than generated HTML. Empty override files count as overrides. Removing a file can reveal an embedded default during rendering, but a later repository open can restore missing built-in files. Understand that lifecycle before cleaning overrides.

See [theme override mechanics](../renderer-themes.md) for inheritance, configuration, online assets and snapshots. Older releases have different theme/extraction workflows.

The palette includes `code_inject` and `code_all`, but their names do not mean the same thing. In the inspected website templates, `code_inject` emits raw Code-block content, while `code_all` removes the normal page-content wrapper and emits its children. The latter does not itself make all code raw. Read [customization and renderer versions](customization.md) before using either; keep documentation examples literal and check behavior against the renderer actually serving the site.

After renaming a parent chapter, regenerate its descendants as well. In the isolated 1.5.0.12 preview, unchanged articles kept the old breadcrumb label even after the parent title changed in the registry. A fresh full render corrected the inspected article's ancestor label. Check the regenerated output before transferring it.
