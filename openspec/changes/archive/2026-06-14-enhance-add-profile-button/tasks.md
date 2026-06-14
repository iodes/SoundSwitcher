## 1. UI 컴포넌트 분석

- [x] 1.1 `MainPage.xaml` 또는 관련된 뷰 파일에서 프로파일 추가 버튼의 코드를 찾고 현재 적용된 스타일 확인

## 2. 스타일 및 효과 구현

- [x] 2.1 프로파일 추가 버튼에 적용할 고유한 스타일(ControlTemplate) 작성 (모서리 둥글게, 테두리 부드럽게)
- [x] 2.2 코드 비하인드를 배제하고, 순수 XAML 레벨에서 `EventTrigger` 혹은 `ControlTemplate.Triggers`를 활용하여 마우스 오버 시(IsMouseOver) 배경색 변화 및 흐림 효과 애니메이션 추가
- [x] 2.3 클릭 시(IsPressed) 버튼 크기가 살짝 줄어들도록 RenderTransform(ScaleX, ScaleY)을 변경하는 애니메이션을 순수 XAML로 작성

## 3. 검증 및 테스트

- [x] 3.1 프로젝트 빌드 후 앱 실행하여 프로파일 추가 버튼에 시각적 피드백(마우스 오버, 클릭 효과)이 정상적으로 동작하는지 확인
- [x] 3.2 기존의 키보드 접근성(FocusVisualStyle) 등이 훼손되지 않았는지 확인
