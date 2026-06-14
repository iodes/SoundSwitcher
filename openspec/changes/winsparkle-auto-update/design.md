## Context

현재 SoundSwitcher 앱은 새로운 버전이 나왔을 때 사용자가 직접 GitHub 저장소나 스토어에 방문하여 다운로드 및 설치를 해야 하는 불편함이 있습니다. 이를 해결하기 위해 WinSparkle C/C++ 라이브러리를 도입하여 앱 내에서 자동 업데이트 알림 및 다운로드/설치를 지원하고, 개발자의 릴리스 프로세스를 자동화하기 위해 GitHub Actions를 구축합니다.

## Goals / Non-Goals

**Goals:**
- WinSparkle을 SoundSwitcher의 C# 코드에 통합(P/Invoke 활용).
- 백그라운드 및 수동(트레이 메뉴) 업데이트 확인 기능 구현.
- GitHub Actions를 사용해 앱 패키징 및 GitHub Release 자동 생성.
- WinSparkle Appcast 피드 자동 업데이트 및 배포.

**Non-Goals:**
- Microsoft Store 앱 업데이트 자동화는 이번 스코프에 포함되지 않습니다. (WinSparkle은 Non-store 버전을 위한 기능입니다.)

## Decisions

- **서명 알고리즘 (EdDSA)**: WinSparkle의 최신 권장 사항에 따라, 보안이 강화된 EdDSA(Ed25519) 알고리즘을 사용하여 업데이트 패키지에 서명하고 앱 내부에서 검증합니다. (기존 DSA 방식은 더 이상 권장되지 않음)
- **WinSparkle 도입**: 다양한 Windows 앱에서 검증된 오픈소스 업데이트 라이브러리이며, 사용하기 쉽고 안정적이므로 선택. C# 래퍼 클래스를 만들어 DLL 인터페이스를 호출합니다. (최신 공식 문서 참고: https://github.com/vslavik/winsparkle/wiki/Basic-Setup)
- **Appcast 호스팅**: 별도의 백엔드 서버를 두지 않고, GitHub Actions 워크플로우를 통해 GitHub Pages에 XML 피드를 정적 배포하여 유지보수 비용을 최소화합니다.
- **다국어 릴리스 노트**: 개발자가 `ReleaseNotes` 폴더에 언어별 마크다운 파일(예: `v1.0.0-ko.md`)을 관리하면, GitHub Actions가 이를 HTML로 변환하여 Appcast XML의 `sparkle:releaseNotesLink`(`xml:lang` 속성 포함)에 다중 연결하도록 파이프라인을 구축합니다.
- **GitHub Actions 패키징**: 기존 빌드 프로세스를 기반으로 CI/CD 스크립트를 작성하여, 태그 푸시 시 자동으로 GitHub Release가 발행되도록 합니다.

## Risks / Trade-offs

- **Risk**: C++로 작성된 WinSparkle DLL을 C#에서 P/Invoke로 호출할 때 아키텍처(x86/x64) 불일치 및 배포 누락 문제.
  - **Mitigation**: 프로젝트 빌드 시 올바른 아키텍처의 WinSparkle DLL 파일이 출력 디렉토리에 복사되도록 빌드 이벤트나 프로젝트 파일(`.csproj`)에 명시적으로 추가합니다.
- **Risk**: MSIX 패키지로 배포할 경우 Windows 자체 패키지 매니저(AppInstaller)를 통한 업데이트가 권장되며 WinSparkle과 충돌할 수 있습니다.
  - **Mitigation**: WinSparkle은 비 스토어 포터블(ZIP)이나 일반 설치 관리자(EXE/MSI) 버전에 중점적으로 사용되도록 빌드 구성을 분리하거나 플래그로 처리합니다.
