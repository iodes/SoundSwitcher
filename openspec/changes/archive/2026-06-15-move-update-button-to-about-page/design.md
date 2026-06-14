## Context

현재 SoundSwitcher의 업데이트 확인 기능은 시스템 트레이 아이콘의 컨텍스트 메뉴(우클릭 메뉴)에 포함되어 있습니다. WinSparkle 라이브러리를 통해 자동 업데이트 및 수동 업데이트 확인이 이루어지고 있습니다. 
사용자 설정 및 정보 확인이 주로 `MainWindow`의 `AboutPage` 등에서 이루어지므로, 트레이 아이콘 메뉴를 간소화하고 업데이트 확인 기능을 `AboutPage`로 이전하여 UI 일관성을 확보하고자 합니다.

## Goals / Non-Goals

**Goals:**
- 트레이 아이콘 우클릭 컨텍스트 메뉴에서 "업데이트 확인" 메뉴 항목 제거 (`App.xaml.cs` 등)
- `SoundSwitcher.Pages.AboutPage`에 "업데이트 확인" 버튼 추가
- `AboutPage`의 버튼 클릭 시 기존의 `win_sparkle_check_update_with_ui` (또는 해당하는 WinSparkle 수동 업데이트 체크 메서드)를 호출하도록 로직 변경

**Non-Goals:**
- WinSparkle 초기화 로직이나 백그라운드 자동 업데이트 로직 자체를 변경하는 것
- 다른 페이지나 설정의 리팩토링

## Decisions

- **업데이트 확인 진입점 이전**: `App.xaml.cs`에서 트레이 컨텍스트 메뉴를 구성할 때 `CreateMenuItem("업데이트 확인", ...)` 로직을 삭제합니다. 대신 `AboutPage.xaml`에 `<ui:Button>`(또는 적절한 컨트롤)을 추가하여 업데이트를 확인하게 합니다.
- **WinSparkle 호출 방식 유지**: 기존 트레이 메뉴에서 호출하던 `WinSparkleInterop.win_sparkle_check_update_with_ui()` (또는 해당 래퍼 메서드)를 `AboutPage.xaml.cs`의 버튼 클릭 이벤트 핸들러에서 그대로 호출합니다.

## Risks / Trade-offs

- [Risk] 트레이 아이콘에서 즉시 업데이트를 확인하던 기존 사용자의 혼선 → [Mitigation] AboutPage가 직관적인 위치이므로, 사용자가 버전 정보를 확인하러 AboutPage에 들어왔을 때 자연스럽게 발견할 수 있도록 버튼을 배치합니다.
- [Risk] WPF UI 스레드와 WinSparkle 호출 간의 교착 상태 → [Mitigation] 기존 트레이에서 호출하던 방식과 동일하게 호출하므로 별도의 추가적인 스레드 문제는 발생하지 않을 것으로 예상됩니다.
