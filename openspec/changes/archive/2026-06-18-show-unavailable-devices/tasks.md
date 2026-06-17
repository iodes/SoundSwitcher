## 1. 모델 및 설정 업데이트

- [x] 1.1 `AppSettings` 클래스에 `ShowUnavailableDevices` (bool) 속성 추가 (기본값 false)

## 2. 뷰모델 업데이트

- [x] 2.1 `MainViewModel` 클래스에 `ShowUnavailableDevices` 속성 노출 (getter/setter 구성, 값 변경 시 `SaveSettings()`, `RefreshDevices()` 호출)
- [x] 2.2 `MainViewModel`의 `LoadSettings()` 및 `SaveSettings()`에 `ShowUnavailableDevices` 필 연동
- [x] 2.3 `MainViewModel`의 `RefreshDevices()` 메서드에서 `_audioService.GetActiveDevices()` 결과 필터링 로직 수정 (설정값, 장치 활성 상태, 현재 프로필에서 선택된 장치 여부 확인 후 목록 구성)

## 3. UI 및 현지화(Localization) 업데이트

- [x] 3.1 `SettingsPage.xaml`의 '기본 통신 장치 전환' 옵션 하단에 '사용할 수 없는 장치 표시' 체크박스/토글 버튼 추가 (바인딩: `ShowUnavailableDevices`)
- [x] 3.2 언어별 현지화 리소스(JSON 등)에 `ShowUnavailableDevices` 문자열 추가 (한국어, 영어 등)
