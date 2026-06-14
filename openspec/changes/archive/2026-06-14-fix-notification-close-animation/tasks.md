## 1. 알림 애니메이션 버그 수정

- [x] 1.1 `Controls/ProfileChangeNotification.xaml.cs`의 `UpdateMessage` 메서드에 `_isClosing` 검사 로직 추가
- [x] 1.2 `_isClosing == true`인 경우 `_isClosing = false`로 상태 초기화 및 `UIElement.OpacityProperty` 애니메이션 중단 로직 구현
- [x] 1.3 끊긴 애니메이션으로 인해 투명도가 0 근처에 머무는 것을 방지하기 위해 `RootBorder.Opacity = 1.0;` 설정 추가

## 2. 테스트 및 검증

- [x] 2.1 애플리케이션 빌드 및 정상 실행 확인
- [x] 2.2 알림 창이 사라지려는 타이밍(약 1.5초 후)에 맞추어 연속으로 트레이 아이콘을 클릭해 장치를 전환하여 테스트
- [x] 2.3 새 알림 창이 즉시 선명하게 표시되고, 이후 시간이 지나면 정상적으로 닫히는지 확인
