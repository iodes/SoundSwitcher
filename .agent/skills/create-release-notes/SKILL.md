---
name: create-release-notes
description: Creates release notes. Asks the user for the version and reviews the draft before creating a folder under ReleaseNotes and writing the release notes into multilingual markdown files.
---

# Create Release Notes

This skill performs the task of creating a new directory named after a given version inside the `ReleaseNotes` folder, and generating multilingual markdown files containing the provided release notes.

## Inputs
- **Version**: The release version in 4-part format without the 'v' prefix (e.g., 1.0.0.0, 1.2.3.4). You MUST ask the user for this if it is not provided.
- **Release Notes**: The contents of the release notes including changelogs, new features, and bug fixes. You can deduce these from git commits.

## Action Guidelines
1. **Determine Version and Changes**:
   - **CRITICAL**: Always ask the user for the target version number if they have not explicitly provided it. Do not assume or guess the version number.
   - Run `git log $(git describe --tags --abbrev=0)..HEAD --oneline` (or check recent commits/diffs if tags aren't available) to review the actual code changes since the last release.
   - Ensure the drafted release notes strictly reflect the actual code changes. Do NOT hallucinate, exaggerate, or fabricate features or fixes that are not present in the commit history.

2. **Draft and Request Approval**:
   - Draft the release notes based on the commits (or user input).
   - **CRITICAL**: Present the drafted release notes and the target version number to the user and explicitly ask for their approval.
   - Do **NOT** create directories or write any files to the disk before the user has reviewed and explicitly approved the draft.

3. **Create Directory (Post-Approval)**:
   - After user approval, create a new directory using the given **version name** under the `ReleaseNotes` folder at the project root (e.g., `ReleaseNotes/1.0.0.0/`).
   - If the `ReleaseNotes` directory does not exist, create it as well.

4. **Translate and Write Markdown Files (Post-Approval)**:
   - Based on the approved release notes, translate and write the contents in all supported languages: `en-US` (English), `ko-KR` (Korean), `ja-JP` (Japanese), `zh-CN` (Simplified Chinese), and `zh-TW` (Traditional Chinese).
   - Generate a markdown file for each language inside the created version folder using the exact locale code as the filename (e.g., `ReleaseNotes/1.0.0.0/en-US.md`, `ReleaseNotes/1.0.0.0/ko-KR.md`, `ja-JP.md`, `zh-CN.md`, `zh-TW.md`).
   - Format the markdown files nicely with headers, lists, and emphasis for better readability.
   - **IMPORTANT**: Do not include any title headers (e.g., `# SoundSwitcher 1.0.1.0 Release Notes`), intro sentences, concluding remarks, greetings, or boilerplate messages. Output ONLY the raw list of changes and formatting (e.g., `## 🚀 Key Changes`).

5. **Reporting**:
   - Verify that the files were created successfully, and clearly report the paths of the generated files and the operation result back to the user.
