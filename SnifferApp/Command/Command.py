from abc import abstractmethod
from enum import Enum
from Hub import SnifferHub
from Network import ServerManager
from Utils.Events import Event
from Command.CommandSignals import CommandSignal


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
