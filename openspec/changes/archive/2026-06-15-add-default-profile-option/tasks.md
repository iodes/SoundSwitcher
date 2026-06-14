## 1. App Settings Update

- [x] 1.1 Add `DefaultProfileId` property to the application settings class/structure to store the selected default profile identifier. (Ensure this property is placed immediately above `lastSelectedProfileId` in the code structure).
- [x] 1.2 Ensure the setting is correctly saved and loaded with existing settings.

## 2. Context Menu UI Update

- [x] 2.1 Add localization strings for "기본 프로파일로 설정" and "기본 프로파일 설정 해제" to all supported languages.
- [x] 2.2 Locate the code responsible for generating the context menu for profile items in the system tray.
- [x] 2.3 Insert a dynamic `MenuItem` whose text toggles between "기본 프로파일로 설정" and "기본 프로파일 설정 해제" depending on whether the profile is currently the default.
- [x] 2.4 Ensure the new menu item is positioned immediately above the "아이콘 변경" (Change Icon) menu item.
- [x] 2.5 Implement the click event handler for this menu item:
  - If setting as default: update `DefaultProfileId` to the selected profile's ID (this automatically unsets any previous default).
  - If unsetting: clear `DefaultProfileId`.
- [x] 2.6 Save settings after toggling the default profile.

## 3. Application Startup Logic

- [x] 3.1 Locate the application initialization sequence where profiles are loaded and the initial system state is evaluated.
- [x] 3.2 Add logic to retrieve the `DefaultProfileId` from settings.
- [x] 3.3 Verify that the profile associated with `DefaultProfileId` actually exists.
- [x] 3.4 Compare the default profile with the current audio configuration using the existing common profile equality logic.
- [x] 3.5 If the configurations are different, invoke the profile application logic to apply the default profile.
