## 1. 트레이 컨텍스트 메뉴 애니메이션 수정

- [x] 1.1 `App.xaml.cs` 파일의 `EnsureTrayIcon` 또는 트레이 메뉴 초기화 메서드에서 `_trayContextMenu` 생성 로직을 확인합니다.
- [x] 1.2 해당 `ContextMenu`에 스타일을 적용하여, `Popup.PopupAnimation` 속성이 `PopupAnimation.Fade` 또는 `PopupAnimation.None`으로 지정되도록 코드를 추가합니다.

## 2. 테스트 및 검증

- [x] 2.1 애플리케이션을 빌드 및 실행한 후, 시스템 트레이 아이콘을 우클릭하여 메뉴를 열어봅니다. 플라이아웃/슬라이드 애니메이션이 정상적으로 제거되었는지 확인합니다.
- [x] 2.2 메뉴가 마우스 커서 위치에 즉시 나타나거나, 부드러운 페이드 효과만 보여주는지 확인합니다.
