import os.path
from abc import abstractmethod
from Hub import SnifferHub
from Network.Servers.ServerManager import ServerManager
from Network.Clients.DeviceClient import DeviceClient
from Utils.Events import Event
from Commands.CommandSignals import CommandSignal
from PySide6.QtWidgets import QFileDialog
from datetime import date, datetime
import re


class Command:
    def __init__(self, snifferHub: SnifferHub, serverManager: ServerManager):
        self.app: SnifferHub = snifferHub
        self.serverManager: ServerManager = serverManager
        self.onCommandExecutedEvent = Event()

    @abstractmethod
    def execute(self) -> bool:
        if self.serverManager.selectedDevices is None:
            return False

    def sendExecutedEventSignal(self, signal: CommandSignal, device: DeviceClient, customMessage=None):
        if customMessage is None:
            self.onCommandExecutedEvent(message=f"Sending signal {signal} to {device.address}")
        else:
            self.onCommandExecutedEvent(message=customMessage)


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
        self.onCommandExecutedEvent(message="[Commands]:: Init Server Executed")
        return True


class RecordCommand(Command):
    def __init__(self, snifferHub: SnifferHub, serverManager: ServerManager):
        super().__init__(snifferHub, serverManager)

    def execute(self) -> bool:
        super().execute()

        for selectedDevice in self.serverManager.selectedDevices:
            selectedDevice.sendSignalToDevice(CommandSignal.RECORD)
            if selectedDevice.id in self.serverManager.streamingServer.helpers:
                self.serverManager.streamingServer.helpers[selectedDevice.id].onRecordingStarted()

        self.onCommandExecutedEvent(message="Sending {} signal".format(CommandSignal.RECORD.value),
                                    signal=CommandSignal.RECORD)
        return True


class StopCommand(Command):
    def __init__(self, snifferHub: SnifferHub, serverManager: ServerManager):
        super().__init__(snifferHub, serverManager)

    def execute(self) -> bool:
        super().execute()
        for selectedDevice in self.serverManager.selectedDevices:
            selectedDevice.sendSignalToDevice(CommandSignal.STOP_REC)
            if selectedDevice.id in self.serverManager.streamingServer.helpers:
                self.serverManager.streamingServer.helpers[selectedDevice.id].onRecordingEnded()

        self.onCommandExecutedEvent(message="Sending {} signal".format(CommandSignal.STOP_REC.value),
                                    signal=CommandSignal.STOP_REC)
        return True


class ReplayCommand(Command):
    def __init__(self, snifferHub: SnifferHub, serverManager: ServerManager):
        super().__init__(snifferHub, serverManager)

    def execute(self) -> bool:
        super().execute()
        for selectedDevice in self.serverManager.selectedDevices:
            selectedDevice.sendSignalToDevice(CommandSignal.REPLAY)
        self.onCommandExecutedEvent(message="Sending {} signal".format(CommandSignal.REPLAY.value),
                                    signal=CommandSignal.REPLAY)
        return True


class StopReplayCommand(Command):
    def __init__(self, snifferHub: SnifferHub, serverManager: ServerManager):
        super().__init__(snifferHub, serverManager)

    def execute(self) -> bool:
        super().execute()
        for selectedDevice in self.serverManager.selectedDevices:
            selectedDevice.sendSignalToDevice(CommandSignal.STOP_REPLAY)
        self.onCommandExecutedEvent(message="Sending {} signal".format(CommandSignal.STOP_REPLAY.value),
                                    signal=CommandSignal.STOP_REPLAY)
        return True


class SaveCommand(Command):
    def __init__(self, snifferHub: SnifferHub, serverManager: ServerManager):
        super().__init__(snifferHub, serverManager)
        self.inputDirectory = "TestRecFiles/"
        self.streamingDirectory = "TestStreamingVideos/"
        self.streamingFileExtension = ".avi"
        self.inputFileExtension = ".inputtrace"

        if not os.path.isdir(self.inputDirectory):
            os.mkdir(self.inputDirectory)

        if not os.path.isdir(self.streamingDirectory):
            os.mkdir(self.streamingDirectory)

    def execute(self) -> bool:
        super().execute()
        qfileTuple: tuple = QFileDialog.getSaveFileName(
            self.app.uiManager.GetWidget(), caption="Save File", filter="*.inputtrace" )

        fileName = qfileTuple[0]
        if fileName != "":
            for device in self.serverManager.selectedDevices:
                self.saveVideo(fileName, device)
                self.saveFile(fileName, device)
                self.sendExecutedEventSignal(CommandSignal.SAVE_FILE, device)
        else:
            print("File wasn't saved")
            return False
        return True

    def saveFile(self, fileName: str, device: DeviceClient):
        realName = self.inputDirectory + f"{device.hostname}_{device.ip}__" + fileName

        self.serverManager.fileServer.sendFile = False
        self.serverManager.fileServer.setFile(device.id, realName)
        device.sendSignalToDevice(CommandSignal.SAVE_FILE)

    def saveVideo(self, fileName: str, device: DeviceClient):
        baseName = os.path.basename(fileName)
        fullName = f"{self.streamingDirectory}{device.hostname}_{device.ip}__{baseName}"
        if self.streamingFileExtension not in fullName:
            fullName += self.streamingFileExtension

        if device.id in self.serverManager.streamingServer.helpers:
            self.serverManager.streamingServer.helpers[device.id].saveFramesToVideo(fullName)

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
            self.serverManager.fileServer.sendFile = True
            for selectedDevice in self.serverManager.selectedDevices:
                self.serverManager.fileServer.setFile(selectedDevice.id, fileName)
                selectedDevice.sendSignalToDevice(CommandSignal.LOAD_FILE)
                self.sendExecutedEventSignal(CommandSignal.LOAD_FILE, selectedDevice)
        else:
            print("File wasn't loaded")

        return True


class AutoSaveCommand(SaveCommand):
    def __init__(self, snifferHub: SnifferHub, serverManager: ServerManager):
        super().__init__(snifferHub, serverManager)

    @staticmethod
    def createFileName(fileExtension: str) -> str:
        now = datetime.now()
        baseName = str(date.today()) + "__" + str(now.time())
        baseName = re.sub(r'[^\w_. -]', '_', baseName)
        currencies = 0
        newPath = f"{baseName}{fileExtension}"
        while os.path.isfile(newPath):
            currencies += 1
            newPath = f"{baseName}_{currencies}{fileExtension}"

        return newPath

    def execute(self) -> bool:
        inputFileName = self.createFileName(self.inputFileExtension)
        streamingName = self.createFileName(self.streamingFileExtension)
        for device in self.serverManager.selectedDevices:
            self.saveVideo(streamingName, device)
            self.saveFile(inputFileName, device)
            self.sendExecutedEventSignal(CommandSignal.SAVE_FILE, device)
        return True


class ReplayOneStepCommand(Command):
    def __init__(self, snifferHub: SnifferHub, serverManager: ServerManager):
        super().__init__(snifferHub, serverManager)

    def execute(self) -> bool:
        for selectedDevice in self.serverManager.selectedDevices:
            selectedDevice.sendSignalToDevice(CommandSignal.REPLAY_ONE_STEP)
            self.sendExecutedEventSignal(CommandSignal.REPLAY_ONE_STEP, selectedDevice)

        return len(self.serverManager.selectedDevices) > 0
