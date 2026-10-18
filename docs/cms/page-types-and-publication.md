# Page types and publication

Type determines what a Notion row contributes. Its page body remains normal Notion blocks.

## Ordinary content

| Type | Use | Composition |
| --- | --- | --- |
| Page | Articles, service pages and a simple homepage | Standalone content at its calculated/custom route |
| Section | Landing-page introduction, services, contact | Same-route sections combine in Sequence order |
| Gallery | Cards for products, photographs, articles or profiles | Each row supplies a card and its page, gathered into a gallery |

Section titles become anchors rather than extra path segments. Root-level Sections named Welcome and Services can combine into the homepage. `sectioned_page` is a generated composition label, not an editable Type.

Prefer one composition strategy per route. Do not put an unrelated Page and multiple Sections at the same address. Gallery rows sharing Root form a collection of cards; categories become badges/filter values. See [routing](routing.md).

File is preserved in the template palette but unsupported by the current CMS parser. Do not select it. Ordinary downloadable attachments within supported pages are separate from that Type choice.

## Shared framing

| Type | Role | Selection/inheritance |
| --- | --- | --- |
| Header | Banner/header fragment | Local by default; `@UseParentHeader` requests the parent header |
| NavBar | Navigation | Nearest local NavBar, otherwise parent's |
| Footer | Footer content | Nearest local Footer, otherwise parent's |
| Internal | Template-token customization | Nearest local Internal row, otherwise parent's |

Framing belongs to its calculated category/custom path. Blank Root, categories and Custom Slug place it at the root; its row title is not appended. One root NavBar/Footer can therefore frame many descendants.

Use one deliberate row of each framing type per route. The nearest Internal source is selected; ancestor token sets are not automatically merged.

Hide tags belong on ordinary content: `@HideHeader`, `@HideNavBar`, `@HideFooter`, `@HideInternal`. Framing rows have Tags and Summary cleared during parsing, so a hide tag on the Footer itself is ineffective. `@HideHeader` takes precedence over `@UseParentHeader`.

## Publication rules

Ordinary public content requires Published status, a non-Internal Type and no future Publish On date, compared with UTC. Draft, QA, Hidden and blank/unrecognized statuses do not publish ordinary content.

**Header, NavBar, Footer and Internal are included as framing independently of this gate.** Draft or future-dated framing can still affect the site. Disable its use through a deliberate placement/framing change, then regenerate and check affected routes.

**Compatibility with 1.5.0.12:** Published framing at the exact address of an ordinary Published Page can be counted as an extra page-content part, causing the “exactly one part” failure. Set pure Header, NavBar and Footer rows to **Hidden** and keep the ordinary Page **Published**. Hidden framing restored the tested Header/NavBar routes. It prevents an unwanted ordinary-content contribution; it does not disable the selected framing.

Scheduling takes effect when a pull and regeneration happen after the due time. Static HTML does not consult Notion on every visit. A change to Hidden also needs a completed publication cycle before you can trust the hosted result.

## Ordering and nested pages

Sequence sorts content at a route; missing values sort after explicit values. Use distinct whole numbers, such as 10, 20, 30. Child nodes have their own sequence-based ordering. Do not depend on a stable tie-breaker for equal values.

A Notion child page inside a CMS row becomes Page content. It inherits publication scheduling, status, tags, authentication and category information; Summary is not inherited. Its parsed Themes are not simply copied, although the render model can layer Page-ancestor themes. Check actual child output when styling nested content.

Use separate CMS rows when a page needs its own independent Type, publication settings or category placement.
