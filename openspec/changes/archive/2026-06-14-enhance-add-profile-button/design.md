## Context

현재 SoundSwitcher 앱의 메인 화면에서 프로파일을 추가하는 버튼은 기본 상태에서 밋밋하여 사용자가 상호작용할 때 시각적 피드백이 부족합니다. 사용자 경험 향상을 위해 WPF/XAML에서 제공하는 스타일 및 트리거(Trigger) 또는 스토리보드(Storyboard)를 사용하여 애니메이션과 테두리 효과를 추가하고자 합니다.

## Goals / Non-Goals

**Goals:**
- 프로파일 추가 버튼에 마우스 오버 시 부드러운 배경색 변화 및 테두리 효과 추가
- 클릭 시 버튼이 살짝 눌리는(Scale Transform) 애니메이션 효과 적용
- 기존 UI 테마에 자연스럽게 녹아드는 시각적 개선

**Non-Goals:**
- 프로파일을 생성하고 저장하는 비즈니스 로직(백엔드 로직)의 변경
- 다른 버튼들의 전역 스타일 변경 (이 작업은 프로파일 추가 버튼에만 국한됨)

## Decisions

- **UI 프레임워크 기능 활용**: 별도의 서드파티 UI 라이브러리를 추가하지 않고, 순수 WPF의 `ControlTemplate`, `EventTrigger`, `VisualStateManager` 등을 활용하여 스타일을 오버라이딩합니다. 특히 코드 비하인드에서 스토리보드를 호출하지 않고, 오직 XAML 레벨의 트리거(`EventTrigger`, `Trigger`)를 통해서만 애니메이션이 동작하도록 구현합니다.
- **애니메이션 기법**: 마우스 오버 시 색상 전환은 `ColorAnimation`을 사용하며, 클릭 시 눌림 효과는 `RenderTransform`과 `DoubleAnimation` (ScaleX, ScaleY)을 사용합니다. 테두리 흐림 효과는 `DropShadowEffect` 또는 `Border`의 `CornerRadius`와 색상을 조절하여 구현합니다.

## Risks / Trade-offs

- 버튼 스타일링을 오버라이딩하면서 기존에 있던 기본적인 포커스나 접근성 관련 스타일이 깨질 수 있으므로, 키보드 네비게이션(포커스 비주얼) 등을 유지해야 합니다.
