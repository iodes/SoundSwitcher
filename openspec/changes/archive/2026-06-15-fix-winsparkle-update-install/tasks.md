## 1. GitHub Actions 워크플로우 수정

- [x] 1.1 `publish.yml`의 'Build and Publish Portable (ZIP)' 단계에서 ZIP 파일(`Compress-Archive`)을 생성하는 로직을 제거하고, 단순히 Inno Setup 컴파일에 사용할 수 있도록 빌드 결과물만 유지합니다. (단계 이름도 적절히 변경)
- [x] 1.2 `publish.yml`의 'Prepare Installer Files' 단계에서 기존 `out/portable` 폴더 참조를 빌드된 출력 폴더로 알맞게 변경합니다.
- [x] 1.3 `publish.yml`의 'Sign Update with WinSparkle EdDSA' 단계에서 서명 대상 파일을 `Installer/Output/SoundSwitcher-{version}-Setup.exe`로 변경합니다.
- [x] 1.4 `publish.yml`의 'Prepare Appcast and Release Notes' 단계에서 `$downloadUrl` 및 `$fileSize` 대상을 `.zip`에서 `-Setup.exe`로 변경합니다.
- [x] 1.5 `publish.yml`의 'Create GitHub Release' 및 'Write Job Summary' 단계에서 Portable ZIP 관련 파일을 업로드하고 요약하는 항목을 완전히 제거합니다.
- [x] 1.6 `publish.yml`의 'Write Job Summary' 단계에서 출력되는 Release Notes 링크를 `release-notes-default.html`에서 실제 생성되는 파일명인 `release-notes.html`로 수정합니다.
