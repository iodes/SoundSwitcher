## Context
현재 `ProfileChangeNotification` 컨트롤은 알림이 닫힐 때 `_isClosing` 플래그를 `true`로 설정하고 페이드아웃 애니메이션을 실행합니다. 하지만 알림이 닫히는 애니메이션(약 0.15초) 도중 새로운 장치 전환 이벤트가 발생해 `UpdateMessage`가 호출될 경우, 텍스트와 타이머는 업데이트되지만 `_isClosing` 플래그가 여전히 `true`로 남아있고, 진행 중인 애니메이션이 취소되지 않아 화면에서 알림이 비정상적으로 사라지거나 다음 닫힘 애니메이션이 무시되는 버그가 있습니다.

## Goals / Non-Goals

**Goals:**
- 알림 애니메이션 도중 새로운 알림이 발생했을 때 기존 애니메이션을 올바르게 중지하고 새 알림을 표시하도록 수정합니다.
- 알림이 정상적으로 다시 닫힐 수 있도록 닫힘 상태 플래그를 초기화합니다.

**Non-Goals:**
- 알림 UI 디자인 자체를 변경하는 작업
- 장치 전환 코어 로직의 변경

## Decisions

- **`UpdateMessage` 로직 내 상태 초기화 추가**:
  - `_isClosing`이 `true`일 경우 `_isClosing` 플래그를 `false`로 변경합니다.
  - 진행 중인 `UIElement.OpacityProperty` 애니메이션을 중단시킵니다. (`RootBorder.BeginAnimation(UIElement.OpacityProperty, null);`)
  - 애니메이션이 중단된 시점의 투명도 상태가 남아 알림이 보이지 않게 되는 문제를 막기 위해 `RootBorder.Opacity = 1.0`으로 다시 고정합니다.

## Risks / Trade-offs

- [Risk] 애니메이션이 취소될 때 투명도가 순간적으로 1.0으로 튀어 보일 수 있음.
  - Mitigation: 알림 메시지가 새로 바뀌는 시점이므로 눈에 띄게 어색하지 않을 것이며, 무엇보다 버그로 인해 알림 자체가 완전히 사라지거나 동작하지 않는 현상을 해결하는 것이 훨씬 중요합니다.
