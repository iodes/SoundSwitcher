## Context

현재 SoundSwitcher의 배포 워크플로우(`publish.yml`)는 GitHub Release에 두 가지 종류의 파일을 업로드합니다:
1. Portable ZIP 파일 (`SoundSwitcher-{version}-Portable.zip`)
2. Inno Setup 설치 파일 (`SoundSwitcher-{version}-Setup.exe`)

하지만 자동 업데이트 메커니즘인 WinSparkle을 위해 생성하는 Appcast(`appcast.xml`)는 Portable ZIP 파일을 업데이트 대상으로 가리키고, 서명(EdDSA)도 ZIP 파일에 대해 수행하고 있습니다.
WinSparkle은 다운로드 받은 파일(`.zip`)을 단순히 실행(Execute)하게 되므로, 설치가 진행되지 않고 기본 연결 프로그램인 Windows 탐색기가 열리게 됩니다.

## Goals / Non-Goals

**Goals:**
- WinSparkle의 Appcast가 Inno Setup 설치 파일(`.exe`)을 다운로드하도록 변경합니다.
- 설치 파일(`.exe`)에 대해 EdDSA 서명을 수행하여 WinSparkle이 안전하게 업데이트를 검증하고 설치를 진행할 수 있도록 합니다.
- 불필요해진 Portable ZIP 파일의 생성을 배포 파이프라인에서 완전히 제거합니다.

**Non-Goals:**
- WinSparkle 라이브러리 자체의 압축 해제 처리 로직을 수정하는 것은 피합니다.

## Decisions

1. **Appcast 대상 변경 (ZIP -> EXE)**
   - **결정:** `publish.yml` 내에서 WinSparkle 서명 대상과 Appcast `<enclosure>`의 `url`을 `Installer/Output/SoundSwitcher-{version}-Setup.exe`로 변경합니다.
   - **이유:** WinSparkle은 `.exe`나 `.msi` 형식의 설치 파일을 다운로드하여 직접 실행하는 것을 지원하며, 이것이 가장 표준적인 자동 업데이트 방식입니다. `.zip`을 사용하려면 별도의 추출기를 구현해야 하지만, 이미 Inno Setup으로 설치 파일이 빌드되고 있으므로 Appcast 대상만 변경하는 것이 가장 간단하고 확실한 방법입니다.

2. **Portable ZIP 아티팩트 제거**
   - **결정:** `publish.yml` 내에서 `SoundSwitcher-{version}-Portable.zip` 파일을 압축하는 과정을 삭제하고 GitHub 릴리스에도 포함하지 않도록 수정합니다.
   - **이유:** 앱캐스트가 EXE만을 가리키게 되므로 자동 업데이트 관점에서는 ZIP 아카이브를 추가로 제공할 필요성이 없어집니다. 배포 프로세스를 간소화하고 혼동을 방지하기 위해 생성 자체를 중단합니다.

## Risks / Trade-offs

- **[Risk] 설치 프로그램이 관리자 권한을 요구할 경우 백그라운드 업데이트에서 UAC 프롬프트 발생**
  - Mitigation: Inno Setup은 기본적으로 관리자 권한을 요구할 수 있으나, 사용자가 명시적으로 설치에 동의하여 업데이트를 진행하는 것이므로 일반적인 소프트웨어 업데이트 과정과 동일합니다.
- **[Risk] 이전 버전(ZIP 기반 앱캐스트를 수신하던 클라이언트)의 호환성**
  - Mitigation: WinSparkle은 Appcast에 명시된 URL을 그대로 다운로드 및 실행하므로, 기존 앱이 새로운 Appcast(EXE)를 수신하더라도 EXE를 정상 다운로드 및 실행할 수 있습니다. 호환성에 문제가 발생하지 않습니다.
