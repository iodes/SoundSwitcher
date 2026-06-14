---
name: create-release-notes
description: Creates release notes. Creates a folder under ReleaseNotes with the provided version name, and writes the provided release notes into multilingual markdown files.
---

# Create Release Notes

This skill performs the task of creating a new directory named after a given version inside the `ReleaseNotes` folder, and generating multilingual markdown files containing the provided release notes.

## Inputs
- **Version**: The release version in 4-part format without the 'v' prefix (e.g., 1.0.0.0, 1.2.3.4)
- **Release Notes**: The contents of the release notes including changelogs, new features, and bug fixes.

## Action Guidelines
1. **Create Directory**:
   - Create a new directory using the given **version name** under the `ReleaseNotes` folder at the project root (e.g., `ReleaseNotes/1.0.0.0/`).
   - If the `ReleaseNotes` directory does not exist, create it as well.

2. **Translate and Write Markdown Files**:
   - Based on the user-provided release notes, write the contents in multiple languages (e.g., English and Korean. If not specified, default to creating `en.md` and `ko.md`).
   - Generate a markdown file for each language inside the created version folder (e.g., `ReleaseNotes/1.0.0.0/en.md`, `ReleaseNotes/1.0.0.0/ko.md`).
   - Format the markdown files nicely with headers, lists, and emphasis for better readability.

3. **Reporting**:
   - Verify that the files were created successfully, and clearly report the paths of the generated files and the operation result back to the user.
