## Why

현재 WinSparkle을 통한 자동 업데이트 시, 업데이트 설치 버튼을 누르면 다운로드된 ZIP 파일의 압축이 풀리거나 앱이 교체되는 것이 아니라 단순히 탐색기에서 ZIP 파일이 열리는 문제가 발생하고 있습니다. 이는 WinSparkle이 다운로드된 파일을 단순히 실행(Execute)하기 때문에, 확장자가 `.zip`인 경우 기본 연결 프로그램인 탐색기가 열리기 때문입니다. 사용자가 원활하게 업데이트를 설치하고 앱을 다시 시작할 수 있도록 이 문제를 수정해야 합니다.

## What Changes

- GitHub Actions 배포 파이프라인에서 단순 ZIP 파일(Portable) 생성을 중단하고 설치 가능한 실행 파일(Installer)만 생성하여 릴리스하도록 수정합니다.
- Appcast (`appcast.xml`)가 ZIP이 아닌 설치용 실행 파일(예: `.exe` 또는 `.msi`)을 가리키도록 하여 WinSparkle이 올바르게 실행하여 설치를 진행할 수 있도록 변경합니다.

## Capabilities

### New Capabilities

### Modified Capabilities
- `auto-update`: 자동 업데이트 시 다운로드 후 올바른 설치 과정을 거치도록 요구사항 및 구현 방식 변경.
- `automated-deployment`: 릴리스 시 ZIP 파일 생성을 중단하고 설치 가능한 아티팩트(.exe 등)만을 생성하도록 배포 파이프라인 수정.

## Impact

- `publish.yml` (GitHub Actions 워크플로우): 빌드 및 릴리스 아티팩트 생성 방식 변경.
- Release Changelog/Appcast 자동 생성 스크립트: ZIP 대신 새로운 설치 파일 확장자를 참조하도록 수정.
