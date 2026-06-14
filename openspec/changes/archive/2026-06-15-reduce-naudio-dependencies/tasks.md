## 1. Project Dependencies

- [x] 1.1 Remove `NAudio` from `SoundSwitcher.csproj`
- [x] 1.2 Add `NAudio.Wasapi` and `NAudio.Core` to `SoundSwitcher.csproj` (version 2.3.0)

## 2. Verification

- [x] 2.1 Rebuild the solution to ensure no missing references
- [x] 2.2 Verify that the `AudioDeviceService` compiles correctly and can manage endpoints as before
