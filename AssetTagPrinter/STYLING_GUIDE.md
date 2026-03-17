# Print Styling & Orientation Guide

## Overview

The `PrintStyleSettings` class provides a flexible, orientation-aware styling system that automatically adjusts formatting without breaking layout. This prevents the common problem of font sizes or margins being hardcoded, which causes issues when orientation changes.

---

## Current Defaults

These are the optimized default values currently in use:

| Property | Value | Notes |
|----------|-------|-------|
| **Header Font** | Arial 11pt Bold | Primary title styling |
| **Secondary Font** | Arial 8pt **Bold** | Address/contact styling |
| **Body Font** | Arial 9pt Regular | ID/label content |
| **Left Margin** | 35pt | Increased for better padding |
| **Top Margin** | 15pt | Optimized top spacing |
| **Right Margin** | 12pt | Standard right spacing |
| **Bottom Margin** | 12pt | Standard bottom spacing |
| **Line Spacing** | 1pt | Compact spacing between lines |
| **Orientation** | Portrait | Can be changed to Landscape |
| **Auto Font Scale** | True | Fonts scale 15% in landscape |

---

## Key Improvements

### 1. **Font Selection: Consolas → Arial**
- **Previous**: Consolas (monospace)
- **New**: Arial (proportional)
- **Why**: Proportional fonts better utilize space for mixed-width content like company names and addresses
- **Benefit**: Text flows naturally and fits page dimensions more intelligently

### 2. **Orientation Support**
- **Portrait** (default): Optimized for standard vertical printing (taller than wide)
- **Landscape**: Wider layout with auto-scaled fonts for better readability

```csharp
// Portrait (default)
var settings = PrintStyleSettings.CreateDefault();
// or
var settings = PrintStyleSettings.CreatePortrait();

// Landscape
var settings = PrintStyleSettings.CreateLandscape();
```

### 3. **Automatic Font Scaling**
When `AutoScaleFonts` is enabled (default), fonts scale based on orientation:
- **Portrait**: Sizes remain as configured (11pt header, 8pt secondary, 9pt body)
- **Landscape**: Fonts increase by 15% for better readability on wider page

| Element | Portrait | Landscape |
|---------|----------|-----------|
| Header | 11pt Bold Arial | 12.65pt Bold Arial |
| Secondary | 8pt Bold Arial | 9.2pt Bold Arial |
| Body | 9pt Arial | 10.35pt Arial |

**Override**: Set `AutoScaleFonts = false` to ignore orientation scaling

### 4. **Margin Management**
All four margins are now configurable:
```csharp
settings.LeftMargin = 35f;    // Space from left edge (default: 35pt)
settings.RightMargin = 12f;   // Space from right edge (default: 12pt)
settings.TopMargin = 15f;     // Space from top (default: 15pt)
settings.BottomMargin = 12f;  // Space from bottom (default: 12pt)
settings.ExtraLineSpacing = 1f; // Space between lines (default: 1pt - compact)
```

Helper methods calculate usable content area:
```csharp
float contentWidth = settings.GetContentWidth(pageWidth);
float contentHeight = settings.GetContentHeight(pageHeight);
```

---

## Why This Architecture Prevents Breaking Orientation

### **Before (Hard-coded)**
```csharp
// Bad - breaks when orientation changes
float contentWidth = e.MarginBounds.Width - 20f; // ❌ Fixed math
Font header = new Font("Consolas", 10f); // ❌ Size doesn't scale
```

### **After (Configuration-based)**
```csharp
// Good - adapts to orientation automatically
float contentWidth = settings.GetContentWidth(e.MarginBounds.Width);
Font header = CreateScaledFont(settings.Header, settings.Orientation);
```

All sizing flows from `settings`, which respects orientation. Change `Orientation` once, everything adapts.

---

## Default Font Properties

### Header (Company Name)
- **Font**: Arial
- **Size**: 11pt (Portrait) / 12.65pt (Landscape)
- **Style**: Bold
- **Purpose**: Primary title - stands out, easily readable

### Secondary (Address/Contact)
- **Font**: Arial
- **Size**: 8pt (Portrait) / 9.2pt (Landscape)
- **Style**: Bold
- **Purpose**: Supplementary info - smaller but still clear

### Body (ID, Label, Reference)
- **Font**: Arial
- **Size**: 9pt (Portrait) / 10.35pt (Landscape)
- **Style**: Regular
- **Purpose**: Main content - readable at standard size

### Margins & Spacing
- **Left Margin**: 35pt (increased for better left padding)
- **Top Margin**: 15pt (optimized top spacing)
- **Right Margin**: 12pt
- **Bottom Margin**: 12pt
- **Line Spacing**: 1pt (compact spacing)

---

## Custom Styling Example

```csharp
var settings = PrintStyleSettings.CreateDefault();

// Adjust from current defaults if needed
settings.Header = new TextSectionStyle("Arial", 11f, FontStyle.Bold);
settings.Secondary = new TextSectionStyle("Arial", 8f, FontStyle.Bold); // Bold by default now
settings.Body = new TextSectionStyle("Arial", 9f, FontStyle.Regular);

// Modify margins for different layout
settings.LeftMargin = 40f;       // More left padding
settings.TopMargin = 20f;        // More top padding
settings.ExtraLineSpacing = 2f;  // Increase spacing from compact default

// Landscape layout with more breathing room
settings.Orientation = PrintOrientation.Landscape;
settings.AutoScaleFonts = true;  // Auto-scale to 1.15x for landscape

printerService.StyleSettings = settings;
```

---

## Font Recommendations

### Best for Receipts/Tickets
- **Arial** (default) - Clean, universally available
- **Segoe UI** - Modern platform font, excellent legibility
- **Helvetica** - Professional, similar to Arial
- **Verdana** - Large x-height, very readable at small sizes

### Avoid
- ❌ **Consolas** - Too wide for proportional layout
- ❌ **Courier New** - Monospace, wastes horizontal space
- ❌ **Decorative fonts** - Poor visibility on thermal printers

---

## Auto-Scale Font Logic

When `AutoScaleFonts = true`:

```
Landscape Multiplier = 1.15 (15% larger)
Portrait Multiplier  = 1.0  (no scaling)

Effective Size = BaseSize × Multiplier
```

Example:
- Header base: 11pt → Landscape becomes 11 × 1.15 = **12.65pt**
- Secondary base: 8pt → Landscape becomes 8 × 1.15 = **9.2pt**
- Body base: 9pt → Landscape becomes 9 × 1.15 = **10.35pt**

This ensures Landscape has better text-to-space ratio without manual adjustment.

---

## Fallback Font Hierarchy

If requested font is unavailable:
1. Try requested font (e.g., "Arial")
2. Fallback to Arial
3. Fallback to system default

```csharp
new TextSectionStyle("CustomFont", 11f, FontStyle.Bold)
// Will use "CustomFont" if available, else Arial, else system default
```

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Text too small in landscape | Increase base size or set `AutoScaleFonts = true` |
| Text gets cut off | Reduce font size or increase margins with `GetContentWidth()` |
| Font not applying | Check font is installed on printing system |
| Orientation not changing | Verify `document.DefaultPageSettings.Landscape` is set from `settings.Orientation` |
| Spacing too tight (default is compact) | Increase `ExtraLineSpacing` from default 1f |
| Secondary text not bold | Ensure `Secondary` style has `FontStyle.Bold` set |

---

## Summary

✅ **Proportional fonts** (Arial) instead of monospace for better layout
✅ **Orientation-aware** sizing prevents format breaking
✅ **Auto-scaling** adjusts fonts intelligently for readability
✅ **Flexible margins** for professional appearance
✅ **Configuration-driven** approach (no hard-coded dimensions)
✅ **Fallbacks** ensure fonts render even if not installed
