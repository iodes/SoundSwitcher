## 1. UI 시각적 상호작용(Hover/Click) 피드백 구현

- [x] 1.1 `ProfileCard.xaml`의 최상위 컨테이너(Border 등)에 `IsMouseOver` 상태에 따라 배경색이 변경되는 스타일 트리거 추가
- [x] 1.2 카드를 클릭(MouseLeftButtonDown)할 때 `ScaleTransform`을 이용해 카드가 미세하게 작아지는 눌림 효과 애니메이션 추가
- [x] 1.3 클릭 해제 시 카드가 원래 크기로 복구되는 애니메이션(MouseLeftButtonUp/MouseLeave) 구성

## 2. 아이콘 변경 진입점을 컨텍스트 메뉴로 이전

- [x] 2.1 `ProfileCard.xaml`의 프로파일 아이콘 영역(Image 혹은 해당 컨테이너)에 바인딩되어 있던 마우스 클릭(혹은 버튼) 이벤트 제거
- [x] 2.2 `ProfileCard.xaml`에 정의된 전체 카드 대상 `ContextMenu` 내부에 '아이콘 변경' `MenuItem` 추가
- [x] 2.3 새로 추가된 '아이콘 변경' 메뉴 클릭 시, 기존에 사용되던 아이콘 파일 선택기 및 앱 데이터 저장 로직이 동일하게 호출되도록 `ProfileCard.xaml.cs` (또는 ViewModel)에 이벤트 연결

## 3. 클릭 영역 확장 확인 및 이벤트 버블링 제어

- [x] 3.1 카드 전역(아이콘 영역 포함 빈 배경 등)을 좌클릭할 때 메인 윈도우의 프로파일 전환 이벤트가 올바르게 트리거되는지 확인
- [x] 3.2 드래그 핸들(Drag Thumb)이나 장치 선택 콤보박스(ComboBox)를 클릭할 때 `e.Handled = true` 등의 처리가 올바르게 동작하여 전체 카드의 눌림 효과나 전환 이벤트가 발생하지 않도록 예외 처리 보완 및 테스트
