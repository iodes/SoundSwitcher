using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace SoundSwitcher.Services;

/// <summary>
/// COM interface for changing the Windows default audio device.
/// This is an undocumented Windows API, but it is widely used
/// for changing default playback/communication devices.
/// </summary>
[GeneratedComInterface]
[Guid("F8679F50-850A-41CF-9C72-430F290290C8")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal partial interface IPolicyConfig
{
    // Unused methods (placeholders to preserve vtable order)
    [PreserveSig]
    int GetMixFormat();

    [PreserveSig]
    int GetDeviceFormat();

    [PreserveSig]
    int ResetDeviceFormat();

    [PreserveSig]
    int SetDeviceFormat();

    [PreserveSig]
    int GetProcessingPeriod();

    [PreserveSig]
    int SetProcessingPeriod();

    [PreserveSig]
    int GetShareMode();

    [PreserveSig]
    int SetShareMode();

    [PreserveSig]
    int GetPropertyValue();

    [PreserveSig]
    int SetPropertyValue();

    [PreserveSig]
    int SetDefaultEndpoint(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceId,
        ERole role);

    [PreserveSig]
    int SetEndpointVisibility();
}

/// <summary>
/// Windows audio device roles.
/// </summary>
internal enum ERole
{
    eConsole = 0, // Default device
    eMultimedia = 1, // Multimedia
    eCommunications = 2 // Communication device
}


