# Chronos Descent Localization Guide

This directory contains all the translation files for Chronos Descent. If you'd like to contribute a translation or improve an existing one, this guide will help you get started.

## Translation Files

- `chronos_descent.pot`: The template file containing all translatable strings
- Language files: Named according to locale codes (e.g., `en.po`, `zh_CN.po`, etc.)

## How to Contribute a Translation

### Method 1: Using Poedit (Recommended)

1. Download and install [Poedit](https://poedit.net/) (free version is sufficient)
2. Open Poedit and select "Create new translation" from the file menu
3. Select the `chronos_descent.pot` file as the template
4. Choose the language you want to translate to
5. Fill in the translations for each string
6. Save the file with the appropriate locale code (e.g., `fr.po` for French)
7. Submit your translation file via pull request or email

### Method 2: Manual Editing

1. Copy the `chronos_descent.pot` file
2. Rename it to match your target locale (e.g., `de.po` for German)
3. Edit the file with a text editor, filling in the `msgstr` fields with your translations
4. Submit your translation file via pull request or email

## Locale Codes

When creating translation files, use the appropriate locale code:

- `en` - English
- `zh_CN` - Simplified Chinese
- `fr` - French
- `de` - German
- `es` - Spanish
- `it` - Italian
- `ja` - Japanese
- `ko` - Korean
- `ru` - Russian
- `pt_BR` - Brazilian Portuguese

For other languages, refer to the [ISO 639-1](https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes) standard.

## Guidelines for Translation

1. **Context**: Understand the context of each string. If you're unsure, check the source code or ask the developers.
2. **Terminology**: Be consistent with terminology across the translation.
3. **Length**: Try to keep translations roughly the same length as the original strings when possible.
4. **Placeholders**: Keep format specifiers like `%d`, `%s`, etc. intact - these will be replaced with values at runtime.
5. **Escape Characters**: Preserve any escape sequences like `\n` (new line) or `\"` (quotation mark).

## Adding a New Language to the Game

After creating your translation file:

1. Place the `.po` file in this directory
2. Add your language to the `_availableLanguages` dictionary in `TranslationManager.cs`
3. Rebuild the game, and your language should appear in the language selector

## Testing Translations

To test your translations:

1. Place your `.po` file in the Translations directory
2. Run the game
3. Select your language from the language selector
4. Navigate through the game to ensure all strings are properly translated

## String Updates

When new strings are added to the game, the `chronos_descent.pot` file will be updated. If you've already created a translation, you'll need to update it with the new strings.

In Poedit, open your translation file and use the "Update from POT file" option in the Catalog menu, selecting the updated `.pot` file.

## Questions or Issues

If you have any questions or issues with the translation process, please contact the development team.

Thank you for helping make Chronos Descent accessible to more players around the world!
