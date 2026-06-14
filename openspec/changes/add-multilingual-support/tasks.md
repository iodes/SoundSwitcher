## 1. 기반 설정 (Localization Setup)

- [x] 1.1 `Localization` 폴더 생성 및 `LocalizationManager` 클래스 구현 (INotifyPropertyChanged 인터페이스 사용)
- [x] 1.2 기본 리소스 파일(`Strings.resx`) 생성 및 기존 하드코딩 문자열 추가
- [x] 1.3 지원 언어별 리소스 파일 생성 (`Strings.ko-KR.resx`, `Strings.ja-JP.resx`, `Strings.zh-CN.resx`, `Strings.zh-TW.resx`)

## 2. 설정 연동 및 초기화 (Configuration & Initialization)

- [x] 2.1 기존 설정 클래스에 `Language` 속성 추가
- [x] 2.2 앱 실행 시 시스템 언어 감지 로직 구현 및 `LocalizationManager` 초기화 연동
- [x] 2.3 `App.xaml.cs` 또는 메인 뷰모델에서 언어 설정 로드 로직 호출

## 3. UI 적용 (UI Implementation)

- [x] 3.1 `MainWindow.xaml` 및 관련된 뷰 파일들에서 하드코딩된 텍스트를 `LocalizationManager` 바인딩으로 교체
- [x] 3.2 설정 창에 언어를 선택할 수 있는 콤보박스(ComboBox) 추가
- [x] 3.3 언어 콤보박스 선택 시 `LocalizationManager`를 통해 실시간으로 언어 변경 및 설정 파일 저장
