## Context

SoundSwitcher는 `Hardcodet.Wpf.TaskbarNotification` 라이브러리를 사용하여 트레이 아이콘을 표시하고 있습니다.
Windows 시스템 트레이의 숨겨진 아이콘 영역(접힘 메뉴)에 아이콘이 들어있을 때, 아이콘을 우클릭하면 컨텍스트 메뉴가 마우스 포인터 위치가 아닌 엉뚱한 화면 구석에 렌더링되는 문제가 발생하고 있습니다. 이는 WPF의 `ContextMenu`가 숨겨진 팝업 윈도우 내에서의 상대 좌표를 잘못 계산하거나 시스템 DPI 스케일링, 그리고 백그라운드 포커스 문제와 겹쳐서 발생하는 전형적인 이슈입니다.

## Goals / Non-Goals

**Goals:**
- 숨겨진 트레이 아이콘 영역에서도 우클릭 시 컨텍스트 메뉴가 마우스 커서의 현재 위치에 정확하게 표시되도록 수정
- 컨텍스트 메뉴 외부 클릭 시 메뉴가 정상적으로 닫히도록 포커스 문제 해결

**Non-Goals:**
- 트레이 아이콘 관리 라이브러리(`Hardcodet.Wpf.TaskbarNotification`) 교체
- 컨텍스트 메뉴의 기능 추가나 전면적인 UI/UX 개편

## Decisions

1. **Win32 API를 이용한 절대 좌표 기반 위치 지정 (`GetCursorPos` + `AbsolutePoint`)**
   - **대안 고려:** `PlacementMode.MousePoint`로 설정. 그러나 숨겨진 트레이 상태이거나 DPI가 100%가 아닌 환경에서는 이 방식만으로 정확한 위치를 보장할 수 없습니다.
   - **결정:** Win32 `GetCursorPos` 함수로 바탕화면 기준 마우스의 물리적 절대 좌표를 구한 후, 시스템 DPI 스케일을 반영해 `HorizontalOffset`, `VerticalOffset`을 계산합니다. 그리고 메뉴의 `Placement` 속성을 `PlacementMode.AbsolutePoint`로 설정해 정확한 위치에 띄웁니다.

2. **메뉴 활성화 시 Foreground Window 강제 (`SetForegroundWindow`)**
   - **이유:** 숨겨진 트레이 영역에서 메뉴를 열 때 애플리케이션이 포커스를 받지 못하면, 다른 곳을 클릭해도 메뉴가 닫히지 않는 버그가 발생할 수 있습니다.
   - **결정:** `ContextMenu`가 열리기 전이나 여는 시점에 Win32 `SetForegroundWindow`를 호출하여 포커스를 가져오고 메뉴 동작의 안정성을 확보합니다.

## Risks / Trade-offs

- **[Risk] 다중 모니터 환경에서의 DPI 스케일 계산 오차:** 각 모니터마다 DPI 스케일링이 다를 경우 논리적 좌표 변환에서 미세한 오차가 발생할 수 있습니다.
  - **Mitigation:** PresentationSource를 사용하거나 마우스가 위치한 현재 모니터의 DPI 값을 올바르게 가져와서 물리적 픽셀을 논리적 픽셀로 정확히 나누어 보정합니다.
