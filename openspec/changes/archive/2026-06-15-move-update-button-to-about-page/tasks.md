## 1. 트레이 아이콘 컨텍스트 메뉴 수정

- [x] 1.1 `SoundSwitcher/App.xaml.cs` 파일의 `CreateMenuItem("업데이트 확인", ...)` 로직을 삭제하여 트레이 우클릭 메뉴에서 업데이트 확인 항목 제거

## 2. AboutPage 업데이트 기능 구현

- [x] 2.1 `SoundSwitcher/Pages/AboutPage.xaml` 파일의 UI에 '업데이트 확인'을 위한 버튼 컨트롤 추가
- [x] 2.2 `SoundSwitcher/Pages/AboutPage.xaml.cs` 파일에 버튼 클릭 이벤트 핸들러를 추가하고 기존 업데이트 체크 로직 연동
