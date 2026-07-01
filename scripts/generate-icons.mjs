/**
 * Rasterize RGBJunkie Stream Deck plugin icons (72 / 144 px PNG).
 * Run: npm install && node generate-icons.mjs
 */
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import sharp from "sharp";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const OUT = path.resolve(__dirname, "../RGBJunkieDeckPlugin/Images");

const BG = "#1c1c22";
const BG_SOFT = "#25252d";
const ACCENT = "#00dc82";
const WHITE = "#f4f4f6";
const MUTED = "#9aa0ad";

/** Official RGBJunkie mark (from public/app-icon.svg, viewBox 0 0 1024 1024). */
const LOGO_PATH =
  "m 1.4133,138.05474 163.8128,159.29391 h 332.48372 c 20.18476,0.33892 19.73286,29.82524 0,29.48632 h -39.87997 l -140.22798,93.53499 27.64019,-93.14587 H 156.5538 L 8.7666,801.58102 255.8316,640.90381 315.82103,428.17301 496.30454,659.94498 h 161.96677 c 2.68916,0.0809 3.09026,1.37035 1.79741,3.91436 -7.0025,13.77927 -19.52724,21.5153 -31.92995,21.21744 H 464.43043 l -147.59264,201.09445 129.8076,-54.34069 h 223.57634 c 63.39674,-3.25863 148.46302,-41.00066 175.90115,-131.84113 L 1022.5867,171.36852 783.491,326.10569 669.7348,641.17201 555.65907,485.23656 C 753.25711,409.84337 750.34912,156.27823 581.1979,137.82878 Z";
const LOGO_VIEW = 1024;
/** Geometric center of the path bbox (~y 138–802) for optical centering on Stream Deck keys. */
const LOGO_ANCHOR_X = 512;
const LOGO_ANCHOR_Y = 470;
const CANVAS = 144;
const CANVAS_CENTER = CANVAS / 2;

/** Center the mark in the 1024×1024 SVG space on canvas point (cx, cy). */
function logoCentered({ cx = CANVAS_CENTER, cy = CANVAS_CENTER, size = 96, fill = ACCENT, opacity = 1 }) {
  const scale = size / LOGO_VIEW;
  return `<g transform="translate(${cx} ${cy}) scale(${scale}) translate(${-LOGO_ANCHOR_X} ${-LOGO_ANCHOR_Y})" opacity="${opacity}">
    <path fill="${fill}" d="${LOGO_PATH}"/>
  </g>`;
}

function gridIconOrigin(cell, cols, rows, gap = 6) {
  const w = cols * cell + (cols - 1) * gap;
  const h = rows * cell + (rows - 1) * gap;
  return {
    x: CANVAS_CENTER - w / 2,
    y: CANVAS_CENTER - h / 2,
    w,
    h,
  };
}

function roundedRect(w, h, r, fill) {
  return `<rect width="${w}" height="${h}" rx="${r}" fill="${fill}"/>`;
}

function chevron(dir, { x, y, size, stroke = WHITE, sw = 10 }) {
  const s = size / 2;
  const d =
    dir === "left"
      ? `M ${x + s} ${y - s} L ${x - s} ${y} L ${x + s} ${y + s}`
      : `M ${x - s} ${y - s} L ${x + s} ${y} L ${x - s} ${y + s}`;
  return `<path d="${d}" fill="none" stroke="${stroke}" stroke-width="${sw}" stroke-linecap="round" stroke-linejoin="round"/>`;
}

/** Three-segment RGB strip (lighting motif, not a painter palette). */
function rgbStrip(cx, cy, totalW, h = 9, gap = 5) {
  const seg = (totalW - gap * 2) / 3;
  const x0 = cx - totalW / 2;
  const colors = ["#ff5566", ACCENT, "#4cc9f0"];
  return colors
    .map(
      (fill, i) =>
        `<rect x="${x0 + i * (seg + gap)}" y="${cy - h / 2}" width="${seg}" height="${h}" rx="${h / 2}" fill="${fill}"/>`
    )
    .join("");
}

/** Effect action: centered RJ mark + optional RGB strip beneath. */
function effectGlyph(style) {
  const isList = style === "list";
  const logoSize = isList ? 68 : 82;
  const lift = isList ? 4 : 8;
  const cy = CANVAS_CENTER - lift;
  const bar = isList ? "" : rgbStrip(CANVAS_CENTER, cy + logoSize * 0.34 + 10, 52, 8);
  return `${logoCentered({ cy, size: logoSize, fill: ACCENT })}${bar}`;
}

/** Previous / next effect: chevron beside a smaller RJ mark. */
function cycleEffectGlyph(dir, style) {
  const isList = style === "list";
  const logoSize = isList ? 54 : 62;
  const chevronSize = isList ? 32 : 38;
  const gap = isList ? 10 : 12;
  const half = logoSize / 2 + gap + chevronSize / 2;
  const logoCx = dir === "left" ? CANVAS_CENTER + half - chevronSize / 2 - gap : CANVAS_CENTER - half + chevronSize / 2 + gap;
  const chevronCx = dir === "left" ? CANVAS_CENTER - half + chevronSize / 2 : CANVAS_CENTER + half - chevronSize / 2;
  return `${logoCentered({ cx: logoCx, size: logoSize, fill: ACCENT })}
    ${chevron(dir, { x: chevronCx, y: CANVAS_CENTER, size: chevronSize, stroke: ACCENT, sw: isList ? 8 : 10 })}`;
}

function gridIcon(x, y, cell, cols, rows) {
  let out = "";
  for (let r = 0; r < rows; r++) {
    for (let c = 0; c < cols; c++) {
      out += `<rect x="${x + c * (cell + 6)}" y="${y + r * (cell + 6)}" width="${cell}" height="${cell}" rx="4" fill="${r === 0 && c === 0 ? ACCENT : WHITE}" opacity="${r === 0 && c === 0 ? 1 : 0.85}"/>`;
    }
  }
  return out;
}

function gear(cx, cy, r) {
  return `<circle cx="${cx}" cy="${cy}" r="${r * 0.55}" fill="none" stroke="${WHITE}" stroke-width="7"/>
    <circle cx="${cx}" cy="${cy}" r="${r * 0.2}" fill="${WHITE}"/>
    ${[0, 45, 90, 135].map((a) => {
      const rad = (a * Math.PI) / 180;
      const x1 = cx + Math.cos(rad) * r * 0.55;
      const y1 = cy + Math.sin(rad) * r * 0.55;
      const x2 = cx + Math.cos(rad) * r * 0.95;
      const y2 = cy + Math.sin(rad) * r * 0.95;
      return `<line x1="${x1}" y1="${y1}" x2="${x2}" y2="${y2}" stroke="${WHITE}" stroke-width="8" stroke-linecap="round"/>`;
    }).join("")}`;
}

function docIcon(cx, cy, w, h) {
  const x = cx - w / 2;
  const y = cy - h / 2;
  return `<path d="M ${x} ${y} h ${w * 0.62} l ${w * 0.38} ${h * 0.38} v ${h * 0.62 - h * 0.38} H ${x} Z" fill="none" stroke="${WHITE}" stroke-width="7" stroke-linejoin="round"/>
    <path d="M ${x + w * 0.62} ${y} v ${h * 0.38} h ${w * 0.38}" fill="none" stroke="${WHITE}" stroke-width="7" stroke-linejoin="round"/>
    <line x1="${x + 14}" y1="${y + h * 0.55}" x2="${x + w * 0.7}" y2="${y + h * 0.55}" stroke="${MUTED}" stroke-width="6" stroke-linecap="round"/>
    <line x1="${x + 14}" y1="${y + h * 0.72}" x2="${x + w * 0.55}" y2="${y + h * 0.72}" stroke="${MUTED}" stroke-width="6" stroke-linecap="round"/>`;
}

function folderIcon(cx, cy, w, h) {
  const x = cx - w / 2;
  const y = cy - h / 2;
  return `<path d="M ${x} ${y + 16} h ${w * 0.42} l ${w * 0.12} 14 h ${w * 0.46} a 10 10 0 0 1 10 10 v ${h - 40} a 10 10 0 0 1 -10 10 H ${x + 10} a 10 10 0 0 1 -10 -10 V ${y + 26} a 10 10 0 0 1 10 -10 z" fill="none" stroke="${WHITE}" stroke-width="7" stroke-linejoin="round"/>`;
}

function restartArc(cx, cy, r) {
  return `<path d="M ${cx - r * 0.35} ${cy - r * 0.15} A ${r} ${r} 0 1 1 ${cx + r * 0.55} ${cy + r * 0.35}" fill="none" stroke="${WHITE}" stroke-width="8" stroke-linecap="round"/>
    <path d="M ${cx + r * 0.55} ${cy - r * 0.35} L ${cx + r * 0.55} ${cy + r * 0.35} L ${cx + r * 0.15} ${cy + r * 0.15}" fill="none" stroke="${WHITE}" stroke-width="8" stroke-linecap="round" stroke-linejoin="round"/>`;
}

function layersIcon(cx, cy) {
  return `<path d="M ${cx} ${cy - 28} L ${cx + 38} ${cy - 8} L ${cx} ${cy + 12} L ${cx - 38} ${cy - 8} Z" fill="${ACCENT}" opacity="0.95"/>
    <path d="M ${cx - 30} ${cy + 8} L ${cx} ${cy + 24} L ${cx + 30} ${cy + 8}" fill="none" stroke="${WHITE}" stroke-width="6" stroke-linejoin="round"/>
    <path d="M ${cx - 30} ${cy + 24} L ${cx} ${cy + 40} L ${cx + 30} ${cy + 24}" fill="none" stroke="${MUTED}" stroke-width="6" stroke-linejoin="round"/>`;
}

function plugIcon(cx, cy) {
  return `<rect x="${cx - 22}" y="${cy - 10}" width="44" height="20" rx="6" fill="none" stroke="${WHITE}" stroke-width="7"/>
    <line x1="${cx - 10}" y1="${cy - 10}" x2="${cx - 10}" y2="${cy - 28}" stroke="${WHITE}" stroke-width="7" stroke-linecap="round"/>
    <line x1="${cx + 10}" y1="${cy - 10}" x2="${cx + 10}" y2="${cy - 28}" stroke="${WHITE}" stroke-width="7" stroke-linecap="round"/>
    <line x1="${cx - 10}" y1="${cy + 10}" x2="${cx - 10}" y2="${cy + 28}" stroke="${ACCENT}" stroke-width="7" stroke-linecap="round"/>
    <line x1="${cx + 10}" y1="${cy + 10}" x2="${cx + 10}" y2="${cy + 28}" stroke="${ACCENT}" stroke-width="7" stroke-linecap="round"/>`;
}

function chipIcon(cx, cy) {
  return `<rect x="${cx - 30}" y="${cy - 22}" width="60" height="44" rx="8" fill="none" stroke="${WHITE}" stroke-width="7"/>
    ${[-1, 0, 1].map((i) => `<line x1="${cx - 42}" y1="${cy + i * 14}" x2="${cx - 30}" y2="${cy + i * 14}" stroke="${MUTED}" stroke-width="5" stroke-linecap="round"/>`).join("")}
    ${[-1, 0, 1].map((i) => `<line x1="${cx + 30}" y1="${cy + i * 14}" x2="${cx + 42}" y2="${cy + i * 14}" stroke="${MUTED}" stroke-width="5" stroke-linecap="round"/>`).join("")}
    <rect x="${cx - 12}" y="${cy - 8}" width="24" height="16" rx="3" fill="${ACCENT}"/>`;
}

function svgDoc(inner, { size = 144, bg = BG, radius = 22 } = {}) {
  return `<svg xmlns="http://www.w3.org/2000/svg" width="${size}" height="${size}" viewBox="0 0 144 144">
    ${roundedRect(144, 144, radius, bg)}
    ${inner}
  </svg>`;
}

const variants = {
  brand: (style) => {
    const sizes = { plugin: 104, category: 96, list: 80, key: 100 };
    return svgDoc(logoCentered({ size: sizes[style] ?? 96, fill: ACCENT }), {
      bg: style === "plugin" ? BG_SOFT : BG,
    });
  },
  effect: (style) => svgDoc(effectGlyph(style), { bg: BG }),
  prevEffect: (style) => svgDoc(cycleEffectGlyph("left", style), { bg: BG }),
  nextEffect: (style) => svgDoc(cycleEffectGlyph("right", style), { bg: BG }),
  scene: (style) => {
    const cell = style === "list" ? 20 : 24;
    const { x, y } = gridIconOrigin(cell, 2, 2);
    return svgDoc(gridIcon(x, y, cell, 2, 2), { bg: BG });
  },
  prevScene: (style) => {
    const cell = style === "list" ? 18 : 22;
    const gridW = 2 * cell + 6;
    const gap = 16;
    const chevronX = CANVAS_CENTER - gridW / 2 - gap;
    const gridX = CANVAS_CENTER + gap / 2;
    const gridY = CANVAS_CENTER - (2 * cell + 6) / 2;
    return svgDoc(
      `${chevron("left", { x: chevronX, y: CANVAS_CENTER, size: 36, stroke: ACCENT, sw: 9 })}
       ${gridIcon(gridX, gridY, cell, 2, 2)}`,
      { bg: BG }
    );
  },
  nextScene: (style) => {
    const cell = style === "list" ? 18 : 22;
    const gridW = 2 * cell + 6;
    const gap = 16;
    const gridX = CANVAS_CENTER - gridW / 2 - gap / 2;
    const chevronX = CANVAS_CENTER + gridW / 2 + gap;
    const gridY = CANVAS_CENTER - (2 * cell + 6) / 2;
    return svgDoc(
      `${gridIcon(gridX, gridY, cell, 2, 2)}
       ${chevron("right", { x: chevronX, y: CANVAS_CENTER, size: 36, stroke: ACCENT, sw: 9 })}`,
      { bg: BG }
    );
  },
  viewEffects: () => svgDoc(layersIcon(CANVAS_CENTER, CANVAS_CENTER), { bg: BG }),
  viewHardware: () => svgDoc(chipIcon(CANVAS_CENTER, CANVAS_CENTER), { bg: BG }),
  viewInstalled: () => svgDoc(plugIcon(CANVAS_CENTER, CANVAS_CENTER), { bg: BG }),
  viewLogs: () => svgDoc(docIcon(CANVAS_CENTER, CANVAS_CENTER, 68, 84), { bg: BG }),
  openPlugins: () => svgDoc(folderIcon(CANVAS_CENTER, CANVAS_CENTER, 76, 72), { bg: BG }),
  restart: () => svgDoc(restartArc(CANVAS_CENTER, CANVAS_CENTER, 42), { bg: BG }),
};

async function writePng(svg, filePath, size) {
  await sharp(Buffer.from(svg)).resize(size, size).png().toFile(filePath);
}

async function writePair(svg, basePath) {
  await writePng(svg, `${basePath}.png`, 72);
  await writePng(svg, `${basePath}@2x.png`, 144);
}

async function main() {
  fs.mkdirSync(path.join(OUT, "Actions"), { recursive: true });

  await writePair(variants.brand("plugin"), path.join(OUT, "pluginIcon"));
  await writePair(variants.brand("category"), path.join(OUT, "categoryIcon"));
  await writePair(variants.brand("list"), path.join(OUT, "icon"));
  await writePair(variants.brand("key"), path.join(OUT, "pluginAction"));

  const actionMap = [
    ["Effect", "effect", variants.effect],
    ["Effect", "prev-effect", variants.prevEffect],
    ["Effect", "next-effect", variants.nextEffect],
    ["Scene", "scene", variants.scene],
    ["Scene", "prev-scene", variants.prevScene],
    ["Scene", "next-scene", variants.nextScene],
    ["Views", "view-effects", variants.viewEffects],
    ["Views", "view-hardware", variants.viewHardware],
    ["Views", "view-installed", variants.viewInstalled],
    ["Views", "view-logs", variants.viewLogs],
    ["Views", "open-plugins", variants.openPlugins],
    ["Views", "restart", variants.restart],
  ];

  for (const [folder, slug, fn] of actionMap) {
    const dir = path.join(OUT, "Actions", folder);
    fs.mkdirSync(dir, { recursive: true });
    await writePair(fn("key"), path.join(dir, `action-${slug}`));
    await writePair(fn("list"), path.join(dir, `actionList-${slug}`));
  }

  console.log(`Wrote RGBJunkie Deck icons to ${OUT}`);
}

main().catch((err) => {
  console.error(err);
  process.exit(1);
});
