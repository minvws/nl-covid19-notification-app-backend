# Google Apple Exposure Notification Format

This folder contains the protocol buffers specifications from Google and Apple along with the tools needed to generate our code from them, and the generate code itself.

## Generating types

To generate the types, first install `protoc`, get it working in your path, and then execute `generate.bat`.

`TemporaryExposureKeyExport.proto` is the specification file we use for our generation. This is based on the Google specification with changes made to make it compatible with Apple's specification.

## Google and Apple specifications

`TemporaryExposureKeyExport.apple.proto` is the Apple specification and is provided for documentation purposes only.
`TemporaryExposureKeyExport.google.proto` is the Google specification and is provided for documentation purposes only.

These were sourced on 2020-07-15 at 14:30 from the urls below:

Google: https://raw.githubusercontent.com/google/exposure-notifications-server/1208580e92e3a9f23d85eb570fbdff8d1f6e7a00/internal/pb/export/export.proto
Apple: https://developer.apple.com/documentation/exposurenotification/setting_up_an_exposure_notification_server
