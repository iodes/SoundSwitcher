# Capability: default-profile

## Purpose
TBD: This capability manages the default audio profile settings, allowing users to designate a profile to be applied on application startup.

## Requirements

### Requirement: Set/Unset Default Profile
The system SHALL allow the user to designate a specific profile as the default profile or unset it using the context menu. The menu item text MUST support multiple languages (e.g., "기본 프로파일로 설정", "기본 프로파일 설정 해제"). The option MUST be placed above the "Change Icon" (아이콘 변경) option.

#### Scenario: User sets a profile as default
- **WHEN** the user right-clicks a profile item that is not the current default profile
- **AND WHEN** the user selects "기본 프로파일로 설정"
- **THEN** the selected profile's identifier is stored as the single default profile in the application settings
- **AND THEN** if there was a previously set default profile, it is automatically unset.

#### Scenario: User unsets a default profile
- **WHEN** the user right-clicks the profile item that is currently set as the default profile
- **AND WHEN** the user selects "기본 프로파일 설정 해제"
- **THEN** the default profile setting is cleared in the application settings.

### Requirement: Apply Default Profile on Startup
The system SHALL automatically apply the default profile when the application starts if the current active audio configuration is different from the default profile. The system MUST use the common profile equality logic to determine if the profiles differ.

#### Scenario: Startup with different current profile
- **WHEN** the application completes its initial startup and loads profiles
- **AND WHEN** a default profile is configured and valid
- **AND WHEN** the current audio state is different from the default profile (determined by common equality logic)
- **THEN** the system automatically applies the default profile.

#### Scenario: Startup with identical current profile
- **WHEN** the application completes its initial startup and loads profiles
- **AND WHEN** a default profile is configured and valid
- **AND WHEN** the current audio state is identical to the default profile (determined by common equality logic)
- **THEN** the system does not re-apply the profile.

#### Scenario: Startup with missing default profile
- **WHEN** the application completes its initial startup
- **AND WHEN** the configured default profile identifier does not match any existing profile
- **THEN** the system skips the automatic profile application and resumes normal operation.
