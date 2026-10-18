# Build your first website

Create a website with a homepage, an About page, shared navigation, a footer and a gallery of cards. Use the default presentation to verify a complete publication cycle before applying Themes.

## 1. Create and initialize

Follow [Create a blank CMS](create-a-cms.md), then use the returned database ID:

```powershell
mkdir website
$notionKey = (Get-Content -LiteralPath $env:NOTION_API_KEY_FILE -Raw).Trim()
localnotion init --cms "NEW_DATABASE_UUID" --profile website --path "./website" --key $notionKey
Remove-Variable notionKey
```

Set `NOTION_API_KEY_FILE` to a protected key file as described in the creation guide. This PowerShell recipe reads that file and explicitly supplies `init --key`; `init` does not resolve the environment variable itself. Protect the generated registry and key file. This example leaves automatic Git and hosting setup disabled.

## 2. Add a homepage

Create a Notion row:

| Field | Value |
| --- | --- |
| Item | Homepage |
| Type | Page |
| Status | Published |
| Custom Slug | `/` |
| Summary | Services, information and public contact details. |
| Root, Category1–Category5 | blank |
| Publish On, Themes, Tags, Authentication | blank |

Open the row and add an introductory paragraph, a Heading 2 and a paragraph explaining the main topic. Wait for Notion to save, then reopen the row to verify persistence. Leave Themes empty for the initial render; `empty` intentionally suppresses visible content, and injection is not required for ordinary text.

## 3. Add About and shared framing

Create a Published Page titled About Us with blank Root/categories and Custom Slug. It becomes `/about-us`. Write its content and Summary.

Create NavBar and Footer rows named Shared Navigation and Shared Footer, with **Status = Hidden** and blank Root/categories/Custom Slug. Keep **Status = Published** on the Homepage Page. Add links to home and About Us inside the framing rows. Notion-page links can be resolved by Local Notion; check the generated targets after pulling.

Their titles are labels, not extra route components. Hidden is the recommended framing value for this installed 1.5.0.12 example: Published framing at the homepage's exact route can be counted as another Page body part. Framing selection still ignores Status, so Hidden does not disable the selected menu or footer. See the [version compatibility note](page-types-and-publication.md), and verify both in the first preview.

## 4. Pull and check

```sh
localnotion pull --all --path "./website"
```

Check home, About, navigation and footer through a suitable preview/staging host. Use [publishing](publishing.md) to verify each stage.

## 5. Try a Section homepage

Move the original Page to a distinct custom route, or set it Hidden and complete regeneration, before introducing root Sections. Avoid competing content at `/`.

Create Published Sections with blank Root/categories/Custom Slug:

| Item | Sequence | Content |
| --- | ---: | --- |
| Welcome | 10 | Introduction |
| What We Do | 20 | Services in Notion columns |
| Contact | 30 | Contact details and a clear next step |

They combine at home with title-based anchors; root navigation/footer still frame the route. Pull and inspect order, anchors, narrow-screen columns and links. Add [themes](themes-and-layout.md) one at a time.

## 6. Add documentation

Create Published Pages:

| Item | Root | Category1 | Path |
| --- | --- | --- | --- |
| Getting Started | Guides | CMS | `/guides/cms/getting-started` |
| Writing Content | Guides | CMS | `/guides/cms/writing-content` |
| Publishing | Guides | CMS | `/guides/cms/publishing` |

Use distinct Sequence values for reading order and concise Summaries for listings. Hierarchy comes from these fields, not Parent relations. A custom overview Page is possible; check how it changes the inferred article/book structure.

## 7. Add a gallery

Create Published Gallery rows named Design service and Website service with Root Cards. Add an image, a Summary and a Category1 label such as Design or Build to each row. Their detail pages sit beneath `/cards`; that parent route becomes the gallery collection. Check card links, images and category filtering. The same structure can present products, photographs, articles or team profiles; Gallery is the component, not a particular content subject.

## 8. Publish deliberately

Configure the host/transfer workflow separately. Confirm its domain, automatic-publication settings and activation mechanism before enabling automation. Verify homepage, article, gallery and shared framing routes. Keep credentials out of content and diagnostics; `@Private` alone is not access control.
