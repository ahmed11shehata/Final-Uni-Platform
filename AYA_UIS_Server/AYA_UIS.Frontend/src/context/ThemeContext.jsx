// src/context/ThemeContext.jsx
import { createContext, useContext, useState, useEffect } from "react";

export const THEMES = {

  /* ── 1. Default (Light) ── */
  default: {
    id: "default", name: "Default", emoji: "☀️",
    desc: "Clean & professional", dark: false,
    vars: {
      "--page-bg": "#f0f4f8", "--sidebar-bg": "#1c2035",
      "--topbar-bg": "#ffffff", "--topbar-border": "#e8edf4",
      "--card-bg": "#ffffff", "--card-bg-2": "#f7f9fc",
      "--card-border": "#e2e8f0", "--border": "#e2e8f0",
      "--text-primary": "#0f172a", "--text-secondary": "#334155",
      "--text-muted": "#64748b", "--prog-track": "#f1f5f9",
      "--detail-bg": "#f7f9fc", "--detail-border": "#edf2f7",
      "--input-bg": "#ffffff", "--hover-bg": "#f1f5f9",
      "--accent": "#818cf8",
    },
  },

  /* ── 2. Light Sage (Light) ── */
  light: {
    id: "light", name: "Light Sage", emoji: "🌿",
    desc: "Fresh, calm & easy on eyes", dark: false,
    vars: {
      "--page-bg": "#f0f5f2", "--sidebar-bg": "#1a3328",
      "--topbar-bg": "#ffffff", "--topbar-border": "#d4e8dc",
      "--card-bg": "#ffffff", "--card-bg-2": "#eef6f1",
      "--card-border": "#cce4d8", "--border": "#cce4d8",
      "--text-primary": "#0d2218", "--text-secondary": "#1e5236",
      "--text-muted": "#4a8a65", "--prog-track": "#d8f0e4",
      "--detail-bg": "#eef6f1", "--detail-border": "#d4e8dc",
      "--input-bg": "#ffffff", "--hover-bg": "#e4f2ea",
      "--accent": "#3a8a60",
    },
  },

  /* ── 3. Dark Slate (Dark) ── */
  dark: {
    id: "dark", name: "Dark Slate", emoji: "🌑",
    desc: "Comfortable dark mode", dark: true,
    vars: {
      "--page-bg": "#111827", "--sidebar-bg": "#0b0f1a",
      "--topbar-bg": "#161e2e", "--topbar-border": "#1e2d40",
      "--card-bg": "#1e2535", "--card-bg-2": "#253045",
      "--card-border": "#2a3850", "--border": "#2e405a",
      "--text-primary": "#e8edf8",   /* ← bright white-blue */
      "--text-secondary": "#a0b8d8", /* ← light blue-grey */
      "--text-muted": "#6a8aaa",     /* ← visible muted */
      "--prog-track": "#253045",
      "--detail-bg": "#253045", "--detail-border": "#2a3850",
      "--input-bg": "#1e2535", "--hover-bg": "#2a3850",
      "--accent": "#818cf8",
    },
  },

  /* ── 4. Ramadan Night (Dark) ── */
  ramadan: {
    id: "ramadan", name: "Ramadan Night", emoji: "🌙",
    desc: "The magic of holy nights", dark: true,
    vars: {
      "--page-bg": "#0e0820", "--sidebar-bg": "#08041a",
      "--topbar-bg": "#140a28", "--topbar-border": "#2c1458",
      "--card-bg": "#1a0e38", "--card-bg-2": "#221548",
      "--card-border": "#3a1e6a", "--border": "#4a2880",
      "--text-primary": "#f0e4c0",   /* ← warm cream gold */
      "--text-secondary": "#d4b050", /* ← bright amber */
      "--text-muted": "#a07840",     /* ← visible warm muted */
      "--prog-track": "#221548",
      "--detail-bg": "#221548", "--detail-border": "#3a1e6a",
      "--input-bg": "#1a0e38", "--hover-bg": "#2c1a50",
      "--accent": "#c4913a",
    },
  },

  /* ── 5. Deep Ocean (Dark) ── */
  ocean: {
    id: "ocean", name: "Deep Ocean", emoji: "🌊",
    desc: "Calm like the deep sea", dark: true,
    vars: {
      "--page-bg": "#071828", "--sidebar-bg": "#040d18",
      "--topbar-bg": "#0b2035", "--topbar-border": "#0f2e48",
      "--card-bg": "#0e2540", "--card-bg-2": "#133050",
      "--card-border": "#1a3e60", "--border": "#1e4a72",
      "--text-primary": "#d0e8f8",   /* ← bright ice blue */
      "--text-secondary": "#80c8e8", /* ← clear sky blue */
      "--text-muted": "#5090b8",     /* ← visible ocean blue */
      "--prog-track": "#133050",
      "--detail-bg": "#133050", "--detail-border": "#1a3e60",
      "--input-bg": "#0e2540", "--hover-bg": "#1a3e60",
      "--accent": "#3b82f6",
    },
  },

  /* ── 6. Cherry Blossom (Light) ── */
  sakura: {
    id: "sakura", name: "Cherry Blossom", emoji: "🌸",
    desc: "Soft spring vibes", dark: false,
    vars: {
      "--page-bg": "#fff4f7", "--sidebar-bg": "#420816",
      "--topbar-bg": "#ffffff", "--topbar-border": "#ffd0de",
      "--card-bg": "#ffffff", "--card-bg-2": "#fff0f5",
      "--card-border": "#ffc8d8", "--border": "#ffb0c8",
      "--text-primary": "#280810", "--text-secondary": "#7a1234",
      "--text-muted": "#c0608a",
      "--prog-track": "#fde8f0",
      "--detail-bg": "#fff5f9", "--detail-border": "#ffd0de",
      "--input-bg": "#ffffff", "--hover-bg": "#fde8f0",
      "--accent": "#e05c8a",
    },
  },

  /* ── 7. Forest Night (Dark) ── */
  forest: {
    id: "forest", name: "Forest Night", emoji: "🌲",
    desc: "Deep emerald & earth tones", dark: true,
    vars: {
      "--page-bg": "#0b1c10", "--sidebar-bg": "#060e08",
      "--topbar-bg": "#0e2014", "--topbar-border": "#1a3822",
      "--card-bg": "#132818", "--card-bg-2": "#1a3520",
      "--card-border": "#204828", "--border": "#285835",
      "--text-primary": "#ccead4",   /* ← bright mint green */
      "--text-secondary": "#78c890", /* ← clear spring green */
      "--text-muted": "#52986a",     /* ← visible forest green */
      "--prog-track": "#1a3520",
      "--detail-bg": "#1a3520", "--detail-border": "#204828",
      "--input-bg": "#132818", "--hover-bg": "#204828",
      "--accent": "#22c55e",
    },
  },

  /* ── 8. Sunset Gold (Light) ── */
  sunset: {
    id: "sunset", name: "Sunset Gold", emoji: "🌅",
    desc: "Warm amber glow", dark: false,
    vars: {
      "--page-bg": "#fef7ec", "--sidebar-bg": "#3a1005",
      "--topbar-bg": "#ffffff", "--topbar-border": "#fdd8a0",
      "--card-bg": "#ffffff", "--card-bg-2": "#fff8ee",
      "--card-border": "#fcdca0", "--border": "#f8c878",
      "--text-primary": "#1a0800", "--text-secondary": "#6a2808",
      "--text-muted": "#a05818",
      "--prog-track": "#fdefd0",
      "--detail-bg": "#fff8ee", "--detail-border": "#fdd8a0",
      "--input-bg": "#ffffff", "--hover-bg": "#fdefd0",
      "--accent": "#d97706",
    },
  },

  /* ── 9. Space Explorer (Dark) ── */
  space: {
    id: "space", name: "Space Explorer", emoji: "🚀",
    desc: "Cosmic dark — stars & nebula", dark: true,
    vars: {
      "--page-bg": "#060810", "--sidebar-bg": "#030508",
      "--topbar-bg": "#0c0e1c", "--topbar-border": "#181c38",
      "--card-bg": "#0e1028", "--card-bg-2": "#151840",
      "--card-border": "#1e2258", "--border": "#252868",
      "--text-primary": "#c8d4f8",   /* ← bright starlight */
      "--text-secondary": "#8898e8", /* ← bright nebula blue */
      "--text-muted": "#5868c0",     /* ← visible space blue */
      "--prog-track": "#151840",
      "--detail-bg": "#151840", "--detail-border": "#1e2258",
      "--input-bg": "#0e1028", "--hover-bg": "#1e2258",
      "--accent": "#8898e8",
    },
  },

  /* ── 10. Nordic Ice (Light) ── */
  nordic: {
    id: "nordic", name: "Nordic Ice", emoji: "❄️",
    desc: "Cool, clean Scandinavian", dark: false,
    vars: {
      "--page-bg": "#f2f5fc", "--sidebar-bg": "#1c2e42",
      "--topbar-bg": "#ffffff", "--topbar-border": "#d4deee",
      "--card-bg": "#ffffff", "--card-bg-2": "#eef2fa",
      "--card-border": "#ccd8ec", "--border": "#b8ccdf",
      "--text-primary": "#132038", "--text-secondary": "#243858",
      "--text-muted": "#527090",
      "--prog-track": "#dce8f4",
      "--detail-bg": "#eef2fa", "--detail-border": "#d4deee",
      "--input-bg": "#ffffff", "--hover-bg": "#e0e8f8",
      "--accent": "#527090",
    },
  },

  /* ── 11. Caramel Latte (Light) ── */
  caramel: {
    id: "caramel", name: "Caramel Latte", emoji: "☕",
    desc: "Warm cozy coffee vibes", dark: false,
    vars: {
      "--page-bg": "#fdf5ec", "--sidebar-bg": "#2a1408",
      "--topbar-bg": "#fffaf5", "--topbar-border": "#e8c89a",
      "--card-bg": "#fffaf5", "--card-bg-2": "#f7ede0",
      "--card-border": "#e0c4a0", "--border": "#d0a878",
      "--text-primary": "#180a00", "--text-secondary": "#502810",
      "--text-muted": "#905830",
      "--prog-track": "#f0e0c8",
      "--detail-bg": "#f7ede0", "--detail-border": "#e8c89a",
      "--input-bg": "#fffaf5", "--hover-bg": "#f0e0c8",
      "--accent": "#905830",
    },
  },

  /* ── 12. Arctic Blizzard (Dark) ── */
  arctic: {
    id: "arctic", name: "Arctic Blizzard", emoji: "🌨️",
    desc: "Frozen tundra — ice, snow & frost", dark: true,
    vars: {
      "--page-bg": "#040c18", "--sidebar-bg": "#020609",
      "--topbar-bg": "#06101e", "--topbar-border": "#0e2238",
      "--card-bg": "#081428", "--card-bg-2": "#0c1e38",
      "--card-border": "#1a3858", "--border": "#1e4870",
      "--text-primary": "#d8f0ff",   /* ← bright ice white */
      "--text-secondary": "#90c8f0", /* ← clear arctic blue */
      "--text-muted": "#5898c8",     /* ← visible glacier blue */
      "--prog-track": "#0c1e38",
      "--detail-bg": "#0c1e38", "--detail-border": "#1a3858",
      "--input-bg": "#081428", "--hover-bg": "#112540",
      "--accent": "#5898c8",
    },
  },

  /* ── 13. Pharaoh's Egypt (Dark) ── */
  pharaoh: {
    id: "pharaoh", name: "Pharaoh's Egypt", emoji: "🏺",
    desc: "Ancient civilization — gold, papyrus & scarab", dark: true,
    vars: {
      "--page-bg": "#150900", "--sidebar-bg": "#0a0500",
      "--topbar-bg": "#1e0e02", "--topbar-border": "#3c1e08",
      "--card-bg": "#241408", "--card-bg-2": "#301c0e",
      "--card-border": "#503010", "--border": "#6a4020",
      "--text-primary": "#f0d888",   /* ← bright papyrus gold */
      "--text-secondary": "#d8a848", /* ← bright amber */
      "--text-muted": "#a07838",     /* ← visible desert gold */
      "--prog-track": "#301c0e",
      "--detail-bg": "#301c0e", "--detail-border": "#503010",
      "--input-bg": "#241408", "--hover-bg": "#3c2010",
      "--accent": "#d4a843",
    },
  },

  /* ── 14. Saladin's Legacy (Dark) ── */
  saladin: {
    id: "saladin", name: "Saladin's Legacy", emoji: "⚔️",
    desc: "Islamic golden age — brass, turquoise & desert sand", dark: true,
    vars: {
      "--page-bg": "#0e0c04", "--sidebar-bg": "#080600",
      "--topbar-bg": "#16120a", "--topbar-border": "#34280e",
      "--card-bg": "#201808", "--card-bg-2": "#2c2210",
      "--card-border": "#4a380a", "--border": "#6a5418",
      "--text-primary": "#f0e0a8",   /* ← bright sand gold */
      "--text-secondary": "#d8b050", /* ← bright brass */
      "--text-muted": "#a08038",     /* ← visible warm gold */
      "--prog-track": "#2c2210",
      "--detail-bg": "#2c2210", "--detail-border": "#4a380a",
      "--input-bg": "#201808", "--hover-bg": "#382a0e",
      "--accent": "#d8b050",
    },
  },

  /* ── 15. Silent Fog (Dark) ── */
  fog: {
    id: "fog", name: "Silent Fog", emoji: "🌫️",
    desc: "Eerie mist — ghostly pale & haunting silence", dark: true,
    vars: {
      "--page-bg": "#0a0e10", "--sidebar-bg": "#050708",
      "--topbar-bg": "#0e1418", "--topbar-border": "#1a2228",
      "--card-bg": "#121c20", "--card-bg-2": "#182428",
      "--card-border": "#243038", "--border": "#2e3e48",
      "--text-primary": "#c0d8d0",   /* ← bright misty white-green */
      "--text-secondary": "#88b0a8", /* ← clear fog teal */
      "--text-muted": "#5c8880",     /* ← visible mist */
      "--prog-track": "#182428",
      "--detail-bg": "#182428", "--detail-border": "#243038",
      "--input-bg": "#121c20", "--hover-bg": "#1e2c32",
      "--accent": "#5c8880",
    },
  },
};

const ThemeContext = createContext(null);

export function ThemeProvider({ children }) {
  const [themeId, setThemeId] = useState(
    () => localStorage.getItem("uni_theme") || "default"
  );
  const theme = THEMES[themeId] || THEMES.default;

  useEffect(() => {
    const root = document.documentElement;
    Object.entries(theme.vars).forEach(([k, v]) => root.style.setProperty(k, v));
    root.setAttribute("data-theme", themeId);
    localStorage.setItem("uni_theme", themeId);
  }, [themeId, theme]);

  return (
    <ThemeContext.Provider value={{ themeId, theme, setTheme: setThemeId, themes: THEMES }}>
      {children}
    </ThemeContext.Provider>
  );
}

export const useTheme = () => useContext(ThemeContext);
