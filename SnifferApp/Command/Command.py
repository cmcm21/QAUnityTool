from abc import abstractmethod
from Hub import SnifferHub
from Network.ServerManager import ServerManager
from Network.FileServer import FileServer
from Utils.Events import Event
from Command.CommandSignals import CommandSignal
from PySide6.QtWidgets import QFileDialog


class Command:
    def __init__(self, snifferHub: SnifferHub, serverManager: ServerManager):
        self.app: SnifferHub = snifferHub
        self.serverManager: ServerManager = serverManager
        self.onCommandExecutedEvent = Event()

    @abstractmethod
    def execute(self) -> bool:
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
        self.serverManager.startServer()
        self.serverManager.fileServer.startServer()
        self.onCommandExecutedEvent(message="[Command]:: Init Server Executed")
        return True


class RecordCommand(Command):
    def __init__(self, snifferHub: SnifferHub, serverManager: ServerManager):
        super().__init__(snifferHub, serverManager)

    def execute(self) -> bool:
        self.serverManager.selectedDevice.sendSignalToDevice(CommandSignal.RECORD)
        self.onCommandExecutedEvent(message="Sending {} signal".format(CommandSignal.RECORD.value),
                                    signal=CommandSignal.RECORD)
        return True


class StopCommand(Command):
    def __init__(self, snifferHub: SnifferHub, serverManager: ServerManager):
        super().__init__(snifferHub, serverManager)

    def execute(self) -> bool:
        self.serverManager.selectedDevice.sendSignalToDevice(CommandSignal.STOP_REC)
        self.onCommandExecutedEvent(message="Sending {} signal".format(CommandSignal.STOP_REC),
                                    signal=CommandSignal.STOP_REC)
        return True


class ReplayCommand(Command):
    def __init__(self, snifferHub: SnifferHub, serverManager: ServerManager):
        super().__init__(snifferHub, serverManager)

    def execute(self) -> bool:
        self.serverManager.selectedDevice.sendSignalToDevice(CommandSignal.REPLAY)
        self.onCommandExecutedEvent(message="Sending {} signal".format(CommandSignal.REPLAY.value),
                                    signal=CommandSignal.REPLAY)
        return True


class StopReplayCommand(Command):
    def __init__(self, snifferHub: SnifferHub, serverManager: ServerManager):
        super().__init__(snifferHub, serverManager)

    def execute(self) -> bool:
        self.serverManager.selectedDevice.sendSignalToDevice(CommandSignal.STOP_REPLAY)
        self.onCommandExecutedEvent(message="Sending {} signal".format(CommandSignal.STOP_REPLAY.value),
                                    signal=CommandSignal.STOP_REPLAY)
        return True


class SaveFileCommand(Command):
    def __init__(self, snifferHub: SnifferHub, serverManager: ServerManager):
        super().__init__(snifferHub, serverManager)

    def execute(self) -> bool:
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

        return True

    def _onFileReceiveFinished(self, *args, **kwargs):
        for progressEvent in self.serverManager.fileServer.progressEventsThreads:
            if not progressEvent.is_alive():
                progressEvent.start()
                progressEvent.join(timeout=3)


class LoadFileCommand(Command):
    def __init__(self, snifferHub:SnifferHub, serverManager:ServerManager):
        super().__init__(snifferHub, serverManager)

    def execute(self) -> bool:
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
