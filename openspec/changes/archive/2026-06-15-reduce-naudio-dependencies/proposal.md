## Why

The `NAudio` package is a large metapackage that includes references to many unneeded components, such as Windows Forms, MIDI, and ASIO. The SoundSwitcher application currently only utilizes `NAudio.CoreAudioApi` for managing audio devices. By trimming down the dependency to only what's necessary, we can reduce the application's overall size and simplify its dependency tree.

## What Changes

- Replace the `NAudio` NuGet package reference in `SoundSwitcher.csproj` with `NAudio.Wasapi` (which includes `NAudio.Core`).
- Ensure the application continues to build and function correctly without the full NAudio suite.

## Capabilities

### New Capabilities

- `reduce-dependencies`: Refactor the application to use smaller, more specific NuGet packages instead of large metapackages to reduce overall application size.

### Modified Capabilities

None.

## Impact

- **Dependencies**: The `NAudio` package will be removed, and `NAudio.Wasapi` will be added.
- **Application Size**: The final application size should be reduced since we are excluding WinForms, MIDI, and other unused components from NAudio.
- **Code**: No code changes are expected since `NAudio.CoreAudioApi` is provided by `NAudio.Wasapi`.
