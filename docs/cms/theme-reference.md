# Theme option reference

Find a theme by the part of the page you want to change. Each entry explains its intended result, the content it needs and a staging comparison. Select the exact option in the owning CMS row's **Themes** field.

For a guided introduction, start with [Design a website in Notion](visual-design.md), then [sections](sections-and-layout.md), [spacing and separators](spacing-and-separators.md), [text and colour](text-and-colour.md), [images and covers](images-and-covers.md), [columns and cards](columns-and-cards.md), [galleries](galleries.md), [headers and navigation](headers-and-navigation.md) or [motion and finishing](motion-and-finishing.md).

## Read this reference

This is the complete canonical palette: **228 choices**, grouped by purpose. Each option retains its original **sequence number** and **Notion editor colour** from the audited staging schema. Those editor colours label choices; they do not set website colours. Within each family, entries keep their original order.

Descriptions below are **expected behavior from inspected templates and styles**, not a claim that every result or interaction has been visually confirmed. The installed website renderer inspected here is 1.5.0.12; newer renderers and local overrides can differ. The source inventory's visual-verification flags remain pending. Open the staging comparison to inspect the actual result, especially for compatibility gaps, motion, image zoom, scrolling and gallery filtering.

A preset in Notion needs a matching installed theme and the right owner/block. One canonical choice is unavailable in the inspected installation and is marked explicitly below. A source-level compatibility gap means the inspected local styles do not supply its requested rule; custom/CDN styles may differ, so do not silently substitute an alias or promise a visible result.

Keep one deliberate choice for each competing property. Shared framing and full-template replacements can bypass individual modifiers. [Themes and layout](themes-and-layout.md) explains layering; [customization](customization.md) covers advanced raw content.

## Content visibility

Choose whether a row contributes visible content. This is a composition decision, not a substitute for publication settings.

| Option | What changes and what it needs | Try example |
| --- | --- | --- |
| **29** `empty` (editor: red) | Suppresses the row's visible output. Useful for deliberate metadata-only content; a successful save/pull can still display nothing. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/sections) |

## Body text

These choices affect compatible paragraphs, quotes and callouts on a Page or Section. Heading and image alignment have their own controls. Choose one alignment and one main text-colour treatment rather than competing values.

| Option | What changes and what it needs | Try example |
| --- | --- | --- |
| **36** `text_left` (editor: purple) | Left-aligns compatible paragraphs, quotes and callouts. Heading/image alignment is separate. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography) |
| **37** `text_center` (editor: gray) | Centres compatible paragraphs, quotes and callouts. Best for short introductions; heading/image alignment is separate. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography) |
| **38** `text_right` (editor: purple) | Right-aligns compatible paragraphs, quotes and callouts. Heading/image alignment is separate. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography) |
| **39** `text_justify` (editor: yellow) | Justifies compatible body text. Inspect line spacing and word gaps at a narrow width. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography) |
| **40** `text_muted` (editor: gray) | Gives compatible body text a muted colour; competes with other main-text colour choices. Check readability on the chosen background. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography) |
| **41** `text_small` (editor: brown) | Makes compatible body blocks smaller. Use for brief supporting notes, with a readable phone preview. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography) |

## Headings

Create real Heading 1, 2 or 3 blocks in a Page, Section or compatible Header, then choose their presentation. Visual size does not change a heading's structural level. The all-heading choices affect all three levels; level-specific choices affect only that level.

| Option | What changes and what it needs | Try example |
| --- | --- | --- |
| **2** `h123_fontsize_default` (editor: default) | Clears CMS font-size overrides for all three heading levels; browser/component sizes remain. Needs real heading blocks. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography) |
| **4** `h123_default_padding` (editor: brown) | Clears the shared heading spacing override for all three levels. Other margins can remain. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography) |
| **5** `h1_large` (editor: blue) | Makes Heading 1 more prominent than the ordinary CMS size; it remains Heading 1 structurally. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography) |
| **9** `h123_color_aux` (editor: yellow) | Switches Heading 1, 2 and 3 to the auxiliary text colour. Pair with an auxiliary-text colour choice. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography) |
| **10** `h3_shadow` (editor: red) | Adds the shared text shadow to Heading 3. Optionally choose a shared shadow strength. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography) |
| **11** `h2_shadow` (editor: brown) | Adds the shared text shadow to Heading 2. Optionally choose a shared shadow strength. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography) |
| **12** `h1_shadow` (editor: default) | Adds the shared text shadow to Heading 1. Optionally choose a shared shadow strength. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography) |
| **46** `h123_shadow` (editor: yellow) | Adds the shared text shadow to Heading 1, 2 and 3. Optionally choose a shared shadow strength. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography) |
| **47** `h1_align_left` (editor: brown) | Aligns Heading 1 left. Compatibility gap: its left-align rule is absent locally; ordinary left appearance is not proof of a working reset. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography) |
| **48** `h1_align_center` (editor: gray) | Aligns Heading 1 center. Body text and images have separate alignment controls. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography) |
| **49** `h1_align_right` (editor: orange) | Aligns Heading 1 right. Body text and images have separate alignment controls. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography) |
| **50** `h1_color_aux` (editor: brown) | Switches Heading 1 to the auxiliary text colour. Pair with an auxiliary-text colour choice. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography) |
| **51** `h1_as_h4` (editor: brown) | Gives Heading 1 a smaller visual size, like Heading 4; its original structural level remains unchanged. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography) |
| **52** `h1_typed` (editor: orange) | Types/rotates Heading 1 text; a vertical bar separates phrases. Needs the typing script and a live check. The inspected Heading 1 template has an unresolved shadow reference. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography/h1-typed-experimental) |
| **53** `h2_align_left` (editor: green) | Aligns Heading 2 left. Compatibility gap: its left-align rule is absent locally; ordinary left appearance is not proof of a working reset. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography) |
| **54** `h2_align_center` (editor: green) | Aligns Heading 2 center. Body text and images have separate alignment controls. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography) |
| **55** `h2_align_right` (editor: orange) | Aligns Heading 2 right. Body text and images have separate alignment controls. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography) |
| **56** `h2_color_aux` (editor: yellow) | Switches Heading 2 to the auxiliary text colour. Pair with an auxiliary-text colour choice. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography) |
| **57** `h2_typed` (editor: brown) | Types/rotates Heading 2 text; a vertical bar separates phrases. Needs the typing script and a live check. The typed template omits the ordinary heading-shadow hook. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography) |
| **58** `h2_as_h5` (editor: pink) | Gives Heading 2 a smaller visual size, like Heading 5; its original structural level remains unchanged. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography) |
| **59** `h3_align_left` (editor: blue) | Aligns Heading 3 left. Compatibility gap: its left-align rule is absent locally; ordinary left appearance is not proof of a working reset. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography) |
| **60** `h3_align_center` (editor: pink) | Aligns Heading 3 center. Body text and images have separate alignment controls. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography) |
| **61** `h3_align_right` (editor: gray) | Aligns Heading 3 right. Body text and images have separate alignment controls. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography) |
| **62** `h3_color_aux` (editor: brown) | Switches Heading 3 to the auxiliary text colour. Pair with an auxiliary-text colour choice. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography) |
| **63** `h3_typed` (editor: yellow) | Types/rotates Heading 3 text; a vertical bar separates phrases. Needs the typing script and a live check. The typed template omits the ordinary heading-shadow hook. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography) |
| **64** `h3_as_h6` (editor: gray) | Gives Heading 3 a smaller visual size, like Heading 6; its original structural level remains unchanged. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/typography) |

**Heading checks:** typed effects replace the whole heading template and need live verification. The Heading 1 typed template has a missing shared-shadow reference; typed Heading 2/3 omit normal shadow hooks. The left-alignment choices request a legacy rule absent from the inspected stylesheet. The ordinary default's left appearance does not prove those choices can reset another alignment.

## Section spacing

Apply these to Section rows, not ordinary Pages. Top and bottom can be chosen independently. The inspected stylesheet's default is 3rem per edge; the small choice is 0.5rem and the large choice is 6rem. Content-block margins can remain after section padding is removed.

| Option | What changes and what it needs | Try example |
| --- | --- | --- |
| **21** `section_padding_top_lg` (editor: brown) | Sets the Section's top padding to a generous 6rem. Does not remove margins inside content blocks. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/spacing) |
| **22** `section_padding_bottom_lg` (editor: brown) | Sets the Section's bottom padding to a generous 6rem. Does not remove margins inside content blocks. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/spacing) |
| **27** `section_padding_top_xxs` (editor: blue) | Sets the Section's top padding to a small 0.5rem. Does not remove margins inside content blocks. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/spacing) |
| **28** `section_padding_bottom_xxs` (editor: green) | Sets the Section's bottom padding to a small 0.5rem. Does not remove margins inside content blocks. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/spacing) |
| **215** `section_padding_top_none` (editor: purple) | Sets the Section's top padding to zero. Does not remove margins inside content blocks. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/spacing) |
| **216** `section_padding_bottom_none` (editor: green) | Sets the Section's bottom padding to zero. Does not remove margins inside content blocks. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/spacing) |

## Section separators

A separator decorates the bottom edge of a Section or Header. All four inspected shapes are white: compare a coloured band above a white one so the edge is visible. They do not automatically match an arbitrary following background. A Notion divider is instead a rule inside content.

| Option | What changes and what it needs | Try example |
| --- | --- | --- |
| **211** `shape_line_up` (editor: orange) | Adds a white angled bottom edge to a Section/Header. Needs contrast with its surroundings; choose one shape per row. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/sections) |
| **212** `shape_curve_up` (editor: green) | Adds a white upward-curved bottom edge to a Section/Header. Needs contrast with its surroundings; choose one shape per row. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/sections) |
| **213** `shape_ascend_right` (editor: orange) | Adds a white ascending bottom edge to a Section/Header. Needs contrast with its surroundings; choose one shape per row. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/sections) |
| **214** `shape_curve_down` (editor: orange) | Adds a white downward-curved bottom edge to a Section/Header. Needs contrast with its surroundings; choose one shape per row. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/sections) |

## Columns

Make a Notion column list before selecting these choices on its Page or Section. Breakpoints control when columns sit side by side and stack, rather than specifying a fixed column width. Choose one breakpoint. Test the real phone layout rather than inferring it from a theme name.

| Option | What changes and what it needs | Try example |
| --- | --- | --- |
| **23** `columns_middle` (editor: orange) | Vertically centres compatible column content. Needs a Notion column list with unequal content heights to make the difference visible. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/sections) |
| **117** `columns_xs` (editor: default) | Chooses the extra-small breakpoint for an existing Notion column list. Compatibility gap: the emitted column rule is absent from the inspected stylesheet. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/sections) |
| **118** `columns_sm` (editor: green) | Chooses the small breakpoint for an existing Notion column list. Columns stack below the chosen breakpoint; test the narrow-screen reading order. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/sections) |
| **119** `columns_md` (editor: purple) | Chooses the medium breakpoint for an existing Notion column list. Columns stack below the chosen breakpoint; test the narrow-screen reading order. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/sections) |
| **120** `columns_lg` (editor: pink) | Chooses the large breakpoint for an existing Notion column list. Columns stack below the chosen breakpoint; test the narrow-screen reading order. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/sections) |
| **121** `columns_xl` (editor: gray) | Chooses the extra-large breakpoint for an existing Notion column list. Columns stack below the chosen breakpoint; test the narrow-screen reading order. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/sections) |
| **122** `columns_xxl` (editor: pink) | Chooses the largest breakpoint for an existing Notion column list. Columns stack below the chosen breakpoint; test the narrow-screen reading order. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/sections) |

## Images

Add a real Image block first. The inspected renderer contains the picture responsively. Alignment needs an image narrower than its container; captions stay centred. Size rules below describe the actual installed definition, including maximums hidden behind some shorter option names. The named scale is 5/10/15/25/35/45/60rem from smallest to largest; a rem follows the site's root text size.

| Option | What changes and what it needs | Try example |
| --- | --- | --- |
| **7** `image_shadow` (editor: blue) | Adds the shared shadow to Image blocks. Optionally choose a shared shadow strength. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **24** `image_align_right` (editor: yellow) | Places a narrower Image at the right of its container; a full-width image has no room to move. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **25** `image_align_center` (editor: pink) | Places a narrower Image in the centre of its container; a full-width image has no room to move. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **26** `image_align_left` (editor: brown) | Places a narrower Image at the left of its container; a full-width image has no room to move. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **65** `image_framed` (editor: blue) | Adds a thumbnail-style padded border/frame to Image blocks. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **66** `image_zoomable` (editor: purple) | Makes an Image a click-to-enlarge target. Needs the image-viewer script; test opening and closing the live viewer. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **67** `image_width_xxs` (editor: red) | Limits the maximum Image width of 5rem. This shorter-named preset is a maximum, not a fixed size. A smaller source may show no change. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **68** `image_width_xs` (editor: red) | Limits the maximum Image width of 10rem. This shorter-named preset is a maximum, not a fixed size. A smaller source may show no change. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **69** `image_width_sm` (editor: blue) | Requests an Image width of 15rem. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **70** `image_width_md` (editor: brown) | Requests an Image width of 25rem. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **71** `image_width_lg` (editor: pink) | Requests an Image width of 35rem. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **72** `image_width_xl` (editor: pink) | Limits the maximum Image width of 45rem. This shorter-named preset is a maximum, not a fixed size. A smaller source may show no change. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **73** `image_width_xxl` (editor: red) | Requests an Image width of 60rem. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **74** `image_width_25pc` (editor: yellow) | Requests an Image width of 25% of its container. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **75** `image_width_50pc` (editor: blue) | Requests an Image width of 50% of its container. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **76** `image_width_75pc` (editor: gray) | Requests an Image width of 75% of its container. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **77** `image_width_100pc` (editor: gray) | Requests an Image width of 100% of its container. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **78** `image_height_xxs` (editor: orange) | Limits the maximum Image height of 5rem. This shorter-named preset is a maximum, not a fixed size. A smaller source may show no change. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **79** `image_height_xs` (editor: brown) | Limits the maximum Image height of 10rem. This shorter-named preset is a maximum, not a fixed size. A smaller source may show no change. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **80** `image_height_sm` (editor: pink) | Requests an Image height of 15rem. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **81** `image_height_lg` (editor: orange) | Requests an Image height of 35rem. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **82** `image_height_xl` (editor: purple) | Limits the maximum Image height of 45rem. This shorter-named preset is a maximum, not a fixed size. A smaller source may show no change. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **83** `image_height_xxl` (editor: purple) | Limits the maximum Image height of 60rem. This shorter-named preset is a maximum, not a fixed size. A smaller source may show no change. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **84** `image_height_md` (editor: yellow) | Requests an Image height of 25rem. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **85** `image_height_25pc` (editor: yellow) | Requests an Image height of 25% of its container. Needs a defined containing height. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **86** `image_height_50pc` (editor: red) | Requests an Image height of 50% of its container. Needs a defined containing height. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **87** `image_height_75pc` (editor: purple) | Requests an Image height of 75% of its container. Needs a defined containing height. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **88** `image_height_100pc` (editor: default) | Requests an Image height of 100% of its container. Needs a defined containing height. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **89** `image_minwidth_xxs` (editor: purple) | Sets the minimum Image width of 5rem. Check for overflow on narrow screens. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **90** `image_minwidth_xs` (editor: blue) | Sets the minimum Image width of 10rem. Check for overflow on narrow screens. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **91** `image_minwidth_sm` (editor: purple) | Sets the minimum Image width of 15rem. Check for overflow on narrow screens. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **92** `image_minwidth_md` (editor: green) | Sets the minimum Image width of 25rem. Check for overflow on narrow screens. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **93** `image_minwidth_lg` (editor: brown) | Sets the minimum Image width of 35rem. Check for overflow on narrow screens. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **94** `image_minwidth_xl` (editor: orange) | Sets the minimum Image width of 45rem. Check for overflow on narrow screens. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **95** `image_minwidth_xxl` (editor: orange) | Sets the minimum Image width of 60rem. Check for overflow on narrow screens. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **96** `image_maxwidth_xxs` (editor: brown) | Limits the maximum Image width of 5rem. A smaller source may show no change. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **97** `image_maxwidth_xs` (editor: default) | Limits the maximum Image width of 10rem. A smaller source may show no change. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **98** `image_maxwidth_sm` (editor: pink) | Limits the maximum Image width of 15rem. A smaller source may show no change. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **99** `image_maxwidth_md` (editor: default) | Limits the maximum Image width of 25rem. A smaller source may show no change. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **100** `image_maxwidth_lg` (editor: pink) | Limits the maximum Image width of 35rem. A smaller source may show no change. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **101** `image_maxwidth_xl` (editor: default) | Limits the maximum Image width of 45rem. A smaller source may show no change. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **102** `image_maxwidth_xxl` (editor: green) | Limits the maximum Image width of 60rem. A smaller source may show no change. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **103** `image_minheight_xxs` (editor: gray) | Sets the minimum Image height of 5rem. Check for overflow on narrow screens. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **104** `image_minheight_xs` (editor: gray) | Sets the minimum Image height of 10rem. Check for overflow on narrow screens. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **105** `image_minheight_sm` (editor: pink) | Sets the minimum Image height of 15rem. Check for overflow on narrow screens. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **106** `image_minheight_md` (editor: yellow) | Sets the minimum Image height of 25rem. Check for overflow on narrow screens. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **107** `image_minheight_lg` (editor: green) | Sets the minimum Image height of 35rem. Check for overflow on narrow screens. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **108** `image_minheight_xl` (editor: blue) | Sets the minimum Image height of 45rem. Check for overflow on narrow screens. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **109** `image_minheight_xxl` (editor: yellow) | Sets the minimum Image height of 60rem. Check for overflow on narrow screens. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **110** `image_maxheight_xxs` (editor: blue) | Limits the maximum Image height of 5rem. A smaller source may show no change. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **111** `image_maxheight_xs` (editor: red) | Limits the maximum Image height of 10rem. A smaller source may show no change. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **112** `image_maxheight_sm` (editor: pink) | Limits the maximum Image height of 15rem. A smaller source may show no change. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **113** `image_maxheight_md` (editor: yellow) | Limits the maximum Image height of 25rem. A smaller source may show no change. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **114** `image_maxheight_lg` (editor: gray) | Limits the maximum Image height of 35rem. A smaller source may show no change. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **115** `image_maxheight_xl` (editor: green) | Limits the maximum Image height of 45rem. A smaller source may show no change. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |
| **116** `image_maxheight_xxl` (editor: green) | Limits the maximum Image height of 60rem. A smaller source may show no change. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/images) |

**Image checks:** percentage height requires a defined containing height; minimums can overflow phones; a maximum larger than the source can look unchanged. The actual output also depends on the column/container and source aspect ratio. Compare both portrait and landscape content before generalizing.

## Colour roles

Names such as primary, success and body refer to the installed theme's palette, not fixed universal colours. Main text, container background, auxiliary text, small accents and callout cards have separate controls. Inline Notion colours and component templates can override inherited colour. Inspect contrast on the final page.

| Option | What changes and what it needs | Try example |
| --- | --- | --- |
| **123** `color_text_black` (editor: purple) | Chooses black as the main text colour on compatible Page/Section blocks. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **124** `color_text_black_50` (editor: brown) | Chooses black at 50% opacity as the main text colour on compatible Page/Section blocks. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **125** `color_text_body` (editor: blue) | Chooses body as the main text colour on compatible Page/Section blocks. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **126** `color_text_body_emphasis` (editor: yellow) | Chooses body emphasis as the main text colour on compatible Page/Section blocks. The inspected mapping uses the dark-emphasis treatment. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **127** `color_text_body_secondary` (editor: green) | Chooses body secondary as the main text colour on compatible Page/Section blocks. Compatibility gap: the emitted colour rule is absent from the inspected stylesheet; visible effect is unproved. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **128** `color_text_body_tertiary` (editor: brown) | Chooses body tertiary as the main text colour on compatible Page/Section blocks. Compatibility gap: the emitted colour rule is absent from the inspected stylesheet; visible effect is unproved. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **129** `color_text_danger` (editor: yellow) | Chooses danger as the main text colour on compatible Page/Section blocks. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **130** `color_text_danger_emphasis` (editor: green) | Chooses danger emphasis as the main text colour on compatible Page/Section blocks. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **131** `color_text_dark` (editor: yellow) | Chooses dark as the main text colour on compatible Page/Section blocks. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **132** `color_text_dark_emphasis` (editor: purple) | Chooses dark emphasis as the main text colour on compatible Page/Section blocks. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **133** `color_text_info` (editor: green) | Chooses info as the main text colour on compatible Page/Section blocks. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **134** `color_text_info_emphasis` (editor: green) | Chooses info emphasis as the main text colour on compatible Page/Section blocks. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **135** `color_text_light` (editor: pink) | Chooses light as the main text colour on compatible Page/Section blocks. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **136** `color_text_light_emphasis` (editor: purple) | Chooses light emphasis as the main text colour on compatible Page/Section blocks. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **137** `color_text_primary` (editor: brown) | Chooses primary as the main text colour on compatible Page/Section blocks. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **138** `color_text_primary_emphasis` (editor: purple) | Chooses primary emphasis as the main text colour on compatible Page/Section blocks. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **139** `color_text_secondary` (editor: green) | Chooses secondary as the main text colour on compatible Page/Section blocks. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **140** `color_text_secondary_emphasis` (editor: yellow) | Chooses secondary emphasis as the main text colour on compatible Page/Section blocks. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **141** `color_text_success` (editor: pink) | Chooses success as the main text colour on compatible Page/Section blocks. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **142** `color_text_success_emphasis` (editor: blue) | Chooses success emphasis as the main text colour on compatible Page/Section blocks. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **143** `color_text_warning` (editor: brown) | Chooses warning as the main text colour on compatible Page/Section blocks. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **144** `color_text_warning_emphasis` (editor: green) | Chooses warning emphasis as the main text colour on compatible Page/Section blocks. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **145** `color_text_white` (editor: yellow) | Chooses white as the main text colour on compatible Page/Section blocks. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **146** `color_text_white_50` (editor: brown) | Chooses white at 50% opacity as the main text colour on compatible Page/Section blocks. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **147** `color_bg_body` (editor: purple) | Chooses body for the CMS container background; does not recolour the generated Gallery card body. Choose/check readable text separately. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **148** `color_bg_body_secondary` (editor: pink) | Chooses body secondary for the CMS container background; does not recolour the generated Gallery card body. Choose/check readable text separately. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **149** `color_bg_body_tertiary` (editor: yellow) | Chooses body tertiary for the CMS container background; does not recolour the generated Gallery card body. Choose/check readable text separately. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **150** `color_bg_danger` (editor: purple) | Chooses danger for the CMS container background; does not recolour the generated Gallery card body. Choose/check readable text separately. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **151** `color_bg_danger_subtle` (editor: pink) | Chooses danger subtle for the CMS container background; does not recolour the generated Gallery card body. Choose/check readable text separately. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **152** `color_bg_dark` (editor: default) | Chooses dark for the CMS container background; does not recolour the generated Gallery card body. Choose/check readable text separately. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **153** `color_bg_dark_subtle` (editor: default) | Chooses dark subtle for the CMS container background; does not recolour the generated Gallery card body. Choose/check readable text separately. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **154** `color_bg_info` (editor: gray) | Chooses info for the CMS container background; does not recolour the generated Gallery card body. Choose/check readable text separately. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **155** `color_bg_info_subtle` (editor: yellow) | Chooses info subtle for the CMS container background; does not recolour the generated Gallery card body. Choose/check readable text separately. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **156** `color_bg_light` (editor: green) | Chooses light for the CMS container background; does not recolour the generated Gallery card body. Choose/check readable text separately. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **157** `color_bg_light_subtle` (editor: yellow) | Chooses light subtle for the CMS container background; does not recolour the generated Gallery card body. Choose/check readable text separately. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **158** `color_bg_primary` (editor: blue) | Chooses primary for the CMS container background; does not recolour the generated Gallery card body. Choose/check readable text separately. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **159** `color_bg_primary_subtle` (editor: red) | Chooses primary subtle for the CMS container background; does not recolour the generated Gallery card body. Choose/check readable text separately. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **160** `color_bg_secondary` (editor: yellow) | Chooses secondary for the CMS container background; does not recolour the generated Gallery card body. Choose/check readable text separately. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **161** `color_bg_secondary_subtle` (editor: pink) | Chooses secondary subtle for the CMS container background; does not recolour the generated Gallery card body. Choose/check readable text separately. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **162** `color_bg_success` (editor: brown) | Chooses success for the CMS container background; does not recolour the generated Gallery card body. Also requests contrasting text in this installed mapping. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **163** `color_bg_success_subtle` (editor: red) | Chooses success subtle for the CMS container background; does not recolour the generated Gallery card body. Choose/check readable text separately. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **164** `color_bg_warning` (editor: orange) | Chooses warning for the CMS container background; does not recolour the generated Gallery card body. Choose/check readable text separately. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **165** `color_bg_warning_subtle` (editor: pink) | Chooses warning subtle for the CMS container background; does not recolour the generated Gallery card body. Choose/check readable text separately. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **166** `color_aux_danger` (editor: gray) | Chooses danger for small accents. Needs a badge, arrow-style link or check-style bullet; no ordinary-paragraph effect. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **167** `color_aux_dark` (editor: yellow) | Chooses dark for small accents. Needs a badge, arrow-style link or check-style bullet; no ordinary-paragraph effect. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **168** `color_aux_info` (editor: green) | Chooses info for small accents. Needs a badge, arrow-style link or check-style bullet; no ordinary-paragraph effect. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **169** `color_aux_light` (editor: brown) | Chooses light for small accents. Needs a badge, arrow-style link or check-style bullet; no ordinary-paragraph effect. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **170** `color_aux_primary` (editor: blue) | Chooses primary for small accents. Needs a badge, arrow-style link or check-style bullet; no ordinary-paragraph effect. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **171** `color_aux_secondary` (editor: orange) | Chooses secondary for small accents. Needs a badge, arrow-style link or check-style bullet; no ordinary-paragraph effect. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **172** `color_aux_success` (editor: brown) | Chooses success for small accents. Needs a badge, arrow-style link or check-style bullet; no ordinary-paragraph effect. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **173** `color_aux_warning` (editor: gray) | Chooses warning for small accents. Needs a badge, arrow-style link or check-style bullet; no ordinary-paragraph effect. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **174** `color_text_aux_50` (editor: green) | Unavailable in the inspected installation: no matching theme folder. A similarly named extra option is a different choice; no automatic substitute or implemented effect is established. | Unavailable; no substitute |
| **175** `color_text_aux_black` (editor: orange) | Chooses black for auxiliary text. Ordinary headings need their auxiliary-colour switch; does not recolour ordinary paragraphs. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **176** `color_text_aux_body` (editor: red) | Chooses body for auxiliary text. Ordinary headings need their auxiliary-colour switch; does not recolour ordinary paragraphs. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **177** `color_text_aux_body_emphasis` (editor: red) | Chooses body emphasis for auxiliary text. Ordinary headings need their auxiliary-colour switch; does not recolour ordinary paragraphs. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **178** `color_text_aux_body_secondary` (editor: orange) | Chooses body secondary for auxiliary text. Ordinary headings need their auxiliary-colour switch; does not recolour ordinary paragraphs. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **179** `color_text_aux_body_tertiary` (editor: blue) | Chooses body tertiary for auxiliary text. Ordinary headings need their auxiliary-colour switch; does not recolour ordinary paragraphs. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **180** `color_text_aux_danger` (editor: purple) | Chooses danger for auxiliary text. Ordinary headings need their auxiliary-colour switch; does not recolour ordinary paragraphs. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **181** `color_text_aux_danger_emphasis` (editor: purple) | Chooses danger emphasis for auxiliary text. Ordinary headings need their auxiliary-colour switch; does not recolour ordinary paragraphs. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **182** `color_text_aux_dark` (editor: pink) | Chooses dark for auxiliary text. Ordinary headings need their auxiliary-colour switch; does not recolour ordinary paragraphs. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **183** `color_text_aux_dark_emphasis` (editor: brown) | Chooses dark emphasis for auxiliary text. Ordinary headings need their auxiliary-colour switch; does not recolour ordinary paragraphs. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **184** `color_text_aux_info` (editor: yellow) | Chooses info for auxiliary text. Ordinary headings need their auxiliary-colour switch; does not recolour ordinary paragraphs. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **185** `color_text_aux_info_emphasis` (editor: pink) | Chooses info emphasis for auxiliary text. Ordinary headings need their auxiliary-colour switch; does not recolour ordinary paragraphs. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **186** `color_text_aux_light` (editor: brown) | Chooses light for auxiliary text. Ordinary headings need their auxiliary-colour switch; does not recolour ordinary paragraphs. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **187** `color_text_aux_light_emphasis` (editor: green) | Chooses light emphasis for auxiliary text. Ordinary headings need their auxiliary-colour switch; does not recolour ordinary paragraphs. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **188** `color_text_aux_primary` (editor: green) | Chooses primary for auxiliary text. Ordinary headings need their auxiliary-colour switch; does not recolour ordinary paragraphs. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **189** `color_text_aux_primary_emphasis` (editor: brown) | Chooses primary emphasis for auxiliary text. Ordinary headings need their auxiliary-colour switch; does not recolour ordinary paragraphs. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **190** `color_text_aux_secondary` (editor: green) | Chooses secondary for auxiliary text. Ordinary headings need their auxiliary-colour switch; does not recolour ordinary paragraphs. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **191** `color_text_aux_secondary_emphasis` (editor: default) | Chooses secondary emphasis for auxiliary text. Ordinary headings need their auxiliary-colour switch; does not recolour ordinary paragraphs. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **192** `color_text_aux_success` (editor: brown) | Chooses success for auxiliary text. Ordinary headings need their auxiliary-colour switch; does not recolour ordinary paragraphs. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **193** `color_text_aux_success_emphasis` (editor: pink) | Chooses success emphasis for auxiliary text. Ordinary headings need their auxiliary-colour switch; does not recolour ordinary paragraphs. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **194** `color_text_aux_warning` (editor: red) | Chooses warning for auxiliary text. Ordinary headings need their auxiliary-colour switch; does not recolour ordinary paragraphs. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **195** `color_text_aux_warning_emphasis` (editor: purple) | Chooses warning emphasis for auxiliary text. Ordinary headings need their auxiliary-colour switch; does not recolour ordinary paragraphs. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **196** `color_text_aux_white` (editor: green) | Chooses white for auxiliary text. Ordinary headings need their auxiliary-colour switch; does not recolour ordinary paragraphs. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **197** `color_text_aux_white_50` (editor: gray) | Chooses white at 50% opacity for auxiliary text. Ordinary headings need their auxiliary-colour switch; does not recolour ordinary paragraphs. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **198** `color_card_danger` (editor: pink) | Chooses danger text/background treatment for authored callout cards. Needs the callout-card style; not a generated Gallery-card control. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **199** `color_card_dark` (editor: red) | Chooses dark text/background treatment for authored callout cards. Needs the callout-card style; not a generated Gallery-card control. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **200** `color_card_info` (editor: blue) | Chooses info text/background treatment for authored callout cards. Needs the callout-card style; not a generated Gallery-card control. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **201** `color_card_light` (editor: default) | Chooses light text/background treatment for authored callout cards. Needs the callout-card style; not a generated Gallery-card control. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **202** `color_card_primary` (editor: gray) | Chooses primary text/background treatment for authored callout cards. Needs the callout-card style; not a generated Gallery-card control. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **203** `color_card_secondary` (editor: orange) | Chooses secondary text/background treatment for authored callout cards. Needs the callout-card style; not a generated Gallery-card control. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **204** `color_card_success` (editor: purple) | Chooses success text/background treatment for authored callout cards. Needs the callout-card style; not a generated Gallery-card control. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |
| **205** `color_card_warning` (editor: purple) | Chooses warning text/background treatment for authored callout cards. Needs the callout-card style; not a generated Gallery-card control. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/colours) |

## Badges, callouts, lists and quotes

These choices style particular Notion blocks inside a Page or Section. A clickable callout card needs URL text and visible child blocks. A badge needs inline-code text, not a Code block. The detailed recipes show how to author the underlying content.

| Option | What changes and what it needs | Try example |
| --- | --- | --- |
| **8** `callout_shadow` (editor: green) | Adds the shared shadow to ordinary or card-style Callouts. Optionally choose a shared shadow strength. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/cards) |
| **30** `badge` (editor: blue) | Turns inline-code text into a rounded badge. Needs inline code, not a Code block; an accent colour is optional. Its script can realign the parent block. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/cards) |
| **31** `badge_align_left` (editor: red) | Requests left alignment for a badge and its parent. Needs badge styling/script. Compatibility gap: the left-align rule is absent locally; test actual parent behavior. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/cards) |
| **32** `badge_align_center` (editor: red) | Centres a styled badge by changing its parent alignment. Needs badge styling/script; inspect adjacent text too. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/cards) |
| **33** `badge_align_right` (editor: default) | Right-aligns a styled badge by changing its parent alignment. Needs badge styling/script; inspect adjacent text too. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/cards) |
| **34** `callout_card` (editor: blue) | Turns a Callout into a clickable card: its own text is the destination URL; child blocks supply the visible heading/body. Test the outer-card link. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/cards) |
| **35** `bullet_check` (editor: purple) | Replaces bulleted-list dots with check-circle accents. Needs the icon script; this is presentation, not interactive task state. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/cards) |
| **45** `quote_no_indent` (editor: brown) | Removes the normal Quote border/indent treatment while preserving its quote role. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/cards) |

## Links and buttons

Create a labelled rich-text link and a real destination first. A button is a presentation of that link. Arrow and muted-link treatments replace the ordinary link template and can bypass button or underline choices; navigation has its own markup.

| Option | What changes and what it needs | Try example |
| --- | --- | --- |
| **6** `button_shadow` (editor: brown) | Adds the shared shadow to button-styled links. Needs the link-button treatment; optionally choose a shadow strength. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/cards) |
| **17** `button_secondary` (editor: blue) | Uses the secondary button treatment. Needs button-styled links; does not change an ordinary link alone. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/cards) |
| **18** `button_dark` (editor: blue) | Uses the dark button treatment. Needs button-styled links; does not change an ordinary link alone. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/cards) |
| **19** `link_button` (editor: pink) | Makes rich-text links look like primary buttons. Needs existing labelled links/destinations; optional button colour and shadow choices refine them. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/cards) |
| **20** `link_no_underline` (editor: blue) | Removes the ordinary CMS link's underline. Full link-template replacements, including NavBar links, can bypass this setting. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/cards) |
| **42** `link_arrow` (editor: gray) | Adds a circular arrow accent to rich-text links. Needs the icon script; an accent-colour choice is optional. Replaces the ordinary link template. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/cards) |
| **43** `link_muted` (editor: brown) | Gives rich-text/page links a muted, underline-on-hover treatment. Replaces the ordinary link template, so button modifiers may be bypassed. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/cards) |
| **44** `link_detect_appstores` (editor: pink) | Converts recognized platform/store labels or bracketed link labels into download treatments. Needs a Section, scripts and external icons; scans links across the document, including other sections/navigation. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/cards/download-links) |

**Download-label checks:** the inspected detector recognizes WINDOWS, WINDOWS STORE, MACOS, APPLE STORE, LINUX and GOOGLE STORE, plus bracketed labels such as [Download]. It needs its Section-end script, supporting libraries and external icons; test other page links because its scan is document-wide.

## Shadows

First enable the shadow on the intended image, callout, button or heading; then choose one shared strength here. A strength alone does not add shadows everywhere. The inspected Gallery card and badge shadows are independent of this shared setting.

| Option | What changes and what it needs | Try example |
| --- | --- | --- |
| **13** `shadow_lg` (editor: red) | Chooses large shared shadow strength. Needs a shadow-enabled image, callout, button or heading; fixed Gallery/badge shadows are separate. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/cards) |
| **14** `shadow_md` (editor: pink) | Chooses medium shared shadow strength. Needs a shadow-enabled image, callout, button or heading; fixed Gallery/badge shadows are separate. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/cards) |
| **15** `shadow_sm` (editor: orange) | Chooses small shared shadow strength. Needs a shadow-enabled image, callout, button or heading; fixed Gallery/badge shadows are separate. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/cards) |
| **16** `shadow_none` (editor: blue) | Chooses no shared shadow strength. Needs a shadow-enabled image, callout, button or heading; fixed Gallery/badge shadows are separate. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/cards) |

## Headers and slideshows

Select these on a Header row resolved by the content page. Closed Notion toggles supply slides; their children contain the heading, copy and image. An image captioned exactly `{BACKGROUND}` becomes a cover-style slide background. The inspected open-toggle path has a missing template, so begin with closed toggles. Percentage heights depend on the containing layout.

| Option | What changes and what it needs | Try example |
| --- | --- | --- |
| **1** `header_no_pause` (editor: orange) | Changes the Header carousel's pause setting. Test actual hover behavior with the installed carousel script; the flag alone is not proof. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/headers/header-no-pause) |
| **3** `header_no_slide_controls` (editor: blue) | Removes previous/next buttons and slide indicators. Test with at least two slides; one-slide headers already omit controls. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/headers/header-no-slide-controls) |
| **217** `header_small` (editor: yellow) | Requests a 25% header height treatment; actual height depends on its containing layout. Needs a resolved Header with slides. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/headers/header-small) |
| **218** `header_tiny` (editor: yellow) | Requests a 15% header height treatment; actual height depends on its containing layout. Needs a resolved Header with slides. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/headers/header-tiny) |
| **219** `header_particles` (editor: gray) | Adds an animated particle layer. Needs the external particle script; use only one such Header per route because the installed layer has a fixed identifier. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/headers/header-particles) |
| **220** `header_fade` (editor: red) | Requests a fade instead of the ordinary slide transition. Needs at least two Header slides and a live transition check. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/headers/header-fade) |
| **221** `header_large` (editor: pink) | Requests a 75% header height treatment; actual height depends on its containing layout. Needs a resolved Header with slides. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/headers/header-large) |
| **222** `header_full` (editor: gray) | Requests a 100% header height treatment, not an unconditional screen-height promise. Needs a resolved Header with slides. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/headers/header-full) |

## Navigation

Use a NavBar row at the intended route or branch. Add linked paragraphs and, when needed, a toggle containing dropdown links. Position changes need a long page and a scroll test; a fixed bar can cover a title or anchor. Use one positioning choice.

| Option | What changes and what it needs | Try example |
| --- | --- | --- |
| **206** `navbar_callout_productlink` (editor: default) | Makes a NavBar Callout a product entry: own text=destination URL, icon=image area, children=visible title/description. Test the complete entry's destination. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/navigation/navbar-callout-productlink) |
| **223** `navbar_fixed_top` (editor: gray) | Fixes the NavBar at the viewport top. Check that it does not cover titles, breadcrumbs or anchor targets. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/navigation/navbar-fixed-top) |
| **224** `navbar_sticky_top` (editor: red) | Requests a NavBar that sticks at the top within its scroll context. Test the complete page's placement and scroll behavior. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/navigation/navbar-sticky-top) |
| **225** `navbar_fixed_bottom` (editor: brown) | Fixes the NavBar at the viewport bottom. Check content/footer clearance and phone usability. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/navigation/navbar-fixed-bottom) |
| **226** `navbar_sticky_bottom` (editor: gray) | Requests bottom sticky positioning within the scroll context. Needs a suitable layout; do not assume it behaves like a fixed bottom bar. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/navigation/navbar-sticky-bottom) |

## Entrance animation

Motion choices need matching blocks and the website's animation scripts. Test their entrance from a fresh load/scroll position. A still screenshot cannot prove a transition, timing or reduced-motion behavior. Column hooks in the inspected templates cover only one-to-four-column layouts.

| Option | What changes and what it needs | Try example |
| --- | --- | --- |
| **207** `animate_columns_slide_fade` (editor: default) | Requests sliding/fading entrances for an existing one-to-four-column list. Needs animation scripts and live scroll/entry testing. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/motion) |
| **208** `animate_columns_flip` (editor: red) | Requests flipping entrances for an existing one-to-four-column list. Needs animation scripts and live scroll/entry testing. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/motion) |
| **209** `animate_columns_zoom` (editor: green) | Requests zooming entrances for an existing one-to-four-column list. Needs animation scripts and live scroll/entry testing. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/motion) |
| **210** `animate_image_zoom` (editor: gray) | Requests a zoom entrance for Image figures. Needs animation scripts and a live check; this is not click-to-enlarge. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/motion) |

## Advanced content rendering

Use these on a Page or Section only for reviewed advanced content, with a version-matched preview. Literal code examples should stay readable. These two presets have different jobs; neither should be selected merely because a page contains documentation.

| Option | What changes and what it needs | Try example |
| --- | --- | --- |
| **227** `code_inject` (editor: yellow) | Renders affected Code blocks as active raw markup/style/script. Never use on examples intended to remain literal; preview reviewed content with the actual renderer. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/motion/raw-html) |
| **228** `code_all` (editor: yellow) | Removes the usual content cover/title/container wrapper and emits its children. Does not by itself make code raw; outer page/framing may remain. | [Compare](https://staging.sphere10.com/products/localnotion/cms/style-lab/motion/content-only) |

## When a choice has no visible effect

Check the owning row's Type, the required Notion block and the installed theme before adding another modifier. Compare one change at a time. A successful save or render does not prove the browser used the expected style, and a still image does not prove an interaction.

For a step-by-step recipe, return to the [visual guide](visual-design.md). For schema and publication questions, use [the database reference](schema.md), [page types](page-types-and-publication.md) and [troubleshooting](troubleshooting.md).

[Notion CMS Guide](index.md)
