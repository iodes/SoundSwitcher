## Context

The SoundSwitcher application currently depends on the `NAudio` package, which is a metapackage that pulls in many sub-packages, including components for Windows Forms (`NAudio.WinForms`), MIDI (`NAudio.Midi`), and ASIO (`NAudio.Asio`). Since SoundSwitcher is a WPF/CLI application (or simply doesn't use these specific features), and only uses the `NAudio.CoreAudioApi` namespace for audio device management, including the entire `NAudio` suite is unnecessary and inflates the application size.

## Goals / Non-Goals

**Goals:**
- Replace the `NAudio` dependency with `NAudio.Wasapi` (and intrinsically `NAudio.Core`).
- Ensure the application still compiles and runs without issues.

**Non-Goals:**
- Refactoring the audio device management logic itself.
- Upgrading or downgrading the NAudio version unless necessary (we will stick to the currently used version, which is 2.3.0).

## Decisions

**Decision 1: Use `NAudio.Wasapi` instead of `NAudio`**
- *Rationale*: The `NAudio.CoreAudioApi` namespace is provided by the `NAudio.Wasapi` package. By directly depending on `NAudio.Wasapi`, we get exactly what we need without the extra payload of the `NAudio` metapackage.
- *Alternatives considered*: Keeping `NAudio` but using assembly linking/trimming. This is more complex and error-prone compared to simply referencing the correct, smaller NuGet package.

## Risks / Trade-offs

- **Risk**: Missing dependencies if there are hidden usages of other NAudio components.
  - *Mitigation*: We have verified through codebase search that only `NAudio.CoreAudioApi` is used. We will compile and run the application to verify.
