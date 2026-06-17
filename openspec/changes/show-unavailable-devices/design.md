## Context

현재 SoundSwitcher 시스템은 모든 장치 상태(사용 가능, 연결 해제됨, 비활성화됨)를 구분 없이 `AudioDeviceService`를 통해 받아와 모두 목록에 표시하고 있습니다. 이로 인해 장치 콤보박스 목록이 너무 길어지고 사용자에게 혼잡감을 주고 있습니다. 혼잡도를 줄이면서도 필요한 경우 끊긴 장치를 미리 설정할 수 있도록, 기본적으로는 사용할 수 없는 장치를 숨기되 옵션을 통해 다시 표시할 수 있게 하는 기능이 필요합니다.

## Goals / 단 Goals

**Goals:**
- 장치 콤보박스 목록의 혼잡도를 줄이기 위해, 기본적으로 연결 해제되거나 사용할 수 없는 장치(`IsActive == false`)를 목록에서 제외 (기본 동작 변경)
- 사용자가 설정 창에서 "사용할 수 없는 장치 표시" 여부를 토글할 수 있는 체크박스 제공
- 현재 프로필에 선택되어 있는 장치가 사용할 수 없는 상태이더라도, 옵션 값과 무관하게 목록에 예외적으로 포함하여 콤보박스에 표시 (다른 장치로 변경 시 다음 번 갱신 시 목록에서 사라짐)

**Non-Goals:**
- 사용할 수 없는 장치를 기본 통신 장치나 기본 오디오 장치로 실제로 전환하려는 시도 (OS에서 막히므로 스위칭 자체는 실패하거나 무시됨)
- AudioDeviceService 내부의 장치 열거(Enumeration) 로직 변경 (이미 모든 상태의 장치를 가져오고 있으므로 뷰모델 필터링만 수정)

## Decisions

1. **`AppSettings` 및 모델 변경**
   - `AppSettings` 클래스에 `bool ShowUnavailableDevices { get; set; } = false;` 속성 추가 (기본값 false로 혼잡도 감소 달성).
   - UI 설정 토글을 위해 `MainViewModel`에 `ShowUnavailableDevices` 프로퍼티를 추가하고, 변경 시 `SaveSettings()`와 `RefreshDevices()`를 호출하도록 구현.

2. **장치 필터링 로직 (`MainViewModel.RefreshDevices()`)**
   - `_audioService.GetActiveDevices()` (실제로는 모든 장치를 반환함)의 결과를 `currentPlayback`과 `currentCapture`로 나눌 때 필터링 조건 적용.
   - 표시할 장치의 조건:
     - 장치의 `IsActive` 상태가 `true`이거나
     - `ShowUnavailableDevices`가 `true`이거나
     - 해당 장치 ID가 현재 `Profiles` 목록 중 어느 하나의 `PlaybackDeviceId` 또는 `CaptureDeviceId`로 선택되어 있는 경우 (예외 처리).

3. **자동 숨김 동작의 처리**
   - 위 2번의 로직에 따라, 사용자가 콤보박스에서 '사용할 수 없는 장치' 대신 '사용 가능한 장치'를 선택하게 되면, 해당 기기는 더 이상 어느 프로필에서도 참조되지 않게 됩니다.
   - 장치가 변경될 때 (예: `ProfileChanged` 이벤트나 `SettingsChanged` 등) `RefreshDevices()`가 호출되거나 다음 번 콤보박스 목록 갱신 시점에 더 이상 예외 조건에 부합하지 않으므로 자동으로 목록에서 사라지게 됩니다.

## Risks / Trade-offs

- **Risk**: 모든 프로필이 전역적인 `AvailablePlaybackDevices`, `AvailableCaptureDevices` 목록을 공유하기 때문에, 특정 프로필 A에서 선택된 비활성 장치가 프로필 B의 콤보박스 목록에도 나타납니다.
  - **Mitigation**: WPF 콤보박스 구조상 ItemsSource를 공유하는 것이 성능 및 메모리 관리에 유리하며, 사용자가 비활성 장치의 존재를 인지할 수 있어 큰 문제는 아닙니다.
- **Risk**: 사용할 수 없는 장치로의 스위칭 실패
  - **Mitigation**: `DeviceSwitchingService`에서 스위칭 전 유효성을 검사하고 실패 시 사용자에게 알림을 보내는 기존 로직이 있으므로 이로 인한 크래시나 치명적 오류는 방지됩니다.
