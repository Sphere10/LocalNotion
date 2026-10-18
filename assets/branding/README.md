# Local Notion branding

Use the cube, cloud and transfer-arrow mark with the **Local Notion** wordmark. The mark retains the original two vector paths; the wordmark uses outlined Inter at weight 650. Outlines keep its appearance consistent without an installed font or a network request.

| Asset | Use |
| --- | --- |
| `local-notion-logo.svg` | Preferred horizontal logo on light backgrounds |
| `local-notion-logo-dark.svg` | Light logo on dark backgrounds |
| `local-notion-logo.png`, `local-notion-logo-dark.png` | Transparent 1600px exports for editors that do not accept SVG |
| `local-notion-icon.svg` | Original, unchanged vector mark |
| `local-notion-icon.png` | 1024px application mark on a white rounded tile |
| `local-notion.ico` | Windows executable icon, with 16, 20, 24, 32, 40, 48, 64, 128 and 256px frames |

Keep the proportions intact and leave clear space of at least one quarter of the mark's height around a lockup. Use the icon alone where the full name would be too small. Prefer SVG on the web; the transparent PNG is suitable for a native Notion image. The dark-background variant changes the ink color without changing geometry.

The native CLI apphost and Windows Docker launcher embed the same ICO. They do not load artwork at runtime. The repository README uses the SVG. The website uses the PNG from this same master, authored as a native Notion image; its existing layout remains the presentation owner.

## Sources

The original product mark is retained byte-for-byte as `local-notion-icon.svg` (SHA-256 `60a642015f12cfb76d59233d924f9adf1b9ef2d8ebc98e3660715eaad0a58c7b`). It is the existing Local Notion mark supplied with this product, separate from the vendored SDK's `Notion.Client/notion-logo.png`.

Typography comes from [Inter](https://github.com/google/fonts/tree/main/ofl/inter), with its [SIL Open Font License](https://github.com/google/fonts/blob/main/ofl/inter/OFL.txt) retained in `Inter-OFL.txt`. Source variable font SHA-256: `29160a80ff49ddcab2c97711247e08b1fab27a484a329ce8b813d820dc559031`; the outlines use `wght=650`, `opsz=32`. The font itself is not a product runtime dependency. The product's existing license remains unchanged.

Edit the vector masters and export raster derivatives from those masters. Do not enlarge a small PNG to create a new master or regenerate the preserved mark from a screenshot. Recheck transparent edges, both background variants and small executable-icon frames after an artwork change.
