## 1. 알림 뷰 (UserControl) 구현

- [ ] 1.1 `Controls` 폴더에 `ProfileChangeNotification.xaml` (UserControl) 생성
- [ ] 1.2 `ProfileChangeNotification.xaml` 내부에 장치 정보를 표시할 UI 구현 (아이콘, 텍스트)
- [ ] 1.3 Windows 11 스타일 디자인 적용 (Mica/Acrylic 느낌의 반투명 배경, 둥근 모서리, 미려한 그림자 테두리 등)
- [ ] 1.4 팝업 표시 후 일정 시간(예: 1.5초) 뒤에 부드러운 애니메이션(Fade-out)과 함께 스스로 닫히는 타이머 로직 추가

## 2. 알림 로직 및 위치 계산 적용

- [ ] 2.1 `App.xaml.cs` 내의 기존 `ShowBalloonTip` 호출 부분을 주석 처리 및 제거
- [ ] 2.2 시스템 작업 표시줄(Taskbar)의 현재 위치(상/하/좌/우)를 동적으로 알아내는 헬퍼 메서드 추가
- [ ] 2.3 `App.xaml.cs`에 커스텀 알림(`TaskbarIcon.ShowCustomBalloon`) 호출 시 작업 표시줄 위치에 맞는 네이티브 팝업 위치 보정(Placement/Offset) 로직 추가
- [ ] 2.4 새 알림 호출 전, 기존에 떠 있는 알림이 있다면 즉시 닫기 위해 `TaskbarIcon.CloseCustomBalloon()` 호출 로직 추가

## 3. 테스트 및 최적화

- [ ] 3.1 트레이 아이콘을 연속 클릭하여 빠른 전환 시 알림 창이 쌓이지 않고 즉시 갱신되는지 테스트
- [ ] 3.2 윈도우 작업 표시줄을 상/하/좌/우로 변경해가며 알림이 윈도우 순정 알림과 동일한 위치에 나타나는지 테스트
- [ ] 3.3 팝업 진입/퇴장 시의 부드러운 애니메이션 프레임 드랍 및 UI 어색함 확인/수정
