## Context

사용자가 커스텀 프로파일 이미지를 선택하면, `IconCacheService`는 해당 이미지를 로컬 앱 데이터 디렉터리로 복사하고 그 절대 경로를 반환합니다. 이 절대 경로가 프로파일 설정(`IconPath`)에 저장됩니다. 이는 프로파일 설정이 이전되거나, 다른 기기와 동기화되거나, 사용자의 로컬 앱 데이터 경로가 변경될 경우 경로가 깨지는 문제를 발생시킵니다.

## Goals / Non-Goals

**Goals:**
- `DeviceProfile`의 `IconPath`에 절대 경로 대신 상대 경로(파일 이름)만 저장합니다.

**Non-Goals:**
- 기존 캐시 디렉터리 위치 자체를 변경하는 것.
- 지원되는 이미지 포맷이나 캐싱 메커니즘을 변경하는 것.

## Decisions

- **`IconCacheService.CacheIcon` 수정**: 절대 경로(`destFilePath`) 대신 생성된 파일 이름(`fileName`)만 반환하도록 메서드를 수정합니다.
- **`IconCacheService.GetIconFullPath` 추가**: 설정에 저장된 파일 이름(`IconPath`)을 인자로 받아 캐시 디렉터리를 기준 삼아 절대 경로를 생성 및 반환하는 새로운 메서드를 추가합니다. 만약 파일이 존재하지 않거나 경로가 손상된 경우 `null`을 반환하여 UI가 이를 즉시 감지하고 기본 장치 아이콘으로 폴백(fallback)할 수 있도록 가드(guard) 역할을 수행합니다.
- **ViewModels 및 App.xaml.cs 업데이트**: 이미지를 불러오거나 존재 여부를 확인할 때, 저장된 파일명을 그대로 사용하지 않고 `IconCacheService.GetIconFullPath(profile.IconPath)`를 통과시킨 후 사용하도록 `App.xaml.cs`, `DeviceProfileViewModel.cs`, `MainViewModel.cs`를 업데이트합니다.
- **`IconCacheService.DeleteIcon` 업데이트**: 삭제를 시도하기 전에 전달받은 파일 이름을 바탕으로 올바른 절대 경로를 구성하도록 로직을 수정합니다.

## Risks / Trade-offs

- **Risk**: 기존에 절대 경로로 저장되어 있던 사용자의 프로파일 이미지는 더 이상 표시되지 않게 되며, 이전 버전과의 하위 호환성은 의도적으로 제공하지 않습니다.
  - **Mitigation**: 이는 향후 경로 구조를 단순화하고 강제하기 위해 감수할 수 있는 트레이드오프로 간주합니다. 기존 사용자들은 아이콘을 다시 한 번 선택해 주어야 합니다.
