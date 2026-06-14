## MODIFIED Requirements

### Requirement: Appcast 피드 생성 및 배포
시스템은(SHALL) 새 릴리스가 생성될 때마다 WinSparkle이 참조할 수 있는 XML 형태의 Appcast 피드를 자동 갱신하고 접근 가능한 URL(예: GitHub Pages)에 배포해야 하며, 앱캐스트의 업데이트 다운로드 대상은 ZIP 아카이브가 아닌 설치 가능한 실행 파일(예: `.exe` 설치 프로그램)이어야 합니다.

#### Scenario: 릴리스 완료 후 Appcast 배포
- **WHEN** 새로운 GitHub Release 작업이 완료되었을 때
- **THEN** 시스템은 릴리스 정보(버전, 설치 파일의 다운로드 링크, 설치 파일에 대한 EdDSA 서명, 릴리스 노트 등)를 포함한 XML 파일을 생성하여 정적 호스팅(GitHub Pages 등)에 배포합니다.
