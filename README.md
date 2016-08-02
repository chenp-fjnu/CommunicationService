# CommunicationService
This project will provide one solution to setup communication between processes via Shared Memory.

Source code from MSDN: https://code.msdn.microsoft.com/windowsdesktop/Inter-process-communication-e96e94e7

My changes:

Change to use EventWaitHandle instead Thread.Sleep for read/write thread.Four signals per communicator, any better way?

Cache MemoryMappedFileCommunicator for specific key in factory.

this is the first commit from SourceTree
