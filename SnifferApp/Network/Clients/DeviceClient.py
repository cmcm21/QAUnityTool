import socket
from Utils.Events import Event
from enum import Enum
from Commands.CommandSignals import CommandSignal
from Network.GeneralSocket import GeneralSocket

BUFFER_SIZE = 1024


class DeviceState(Enum):
    IDLE = "IDLE"
    RECORDING = "RECORDING"
    PLAYING_BACK = "PLAYING_BACK"


class DeviceClient(GeneralSocket):
    def __init__(self, deviceSocket: socket.socket, address):
        super().__init__(deviceSocket, address)
        print("Device client: " + str(address) + " connected")
        self.deviceState = DeviceState.IDLE
        self.msgReceivedEvent = Event()
        self.stateChangedEvent = Event()
        self.replayFinished = Event()
        self.deviceDisconnectedEvent = Event()

    def start(self):
        self.listeningSocket = True
        self.socketThread.start()

    def _socketWorker(self):
        while self.listeningSocket:
            try:
                message = self.socket.recv(BUFFER_SIZE)
                if message == b'':
                    self._handleDeviceDisconnection()
                else:
                    self.msgReceivedEvent(message="Device[{}]: sniffer core {}".format(self.ip, message.decode()))
                    self._decodeClientMessage(message.decode())
            except ConnectionError:
                self._handleDeviceDisconnection()

    def _handleDeviceDisconnection(self):
        print(f"Device client {self.address} disconnected")
        self.deviceDisconnectedEvent(device=self)
        self.close()

    def _decodeClientMessage(self, message: str):
        if message == DeviceState.IDLE.value:
            self.deviceState = DeviceState.IDLE
        elif message == DeviceState.RECORDING.value:
            self.deviceState = DeviceState.RECORDING
        elif message == DeviceState.PLAYING_BACK.value:
            self.deviceState = DeviceState.PLAYING_BACK

        self.stateChangedEvent(state=self.deviceState)

    def sendSignalToDevice(self, signal: CommandSignal):
        try:
            send = self.socket.send(signal.value.encode())
            if send == 0:
                self._handleDeviceDisconnection()
        except ConnectionError:
            self._handleDeviceDisconnection()

    def close(self):
        super().close()
