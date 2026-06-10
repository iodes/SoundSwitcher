## 1. Win32 API 연동

- [ ] 1.1 `NativeMethods` 헬퍼(또는 관련 클래스)에 `GetCursorPos` 및 `SetForegroundWindow` PInvoke 선언 추가
- [ ] 1.2 `GetCursorPos`에 사용할 `POINT` 구조체 정의

## 2. 트레이 컨텍스트 메뉴 위치 보정 적용 (App.xaml.cs)

- [ ] 2.1 기존 `TaskbarIcon.ContextMenu` 기본 할당 로직을 제거하고, `TrayRightMouseUp` (또는 `TrayContextMenuOpen`) 이벤트 핸들러 추가
- [ ] 2.2 우클릭 이벤트 핸들러 내에서 `SetForegroundWindow`를 호출하여 포커스 강제
- [ ] 2.3 `GetCursorPos`를 통해 마우스의 현재 물리 좌표 획득
- [ ] 2.4 현재 시스템의 DPI 스케일 값을 가져와 물리 좌표를 논리 좌표로 보정
- [ ] 2.5 `ContextMenu`의 `Placement` 속성을 `AbsolutePoint`로 지정하고, 보정된 좌표를 `HorizontalOffset`과 `VerticalOffset`에 적용한 뒤 `IsOpen = true` 설정

## 3. 테스트 및 검증

- [ ] 3.1 트레이 아이콘 우클릭 시 컨텍스트 메뉴가 마우스 커서 위치에 정확히 열리는지 확인
- [ ] 3.2 숨겨진 트레이 아이콘 영역(접힘 메뉴) 안에서도 동일하게 올바른 위치에 열리는지 검증
- [ ] 3.3 컨텍스트 메뉴 바깥을 클릭했을 때 메뉴가 정상적으로 닫히는지 확인
