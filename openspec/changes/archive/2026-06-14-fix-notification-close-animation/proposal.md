## Why

알림 팝업이 닫히면서 페이드아웃 애니메이션이 실행되는 도중에 장치가 변경되면, 알림 메시지와 타이머는 업데이트되지만 기존의 닫힘 상태(`_isClosing = true`)와 페이드아웃 애니메이션이 올바르게 취소되지 않습니다. 이로 인해 알림이 투명해진 상태로 멈추거나 닫힘 애니메이션이 더 이상 동작하지 않게 되는 시각적 오류가 발생합니다. 이 변경은 해당 버그를 수정하여 연속된 장치 변경 시에도 알림이 정상적으로 표시되도록 하기 위함입니다.

## What Changes

- `ProfileChangeNotification.xaml.cs`의 `UpdateMessage` 메서드 내에 알림이 닫히는 중(`_isClosing == true`)인지 확인하는 로직 추가
- 알림이 닫히는 중인 경우, `_isClosing`을 `false`로 되돌리고 진행 중인 투명도 애니메이션을 중단시킨 후 `Opacity` 값을 1.0으로 복원

## Capabilities

### New Capabilities
None

### Modified Capabilities
- `device-switching`: 알림이 닫히는 애니메이션 도중 새로운 알림이 연속으로 발생할 때의 애니메이션 처리 로직 개선

## Impact

- `Controls/ProfileChangeNotification.xaml.cs` (알림 UI 컨트롤)
- 트레이 아이콘을 통한 프로필 변경 알림 표시 기능
