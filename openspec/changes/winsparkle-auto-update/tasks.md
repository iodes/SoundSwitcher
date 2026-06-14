## 1. WinSparkle 라이브러리 연동 준비

- [x] 1.1 WinSparkle 바이너리(DLL) 파일 다운로드 및 프로젝트에 추가 (참고: [Basic Setup 가이드](https://github.com/vslavik/winsparkle/wiki/Basic-Setup))
- [x] 1.2 빌드 스크립트(.csproj) 수정하여 빌드 출력 디렉토리에 WinSparkle.dll이 올바르게 복사되도록 설정
- [x] 1.3 C# P/Invoke를 위한 `WinSparkleNative` 래퍼 클래스 선언 작성
- [x] 1.4 EdDSA(Ed25519) 키 페어 생성 및 C# 앱 내부에 공개키(Public Key) 적용 설정

## 2. 자동 업데이트 기능 구현

- [x] 2.1 앱 시작 시 백그라운드 업데이트 확인 로직 구현 (`win_sparkle_init` 호출 등)
- [x] 2.2 앱 종료 시 WinSparkle 리소스 정리 로직 추가 (`win_sparkle_cleanup`)
- [x] 2.3 Appcast URL, 앱 이름, 버전 등 WinSparkle 초기화 설정 코드 작성
- [x] 2.4 트레이 아이콘 메뉴에 "업데이트 확인" 항목 추가 및 수동 확인 기능(`win_sparkle_check_update_with_ui`) 연결

## 3. GitHub Actions 기반 자동화 구축

- [x] 3.1 GitHub Actions 릴리스 워크플로우 파일 생성 (`.github/workflows/release.yml`)
- [x] 3.2 코드 빌드 및 배포용 패키징(MSIX, 포터블용 ZIP 등) 스텝 추가
- [x] 3.3 버전 태그(tag) 푸시 이벤트 시 자동으로 GitHub Release 생성 및 에셋 업로드 설정

## 4. Appcast 피드 및 다국어 릴리스 노트 파이프라인 연동

- [x] 4.1 프로젝트 내 `ReleaseNotes` 폴더의 언어별 마크다운 파일(예: `v1.0.0-ko.md`)을 HTML로 자동 변환하는 스텝 추가
- [x] 4.2 GitHub Secrets의 EdDSA 개인키를 이용해 배포 파일 서명(Signature) 생성
- [x] 4.3 생성된 서명 값과 다국어 릴리스 노트 HTML 링크(`xml:lang` 매핑)를 포함하여 Appcast XML 생성
- [x] 4.4 생성된 Appcast XML과 HTML 릴리스 노트들을 GitHub Pages 등에 배포하도록 설정
- [x] 4.5 C# 코드에 설정할 Appcast URL을 실제 배포 위치로 매핑 및 테스트
