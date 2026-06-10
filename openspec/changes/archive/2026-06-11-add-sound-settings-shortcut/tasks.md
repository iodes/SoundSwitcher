## 1. 트레이 메뉴 구조 업데이트

- [x] 1.1 `SoundSwitcher/App.axaml` 또는 관련 트레이 뷰 모델 파일을 열어 기존 메뉴 구조 확인
- [x] 1.2 트레이 컨텍스트 메뉴에 "시스템 소리 설정"(`NativeMenuItem`) 추가 및 클릭 커맨드 바인딩
- [x] 1.3 "시스템 소리 설정" 메뉴와 "설정" 메뉴 사이에 구분선(`NativeMenuItemSeparator`) 추가

## 2. 실행 기능 구현

- [x] 2.1 트레이 뷰 모델(또는 관련된 ViewModel/Command 영역)에 "시스템 소리 설정" 커맨드 핸들러 구현
- [x] 2.2 `System.Diagnostics.Process.Start(new ProcessStartInfo { FileName = "mmsys.cpl", UseShellExecute = true })` 형태의 호출 코드 작성
- [x] 2.3 실행 시 발생할 수 있는 예외를 방지하기 위해 `try-catch` 블록 추가

## 3. 검증

- [x] 3.1 앱 빌드 및 실행
- [x] 3.2 트레이 우클릭 시 "시스템 소리 설정" 항목이 정상적으로 표시되는지 확인
- [x] 3.3 해당 항목 클릭 시 Windows 소리 제어판 창이 열리는지 확인
