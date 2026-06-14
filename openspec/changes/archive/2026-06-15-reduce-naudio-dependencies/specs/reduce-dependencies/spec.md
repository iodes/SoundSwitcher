## ADDED Requirements

### Requirement: Reduced NAudio Dependencies
The system SHALL use only the necessary NAudio packages required for its operation, specifically `NAudio.Wasapi` and `NAudio.Core`, instead of the monolithic `NAudio` metapackage.

#### Scenario: Compiling with specific NAudio packages
- **WHEN** the application is compiled
- **THEN** it successfully builds without referencing `NAudio.WinForms`, `NAudio.Midi`, or `NAudio.Asio`
- **AND** all audio device management functionality works as expected
