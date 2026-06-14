## ADDED Requirements

### Requirement: GitHub Actions 자동 빌드 및 패키징
시스템은(SHALL) 메인 브랜치에 코드가 푸시되거나 새 태그가 생성될 때 GitHub Actions를 통해 앱을 빌드하고 패키징해야 합니다.

#### Scenario: 릴리스 태그 푸시
- **WHEN** 개발자가 새로운 버전 태그(예: v1.0.1)를 푸시할 때
- **THEN** 시스템은 빌드, 테스트, 패키징을 수행한 후 GitHub Release를 생성하여 패키지를 업로드합니다.

### Requirement: Appcast 피드 생성 및 배포
시스템은(SHALL) 새 릴리스가 생성될 때마다 WinSparkle이 참조할 수 있는 XML 형태의 Appcast 피드를 자동 갱신하고 접근 가능한 URL(예: GitHub Pages)에 배포해야 합니다.

#### Scenario: 릴리스 완료 후 Appcast 배포
- **WHEN** 새로운 GitHub Release 작업이 완료되었을 때
- **THEN** 시스템은 릴리스 정보(버전, 다운로드 링크, 릴리스 노트 등)를 포함한 XML 파일을 생성하여 정적 호스팅(GitHub Pages 등)에 배포합니다.
