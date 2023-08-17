from abc import abstractmethod
from enum import Enum
from Hub import SnifferHub
from Network import ServerManager
from Utils.Events import Event


class CommandSignal(Enum):
    PLAY = "PLAY"
    STOP_PLAY = "STOP_PLAY"
    REPLAY = "REPLAY"
    STOP_REPLAY = "STOP_REPLAY"
    SEND_FILE = "SEND_FILE"
    GET_FILE = "GET_FILE"
    GET_DEVICE_DATA = "GET_DEVICE_DATA"


class Command:
    def __init__(self, snifferHub: SnifferHub, serverManager: ServerManager):
        self.app = snifferHub
        self.server = serverManager
        self.onCommandExecutedEvent = Event()

    @abstractmethod
    def execute(self) -> bool:
        return False


class CommandHistory:
    def __init__(self):
        self.history = []

    def push(self,command: Command):
        self.history.append(command)

    def pop(self):
        if len(self.history) > 0:
            return self.history.pop()


class InitServerCommand(Command):
    def __init__(self, app: SnifferHub, server: ServerManager):
        super().__init__(app, server)

    def execute(self) -> bool:
        self.server.startServer()
        self.onCommandExecutedEvent(message="[Command]:: Init Server Executed")
        return True


class PlayCommand(Command):
    def __init__(self, snifferHub: SnifferHub, serverManager: ServerManager):
        super().__init__(snifferHub, serverManager)

    def execute(self) -> bool:
        self.onCommandExecutedEvent(message="Sending {} signal".format(CommandSignal.PLAY))
        return True
