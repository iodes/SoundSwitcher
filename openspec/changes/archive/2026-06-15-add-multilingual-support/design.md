## Context

현재 SoundSwitcher는 UI가 한국어(또는 영어 등 특정 언어)로 하드코딩되어 있어, 릴리스 노트에 명시된 다국어 사용자(영어, 일본어, 한국어, 중국어 간체, 번체)에게 최적화된 경험을 제공하지 못하고 있습니다. 
따라서 다국어 지원을 위해 `LocalizationManager`를 도입하여 프로그램 내에서 동적으로 언어를 전환하고 관리할 수 있도록 설계합니다. 모델은 GestureWheel 리포지토리의 로컬라이제이션 패턴을 참고합니다.

## Goals / Non-Goals

**Goals:**
- UI 텍스트를 리소스(`.resx`) 파일로 분리
- 런타임에 동적으로 언어 전환 지원 (`en-US`, `ko-KR`, `ja-JP`, `zh-CN`, `zh-TW`)
- 설정 파일에 선택한 언어를 저장 및 로드
- XAML 및 뷰모델에서의 데이터 바인딩을 이용한 텍스트 반영

**Non-Goals:**
- 알림음이나 사운드 파일의 지역화는 제외 (UI 텍스트에 한정)
- 현재 제공되는 5개 언어 외의 추가 언어 자동 번역 시스템 구축

## Decisions

1. **LocalizationManager 구현 (INotifyPropertyChanged 활용)**
   - `GestureWheel` 저장소를 참고하여 싱글톤 패턴의 `LocalizationManager` 클래스를 `Localization` 네임스페이스 아래에 생성합니다.
   - `INotifyPropertyChanged` 인터페이스를 구현하고 인덱서(`this[string key]`)를 제공하여 XAML 바인딩 시 리소스 키를 쉽게 가져오도록 합니다.

2. **리소스 파일(.resx) 활용**
   - 기존의 하드코딩된 문자열들을 `Strings.resx`(기본), `Strings.ko-KR.resx`, `Strings.ja-JP.resx`, `Strings.zh-CN.resx`, `Strings.zh-TW.resx` 로 분리합니다.
   - WPF에서 `x:Static` 또는 `Binding`을 활용해 문자열을 동적으로 바꿉니다.

3. **설정 연동**
   - 기존 설정 스키마(`Configuration` 클래스 등)에 `Language` 속성을 추가합니다.
   - 앱이 시작될 때 설정에서 언어를 불러오고, 없을 경우 OS 기본 언어에 맞춥니다.

## Risks / Trade-offs

- **[Risk] 하드코딩 문자열 누락** → XAML 파일과 코드 비하인드에 산재한 문자열을 꼼꼼히 검색(`grep`)하여 놓치지 않도록 주의합니다.
- **[Risk] 번역 퀄리티 및 길이** → 긴 문자열로 인한 UI 깨짐이 발생할 수 있으므로 텍스트 길이를 고려해 유연한 UI 레이아웃(Wrap, Auto Size 등)을 구성해야 합니다.
