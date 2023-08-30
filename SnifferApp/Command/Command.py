import os.path
from abc import abstractmethod
from Hub import SnifferHub
from Network.Servers.ServerManager import ServerManager
from Utils.Events import Event
from Command.CommandSignals import CommandSignal
from PySide6.QtWidgets import QFileDialog
from datetime import date


class Command:
    def __init__(self, snifferHub: SnifferHub, serverManager: ServerManager):
        self.app: SnifferHub = snifferHub
        self.serverManager: ServerManager = serverManager
        self.onCommandExecutedEvent = Event()

    @abstractmethod
    def execute(self) -> bool:
        if self.serverManager.selectedDevice is None:
            return False


class CommandHistory:
    def __init__(self):
        self.history = []

    def push(self, command: Command):
        self.history.append(command)

    def pop(self):
        if len(self.history) > 0:
            return self.history.pop()


class InitServerCommand(Command):
    def __init__(self, app: SnifferHub, server: ServerManager):
        super().__init__(app, server)

    def execute(self) -> bool:
        self.serverManager.start()
        self.serverManager.fileServer.start()
        self.serverManager.streamingServer.start()
        self.onCommandExecutedEvent(message="[Command]:: Init Server Executed")
        return True


class RecordCommand(Command):
    def __init__(self, snifferHub: SnifferHub, serverManager: ServerManager):
        super().__init__(snifferHub, serverManager)

    def execute(self) -> bool:
        super().execute()
        self.serverManager.selectedDevice.sendSignalToDevice(CommandSignal.RECORD)
        self.serverManager.streamingServer.streamingHelper.onRecordingStarted()
        self.onCommandExecutedEvent(message="Sending {} signal".format(CommandSignal.RECORD.value),
                                    signal=CommandSignal.RECORD)
        return True


class StopCommand(Command):
    def __init__(self, snifferHub: SnifferHub, serverManager: ServerManager):
        super().__init__(snifferHub, serverManager)

    def execute(self) -> bool:
        super().execute()
        self.serverManager.selectedDevice.sendSignalToDevice(CommandSignal.STOP_REC)
        self.serverManager.streamingServer.streamingHelper.onRecordingEnded()
        self.onCommandExecutedEvent(message="Sending {} signal".format(CommandSignal.STOP_REC.value),
                                    signal=CommandSignal.STOP_REC)
        return True


class ReplayCommand(Command):
    def __init__(self, snifferHub: SnifferHub, serverManager: ServerManager):
        super().__init__(snifferHub, serverManager)

    def execute(self) -> bool:
        super().execute()
        self.serverManager.selectedDevice.sendSignalToDevice(CommandSignal.REPLAY)
        self.onCommandExecutedEvent(message="Sending {} signal".format(CommandSignal.REPLAY.value),
                                    signal=CommandSignal.REPLAY)
        return True


class StopReplayCommand(Command):
    def __init__(self, snifferHub: SnifferHub, serverManager: ServerManager):
        super().__init__(snifferHub, serverManager)

    def execute(self) -> bool:
        super().execute()
        self.serverManager.selectedDevice.sendSignalToDevice(CommandSignal.STOP_REPLAY)
        self.onCommandExecutedEvent(message="Sending {} signal".format(CommandSignal.STOP_REPLAY.value),
                                    signal=CommandSignal.STOP_REPLAY)
        return True


class SaveFileCommand(Command):
    def __init__(self, snifferHub: SnifferHub, serverManager: ServerManager):
        super().__init__(snifferHub, serverManager)

    def execute(self) -> bool:
        super().execute()
        qfileTuple: tuple = QFileDialog.getSaveFileName(
            self.app.uiManager.GetWidget(), caption="Save File", filter="*.inputtrace")
        fileName = qfileTuple[0]
        if fileName != "":
            self.serverManager.fileServer.setFile(fileName)
            self.serverManager.fileServer.sendFile = False
            self.serverManager.selectedDevice.sendSignalToDevice(CommandSignal.SAVE_FILE)
            self.onCommandExecutedEvent(message="Sending {} signal".format(CommandSignal.SAVE_FILE.value),
                                        signal=CommandSignal.SAVE_FILE)
        else:
            print("File wasn't saved")
            return False

        return True

    def _onFileReceiveFinished(self, *args, **kwargs):
        super().execute()
        for progressEvent in self.serverManager.fileServer.progressEventsThreads:
            if not progressEvent.is_alive():
                progressEvent.start()
                progressEvent.join(timeout=3)


class LoadFileCommand(Command):
    def __init__(self, snifferHub: SnifferHub, serverManager: ServerManager):
        super().__init__(snifferHub, serverManager)

    def execute(self) -> bool:
        super().execute()
        qfileTuple: tuple = QFileDialog.getOpenFileName(
            self.app.uiManager.GetWidget(), caption="Load File", filter="*.inputtrace")

        fileName = qfileTuple[0]
        if fileName != "":
            self.serverManager.fileServer.setFile(fileName)
            self.serverManager.fileServer.sendFile = True
            self.serverManager.selectedDevice.sendSignalToDevice(CommandSignal.LOAD_FILE)
            self.onCommandExecutedEvent(message="Sending {} signal".format(CommandSignal.LOAD_FILE.value),
                                        signal=CommandSignal.LOAD_FILE)
        else:
            print("File wasn't loaded")

        return True


class SaveStreaming(Command):
    def __init__(self, snifferHub: SnifferHub, serverManager: ServerManager):
        super().__init__(snifferHub, serverManager)

    def execute(self) -> bool:
        super().execute()
        qfileTuple: tuple = QFileDialog.getSaveFileName(
            self.app.uiManager.GetWidget(), caption="Save streaming", filter="*.avi")
        streamingFileName = qfileTuple[0]
        if streamingFileName != "":
            self.serverManager.streamingServer.streamingHelper.saveFramesToVideo(streamingFileName)
        else:
            print("File wasn't saved")

        return True


class AutoSaveCommand(Command):
    def __init__(self, snifferHub: SnifferHub, serverManager: ServerManager):
        super().__init__(snifferHub, serverManager)
        self.inputDirectory = "TestRecFiles/"
        self.streamingDirectory = "TestStreamingVideos/"
        self.streamingFileExtension = ".avi"
        self.inputFileExtension = ".inputtrace"

    @staticmethod
    def createFileName(directory: str, fileExtension: str) -> str:
        today = date.today()
        currencies = 0
        newPath = f"{directory}{today}{fileExtension}"
        while os.path.isfile(newPath):
            currencies += 1
            newPath = f"{directory}{today}_{currencies}{fileExtension}"

        return newPath

    def execute(self) -> bool:
        #super().execute()
        inputFileName = self.createFileName(self.inputDirectory, self.inputFileExtension)
        streamingName = self.createFileName(self.streamingDirectory, self.streamingFileExtension)

        self.serverManager.streamingServer.streamingHelper.saveFramesToVideo(streamingName)

        self.serverManager.fileServer.setFile(inputFileName)
        self.serverManager.fileServer.sendFile = False
        self.serverManager.selectedDevice.sendSignalToDevice(CommandSignal.SAVE_FILE)
        self.onCommandExecutedEvent(message="Sending {} signal".format(CommandSignal.SAVE_FILE.value),
                                    signal=CommandSignal.SAVE_FILE)

        return True

