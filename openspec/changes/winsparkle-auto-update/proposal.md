## Why

사용자가 최신 버전의 앱을 수동으로 다운로드하고 설치하는 번거로움을 해결하기 위해, WinSparkle 라이브러리를 활용하여 자동 업데이트 기능을 제공하고자 합니다. 더불어, GitHub Actions를 이용해 릴리스 빌드와 배포를 자동화함으로써 유지보수 및 배포 효율성을 높입니다.

## What Changes

- WinSparkle(https://github.com/vslavik/winsparkle) C/C++ 라이브러리를 C# 애플리케이션에 통합.
- 앱 실행 시 백그라운드에서 업데이트 확인 기능 추가.
- 사용자가 원할 경우 수동으로 업데이트를 확인할 수 있는 UI 메뉴(예: 트레이 아이콘 메뉴) 제공.
- GitHub Actions를 사용해 코드 푸시 시 애플리케이션 자동 빌드 및 GitHub Release 생성 파이프라인 구축.
- WinSparkle이 사용할 Appcast XML 피드 자동 생성 및 배포.

## Capabilities

### New Capabilities
- `auto-update`: WinSparkle을 통한 앱의 자동 업데이트 확인 및 설치 지원.
- `automated-deployment`: GitHub Actions 기반의 릴리스 생성 및 Appcast 피드 자동 배포 파이프라인.

### Modified Capabilities

## Impact

- `SoundSwitcher` 프로젝트의 네이티브 종속성에 WinSparkle(DLL) 파일이 포함됨.
- `.github/workflows` 디렉토리에 새로운 CI/CD 파이프라인 추가.
- 앱 시작 시 초기화 로직 및 트레이 아이콘 메뉴 코드 수정.
