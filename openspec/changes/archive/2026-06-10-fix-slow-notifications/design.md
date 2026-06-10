## Context

현재 SoundSwitcher는 프로파일(장치) 전환 시 `Hardcodet.Wpf.TaskbarNotification` 라이브러리의 `ShowBalloonTip`을 사용하여 사용자에게 알림을 제공하고 있습니다. 하지만 Windows의 기본 Balloon 알림(Windows 10/11에서는 Action Center Toast로 연결됨)은 표시 및 애니메이션 속도가 느리며, 연속으로 전환할 경우 알림이 큐(Queue)에 쌓여 뒤늦게 표시되는 문제가 발생합니다.

## Goals / Non-Goals

**Goals:**
- 장치 전환 시 발생하는 알림의 응답성을 개선하여 즉각적인 피드백 제공
- 사용자가 여러 번 연속으로 프로파일을 전환할 때 이전 알림을 즉시 닫거나 덮어써서 알림이 쌓이지 않도록 방지
- Windows 11 스타일(Mica/Acrylic 배경, 둥근 모서리, 미려한 그림자 등)에 맞춘 네이티브하고 아름다운 알림 UI 제공
- 작업 표시줄(Taskbar)의 위치(상/하/좌/우)를 동적으로 감지하여 윈도우 순정 알림과 동일한 위치에 표시

**Non-Goals:**
- Action Center 시스템 자체의 동작 변경

## Decisions

1. **`ShowBalloonTip` 대신 `ShowCustomBalloon` 사용 (WPF Custom Popup)**
   - **Rationale**: Windows Native Toast는 애니메이션 속도와 대기열(Queue)을 제어하기 어렵습니다. `Hardcodet.Wpf.TaskbarNotification`에서 제공하는 Custom Balloon(WPF 팝업) 기능을 사용하면 OS 레벨의 큐를 우회하고 애플리케이션 내에서 즉각적이고 부드러운 알림을 표시할 수 있습니다.
   - **Alternatives Considered**: 기존 `ShowBalloonTip` 호출 직전에 `CloseBalloonTip()`을 호출하는 방법. 이 방법은 여전히 Windows Toast 시스템의 느린 애니메이션과 딜레이에 의존하므로 근본적인 해결책이 되지 못합니다.

2. **단일 알림 인스턴스 유지 및 갱신**
   - **Rationale**: 새 알림이 발생하면 기존에 열려있는 Custom Balloon을 닫고(`CloseCustomBalloon()`) 새로운 팝업을 즉시 띄우도록 합니다. 

3. **Notification 뷰(UserControl) 추가 및 Windows 11 미적용**
   - **Rationale**: Custom Balloon으로 띄울 WPF UserControl(`ProfileChangeNotification.xaml`)을 생성합니다. Windows 11의 디자인 언어를 차용하여 둥근 모서리(CornerRadius), 부드러운 그림자(DropShadow), 그리고 모던한 배경(테마 연동 또는 불투명 아크릴 느낌)을 적용합니다. 자체적인 Storyboard를 통해 부드러운 Fade-In/Out을 구현하며, 1.5초 정도 후 스스로 닫히게 합니다.

4. **작업 표시줄 위치 인식 및 알림 위치 조정**
   - **Rationale**: `TaskbarIcon.ShowCustomBalloon`은 기본적으로 시스템의 Taskbar 위치를 어느 정도 고려하지만, Windows 11 네이티브 토스트와 완벽하게 동일한 오프셋 및 위치(우측 하단, 우측 상단 등)를 보장하기 위해 팝업의 위치(Placement) 속성을 세밀하게 조정하거나, Wpf.Ui 라이브러리의 Popup/Snackbar 기능을 Taskbar 위치에 맞게 커스텀 렌더링합니다.

## Risks / Trade-offs

- [Risk] Custom Balloon은 Windows Action Center(알림 센터)에 기록이 남지 않습니다.
  - Mitigation: 오디오 장치 전환 알림은 일시적인 정보이므로 알림 센터에 기록이 남지 않는 편이 오히려 사용자 경험에 좋습니다. 트레이 아이콘의 ToolTip으로 현재 장치를 언제든 확인할 수 있으므로 문제되지 않습니다.
