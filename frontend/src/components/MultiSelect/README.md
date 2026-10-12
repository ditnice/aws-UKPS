# MultiSelect

A controlled, searchable multi-select combobox styled with NICE Design System tokens.
It does not fetch options or save selections.

```tsx
<MultiSelect
  label="Categories (optional)"
  name="categories"
  options={categories}
  value={selectedCategoryIds}
  onChange={setSelectedCategoryIds}
  maxSelections={3}
  hint="Select up to 3 categories."
/>
```

- Options have unique string `value` IDs, a `label`, and optional `disabled` state.
  Keep selected options in the supplied list so their labels remain available.
- Omit `maxSelections` for unlimited selections. Validate limits, recognised IDs and
  uniqueness in the consuming form and server as well. The component prevents new
  additions at the limit but never silently discards controlled values.
- Selected items appear below the input with labelled remove buttons. Even a disabled
  option can be removed; disabling the whole component disables every control.
- `onBlur` runs when focus leaves the whole component. `inputRef` supports focusing
  the input from form error summaries. Hint and error IDs are merged with
  `aria-describedby`; errors set `aria-invalid`.
- `width` uses the existing `InputWidth` scale and applies to the input and panel.
  `className` and `style` apply to the outer wrapper. IDs are unique by default;
  explicitly supplied IDs must also be unique.
- Native form submission includes a hidden input per selected value using `name`.
  Search text is never submitted. Empty selections submit no values.
- Arrow keys navigate options; Enter toggles the active option; Escape closes;
  Tab moves normally. Text-editing keys and IME composition are preserved. The
  scrollable listbox stays in document flow rather than covering other controls.

## Verification

Run `pnpm exec vitest run src/components/MultiSelect/MultiSelect.spec.tsx` for unit tests.
Unit tests cover filtering, selection limits, keyboard behaviour, focus management,
disabled states and form values. Interactive examples are available at
`/portal/components/multi-select`. Before release, check axe WCAG 2.2 AA results,
NVDA with Firefox/Chrome, VoiceOver with Safari, 200% text resizing, 400% browser
zoom, forced colours, touch interaction and focus visibility in the consuming page.
Automated checks do not establish complete WCAG conformance.
